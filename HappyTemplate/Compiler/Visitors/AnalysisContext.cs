using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;

namespace HappyTemplate.Compiler.Visitors
{
	class AnalysisContext
	{
		public const string CurrentOutputIdentifier = "__currentOutput__";

		public ParameterExpression CurrentOutputExpression { get; set; }
		public Expression RuntimeContextExpression { get; private set; }
		public Expression GlobalScopeExpression { get; private set; }
		public PropertyInfo OutputWriterProperty { get { return typeof (HappyRuntimeContext).GetProperty("OutputWriter"); } }
		public Stack<HappySymbolTable> ScopeStack { get; private set; }
		public HappySymbolTable TopSymbolTable { get { return this.ScopeStack.Peek(); } }
		public LabelTarget ReturnLabelTarget { get; set; }
		public HappyLanguageContext LanguageContext { get; private set; }
		public ErrorCollector ErrorCollector { get; private set;  }

		public AnalysisContext(ErrorCollector errorCollector, HappyLanguageContext languageContext, Expression runtimeContextExpression, Expression globalScopeExpression)
		{
			this.ErrorCollector = errorCollector;
			this.GlobalScopeExpression = globalScopeExpression;
			this.RuntimeContextExpression = runtimeContextExpression;
			this.LanguageContext = languageContext;
			this.ScopeStack = new Stack<HappySymbolTable>();
		}

		public DynamicExpression PropertyOrFieldGet(string name, Expression instance)
		{
			return Expression.Dynamic(this.LanguageContext.CreateGetMemberBinder(name, false), typeof(object), instance);
		}

		public DynamicExpression PropertyOrFieldSet(string name, Expression instance, Expression newValue)
		{
			return Expression.Dynamic(this.LanguageContext.CreateSetMemberBinder(name, false), typeof(object), instance, newValue);
		}

		public Expression GetGlobalScopeGetter(string globalName)
		{
			return this.PropertyOrFieldGet(globalName, this.GlobalScopeExpression);
		}

		public Expression GetGlobalScopeSetter(string globalName, Expression value)
		{
			return this.PropertyOrFieldSet(globalName, this.GlobalScopeExpression, value);
		}

		public Expression CreateWriteToOutputExpression(Expression value)
		{
			if (this.CurrentOutputExpression == null)
				throw new InternalException("AnalysisContext.CurrentOutputExpression must be set");

			MethodInfo write = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
			MethodInfo toString = typeof(object).GetMethod("ToString");

			if (value.NodeType == ExpressionType.Constant || value.Type.IsPrimitive)
			{
				if (value.Type != typeof(string))
					value = Expression.Call(value, toString);

				return Expression.Call(this.CurrentOutputExpression, write, value);
			}

			ParameterExpression tmp = Expression.Parameter(typeof(object), "tmp");
			Expression convertedValue = Expression.Call(tmp, toString);
			return
				Expression.Block(new[] { tmp },
								 Expression.Assign(tmp, Expression.Convert(value, typeof(object))),
								 Expression.IfThen(Expression.NotEqual(tmp, Expression.Constant(null)),
												   Expression.Call(this.CurrentOutputExpression, write, convertedValue)));

		}
	}
}
