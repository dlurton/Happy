/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Runtime;
using SwitchCase = HappyTemplate.Compiler.Ast.SwitchCase;

namespace HappyTemplate.Compiler.AstVisitors
{
	partial class AstAnalyzer
	{
		public override void AfterVisit(StatementBlock node)
		{
			List<Expression> expressions = _expressionStack.Pop(node.Statements.Length);
			if (expressions.Count == 0)
				expressions.Add(Expression.Empty());

			if (node.GetAnalyzeSymbolsExternally())
				_expressionStack.Push(node, MaybeBlock(expressions));
			else
				_expressionStack.Push(node, MaybeBlock(node.SymbolTable.GetParameterExpressions(), expressions));
			base.AfterVisit(node);
		}

		public override void AfterVisit(DefStatement node)
		{
			var initializers = _expressionStack.Pop(node.VariableDefs.Count(vd => vd.InitializerExpression != null)).ToList();
			_expressionStack.Push(node, initializers.Count == 0 ? Expression.Empty() : MaybeBlock(initializers));
			base.AfterVisit(node);
		}

		public override void AfterVisit(VariableDef node)
		{
			if (node.InitializerExpression != null)
				_expressionStack.Push(node, node.Symbol.GetSetExpression(_expressionStack.Pop()));
			base.AfterVisit(node);
		}

		public override void AfterVisit(OutputStatement node)
		{
			var writeExps = _expressionStack.Pop(node.ExpressionsToWrite.Length).Select(SafeWriteToTopWriter).ToArray();
			_expressionStack.Push(node, MaybeBlock(writeExps));
			base.AfterVisit(node);
		}

		public override void AfterVisit(ReturnStatement node)
		{
			Expression @return = node.ReturnExp == null ? Expression.Constant(null) : _expressionStack.Pop();
			_expressionStack.Push(node, Expression.Goto(_returnLabelTarget, Expression.Convert(@return, typeof(object))));
			base.AfterVisit(node);
		}

		public override void AfterVisit(IfStatement node)
		{
			Expression falseBlock = null;

			if (node.FalseStatementBlock != null)
				falseBlock = _expressionStack.Pop();

			Expression trueBlock = _expressionStack.Pop();
			Expression condition = Expression.Convert(_expressionStack.Pop(), typeof(Boolean));

			ConditionalExpression ifThenExpression;
			if (falseBlock != null)
				ifThenExpression = Expression.IfThenElse(condition, trueBlock, falseBlock);
			else
				ifThenExpression = Expression.IfThen(condition, trueBlock);

			_expressionStack.Push(node, ifThenExpression);
			base.AfterVisit(node);
		}

		public override void AfterVisit(SwitchStatement node)
		{
			//In the case of a switch statement with no cases, the default block always executes and the expression should be evaluated too, in case there are side effects.
			//Note:  Can't pass an empty cases collection to Expression.Switch() - an ArgumentException will result.
			if (node.Cases.Length == 0)
			{
				var notSwitchExprs = new List<Expression>();
				if (node.DefaultStatementBlock != null)
					notSwitchExprs.Add(_expressionStack.Pop());

				notSwitchExprs.Add(_expressionStack.Pop());
				_expressionStack.Push(node, MaybeBlock(notSwitchExprs));
				return;
			}

			Expression defaultBlock = null;
			if (node.DefaultStatementBlock != null)
				defaultBlock = _expressionStack.Pop();
			var cases = _expressionStack.Pop(node.Cases.Length, false).Cast<ContainerPseudoExpression<System.Linq.Expressions.SwitchCase>>().Select(container => container.ContainedItem).ToList();
			var switchExpr = _expressionStack.Pop();
			MethodInfo comparerMethodInfo = typeof(RuntimeHelpers).GetMethod("HappyEq");

			_expressionStack.Push(node, Expression.Switch(switchExpr, defaultBlock, comparerMethodInfo, cases));

			base.AfterVisit(node);
		}

		public override void AfterVisit(SwitchCase node)
		{
			var caseBlock = EnsureVoidResult(_expressionStack.Pop()); //Should be a block expression
			var caseValues = _expressionStack.Pop(node.CaseValues.Length);
			_expressionStack.Push(node, new ContainerPseudoExpression<System.Linq.Expressions.SwitchCase>(Expression.SwitchCase(caseBlock, caseValues)));

			base.AfterVisit(node);
		}

		Expression EnsureVoidResult(Expression expr)
		{
			if (expr.Type != typeof(void))
				return Expression.Block(typeof(void), expr);

			return expr;
		}
	}
}

