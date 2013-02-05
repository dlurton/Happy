/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace Happy.ScriptEngine.Compiler.Ast
{
	abstract class NamedExpressionNodeBase : ExpressionNodeBase
	{
		readonly protected Identifier _identifier;

		public Identifier Identifier { get { return _identifier; } }


		protected NamedExpressionNodeBase(HappySourceLocation startsAt, HappySourceLocation endsAt, Identifier identifier)
			: base(startsAt, endsAt)
		{
			_identifier = identifier;
		}

		protected NamedExpressionNodeBase(Identifier identifier) : base(identifier.Location)
		{
			_identifier = identifier;
		}

		public override string ToString()
		{
			return this.Identifier.ToString();
		}
	}
}

