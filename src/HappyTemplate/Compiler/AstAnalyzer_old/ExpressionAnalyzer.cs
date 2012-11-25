/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using BinaryExpression = HappyTemplate.Compiler.Ast.BinaryExpression;
using UnaryExpression = HappyTemplate.Compiler.Ast.UnaryExpression;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class ExpressionAnalyzer : AstVisitorBase
	{
		readonly AnalysisContext _analysisContext;
		Expression _leftExpression;
		Expression _rightExpression;

		Expression _result;

		ExpressionAnalyzer(AnalysisContext analysisContext)
			: base(VisitorMode.VisitNodeOnly)
		{
			_analysisContext = analysisContext;
		}

		public static Expression Analyze(AnalysisContext analysisContext, ExpressionNodeBase node)
		{
			var analyzer = new ExpressionAnalyzer(analysisContext);
			node.Accept(analyzer);

			if (analyzer._result == null)
				throw new InternalException("Node type " + node.NodeKind + " not recognized by " + analyzer.GetType().FullName);

			return analyzer._result;
		}

		public override void Visit(LiteralExpression node)
		{
			base.Visit(node);
			_result = Expression.Constant(node.Value, typeof(object));
		}

		public override void BeforeVisit(UnaryExpression node)
		{
			base.BeforeVisit(node);
			switch (node.Operator.Operation)
			{
				case OperationKind.Not:
					Expression expression = Analyze(_analysisContext, node.Value);
					_result = Expression.MakeUnary(AnalyzerUtil.ToExpressionType(node.Operator), Expression.Convert(expression, typeof(bool)), typeof(object));
					break;
				default:
					throw new UnhandledCaseException("Semantic checking should have prevented this unhandled case");
			}
		}

		public override void AfterLeftExpressionVisit(ExpressionNodeBase node)
		{
			base.AfterLeftExpressionVisit(node);
			_leftExpression = _result;
			_result = null;
		}

		public override void AfterRightExpressionVisit(ExpressionNodeBase node)
		{
			base.AfterRightExpressionVisit(node);
			_rightExpression = _result;
			_result = null;
		}

		public override void AfterVisit(Ast.BinaryExpression node)
		{
			//Coming into this method, if _result is not null, _leftExpression and _rightExpression must be null
			if (_result != null)
			{
				if (_leftExpression != null || _rightExpression != null)
					throw new InternalException("Possible unreduced expression detected (1)");

				return;
			}

			//if _result is null, both _leftExpression and _rightExpression must not be null
			if (_leftExpression == null || _rightExpression == null)
				throw new InternalException("Possible unreduced expression detected (2)");


			base.BeforeVisit(node);
		}

		public override void BeforeVisit(Ast.BinaryExpression node)
		{
			if (node.Operator.Operation == OperationKind.MemberGet)
			{
				_result = analyzeMemberGetOperation(node);
				return;
			}

			ExpressionType expType = AnalyzerUtil.ToExpressionType(node.Operator);
			if (expType == ExpressionType.Index)
			{
				_result = analyzeIndexExpression(node);
				return;
			}

			Expression rvalue = RuntimeHelpers.EnsureObjectResult(Analyze(_analysisContext, node.RightValue));
			switch (expType)
			{
				case ExpressionType.Assign:
					_result = analyzeAssignmentExpression(node, rvalue);
					return;
				case ExpressionType.OrElse:
					_result = analyzeOrElseExpression(node, rvalue);
					return;
				case ExpressionType.AndAlso:
					_result = analyzeAndAlsoExpression(node, rvalue);
					return;
			}

			_result = analyzeBinaryExpression(node, expType, rvalue);
		}

		DynamicExpression analyzeBinaryExpression(BinaryExpression node, ExpressionType expType, Expression rvalue)
		{
			return Expression.Dynamic(_analysisContext.LanguageContext.CreateBinaryOperationBinder(expType), typeof(object), Analyze(_analysisContext, node.LeftValue), rvalue);
		}

		System.Linq.Expressions.BinaryExpression analyzeAndAlsoExpression(BinaryExpression node, Expression rvalue)
		{
			return Expression.AndAlso(RuntimeHelpers.EnsureBoolResult(Analyze(_analysisContext, node.LeftValue)), RuntimeHelpers.EnsureBoolResult(rvalue));
		}

		System.Linq.Expressions.BinaryExpression analyzeOrElseExpression(BinaryExpression node, Expression rvalue)
		{
			return Expression.OrElse(RuntimeHelpers.EnsureBoolResult(Analyze(_analysisContext, node.LeftValue)), RuntimeHelpers.EnsureBoolResult(rvalue));
		}

		Expression analyzeAssignmentExpression(BinaryExpression node, Expression rvalue)
		{
			if (node.LeftValue.NodeKind == AstNodeKind.BinaryExpression)
			{
				BinaryExpression leftBinary = (BinaryExpression)node.LeftValue;
				switch (leftBinary.Operator.Operation)
				{
					case OperationKind.MemberSet:
						IdentifierExpressionNode memberExpressionNodeBase = leftBinary.RightValue as IdentifierExpressionNode;
						if (memberExpressionNodeBase == null)
						{
							_analysisContext.ErrorCollector.InvalidLValueForAssignment(leftBinary.Location);
							return Expression.Empty();

						}
						return _analysisContext.PropertyOrFieldSet(memberExpressionNodeBase.Identifier.Text, Analyze(_analysisContext, leftBinary.LeftValue), rvalue);
					case OperationKind.Index:
						ArgumentList argList = (ArgumentList)leftBinary.RightValue;
						List<Expression> args = argList.Arguments.Select(arg => Analyze(_analysisContext, arg)).ToList();
						args.Insert(0, Analyze(_analysisContext, leftBinary.LeftValue));
						args.Add(rvalue);
						return Expression.Dynamic(_analysisContext.LanguageContext.CreateSetIndexBinder(new CallInfo(args.Count)), typeof(object), args);
				}
				_analysisContext.ErrorCollector.InvalidLValueForAssignment(leftBinary.Location);
				return Expression.Empty();
			}

			if (node.LeftValue.NodeKind == AstNodeKind.IdentifierExpression)
			{
				return this.Analyze((IdentifierExpressionNode)node.LeftValue, rvalue);
			}

			throw new UnhandledCaseException();
		}

		Expression analyzeIndexExpression(BinaryExpression node)
		{
			ArgumentList argList = (ArgumentList)node.RightValue;
			List<Expression> args = argList.Arguments.Select(arg => Analyze(_analysisContext, arg)).ToList();
			args.Insert(0, Analyze(_analysisContext, node.LeftValue));
			return Expression.Dynamic(_analysisContext.LanguageContext.CreateGetIndexBinder(new CallInfo(args.Count)), typeof(object), args);
		}

		Expression analyzeMemberGetOperation(BinaryExpression node)
		{
			Expression lvalue = Analyze(_analysisContext, node.LeftValue);
			switch (node.RightValue.NodeKind)
			{
				case AstNodeKind.IdentifierExpression:
					string propName = ((IdentifierExpressionNode)node.RightValue).Identifier.Text;
					return _analysisContext.PropertyOrFieldGet(propName, lvalue);
				case AstNodeKind.FunctionCallExpression:
					FunctionCallExpressionNodeBase funcCallExpr = (FunctionCallExpressionNodeBase)node.RightValue;
					List<Expression> args = funcCallExpr.Arguments.Select(arg => Analyze(_analysisContext, arg)).ToList();

					args.Insert(0, lvalue);
					return Expression.Dynamic(
						_analysisContext.LanguageContext.CreateCallBinder(funcCallExpr.Identifier.Text, false, new CallInfo(args.Count)), typeof(object), args);
				default:
					throw new InternalException("NodeKind " + node.RightValue.NodeKind + " not allowed here (DCL thinks)");
			}
		}

		public override void Visit(IdentifierExpressionNode node)
		{
			base.Visit(node);
			_result = this.Analyze(node);
		}

		Expression Analyze(IdentifierExpressionNode node, Expression value = null)
		{
			var searchResult = _analysisContext.TopSymbolTable.FindInScopeTree(node.Identifier.Text);

			if (searchResult == null)
			{
				_analysisContext.ErrorCollector.UndefinedVariable(node.Identifier);
				return Expression.Empty();
			}

			return value == null ? searchResult.GetGetExpression() : searchResult.GetSetExpression(value);
		}

		public Expression PushWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PushWriter");
			return Expression.Call(Expression.Convert(_analysisContext.RuntimeContextExpression, typeof(HappyRuntimeContext)), push);
		}

		public Expression PopWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PopWriter");
			return Expression.Call(Expression.Convert(_analysisContext.RuntimeContextExpression, typeof(HappyRuntimeContext)), push);

		}

		public override void BeforeVisit(AnonymousTemplate node)
		{
			base.AfterVisit(node);
			List<Expression> body = new List<Expression>();
			var previousCurrentOutputExp = _analysisContext.CurrentOutputExpression;
			_analysisContext.CurrentOutputExpression = Expression.Parameter(typeof(TextWriter), AnalysisContext.CurrentOutputIdentifier);

			_analysisContext.ScopeStack.Push(node.Body.SymbolTable);

			body.Add(PushWriter());
			body.Add(Expression.Assign(_analysisContext.CurrentOutputExpression, Expression.Property(_analysisContext.RuntimeContextExpression, _analysisContext.OutputWriterProperty)));

			body.Add(StatementBlockAnalyzer.Analyze(_analysisContext, node.Body));
			body.Add(PopWriter());

			var tmpOutputExpression = _analysisContext.CurrentOutputExpression;
			_analysisContext.CurrentOutputExpression = previousCurrentOutputExp;

			_result = Expression.Block(_analysisContext.TopSymbolTable.GetParameterExpressions().Union(new[] { tmpOutputExpression }), body);
		}

		/*
		 * AnonymousTemplate
		 * BinaryExpression
		 * FunctionCallExpressionNodeBase
		 * IdentifierExpressionNode
		 * LiteralExpression
		 * NamedExpressionNodeBase
		 * NewObjectExpression
		 * NullExpression
		 * UnaryExpression
		 */
	}
}

