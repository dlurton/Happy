/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Linq.Expressions;
using HappyTemplate.Runtime;

namespace HappyTemplate.Compiler.AstVisitors
{
	partial class AstAnalyzer : ScopedAstVisitorBase, IGlobalScopeHelper
	{
		const string RuntimeContextIdentifier = "__runtimeContext__";
		
		readonly ErrorCollector _errorCollector;
		readonly ExpressionStack _expressionStack = new ExpressionStack(true);
		readonly Expression _globalScopeExp;
		readonly ParameterExpression _runtimeContextExp;
		readonly HappyLanguageContext _languageContext;

		public AstAnalyzer(HappyLanguageContext languageContext) : base(VisitorMode.VisitNodeAndChildren)
		{
			_languageContext = languageContext;
			_errorCollector = new ErrorCollector(languageContext.ErrorSink);
			_runtimeContextExp = Expression.Parameter(typeof(HappyRuntimeContext), RuntimeContextIdentifier);
			_globalScopeExp = this.PropertyOrFieldGet("Globals", _runtimeContextExp);
		}
	}
}

