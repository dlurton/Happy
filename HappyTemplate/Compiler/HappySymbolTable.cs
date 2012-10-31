using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HappyTemplate.Compiler
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

		public HappySymbolBase Add(HappySymbolBase hsi)
		{
			_items.Add(hsi.Name, hsi);
			return hsi;
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