/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;
using HappyTemplate.Exceptions;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class StatementBlockAnalyzer : AstVisitorBase
	{
		readonly List<Expression> _statements = new List<Expression>();
		Expression _result;
		readonly AnalysisContext _analysisContext;

		StatementBlockAnalyzer(AnalysisContext analysisContext)
			: base(VisitorMode.VisitNodeAndChildren)
		{
			_analysisContext = analysisContext;
		}

		public static Expression Analyze(AnalysisContext analysisContext, StatementBlock statementBlock)
		{
			var analyzer = new StatementBlockAnalyzer(analysisContext);
			statementBlock.Accept(analyzer);

			if (analyzer._result == null)
				throw new InternalException("A StatementBlockAnalyzer did not generate a _result");

			return analyzer._result;
		}

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.BeforeVisitCatchAll(node);
			var statementNode = node as StatementNodeBase;
			if (statementNode == null) 
				return;

			base.PushMode(VisitorMode.VisitNodeOnly);
			_statements.Add(StatementAnalyzer.Analyze(_analysisContext, statementNode));
		}

		public override void AfterVisitCatchAll(AstNodeBase node)
		{
			base.AfterVisitCatchAll(node);
			var statementNode = node as StatementNodeBase;
			if (statementNode == null) 
				return;

			base.PopMode();
		}

		public override void BeforeVisit(StatementBlock node)
		{
			base.BeforeVisit(node);
			_analysisContext.ScopeStack.Push(node.SymbolTable);
		}

		public override void AfterVisit(StatementBlock node)
		{
			base.AfterVisit(node);
			_result = Expression.Block(node.SymbolTable.GetParameterExpressions(), _statements.Where(e => e != null));
			_analysisContext.ScopeStack.Pop();
		}
	}
}

