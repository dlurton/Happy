/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace HappyTemplate.Compiler.Ast
{
	/// <summary>
	/// This class exists for no reason except to provide type safety.
	/// </summary>
	abstract class StatementNodeBase : AstNodeBase
	{
		protected StatementNodeBase(HappySourceLocation location) : base(location)
		{
			
		}

		protected StatementNodeBase(HappySourceLocation startsAt, HappySourceLocation endsAt) : base(startsAt, endsAt)
		{
			
		}
	}
}

