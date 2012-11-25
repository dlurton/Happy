/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using BinaryExpression = HappyTemplate.Compiler.Ast.BinaryExpression;

namespace HappyTemplate.Compiler.AstVisitors
{
	partial class AstAnalyzer
	{
		public override void AfterVisit(BinaryExpression node)
		{
			var expType = ToExpressionType(node.Operator);
			switch (node.AccessType)
			{
				case ExpressionAccessType.Read:
					analyzeReadAccessExpression(node, expType);
					break;
				//This case is only included as a sanity test
				//analysis for writable expressions are handled by their parent BinaryExpression (the assignment expression itself)
				case ExpressionAccessType.Write:
					switch (node.Operator.Operation)
					{
						//The only writeable expression types are:
						case OperationKind.Index:
						case OperationKind.MemberAccess:
							break;
						default:
							throw new UnhandledCaseException("Expression returnType " + expType + " is not writable.  (user error?)");
					}
					break;
				default:
					throw new UnhandledCaseException();
			}

			base.AfterVisit(node);
		}

		void analyzeReadAccessExpression(BinaryExpression node, ExpressionType expType)
		{
			//Read operations which do not pop rvalue or lvalues from the stack.
			switch (expType)
			{
				case ExpressionType.Index:
					analyzeReadIndexExpression(node);
					return;
				case ExpressionType.MemberAccess:
					analyzeReadMemberAccess(node);
					return;
			}

			Expression rvalue = _expressionStack.Pop();
			//These operations easily pop their rvalue from _expressionStack but lvalue requires special handling
			if (expType == ExpressionType.Assign)
			{
				analyzeAssignmentExpression(node, rvalue);
				return;
			}

			//All other operations can expect to find both their rvalue and lvalues on the stack.
			Expression lvalue = _expressionStack.Pop();
			switch (expType)
			{
				case ExpressionType.OrElse:
					_expressionStack.Push(node, Expression.OrElse(RuntimeHelpers.EnsureBoolResult(lvalue), RuntimeHelpers.EnsureBoolResult(rvalue)));
					return;
				case ExpressionType.AndAlso:
					_expressionStack.Push(node, Expression.AndAlso(RuntimeHelpers.EnsureBoolResult(lvalue), RuntimeHelpers.EnsureBoolResult(rvalue)));
					return;
				default:
					//_expressionStack.Push(node, this.DynamicExpression(_languageContext.CreateBinaryOperationBinder(expType), typeof(object), lvalue, rvalue));
					_expressionStack.Push(node, this.dynamicBinaryExpression(expType, lvalue, rvalue));
					return;
			}
		}

		void analyzeAssignmentExpression(BinaryExpression node, Expression rvalue)
		{
			if (rvalue.Type.IsValueType)
				rvalue = Expression.Convert(rvalue, typeof(object));

			switch (node.LeftValue.NodeKind)
			{
				case AstNodeKind.IdentifierExpression:
					var identifierExp = (IdentifierExpression)node.LeftValue;
					_expressionStack.Push(node, identifierExp.GetSymbol().GetSetExpression(rvalue));
					return;
				case AstNodeKind.BinaryExpression:
					if (node.LeftValue.NodeKind != AstNodeKind.BinaryExpression)
						return;
					analyzeAssignmentToBinaryExpression(rvalue, node);
					return;
				default:
					_errorCollector.InvalidLValueForAssignment(node.LeftValue.Location);
					return;
			}
		}

		void analyzeAssignmentToBinaryExpression(Expression rvalue, BinaryExpression node)
		{
			BinaryExpression binaryLvalue = (BinaryExpression)node.LeftValue;
			//A BinaryExpression on as the LeftValue can be either an Index or MemberAccess operation
			switch (binaryLvalue.Operator.Operation)
			{
				case OperationKind.Index:
					//Have to handle array index and member assignments here because we don't know the rvalue until now.
					var args = popIndexArguments(binaryLvalue);
					args.Add(rvalue);
					var indexExpression = this.dynamicSetIndex(args.ToArray());
					_expressionStack.Push(node, indexExpression);
					return;
				case OperationKind.MemberAccess:
					//In this case, the rvalue is the value to be assigned (it does not correspond to node.RightValue) 
					var instanceExpr = _expressionStack.Pop();
					_expressionStack.Push(node, this.PropertyOrFieldSet(((IdentifierExpression)binaryLvalue.RightValue).Identifier.Text, instanceExpr, rvalue));
					return;
				default:
					_errorCollector.InvalidLValueForAssignment(binaryLvalue.Location);
					break;
			}
		}


		void analyzeReadMemberAccess(BinaryExpression node)
		{
			switch (node.RightValue.NodeKind)
			{
				case AstNodeKind.IdentifierExpression:
					Expression instanceExpr = _expressionStack.Pop();
					var propertyOrFieldGet = this.PropertyOrFieldGet(((IdentifierExpression)node.RightValue).Identifier.Text, instanceExpr);
					_expressionStack.Push(node, propertyOrFieldGet);
					break;
				case AstNodeKind.FunctionCallExpression:
					FunctionCallExpression funcCallExpression = (FunctionCallExpression)node.RightValue;
					List<Expression> args = _expressionStack.Pop(funcCallExpression.Arguments.Length);
					args.Insert(0, _expressionStack.Pop()); //<--object instance
					_expressionStack.Push(node, this.dynamicCall(funcCallExpression.Identifier.Text, args.ToArray()));
					break;
				default:
					throw new UnhandledCaseException();
			}
		}

		void analyzeReadIndexExpression(BinaryExpression node)
		{
			var args = popIndexArguments(node).ToArray();
			var indexExpression = this.dynamicGetIndex(args);
			_expressionStack.Push(node, indexExpression);
		}

		List<Expression> popIndexArguments(BinaryExpression node)
		{
			ArgumentList argList = (ArgumentList)node.RightValue;
			List<Expression> args = new List<Expression>();
			List<Expression> indexerArguments = _expressionStack.Pop(argList.Arguments.Length);
			Expression array = _expressionStack.Pop();
			args.Add(array);
			args.AddRange(indexerArguments);
			return args;
		}
	}
}

