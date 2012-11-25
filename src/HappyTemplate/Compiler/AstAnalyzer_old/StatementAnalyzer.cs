/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;
using HappyTemplate.Exceptions;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class StatementAnalyzer : AstVisitorBase
	{
		readonly AnalysisContext _analysisContext;
		Expression _result;

		protected StatementAnalyzer(AnalysisContext analysisContext) : base(VisitorMode.VisitNodeOnly)
		{
			_analysisContext = analysisContext;
		}

		public static Expression Analyze(AnalysisContext analysisContext, StatementNodeBase node)
		{
			var analyzer = new StatementAnalyzer(analysisContext);
			node.Accept(analyzer);
			if(analyzer._result == null)
				throw new InternalException("StatementAnalyzer did not handle a " + node.GetType() + " which is a descendant of StatementNodeBase");
			return analyzer._result;
		}

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.BeforeVisitCatchAll(node);
			var expressionNode = node as ExpressionNodeBase;
			if (expressionNode != null)
				_result = ExpressionAnalyzer.Analyze(_analysisContext, expressionNode);
		}

		public override void Visit(VerbatimSection node)
		{
			base.Visit(node);
			_result = CreateWriteToOutputExpression(_analysisContext.CurrentOutputExpression, node.Text);
		}

		static Expression CreateWriteToOutputExpression(Expression writerExpression, string text)
		{
			MethodInfo write = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
			return Expression.Call(writerExpression, write, Expression.Constant(text));
		}

		public override void BeforeVisit(IfStatement node)
		{
			base.PushMode(VisitorMode.VisitNodeOnly);
			_result = IfStatementAnalyzer.Analyze(_analysisContext, node);
		}

		public override void AfterVisit(IfStatement node)
		{
			base.PopMode();
		}
	
		public override void BeforeVisit(DefStatement node)
		{
			base.BeforeVisit(node);
			var initializers = (from varDef in node.VariableDefs
								where varDef.InitializerExpression != null
								select varDef.Symbol.GetSetExpression(ExpressionAnalyzer.Analyze(_analysisContext, varDef.InitializerExpression))).ToList();

			_result = initializers.Count > 0 ? (Expression)Expression.Block(initializers) : Expression.Constant(null);
		}

		public override void BeforeVisit(OutputStatement node)
		{
			base.AfterVisit(node);
			_result = OutputStatementAnalyzer.Analyze(_analysisContext, node);
		}
	}
}

