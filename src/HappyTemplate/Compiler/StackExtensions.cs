/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using HappyTemplate.Exceptions;

namespace HappyTemplate.Compiler
{
	static class StackExtensions
	{
		public static TC PopCast<TC>(this Stack<Expression> stack)
			where TC : class
		{
			var retval = stack.Pop() as TC;
			DebugAssert.IsNotNull(retval, "Popped a {0} off a stack when {1} was expected", retval.GetType(), typeof(TC));
			return retval;
		}

		public static List<T> Pop<T>(this Stack<T> stack, int count)
		{
			DebugAssert.IsGreaterOrEqual(stack.Count, count, "Attempted to pop {0} items when there were actually only {1} items in the stack", count, stack.Count);
			List<T> list = new List<T>(count);
			for (int i = 0; i < count; ++i)
				list.Insert(0, stack.Pop());
			return list;
		}

		public static U Pop<T,U>(this Stack<T> stack) 
			where U : class
			where T : class
		{
			object maybeU = stack.Pop();
			DebugAssert.IsInstanceOfType(typeof (U), maybeU, "Item poppped from stack was not of expected type");
			return (U)maybeU;
		}
	}
}

