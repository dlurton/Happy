using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Happy.ScriptEngine.Compiler.Ast;

namespace Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions
{
	class BinaryExpressionExtension
	{
		public ExpressionAccessType AccessType { get; set; }

		public BinaryExpressionExtension()
		{
			this.AccessType = ExpressionAccessType.Read;
		}
	}
}
