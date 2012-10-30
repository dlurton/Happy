using System;
using System.Data.SqlClient;
using System.IO;
using HappyTemplate.Runtime;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SqlGen.Schema;

namespace SqlGen
{
	public class Program
	{
		public static string GetResourceString(string resourceName)
		{
			using (Stream s = typeof(Program).Assembly.GetManifestResourceStream("SqlGen." + resourceName))
			{
				if(s == null)
					throw new ApplicationException("Unable to load resource " + resourceName);
				using(StreamReader sr = new StreamReader(s))
				{
					return sr.ReadToEnd();
				}
			}
		}
		
		static string ReadFile(string path)
		{
			using(var file = File.OpenText(path))
			{
				return file.ReadToEnd();
			}
		}

		class CmdLineArgs
		{
			public string Server;
			public string Database;
			public string HappyModuleFile;
			public string OutputFile;
		}

		class FatalException : Exception
		{
			public FatalException(string message) : base(message)
			{
				
			}
		}

		public static void Main(string[] args)
		{
			try
			{
				
				CmdLineArgs cmdline = GetCmdLineArgs(args);
				ReadASchema(cmdline);

				string module = ReadHappySource(cmdline);
				if(Environment.ExitCode != 0) return;

				HappyRuntimeContext hrc = CompileModule(module);
				if(Environment.ExitCode != 0) return;

				Console.WriteLine("Connecting to sql server...");
				string connectionString = GetConnectionString(cmdline);
				using(SqlConnection con = new SqlConnection(connectionString))
				{
					Database database = GetDatabase(con);

					//database.Tables[0].
					Console.WriteLine("Generating...");

					hrc.Globals.w_GenerateSprocs(database);

					Console.WriteLine("Saving " + cmdline.OutputFile);
					using(FileStream fs = File.Open(cmdline.OutputFile, FileMode.Create, FileAccess.Write))
					using(StreamWriter sw = new StreamWriter(fs))
						sw.Write(hrc.OutputWriter.ToString());

					Console.WriteLine("Complete!");

				}
			}
			catch(FatalException e)
			{
				Console.Error.WriteLine(e.Message);
				Environment.ExitCode = -1;
			}
		}

		private static void ReadASchema(CmdLineArgs args)
		{
			DatabaseData data = new DatabaseData(args.Server, args.Database);

			foreach(var table in data.Tables)
			{
				Console.WriteLine("{0}.{1} {2}", table.Schema, table.Name, table.ColumnCount);
				foreach(var column in table.Columns)
				{
					Console.WriteLine("\t{0} {1}({2})", column.Name, column.DataType, column.Length);
				}
			}

		}

		static Database GetDatabase(SqlConnection con)
		{
			ServerConnection connection = new ServerConnection(con);
			Server server = new Server(connection);
			Database database = server.Databases[con.Database];

			if(database == null)
				throw new FatalException("Connected to db server but the database specified in connection string does not appear to be present.");
			return database;
		}

		static string GetConnectionString(CmdLineArgs cmdline)
		{
			SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
			                                                     {
			                                                     	DataSource = cmdline.Server,
			                                                     	InitialCatalog = cmdline.Database,
																	IntegratedSecurity = true
			                                                     };
			return connectionStringBuilder.ToString();
		}

		static CmdLineArgs GetCmdLineArgs(string[] args)
		{
			if(args.Length != 4)
				throw new FatalException("usage:  SqlGen <server> <database> <happy module> <output file>");

			return new CmdLineArgs
			       {
			       	Server = args[0],
					Database = args[1],
					HappyModuleFile = args[2],
					OutputFile = args[3]
			       };
		}

		static HappyRuntimeContext CompileModule(string module)
		{
			Console.WriteLine("Compiling template set...");
			HappyRuntimeContext hrc;
			try
			{
				hrc = HappyRuntimeContext.CompileModule(module,
				                                        Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(Program).Assembly.CodeBase).AbsolutePath)) + ".config");
			}
			catch(HappyTemplate.Exceptions.SourceException e)
			{
				throw new FatalException(string.Format("{0}: {1}", e.Location.Span.Start, e.Message));
			}
			catch(Microsoft.Scripting.SyntaxErrorException se)
			{
				throw new FatalException(se.Message);
			}
			return hrc;
		}

		static string ReadHappySource(CmdLineArgs args)
		{
			string module;
			try
			{
				module = ReadFile(args.HappyModuleFile);
			}
			catch(Exception e)
			{
				throw new FatalException(string.Format("Couldn't open {0}\n{1}:  {2}", args.HappyModuleFile, e.GetType(), e.Message));
			}
			return module;
		}
	}
}
