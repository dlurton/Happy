using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class FunctionParameterList : ScopeAstNodeBase
	{
		readonly FunctionParameter[] _parameters;
		public FunctionParameter this[int index] { get { return _parameters[index]; } }
		public FunctionParameterList(HappySourceLocation location, FunctionParameter[] parameters) : base(location)
		{
			_parameters = parameters;
		}

		public int Count { get { return _parameters.Length; }}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.FunctionParameterList; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				_parameters.ExecuteOverAll(p => p.Accept(visitor));

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write("(");
			writer.Write(_parameters, ", ");
			writer.Write(")");
		}

		
	}
}
