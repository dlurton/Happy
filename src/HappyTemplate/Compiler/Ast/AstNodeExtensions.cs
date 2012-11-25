/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;

namespace HappyTemplate.Compiler.Ast
{
	static class AstNodeExtensions
	{
		public static void ExecuteOverAll(this IEnumerable<AstNodeBase> enumerable, Action<AstNodeBase> nodeAction)
		{
			if(enumerable == null)
				throw new ArgumentNullException("enumerable");

			foreach (AstNodeBase node in enumerable)
				nodeAction(node);
		}
	}
}

