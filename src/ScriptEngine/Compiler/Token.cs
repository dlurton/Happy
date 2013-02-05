/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;

namespace Happy.ScriptEngine.Compiler
{
	public class Token
	{
		internal readonly HappyTokenKind HappyTokenKind;
		public HappySourceLocation Location { get; private set;  }

		internal readonly string Text;
		internal Token(HappySourceLocation loc, HappyTokenKind happyTokenKind, string text)
		{
			this.HappyTokenKind = happyTokenKind;
			this.Location = loc;
			this.Text = text;
		}

		internal Identifier ToIdentifier()
		{
			return new Identifier(this.Location, this.Text);
		}

		static HashSet<HappyTokenKind> _operatorKinds;

		public bool IsOperator
		{
			get
			{
				if(_operatorKinds == null)
				{
					_operatorKinds = new HashSet<HappyTokenKind>();
					var operators = from v in typeof(HappyTokenKind).GetEnumValues().Cast<HappyTokenKind>()
								   let vstr = v.ToString()
								   where vstr.StartsWith("Operator") || vstr.StartsWith("UnaryOperator")
								   select v;
					operators.ForAll(htk => _operatorKinds.Add(htk));
				}
					
				return _operatorKinds.Contains(this.HappyTokenKind);
			}
		}
		public bool IsKeyword
		{
			get
			{
				//TO DO:  is there a better way to do this?
				return this.HappyTokenKind.ToString().StartsWith("Keyword");
			}
		}
	}
}

