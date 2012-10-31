using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.AstVisitors;
using Microsoft.Scripting.Hosting;

namespace HappyTemplate.Runtime
{
	public class HappyRuntimeContext
	{
		readonly dynamic _globals;

		public dynamic Globals { get { return _globals;  } }
		
		readonly Stack<TextWriter> _writerStack = new Stack<TextWriter>();
		public TextWriter OutputWriter { get { return _writerStack.Peek(); } }

		public HappyRuntimeContext(TextWriter outputWriter)
		{
			_writerStack.Push(outputWriter);
			_globals = new ExpandoObject();
		}

		public HappyRuntimeContext() : this(new StringWriter()) { }

		public StringWriter PushWriter()
		{
			var stringWriter = new StringWriter();
			_writerStack.Push(stringWriter);
			return stringWriter;
		}

		public string PopWriter()
		{
			TextWriter retval = _writerStack.Pop();
			if (retval is StringWriter)
				return retval.ToString();

			return "";
		}

		public void WriteToTopWriter(object obj)
		{
			_writerStack.Peek().Write(obj.ToString());
		}


		public void SafeWriteToTopWriter(object obj)
		{
			if(obj == null)
				return;

			_writerStack.Peek().Write(obj.ToString());
		}

	}
}
