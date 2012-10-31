using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.AstVisitors
{
	class PreAnalyzeVisitor : AstVisitorBase
	{
		public PreAnalyzeVisitor() : base(VisitorMode.VisitNodeAndChildren) { }

		public override void BeforeVisit(BinaryExpression node)
		{
			base.BeforeVisit(node);
			if (node.Operator.Operation == OperationKind.Assign)
			{
				node.LeftValue.AccessType = ExpressionAccessType.Write;
				return;
			}
		}
	}
}
