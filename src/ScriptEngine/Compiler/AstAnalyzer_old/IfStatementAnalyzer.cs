/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class IfStatementAnalyzer : AstVisitorBase
	{
		Expression _result;
		Expression _condition;
		Expression _trueBlock;
		Expression _falseBlock;
		readonly AnalysisContext _analysisContext;

		protected IfStatementAnalyzer(AnalysisContext analysisContext) : base(VisitorMode.VisitNodeAndChildren)
		{
			_analysisContext = analysisContext;
		}

		public static Expression Analyze(AnalysisContext analysisContext, IfStatement node)
		{
			var analyzer = new IfStatementAnalyzer(analysisContext);
			node.Accept(analyzer);
			return analyzer._result;
		}

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.AfterVisitCatchAll(node);
			var expressionNode = node as ExpressionNodeBase;
			if (expressionNode == null)
				return;

			base.PushMode(VisitorMode.VisitNodeOnly);
			_result = ExpressionAnalyzer.Analyze(_analysisContext, (ExpressionNodeBase)node);
		}

		public override void BeforeVisit(IfStatement node)
		{
			this.BeforeVisitCatchAll(node);

		}

		public override void AfterIfStatementCondition(IfStatement node)
		{
			base.AfterIfStatementCondition(node);
			_condition = Expression.Convert(_result, typeof(Boolean));
		}

		public override void AfterVisitCatchAll(AstNodeBase node)
		{
			base.AfterVisitCatchAll(node);
			var expressionNode = node as ExpressionNodeBase;
			if (expressionNode == null)
				return;

			base.PopMode();
		}

		public override void BeforeVisit(StatementBlock node)
		{
			base.BeforeVisit(node);
			_result = StatementBlockAnalyzer.Analyze(_analysisContext, node);
			base.PushMode(VisitorMode.VisitNodeOnly);
		}

		public override void AfterVisit(StatementBlock node)
		{
			base.AfterVisit(node);
			base.PopMode();
		}

		public override void AfterIfStatementTrueBlock(IfStatement node)
		{
			base.AfterIfStatementTrueBlock(node);
			_trueBlock = _condition;
		}

		public override void AfterIfStatementFalseBlock(IfStatement node)
		{
			base.AfterIfStatementFalseBlock(node);
			_falseBlock = _condition;
		}

		public override void AfterVisit(IfStatement node)
		{
			this.AfterVisitCatchAll(node);
			_result = _falseBlock == null ? Expression.IfThen(_condition, _trueBlock) : Expression.IfThenElse(_condition, _trueBlock, _falseBlock);
		}
	}
}

