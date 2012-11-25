/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace HappyTemplate.Compiler.Ast
{
    class AstWriter
    {
    	readonly TextWriter _writer;
        public int IndentCount { get; set;  }
        public AstWriter(TextWriter writer)
        {
            _writer = writer;
        }

		
		/// <summary>
		/// Seems a little silly, sure, but I like the pattern of calling this 
		/// better than the alternative...
		/// </summary>
		/// <param name="node"></param>
		public void Write(AstNodeBase node)
		{
			node.WriteString(this);
		}

		public void Write(IEnumerable<AstNodeBase> nodes, string between)
		{
			IEnumerator<AstNodeBase> enumerator = nodes.GetEnumerator();

			if(enumerator.MoveNext())
			{
				enumerator.Current.WriteString(this);
				while (enumerator.MoveNext())
				{
					_writer.Write(between);
					enumerator.Current.WriteString(this);
				}
			}
		}

		public void Write(IEnumerable<AstNodeBase> nodes)
		{
			Write(nodes, "");
		}


    	public void Write(char c)
		{
			_writer.Write(c);
			if(c == '\n')
				for (int i = 0; i < this.IndentCount; ++i)
					_writer.Write('\t');
		}

		public void Write(string str)
		{
			foreach (char c in str)
				this.Write(c);			
		}

        public void Write(string fmt, params object[] args)
        {
        	string output = String.Format(fmt, args);
			this.Write(output);
        }

		public void WriteLine(string str)
		{
			this.Write('\n');			
			this.Write(str);

		}

		public void WriteLine(string fmt, params object[] args)
		{
			this.Write('\n');			
			this.Write(fmt, args);

		}

    	public IndentTracker Indent()
		{
			return new IndentTracker(this);
		}

		public BlockTracker CurlyBraces()
		{
			return new BlockTracker(this, "{", "}");
		}

		public BlockTracker Parens()
		{
			return new BlockTracker(this, "(", ")");
		}

		public BlockTracker Brackets()
		{
			return new BlockTracker(this, "[", "]");
		}

		public struct IndentTracker : IDisposable
		{
			readonly AstWriter _writer;
			public IndentTracker(AstWriter writer)
			{
				_writer = writer;
				_writer.IndentCount++;
			}
			
			public void Dispose()
			{
				_writer.IndentCount--;
			}
		}

		public struct BlockTracker : IDisposable
		{
			readonly AstWriter _writer;
			readonly string _end;
			public BlockTracker(AstWriter writer, string start, string end)
			{
				_writer = writer;
				_end = end;
				_writer.WriteLine(start);
				_writer.IndentCount++;

			}


			public void Dispose()
			{
				_writer.IndentCount--;
				_writer.WriteLine(_end);
			}
		}

    	public IDisposable Block(string start, string end)
    	{
    		return new BlockTracker(this, start, end);
    	}

    	
    }
}

