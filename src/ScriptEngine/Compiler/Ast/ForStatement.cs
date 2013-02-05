/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	partial class ForStatement : LoopStatementBase
	{
		readonly Identifier _loopVariable;
		readonly ExpressionNodeBase _enumerable;
		readonly StatementBlock _loopBody;
		readonly ExpressionNodeBase _between;
		readonly ForWhereClause _where;

		internal override AstNodeKind NodeKind { get { return AstNodeKind.ForStatement; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
			{
				_enumerable.Accept(visitor);
				_loopBody.Accept(visitor);
				if (_between != null)
					_between.Accept(visitor);

				if (_where != null)
					_where.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write("for({0} ", _loopVariable.Text);
			using (writer.Indent())
			{
				writer.Write("in ");
				writer.Write(this.Enumerable);
				if (this.Between != null)
				{
					writer.Write("between ");
					writer.Write(this.Between);
				}
				if(this.Where != null)
					writer.Write(this.Where);
				writer.Write(this.LoopBody);
			}
		}

		public Identifier LoopVariable { get { return _loopVariable; }}
		public ExpressionNodeBase Enumerable { get { return _enumerable; } }
		public StatementBlock LoopBody { get { return _loopBody;  } }
		public ExpressionNodeBase Between { get { return _between; } }
		public ForWhereClause Where { get { return _where; } }

		public ForStatement(HappySourceLocation span, Identifier loopVariable, ExpressionNodeBase enumerable, ExpressionNodeBase between, ExpressionNodeBase where, StatementBlock loopBody)
			: base(span, loopBody.Location)
		{
			_loopVariable = loopVariable;
			_enumerable = enumerable;
			_between = between;
			_where = where != null ? new ForWhereClause(where.Location, _loopVariable.Text, where) : null;
			_loopBody = loopBody;
		}
	}
}

