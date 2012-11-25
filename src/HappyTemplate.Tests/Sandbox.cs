/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using HappyTemplate.Runtime.Binding;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace HappyTemplate.Tests
{
	[TestFixture, Ignore]
	public class Sandbox : TestFixtureBase
	{
		[Test]
		public void StackEmpty3()
		{
			var script = @"
function foo()
{
	for(table in sessionData between)";

			base.CompileModule(script);
		}
		[Test]
		public void StackEmpty2()
		{
			var script = @"
function foo()
{
        ~<|readonly Type[] _mapperTypes = new Type[] {
            |% for(table in sessionData.tables between ) { def mapperName = getMapperName(table);
                ~<|d
            %|
        };
}
|>;
}";
			base.CompileModule(script);
		}

		[Test]
		public void StackEmpty()
		{
			var script = @"use System;

def endl = ""\r\n"";

def gQuickDb = new(QuickConnection, ""Data Source=(local);Initial Catalog=trunk_Healinx;Integrated Security=Yes"");
def gTableCmd = gQuickDb.CreateCommand(
    <|
        SELECT
            * 
        FROM sys.tables
     |>);

def gColumnCmd = gQuickDb.CreateCommand(<|
SELECT 
	c.name as column_name, 
	types.name as [type_name],
    c.is_identity,
	c.max_length,
	c.precision,
	c.scale,
	c.is_nullable,
	types.is_user_defined AS is_user_defined_type
FROM sys.columns c
	JOIN sys.types types ON types.system_type_id = c.system_type_id
	JOIN sys.tables tables ON tables.object_id = c.object_id
	JOIN sys.schemas schemas ON tables.schema_id = tables.schema_id
WHERE 
	tables.name = @table_name AND schemas.name = @schema_name
|>);



function main()
{
    def sessionData = QuickYaml.FromFile(""ExampleSession.yaml"");
    gColumnCmd.SetParameter(""@schema_name"", sessionData.schema);
~<|
using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;

namespace $sessionData.namespace$
{
    public class $sessionData.sessionFactoryHelperName$
    {
        static ISessionFactory _sessionFactory;

        public static ISession OpenSession()
        {
            if(_sessionFactory == null)
            {
                _sessionFactory = new SessionFactoryProvider
                (
                    ConfigurationManager.ConnectionStrings[""$sessionData.connectionStringName$""],
                    mapperTypes
                ).GetSessionFactory();
            }
            return _sessionFactory.OpenSession();
        }

        readonly Type[] _mapperTypes = new Type[] {
            |% for(table in sessionData.tables between ) { def mapperName = getMapperName(table);
                ~<|d
            %|
            

        };
    }
}

namespace $sessionData.namespace$.Entities
{
    |% for(table in sessionData.tables) { %|
    public partial class $sanitizeId(table.name)$
    {
        |% 
        gColumnCmd.SetParameter(""@table_name"", table.name);
        for(column in gColumnCmd.ExecuteQuery() where column.is_identity between endl + repeat("" "", 8)) 
            writePropertyDef(column);
        %|
        |%
        for(column in gColumnCmd.ExecuteQuery() where !column.is_identity between endl + repeat("" "", 8)) 
            writePropertyDef(column);
        %|
    }
    |% } %|
}

namespace $sessionData.namespace$.Mappers
{
    |% 
    for(table in sessionData.tables) { def mapperName = getMapperName(table); 
    %|
    public partial class mapperName : ClassMapper<mapperName>
    {
        partial void setupAdditionalMaps();
        public $mapperName$()
        {
            |% 
            gColumnCmd.SetParameter(""@table_name"", table.name);
            for(column in gColumnCmd.ExecuteQuery() where column.is_identity between endl + repeat("" "", 12)) 
                ~<|Id(x => x.Id, map => { map.Generator(Generators.Identity); map.Column(""$column.column_name$""); });|>;
            %|
            |%
            for(column in gColumnCmd.ExecuteQuery() where !column.is_identity between endl + repeat("" "", 12))
                ~<|Property(x => x.$getColumnPropertyName(column)$, map => map.Column(""$column.column_name$""));|>;
            %|

            setupAdditionalMaps();
        }
    }
    |% } %|
}
|>;
}";
			base.CompileModule(script);
		}

		[Test]
		public void Bug1()
		{
			 
			const string hs = @"
use System; 
function test(id, sb)
{
	def id0 = id[0];
	def notStartsWith = !id.StartsWith(""_"");
	def boolValue = id0 && notStartsWith; /* Results in InvalidCastException - would *love* a better error message than that.*/  
	if(boolValue)
    /*if(Char.IsLetter(id[0] && !id.notStartsWith(""_"")))*/
		sb.Append(""@"");
}
";

			var gs = this.CompileModule(hs);
			gs.Globals.test("_someString", new StringBuilder());

		}

		[Test]
		public void Bug1_1()
		{

			const string hs = @"
use System; 
function test(id, sb)
    if(Char.IsLetter(id[0]))
		sb.Append(""@"");
";

			var gs = this.CompileModule(hs);
			gs.Globals.test("_someString", new StringBuilder());

		}
		[Test]
		public void Bug1_2()
		{

			const string hs = @"
use System; 
function test(id, sb)
    if(!id.StartsWith(""_""))
		sb.Append(""@"");
";

			var gs = this.CompileModule(hs);
			gs.Globals.test("_someString", new StringBuilder());

		}

		[Test]
		public void Bug2()
		{
			const string hs = @"
use System; 
function test(id, sb)
	if (!(Char.IsLetter(id[0]) && id[0] != ""_""[0]))
		sb.Append(""@"");
";

			var gs = this.CompileModule(hs);
			gs.Globals.test("_someString", new StringBuilder());
		}


		AssemblyName _assemblyName;
		AssemblyBuilder _assemblyBuilder;
		ModuleBuilder _moduleBuilder;

		[Test, Ignore]
		public void DebugInfo1()
		{
			var block = Expression.Block(
				Expression.DebugInfo(Expression.SymbolDocument("foo.txt"), 10, 11, 12, 13),
				Expression.Throw(Expression.New(typeof(Exception))));

			var l = Expression.Lambda(block);

			Action a = (Action)l.Compile(DebugInfoGenerator.CreatePdbGenerator());
			a();
		}

		interface IGlobalScopeInitializer
		{
			void DefineGlobalScope(dynamic theGlobalScope);
		}

		[Test, Ignore]
		public void DebugInfo2()
		{
			var l = getExpressionToCompile();

			AssemblyName assemblyName = new AssemblyName { Name = "MyDynamicAssembly" };

			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MyDynamicModule", true);
			TypeBuilder typeBuilder = moduleBuilder.DefineType("GlobalScopeInitializer", TypeAttributes.Public, null, new[] { typeof(IGlobalScopeInitializer) });
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("TestMethod", MethodAttributes.Public | MethodAttributes.Static);
			l.CompileToMethod(methodBuilder, DebugInfoGenerator.CreatePdbGenerator());

			typeBuilder.CreateType();

			Type t = assemblyBuilder.GetType("MyDynamicType");
			var mi = t.GetMethod("TestMethod");
			mi.Invoke(null, null);
		}


		static void foo()
		{
			dynamic foo2 = new ExpandoObject();
			foo2.Bar = 1;
		}

		[Test]
		public void test2()
		{
			dynamic foo2 = new ExpandoObject();
			var callSiteBinder = Binder.SetMember(CSharpBinderFlags.None, "Bar", typeof(Sandbox), 
				new[]
				{
					//CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), 
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				});

			CallSite<Func<CallSite, object, int, object>> callSite = CallSite<Func<CallSite, object, int, object>>.Create(callSiteBinder);
			callSite.Target((CallSite)callSite, foo2, 1);
			Assert.AreEqual(1, foo2.Bar);
		}

		[Test]
		public void test3()
		{

			dynamic foo2 = new ExpandoObject();
			var callSiteBinder = new HappySetMemberBinder("Bar");
			CallSite<Func<CallSite, object, int, object>> callSite = CallSite<Func<CallSite, object, int, object>>.Create(callSiteBinder);
			callSite.Target((CallSite)callSite, foo2, 1);
			Assert.AreEqual(1, foo2.Bar);
		}

		[Test]
		public void test4()
		{
			dynamic foo2 = new ExpandoObject();
			var callSiteBinder = new HappySetMemberBinder("Bar");

			CallSite<Func<CallSite, object, int, object>> callSite = CallSite<Func<CallSite, object, int, object>>.Create(callSiteBinder);
			callSite.Target(callSite, foo2, 1);
			Assert.AreEqual(1, foo2.Bar);
		}


		[Test]
		public void dynamicMethodInvoke_ManyArguments()
		{
			dynamic d = new ExpandoObject();
			dynamic foo = d.Method(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 5, 6, 7, 8, 9);
		}

		[Test]
		public void dynamicMethodInvoke_AFewArguments()
		{
			dynamic d = new ExpandoObject();
			dynamic foo = d.Method(1, 2);
		}

		[Test]
		public void dynamicMemberSet()
		{
			dynamic d = new ExpandoObject();
			d.Dude = "Walawalabingbang!";
		}


		public void food()
		{
			//Modified from http://blogs.msdn.com/b/joelpob/archive/2004/02/15/73239.aspx
			AssemblyName assembly;
			AssemblyBuilder assemblyBuilder;
			ModuleBuilder modbuilder;
			TypeBuilder typeBuilder;
			MethodBuilder methodBuilder;

			assembly = new AssemblyName();
			assembly.Version = new Version(1, 0, 0, 0);
			assembly.Name = "ReflectionEmitDelegateTest";
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.RunAndSave);
			modbuilder = assemblyBuilder.DefineDynamicModule("MyModule", "ReflectionEmitDelegateTest.dll", true);

			// Grab a method from somewhere, via Reflection, so we can emit a delegate that can hook up to it
			MethodInfo targetMethod = Type.GetType("DelegateReflectionEmit.MyClass").GetMethod("MyConsoleWriteLineMethod");

			// Create a delegate that has the same signature as the method we would like to hook up to
			typeBuilder = modbuilder.DefineType("MyDelegateType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(System.MulticastDelegate));
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(System.IntPtr) });
			constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			// Grab the parameters of the method
			ParameterInfo[] parameters = targetMethod.GetParameters();
			Type[] paramTypes = new Type[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				paramTypes[i] = parameters[i].ParameterType;
			}

			// Define the Invoke method for the delegate
			methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, targetMethod.ReturnType, paramTypes);
			methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			// bake it!
			Type t = typeBuilder.CreateType();
			assemblyBuilder.Save("ReflectionEmitDelegateTest.dll"); 
		}

		[Test]
		public void test6()
		{
			_assemblyName = new AssemblyName { Name = "MyDynamicAssembly" };
			_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
			_moduleBuilder = _assemblyBuilder.DefineDynamicModule("MyDynamicModule", true);

			// Define the Invoke method for the delegate
			var returnType = typeof(object);
			Type[] paramTypes = new[] { typeof(CallSite), typeof(object), typeof(int) };

			// Create a delegate that has the same signature as the method we would like to hook up to
			var delegateBuilder = _moduleBuilder.DefineType(
				"MyDelegateType", 
				TypeAttributes.Class | 
				TypeAttributes.Public | 
				TypeAttributes.Sealed | 
				TypeAttributes.AnsiClass | 
				TypeAttributes.AutoClass, 
				typeof(MulticastDelegate));

			ConstructorBuilder constructorBuilder = delegateBuilder.DefineConstructor(
				MethodAttributes.RTSpecialName | 
				MethodAttributes.HideBySig | 
				MethodAttributes.Public, 
				CallingConventions.Standard,
				new [] { typeof(object), typeof(IntPtr) });

			constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
			var invokeMethodBuilder = delegateBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, returnType, paramTypes);
			invokeMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			var delegateType = delegateBuilder.CreateType();

			TypeBuilder typeBuilder = _moduleBuilder.DefineType("GlobalScopeInitializer", TypeAttributes.Public);
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("TestMethod", MethodAttributes.Public | MethodAttributes.Static);

			var l = getDynamicExpressionToCompile(delegateType);
			l.CompileToMethod(methodBuilder, DebugInfoGenerator.CreatePdbGenerator());

			Type t = typeBuilder.CreateType(); ;// assemblyBuilder.GetType("MyDynamicType");
			var mi = t.GetMethod("TestMethod");
			dynamic foo2 = new ExpandoObject();
			mi.Invoke(null, new object[] { foo2 });
			Assert.AreEqual(1, foo2.Bar);
		}


		static LambdaExpression getDynamicExpressionToCompile(Type delegateType)
		{
			Type callSiteType = typeof(CallSite<>).MakeGenericType(new[] { delegateType });
			
			List<Expression> exprList = new List<Expression>();
			ParameterExpression parameterExpr = Expression.Parameter(typeof(object), "anObject");
			ParameterExpression callSiteExpr = Expression.Parameter(callSiteType, "callSite1");

			var assignExpression = createBinder(callSiteType, callSiteExpr, typeof(HappySetMemberBinder), new [] { Expression.Constant("Bar", typeof(string)) });

			exprList.Add(assignExpression);

			var targetFieldInfo = callSiteType.GetField("Target");
			var invokeExpression = Expression.Invoke(Expression.Field(callSiteExpr, targetFieldInfo),
					new Expression[] { callSiteExpr, parameterExpr, Expression.Constant(1, typeof(int)) });

			exprList.Add(invokeExpression);

			var block = Expression.Block(new[] { callSiteExpr }, exprList);

			var l = Expression.Lambda(block, new[] { parameterExpr });
			return l;
		}

		static BinaryExpression createBinder(Type callSiteType, ParameterExpression callSiteExpr, Type binderType, IEnumerable<Expression> arguments)
		{
			MethodInfo createCallSiteInfo = callSiteType.GetMethod("Create", new[] { typeof(CallSiteBinder) });
			var setMemberBinderConstructor = binderType.GetConstructor(new[] { typeof(string) });
			var createCallSiteExpr = Expression.Call(createCallSiteInfo, Expression.New(setMemberBinderConstructor, arguments));
			var assignExpression = Expression.Assign(callSiteExpr, createCallSiteExpr);
			return assignExpression;
		}

		static LambdaExpression getExpressionToCompile()
		{
			var block = Expression.Block(
				Expression.DebugInfo(Expression.SymbolDocument("foo.txt"), 10, 11, 12, 13),
				Expression.Throw(Expression.New(typeof(Exception))));

			var l = Expression.Lambda(block);
			return l;
		}
	}
}

