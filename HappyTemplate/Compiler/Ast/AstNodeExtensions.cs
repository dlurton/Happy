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
