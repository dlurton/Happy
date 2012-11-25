/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	//TO DO:  need to eliminate this as a public class
	//to do that, first have to remove Ast.Module from the 
	//public API.
	public abstract class AstNodeBase
	{
		static int _numAstNodes;
		readonly HappySourceLocation _location;

		internal HappySourceLocation Location { get { return _location; } }

		internal abstract AstNodeKind NodeKind { get; }
		internal int Id { get; private set; }

		protected AstNodeBase(HappySourceLocation location)
			: this(location, null)
		{
		}

		protected AstNodeBase(HappySourceLocation startsAt, HappySourceLocation endsAt)
		{
			_location = endsAt != null ? HappySourceLocation.Merge(startsAt, endsAt) : startsAt;
			this.Id = ++_numAstNodes;
		}

		internal abstract void Accept(AstVisitorBase visitor);
	    internal abstract void WriteString(AstWriter writer);
	}
}

