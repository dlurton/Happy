using System;
using System.Collections.Generic;

namespace Happy.ScriptEngine
{
	public class LooselyExtendableBase
	{
		readonly Dictionary<Type, object> _extensions = new Dictionary<Type, object>();

		public T GetExtension<T>() where T : new()
		{
			object retval;
			if(_extensions.TryGetValue(typeof(T), out retval))
				return (T)retval;

			retval = new T();
			_extensions[typeof(T)] = retval;
			return (T)retval;
		}
	}
}
