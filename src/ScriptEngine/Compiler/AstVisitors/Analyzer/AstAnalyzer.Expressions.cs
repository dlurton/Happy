/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Linq.Expressions;
using Happy.ScriptEngine.Compiler.Ast;
using Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions;
using Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables;
using Happy.ScriptEngine.Exceptions;
using UnaryExpression = Happy.ScriptEngine.Compiler.Ast.UnaryExpression;

namespace Happy.ScriptEngine.Compiler.AstVisitors.Analyzer
{
	partial class AstAnalyzer
	{
		public override void Visit(LiteralExpression node)
		{
			base.Visit(node);
			_expressionStack.Push(node, Expression.Constant(node.Value, node.Value.GetType()));
		}

		public override void AfterVisit(UnaryExpression node)
		{
			switch (node.Operator.Operation)
			{
				case OperationKind.Not:
					_expressionStack.Push(node, Expression.MakeUnary(ToExpressionType(node.Operator), Expression.Convert(_expressionStack.Pop(), typeof(bool)), typeof(object)));
					break;
				default:
					throw new UnhandledCaseException("Semantic checking should have prevented this unhandled case");
			}
			base.AfterVisit(node);
		}

		public override void Visit(IdentifierExpression node)
		{
			base.Visit(node);
			//Member references must be handled in AfterVisit(BinaryExpression) 
			//because it requires knowledge of the instance which we don't have yet
			//and if it's a write operation, the value being assigned.
			if (node.GetExtension<NamedExpressionNodeExtension>().IsMemberReference)
				return;

			//Also handle reading only because Assigning a value to IdentifierExpressions must be 
			//handled in AfterVisit(BinaryExpression) since it requires knowledge of the expression 
			//who's value is to be assigned.
			if (node.GetExtension<BinaryExpressionExtension>().AccessType == ExpressionAccessType.Read)
				_expressionStack.Push(node, node.GetExtension<SymbolExtension>().Symbol.GetGetExpression());
		}

		public override void AfterVisit(AnonymousTemplate node)
		{
			var body = new[]
			{
				PushWriter(),
				_expressionStack.Pop(),
				PopWriter()
			};

			_expressionStack.Push(node, MaybeBlock(node.Body.GetExtension<ScopeExtension>().SymbolTable.GetParameterExpressions(), body));
			base.AfterVisit(node);
		}

		public override void Visit(VerbatimSection node)
		{
			base.Visit(node);
			_expressionStack.Push(node, WriteVerbatimToTopWriter(node.Text));
		}

		/// <summary>
		/// This only hanldes function calls that are at the global scope, i.e. not in dotted expressions.
		/// </summary>
		/// <param name="node"></param>
		public override void AfterVisit(FunctionCallExpression node)
		{
			//Member references are handled during analysis of the parent BinaryExpression node
			//because knowledge of both the instance and member are required.
			if (!node.GetExtension<NamedExpressionNodeExtension>().IsMemberReference)
			{
				Expression getMethod = this.PropertyOrFieldGet(node.Identifier.Text, _globalScopeExp);
				var args = _expressionStack.Pop(node.Arguments.Length).ToArray();
				_expressionStack.Push(node, this.dynamicInvoke(getMethod, args));
			}
			base.AfterVisit(node);
		}

		public override void AfterVisit(NewObjectExpression node)
		{
			var args = _expressionStack.Pop(node.ConstructorAgs.Length + 1).ToArray();
			_expressionStack.Push(node, this.dynamicCreate(args));

			base.AfterVisit(node);
		}

		public override void Visit(NullExpression node)
		{
			_expressionStack.Push(node, Expression.Constant(null, typeof(object)));
		}
	}
}

