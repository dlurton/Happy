/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Linq.Expressions;
using Happy.ScriptEngine.Runtime;

namespace Happy.ScriptEngine.Compiler
{
	abstract class HappySymbolBase
	{ 
		public string Name { get; private set; }
		public abstract Expression GetGetExpression();
		public abstract Expression GetSetExpression(Expression value);
		public HappySymbolBase(string name)
		{
			this.Name = name;
		}
	}

	class HappyParameterSymbol : HappySymbolBase
	{
		readonly ParameterExpression _parameter;

		public override Expression GetGetExpression() { return _parameter; }
		public override Expression GetSetExpression(Expression value)
		{
			return Expression.Assign(_parameter, RuntimeHelpers.EnsureObjectResult(value));
		} 

		public ParameterExpression Parameter { get { return _parameter; } }

		public HappyParameterSymbol(string name) : base(name)
		{
			_parameter = Expression.Parameter(typeof(object), name);
		}

		public HappyParameterSymbol(ParameterExpression parameter)
			: base(parameter.Name)
		{
			_parameter = parameter;
		}
	}

	class HappyNamedExpressionSymbol : HappySymbolBase
	{
		readonly Func<string, Expression> _getGetter;
		readonly Func<string, Expression, Expression> _setGetter;

		public override Expression GetGetExpression()
		{
			return _getGetter(base.Name);
		}

		public override Expression GetSetExpression(Expression value)
		{
			return _setGetter == null ? null : _setGetter(base.Name, value);
		}

		public HappyNamedExpressionSymbol(string name, Func<string, Expression> getGetter, Func<string, Expression, Expression> setGetter = null)
			: base(name)
		{
			_getGetter = getGetter;
			_setGetter = setGetter;
		}
	}
}

