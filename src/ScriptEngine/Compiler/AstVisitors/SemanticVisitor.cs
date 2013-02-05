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
	class SemanticVisitor : AstVisitorBase
	{
		Stack<LoopStatementBase> _loopStack;
		readonly ErrorCollector _errorCollector;

		public SemanticVisitor(ErrorCollector errorCollector)
			: base(VisitorMode.VisitNodeAndChildren) 
		{
			_errorCollector = errorCollector;
		}

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.BeforeVisitCatchAll(node);
			var loopStatement = node as LoopStatementBase;
			if(loopStatement != null)
				_loopStack.Push(loopStatement);
		}

		public override void AfterVisitCatchAll(AstNodeBase node)
		{
			var loopStatement = node as LoopStatementBase;
			if (loopStatement != null)
			{
				DebugAssert.AreSameObject(loopStatement, _loopStack.Peek(), "top of loop stack is not current loop statement");
				_loopStack.Pop();
			}
			base.AfterVisitCatchAll(node);
		}

		public override void BeforeVisit(Function node)
		{
			base.BeforeVisit(node);
			_loopStack = new Stack<LoopStatementBase>();
		}

		public override void AfterVisit(Function node)
		{
			base.AfterVisit(node);
			DebugAssert.AreEqual(0, _loopStack.Count, "_loopStack not empty");
			_loopStack = null;
		}

		public override void BeforeVisit(BinaryExpression expression)
		{
			base.BeforeVisit(expression);
			switch (expression.Operator.Operation)
			{
				case OperationKind.Not:
					_errorCollector.OperatorIsNotBinary(expression.Location, expression.Operator.ToString());
					break;
			}
		}

		public override void BeforeVisit(UnaryExpression expression)
		{
			base.BeforeVisit(expression);
			switch(expression.Operator.Operation)
			{
			case OperationKind.Not:
				break;
			default:
				_errorCollector.OperatorIsNotUnary(expression.Location, expression.Operator.ToString());
				break;
			}
		}

		public override void Visit(BreakStatement breakStatement)
		{
			base.Visit(breakStatement);
			if(_loopStack.Count == 0)
				_errorCollector.BreakNotAllowedHere(breakStatement.Location);
		}

		public override void Visit(ContinueStatement continueStatement)
		{
			base.Visit(continueStatement);
			if(_loopStack.Count == 0)
				_errorCollector.ContinueNotAllowedHere(continueStatement.Location);
		}
	}
}

