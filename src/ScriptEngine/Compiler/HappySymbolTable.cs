/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Happy.ScriptEngine.Compiler
{
	class HappySymbolTable
	{
		public HappySymbolTable Parent { get; private set; }
		readonly Dictionary<string, HappySymbolBase> _items = new Dictionary<string, HappySymbolBase>();
		public Dictionary<string, HappySymbolBase> Items { get { return _items; } }

		public string SymbolTableName { get; private set; }

		public HappySymbolTable(string symbolTableName, HappySymbolTable parent)
		{
			Parent = parent;
			this.SymbolTableName = symbolTableName;
		}


		public override string ToString()
		{
			return this.SymbolTableName;
		}

		public HappySymbolBase Add(HappySymbolBase hsb)
		{
			DebugAssert.IsFalse(_items.ContainsKey(hsb.Name), "Symbol table already contains symbol '{0}'", hsb.Name);
				
			_items.Add(hsb.Name, hsb);
			return hsb;
		}

		public HappyParameterSymbol Add(string name)
		{
			HappyParameterSymbol retval = new HappyParameterSymbol(name);
			_items.Add(name, retval);
			return retval;
		}

		public HappyParameterSymbol Add(ParameterExpression parameter)
		{
			HappyParameterSymbol retval = new HappyParameterSymbol(parameter);
			_items.Add(parameter.Name, retval);
			return retval;
		}
	
		public bool ExistsInCurrentScope(string name)
		{
			return this.FindInCurrentScope(name) != null;
		}

		public HappySymbolBase FindInCurrentScope(string name)
		{
			HappySymbolBase expression;
			if(_items.TryGetValue(name, out expression))
				return expression;

			return null;
		}
	
		public ParameterExpression[] GetParameterExpressions()
		{
			return _items.Values.Where(i => i is HappyParameterSymbol).Select(i => ((HappyParameterSymbol)i).Parameter).ToArray();
		}

		public HappySymbolBase FindInScopeTree(string name)
		{
			HappySymbolBase retval;
			if(_items.TryGetValue(name, out retval))
				return retval;

			return this.Parent != null ? this.Parent.FindInScopeTree(name) : null;
		}

		public bool ExistsInScopeTree(string name)
		{
			return this.FindInScopeTree(name) != null;
		}

	}
}

