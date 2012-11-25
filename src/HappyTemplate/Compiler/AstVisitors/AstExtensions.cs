/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.AstVisitors
{
	static class AstExtensions
	{
		readonly static Dictionary<int, Dictionary<string, dynamic>> _extendedProperties = new Dictionary<int, Dictionary<string, dynamic>>();

		static T getProperty<T>(AstNodeBase node, string key)
		{
			Dictionary<string, dynamic> bag;
			dynamic retval;
			if (_extendedProperties.TryGetValue(node.Id, out bag) && bag.TryGetValue(key, out retval))
				return retval;

			return default(T);
		}

		static void setProperty(AstNodeBase node, string key, dynamic value)
		{
			Dictionary<string, dynamic> bag;
			if (!_extendedProperties.TryGetValue(node.Id, out bag))
			{
				bag = new Dictionary<string, dynamic>();
				_extendedProperties[node.Id] = bag;
			}
			bag[key] = value;
		}

		public static HappySymbolBase GetSymbol(this IdentifierExpression node)
		{
			return getProperty<HappySymbolBase>(node, "Symbol");
		}

		public static void SetSymbol(this IdentifierExpression node, HappySymbolBase symbol)
		{
			setProperty(node, "Symbol", symbol);
		}

		public static void SetAnalyzeSymbolsExternally(this ScopeAstNodeBase node, bool value)
		{
			setProperty(node, "AnalyzeSymbolsExternally", value);
		}

		public static bool GetAnalyzeSymbolsExternally(this ScopeAstNodeBase node)
		{
			return getProperty<bool>(node, "AnalyzeSymbolsExternally");
		}

		public static void SetIsMemberReference(this NamedExpressionNodeBase node, bool value)
		{
			setProperty(node, "IsMemberReference", value);
		}

		public static bool GetIsMemberReference(this NamedExpressionNodeBase node)
		{
			return getProperty<bool>(node, "IsMemberReference");
		}
	}
}

