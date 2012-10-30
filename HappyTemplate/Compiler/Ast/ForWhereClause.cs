using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class ForWhereClause : ScopeAstNodeBase
	{
		public string LoopVariableName { get; private set; }
		readonly ExpressionNodeBase _expressionNode;
		public ForWhereClause(HappySourceLocation location, string loopVariableName, ExpressionNodeBase expressionNode) : base(location)
		{
			LoopVariableName = loopVariableName;
			_expressionNode = expressionNode;
		}

		public override AstNodeKind NodeKind { get { return AstNodeKind.ForWhereClause; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if(_expressionNode != null)
				_expressionNode.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			if (_expressionNode != null)
			{
				writer.Write("where ");
				writer.Write(_expressionNode);
			}
		}
	}
}
