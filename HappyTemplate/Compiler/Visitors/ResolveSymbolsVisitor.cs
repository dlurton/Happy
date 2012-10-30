using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.Visitors
{
	class ResolveSymbolsVisitor : ScopedAstVisitorBase
	{
		readonly ErrorCollector _errorCollector;
		public ResolveSymbolsVisitor(ErrorCollector errorCollector) : base(VisitorMode.VisitNodeAndChildren)
		{
			_errorCollector = errorCollector;
		}

		public override void BeforeVisit(BinaryExpression node)
		{
			base.BeforeVisit(node);

			if(node.Operator.Operation == OperationKind.MemberAccess)
				((NamedExpressionNodeBase)node.RightValue).SetIsMemberReference(true);
		}

		public override void Visit(IdentifierExpression node)
		{
			base.Visit(node);

			if (node.GetIsMemberReference())
				return;

			var searchResult = base.TopSymbolTable.FindInScopeTree(node.Identifier.Text);

			if (searchResult == null)
				_errorCollector.UndefinedVariable(node.Identifier);

			node.SetSymbol(searchResult);
		}

	}
}
