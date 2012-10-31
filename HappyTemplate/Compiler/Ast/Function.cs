using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class Function : NamedAstNodeBase
	{
		readonly StatementBlock _body;
		readonly FunctionParameterList _parameterList;

		public Function(Identifier name, FunctionParameterList parameterList, StatementBlock body)
			: base(name)
		{
			_parameterList = parameterList;
			_body = body;
		}

		public override AstNodeKind NodeKind { get { return AstNodeKind.Template; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				_parameterList.Accept(visitor);
				_body.Accept(visitor);
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("");
			writer.Write("function {0}", this.Name.Text);
			
			writer.Write(")");
			using(writer.CurlyBraces())
				writer.Write(_body);
		}

		public FunctionParameterList ParameterList { get { return _parameterList; } }
		public StatementBlock Body { get { return _body; } }

		
	}
}