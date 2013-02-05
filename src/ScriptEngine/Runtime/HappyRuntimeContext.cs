/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace Happy.ScriptEngine.Runtime
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

        public void PushWriter(TextWriter writer)
        {
            _writerStack.Push(writer);
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

