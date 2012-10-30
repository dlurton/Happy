using System.Collections.Generic;
using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.Visitors
{
	class ScopedAstVisitorBase : AstVisitorBase
	{
		protected ScopedAstVisitorBase(VisitorMode intitialMode) : base(intitialMode)
		{
			
		}

		readonly Stack<HappySymbolTable> _scopeStack = new Stack<HappySymbolTable>();
		protected HappySymbolTable TopSymbolTable { get { return _scopeStack.Peek(); } }

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.BeforeVisitCatchAll(node);
			ScopeAstNodeBase scopeNode = node as ScopeAstNodeBase;
			if(scopeNode != null)
				_scopeStack.Push(scopeNode.SymbolTable);
		}

		public override void AfterVisitCatchAll(AstNodeBase node)
		{
			ScopeAstNodeBase scopeNode = node as ScopeAstNodeBase;
			if (scopeNode != null)
				_scopeStack.Pop();
			base.AfterVisitCatchAll(node);
		}


		public override void BeforeVisit(ForStatement node)
		{
			base.BeforeVisit(node);			
			_scopeStack.Push(node.LoopBody.SymbolTable);
		}

		public override void AfterVisit(ForStatement node)
		{
			_scopeStack.Pop();
			base.AfterVisit(node);
		}

		//public override void BeforeVisit(Function node)
		//{
		//	base.BeforeVisit(node);
		//	_scopeStack.Push(node.ParameterList.SymbolTable);
		//}

		//public override void AfterVisit(Function node)
		//{
		//	base.AfterVisit(node);
		//	_scopeStack.Pop();
		//}

		//public override void BeforeVisit(FunctionParameterList node)
		//{
		//	base.BeforeVisit(node);
		//	_scopeStack.Push(node.SymbolTable);
		//}

		//public override void AfterVisit(FunctionParameterList node)
		//{
		//	base.AfterVisit(node);
		//	_scopeStack.Pop();
		//}

		//public override void BeforeVisit(StatementBlock node)
		//{
		//	base.BeforeVisit(node);
		//	_scopeStack.Push(node.SymbolTable);
		//}

		//public override void AfterVisit(StatementBlock node)
		//{
		//	base.AfterVisit(node);
		//	_scopeStack.Pop();
		//}
	}
}
