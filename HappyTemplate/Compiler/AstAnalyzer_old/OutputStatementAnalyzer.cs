using System.Collections.Generic;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class OutputStatementAnalyzer
	{
		public static Expression Analyze(AnalysisContext analysisContext, OutputStatement node)
		{
			List<Expression> expressionsToWrite = new List<Expression>();

			foreach (var expr in node.ExpressionsToWrite)
			{
				Expression expressionToWrite = ExpressionAnalyzer.Analyze(analysisContext, expr);
				Expression writeToOutputExpression = analysisContext.CreateWriteToOutputExpression(expressionToWrite);
				expressionsToWrite.Add(writeToOutputExpression);
			}

			DebugAssert.IsGreater(0, expressionsToWrite.Count, "number of output expressions should have been > 0");
			return Expression.Block(expressionsToWrite);
		}
		//readonly AnalysisContext _analysisContext;
		//BlockExpression _result;

		//public OutputStatementAnalyzer(AnalysisContext analysisContext) : base(VisitorMode.VisitNodeOnly)
		//{
		//	_analysisContext = analysisContext;
		//}

		//public static Expression Analyze(AnalysisContext analysisContext, OutputStatement node)
		//{
		//	var analyzer = new OutputStatementAnalyzer(analysisContext);
		//	node.Accept(analyzer);
		//	return analyzer._result;
		//}

		//public override void BeforeVisit(OutputStatement node)
		//{
		//	base.BeforeVisit(node);
		//	List<Expression> expressionsToWrite = new List<Expression>();

		//	foreach (var expr in node.ExpressionsToWrite)
		//	{
		//		Expression expressionToWrite = ExpressionAnalyzer.Analyze(_analysisContext, expr);
		//		Expression writeToOutputExpression = _analysisContext.CreateWriteToOutputExpression(expressionToWrite);
		//		expressionsToWrite.Add(writeToOutputExpression);
		//	}

		//	DebugAssert.IsGreater(0, expressionsToWrite.Count, "number of output expressions should have been > 0");
		//	_result = Expression.Block(expressionsToWrite);
		//}
	}
}