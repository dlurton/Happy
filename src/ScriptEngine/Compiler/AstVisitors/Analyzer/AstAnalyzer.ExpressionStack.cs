/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq.Expressions;
using Happy.ScriptEngine.Compiler.Ast;

namespace Happy.ScriptEngine.Compiler.AstVisitors.Analyzer
{
    partial class AstAnalyzer
    {
	    class DebugExpression
	    {
		    public HappySourceLocation Location;
		    public Expression Expression;
	    }

	    class ExpressionStack
		{
			readonly bool _includeDebugInfo;
			
			readonly Stack<DebugExpression> _stack = new Stack<DebugExpression>();

			public ExpressionStack(bool includeDebugInfo)
			{
				_includeDebugInfo = includeDebugInfo;
			}

			public int Count { get { return _stack.Count; } }

			public void Push(AstNodeBase node, Expression expr)
			{
				_stack.Push(new DebugExpression { Location = node.Location, Expression = expr });
			}

			public Expression Pop(bool includeDebugInfo = true)
			{
				var expr = _stack.Pop();
				return wrapInDebugInfo(expr, includeDebugInfo);
			}

			public List<Expression> Pop(int count, bool includeDebugInfo = true)
			{
				DebugAssert.IsGreaterOrEqual(_stack.Count, count, "Attempted to pop {0} items when there were actually only {1} items in the stack", count, _stack.Count);
				List<Expression> list = new List<Expression>(count);
				for (int i = 0; i < count; ++i)
				{
					var stackEntry = _stack.Pop();
					list.Insert(0, wrapInDebugInfo(stackEntry, includeDebugInfo));
				}

				return list;
			}

			Expression wrapInDebugInfo(DebugExpression expr, bool includeDebugInfo)
			{
				var location = expr.Location;
				if (!_includeDebugInfo || !includeDebugInfo || location == HappySourceLocation.None || location == HappySourceLocation.Invalid)
					return expr.Expression;

				return Expression.Block(
					Expression.DebugInfo(
						Expression.SymbolDocument(location.Unit.Path),
						location.Span.Start.Line,
						location.Span.Start.Column,
						location.Span.End.Line,
						location.Span.End.Column),
					expr.Expression
					);
			}
		}
    }
}

