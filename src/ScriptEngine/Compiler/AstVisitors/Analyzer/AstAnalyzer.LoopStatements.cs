/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Happy.ScriptEngine.Compiler.Ast;
using Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions;
using Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables;
using Happy.ScriptEngine.Runtime;

namespace Happy.ScriptEngine.Compiler.AstVisitors.Analyzer
{
	partial class AstAnalyzer
	{
		class LoopContext
		{
			public LabelTarget BreakLabel;
			public LabelTarget ContinueLabel;
		}

		readonly Stack<LoopContext> _loopContextStack = new Stack<LoopContext>();

		const string EnumeratorParameterExpressionName = "__enumerator__";
		void pushLoopContext()
		{
			LabelTarget breakTarget = Expression.Label("break");
			LabelTarget continueTarget = Expression.Label("continue");

			LoopContext context = new LoopContext { BreakLabel = breakTarget, ContinueLabel = continueTarget };
			_loopContextStack.Push(context);
		}

		public override void BeforeVisit(WhileStatement node)
		{
			base.BeforeVisit(node);
			pushLoopContext();
		}

		public override void AfterVisit(WhileStatement node)
		{
			Expression loopBody = _expressionStack.Pop();
			Expression condition = _expressionStack.Pop();
			var loopContext = _loopContextStack.Pop();

			var loopController = Expression.IfThen(Expression.Not(RuntimeHelpers.EnsureBoolResult(condition)), Expression.Goto(loopContext.BreakLabel));

			var loopExprs = new List<Expression>
			{
				//Expression.Label(loopContext.ContinueLabel), 
				loopController,
				loopBody,
				//Expression.Label(loopContext.BreakLabel)
			};

			_expressionStack.Push(node, Expression.Loop(MaybeBlock(loopExprs), loopContext.BreakLabel, loopContext.ContinueLabel));

			base.AfterVisit(node);
		}

		public override void BeforeVisit(ForStatement node)
		{
			base.BeforeVisit(node);

			pushLoopContext();

			node.LoopBody.GetExtension<StatementBlockExtensions>().AnalyzeSymbolsExternally = true;
		}

		public override void AfterVisit(ForStatement node)
		{
			Expression whereCondition = null, between = null;
			var currentLoopContext = _loopContextStack.Pop();

			if (node.Where != null)
				whereCondition = _expressionStack.Pop();

			if (node.Between != null)
				between = _expressionStack.Pop();

			var loopBody = _expressionStack.Pop();
			Expression enumerable = _expressionStack.Pop();

			ParameterExpression loopVariable = node.GetExtension<ForExtension>().LoopVariableSymbol.GetGetExpression() as ParameterExpression;
			Expression getEnumerator = whereCondition == null
										   ? Expression.Call(Expression.Convert(enumerable, typeof(IEnumerable)), typeof(IEnumerable).GetMethod("GetEnumerator"))
										   : getWhereEnumerator(node, enumerable, whereCondition);

			ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator), EnumeratorParameterExpressionName);
			List<Expression> loop = new List<Expression> { Expression.Assign(enumerator, getEnumerator) };

			//The loop "wrapper", which wraps the actual loop body
			//It's purpose is to assign the loopVariable after each iteration and 
			//to write the between clause to the current output 
			var iteratorAdvancerAndLoopExiter =
				Expression.IfThenElse(
					Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
					between == null ? Expression.Empty() : SafeWriteToTopWriter(between),
					Expression.Goto(currentLoopContext.BreakLabel));
			BlockExpression loopWrapper = Expression.Block(
				// ReSharper disable AssignNullToNotNullAttribute
				Expression.Assign(loopVariable, Expression.Property(enumerator, typeof(IEnumerator).GetProperty("Current"))),
				// ReSharper restore AssignNullToNotNullAttribute
				loopBody,
				Expression.Label(currentLoopContext.ContinueLabel),
				iteratorAdvancerAndLoopExiter);

			//Primes the loop
			ConditionalExpression outerIf = Expression.IfThen(
				Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
				Expression.Loop(loopWrapper, currentLoopContext.BreakLabel));
			loop.Add(outerIf);

			var parameters = node.LoopBody.GetExtension<ScopeExtension>().SymbolTable.GetParameterExpressions().Union(new[] { enumerator }).ToArray();
			_expressionStack.Push(node, MaybeBlock(parameters, loop));
			base.AfterVisit(node);
		}

		public override void AfterVisit(ForWhereClause node)
		{
			var whereCondition = _expressionStack.Pop();
			_expressionStack.Push(node, RuntimeHelpers.EnsureBoolResult(whereCondition));
			base.AfterVisit(node);
		}

		static Expression getWhereEnumerator(ForStatement node, Expression enumerable, Expression whereExpression)
		{
			var methodInfo = typeof(RuntimeHelpers).GetMethod("GetWhereEnumerable");
			var arguments = new Expression[]
			{
				Expression.Convert(enumerable, typeof(IEnumerable)),
				Expression.Lambda(whereExpression, node.Where.GetExtension<ScopeExtension>().SymbolTable.GetParameterExpressions())
			};

			Expression retval = Expression.Call(methodInfo, arguments);
			return Expression.Call(retval, "GetEnumerator", new Type[] { });
		}

		public override void Visit(BreakStatement node)
		{
			base.Visit(node);
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			_expressionStack.Push(node, Expression.Break(_loopContextStack.Peek().BreakLabel));
		}

		public override void Visit(ContinueStatement node)
		{
			base.Visit(node);
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			_expressionStack.Push(node, Expression.Continue(_loopContextStack.Peek().ContinueLabel));
		}
	}
}

