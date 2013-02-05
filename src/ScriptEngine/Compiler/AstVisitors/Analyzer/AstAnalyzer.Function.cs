/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Happy.ScriptEngine.Compiler.Ast;
using Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables;

namespace Happy.ScriptEngine.Compiler.AstVisitors.Analyzer
{
	partial class AstAnalyzer
	{
		LabelTarget _returnLabelTarget;
		public override void Visit(FunctionParameter node)
		{
			var parameterSymbol = (HappyParameterSymbol)node.GetExtension<SymbolExtension>().Symbol;
			_expressionStack.Push(node, parameterSymbol.Parameter);
		}

		public override void BeforeVisit(Function node)
		{
			base.BeforeVisit(node);
			//_currentOutputExp = Expression.Parameter(typeof (TextWriter), CurrentOutputIdentifier);
			//Create return target
			_returnLabelTarget = Expression.Label(typeof(object), "lambdaReturn");
		}

		public override void AfterVisit(Function node)
		{
			Expression expression = _expressionStack.Pop(); //Should be a BlockExpression
			List<ParameterExpression> parameters = null;
			if (node.ParameterList != null)
				parameters = _expressionStack.Pop(node.ParameterList.Count, false).Cast<ParameterExpression>().ToList();

			List<Expression> outerExpressions = new List<Expression>();

			LabelExpression returnLabel = Expression.Label(_returnLabelTarget, Expression.Default(typeof(object)));

			outerExpressions.Add(expression);
			outerExpressions.Add(returnLabel);

			var completeFunctionBody = MaybeBlock(outerExpressions);
			LambdaExpression funcLambda = Expression.Lambda(completeFunctionBody, node.Name.Text, parameters);
			_expressionStack.Push(node, this.PropertyOrFieldSet(node.Name.Text, _globalScopeExp, funcLambda));
			base.AfterVisit(node);
		}
	}
}

