using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	//TODO:  there must be a way to make this inherit form AstVisitorBase...
	class FunctionAnalyzer
	{
		public static LambdaExpression Analzye(AnalysisContext analysisContext, Function function)
		{
			analysisContext.CurrentOutputExpression = Expression.Parameter(typeof(TextWriter), AnalysisContext.CurrentOutputIdentifier);

			List<Expression> outerExpressions = new List<Expression>
			{
				Expression.Assign(
					analysisContext.CurrentOutputExpression,
					Expression.Property(analysisContext.RuntimeContextExpression,
					                    analysisContext.OutputWriterProperty))
			};

			using (analysisContext.ScopeStack.TrackPush(function.ParameterList.SymbolTable))
			{
				//Create return target
				analysisContext.ReturnLabelTarget = Expression.Label(typeof(object), "lambdaReturn");
				LabelExpression returnLabel = Expression.Label(analysisContext.ReturnLabelTarget, Expression.Default(typeof(object)));

				outerExpressions.Add(StatementBlockAnalyzer.Analyze(analysisContext, function.Body));
				outerExpressions.Add(returnLabel);

				BlockExpression completeFunctionBody = Expression.Block(new[] { analysisContext.CurrentOutputExpression }, outerExpressions);
				LambdaExpression lambda = Expression.Lambda(completeFunctionBody, function.Name.Text, function.ParameterList.SymbolTable.GetParameterExpressions());
				return lambda;																   
			}			
		}
	}
}
