/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Dynamic;
using System.Linq;
using Microsoft.Scripting.ComInterop;

namespace Happy.ScriptEngine.Runtime.Binding
{
	class HappyGetIndexBinder : GetIndexBinder
	{
		public HappyGetIndexBinder(CallInfo callinfo)
			: base(callinfo)
		{
		}

		public override DynamicMetaObject FallbackGetIndex(
			DynamicMetaObject target, DynamicMetaObject[] indexes,
			DynamicMetaObject errorSuggestion)
		{
			// First try COM binding.
			DynamicMetaObject result;
			if (ComBinder.TryBindGetIndex(this, target, indexes, out result))
			{
				return result;
			}
			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!target.HasValue || indexes.Any((a) => !a.HasValue))
			{
				var deferArgs = new DynamicMetaObject[indexes.Length + 1];
				for (int i = 0; i < indexes.Length; i++)
				{
					deferArgs[i + 1] = indexes[i];
				}
				deferArgs[0] = target;
				return Defer(deferArgs);
			}
			// Give good error for Cons.
			//if (target.LimitType == typeof(Cons))
			//{
			//    if (indexes.Length != 1)
			//        return errorSuggestion ??
			//            RuntimeHelpers.CreateThrow(
			//                 target, indexes, BindingRestrictions.Empty,
			//                 typeof(InvalidOperationException),
			//                 "Indexing list takes single index.  " + "Got " +
			//                 indexes.Length.ToString());
			//}
			// Find our own binding.
			//
			// Conversions created in GetIndexExpression must be consistent with
			// restrictions made in GetTargetArgsRestrictions.
			var indexingExpr = RuntimeHelpers.EnsureObjectResult(
				RuntimeHelpers.GetIndexingExpression(target,
					indexes));
			var restrictions = RuntimeHelpers.GetTargetArgsRestrictions(
				target, indexes, false);
			return new DynamicMetaObject(indexingExpr, restrictions);
		}
	}
}

