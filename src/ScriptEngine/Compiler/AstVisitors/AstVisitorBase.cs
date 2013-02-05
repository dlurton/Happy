/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using Happy.ScriptEngine.Compiler.Ast;

namespace Happy.ScriptEngine.Compiler.AstVisitors
{
	class AstVisitorBase
	{
		readonly Stack<VisitorMode> _modeStack = new Stack<VisitorMode>();
		public VisitorMode Mode { get { return _modeStack.Peek();  } }
		public bool ShouldVisitChildren { get { return this.Mode == VisitorMode.VisitNodeAndChildren;  } }

		protected AstVisitorBase(VisitorMode intitialMode) 
		{
			this.PushMode(intitialMode);
		}
		
		protected void PushMode(VisitorMode mode)
		{
			_modeStack.Push(mode);
		}

		protected void PopMode()
		{
			_modeStack.Pop();
		}

		public virtual void BeforeVisitCatchAll(AstNodeBase node) { }
		public virtual void AfterVisitCatchAll(AstNodeBase node) { }

		public virtual void BeforeVisit(Module node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(Module node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void Visit(LoadDirective node)
		{
			this.BeforeVisitCatchAll(node);
		}
		//public virtual void Visit(AstNodeBase node)
		//{
		//    this.BeforeVisitCatchAll(node);
		//}
		//public virtual void Visit(Module node, SourceUnit sourceUnit)
		//{
		//    this.BeforeVisitCatchAll(node);
		//}
		public virtual void Visit(UseStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void BeforeVisit(Function node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(Function node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void Visit(FunctionParameter node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void BeforeVisit(AnonymousTemplate node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(AnonymousTemplate node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(OutputStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(OutputStatement node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void Visit(VerbatimSection node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void Visit(NullExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void Visit(LiteralExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void BeforeVisit(IfStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterIfStatementCondition(IfStatement ifStatement)
		{
			//Intentionally left blank
		}

		public virtual void AfterIfStatementTrueBlock(IfStatement ifStatement)
		{
			//Intentionally left blank
		}

		public virtual void AfterIfStatementFalseBlock(IfStatement ifStatement)
		{
			//Intentionally left blank
		}
		public virtual void AfterVisit(IfStatement node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void Visit(IdentifierExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void BeforeVisit(BinaryExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterLeftExpressionVisit(ExpressionNodeBase node)
		{
			//Intentionally left blank
		}

		public virtual void AfterRightExpressionVisit(ExpressionNodeBase node)
		{
			//Intentionally left blank
		}

		public virtual void AfterVisit(BinaryExpression node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(ForStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(ForStatement node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(ForWhereClause node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(ForWhereClause node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(ArgumentList node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(ArgumentList node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(FunctionCallExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(FunctionCallExpression node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(DefStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(DefStatement node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(NewObjectExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(NewObjectExpression node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(VariableDef node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(VariableDef node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void BeforeVisit(UnaryExpression node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void AfterVisit(UnaryExpression node)
		{
			this.AfterVisitCatchAll(node);
		}
		public virtual void Visit(BreakStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}
		public virtual void Visit(ContinueStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void BeforeVisit(StatementBlock node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(StatementBlock node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void BeforeVisit(FunctionParameterList node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(FunctionParameterList node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void BeforeVisit(ReturnStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(ReturnStatement node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void BeforeVisit(UseStatementList node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(UseStatementList node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void Visit(UnexpectedToken node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void BeforeVisit(SwitchCase node)
		{
			this.BeforeVisitCatchAll(node);

		}

		public virtual void AfterVisit(SwitchCase node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void BeforeVisit(SwitchStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}

		public virtual void AfterVisit(SwitchStatement node)
		{
			this.AfterVisitCatchAll(node);
		}

		public virtual void BeforeVisit(WhileStatement node)
		{
			this.BeforeVisitCatchAll(node);
		}


		public virtual void AfterVisit(WhileStatement node)
		{
			this.AfterVisitCatchAll(node);
		}
	}
}

