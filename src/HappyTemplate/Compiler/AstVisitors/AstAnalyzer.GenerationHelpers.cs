/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using BinaryExpression = System.Linq.Expressions.BinaryExpression;

namespace HappyTemplate.Compiler.AstVisitors
{
	partial class AstAnalyzer
	{
		class ContainerPseudoExpression<T> : Expression
		{
			public T ContainedItem { get; private set; }

			public ContainerPseudoExpression(T item)
			{
				this.ContainedItem = item;
			}
		}

		public BinaryExpression IsNullExpression(Expression expr)
		{
			return Expression.Equal(expr, Expression.Constant(null, typeof(object)));
		}

		public Expression GetGlobalScopeGetter(string globalName)
		{
			return this.PropertyOrFieldGet(globalName, _globalScopeExp);
		}

		public Expression GetGlobalScopeSetter(string globalName, Expression value)
		{
			return this.PropertyOrFieldSet(globalName, _globalScopeExp, value);
		}

		static Expression MaybeBlock(ParameterExpression[] parameters, IList<Expression> list)
		{
			if(list.Count == 1 && parameters.Length == 0)
				return list[0];

			return Expression.Block(parameters, list);
		}

		static Expression MaybeBlock(IList<Expression> list)
		{
			if(list.Count == 1)
				return list[0];
			return Expression.Block(list);
		}


		static Expression MaybeBlock(Expression[] array)
		{
			if(array.Length == 1)
				return array[0];

			return Expression.Block(array);
		}

		Expression PushWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PushWriter", new Type[0]);
			return Expression.Call(_runtimeContextExp, push);
		}

		Expression PopWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PopWriter");
			return Expression.Call(_runtimeContextExp, push);

		}

		Expression WriteVerbatimToTopWriter(string text)
		{
			MethodInfo mi = typeof(HappyRuntimeContext).GetMethod("WriteToTopWriter", new[] { typeof(string) });
			return Expression.Call(_runtimeContextExp, mi, Expression.Constant(text, typeof(string)));
		}

		Expression WriteConstantToTopWriter(ConstantExpression value)
		{
			if(value.Type == typeof(string))
				return WriteVerbatimToTopWriter(value.Value.ToString());

			MethodInfo toStringMi = typeof(object).GetMethod("ToString");
			Expression stringExpression = Expression.Call(value, toStringMi);
			MethodInfo writeMi = typeof(HappyRuntimeContext).GetMethod("WriteToTopWriter", new[] { typeof(string) });
			return Expression.Call(_runtimeContextExp, writeMi, stringExpression);
		}

		Expression SafeWriteToTopWriter(Expression value)
		{
			if(value.NodeType == ExpressionType.Constant)
				return WriteConstantToTopWriter((ConstantExpression)value);

			MethodInfo mi = typeof(HappyRuntimeContext).GetMethod("SafeWriteToTopWriter");
			return Expression.Call(_runtimeContextExp, mi, RuntimeHelpers.EnsureObjectResult(value));
		}

		Expression PropertyOrFieldGet(string name, Expression instance)
		{
			//Only make a dynamic expression if we have have to
			var memberInfo = findPropertyOrField(instance.Type, name);
			if(memberInfo == null)
				return this.dynamicGetMember(instance, name);

			switch(memberInfo.MemberType)
			{
			case MemberTypes.Field:
				return Expression.Field(instance, (FieldInfo)memberInfo);
			case MemberTypes.Property:
				return Expression.Property(instance, (PropertyInfo)memberInfo);
			default:
				throw new UnhandledCaseException();
			}
		}

		static MemberInfo findPropertyOrField(Type type, string name)
		{
			return type.GetProperty(name) ?? (MemberInfo)type.GetField(name);
		}

		Expression PropertyOrFieldSet(string name, Expression instance, Expression newValue)
		{

			Expression propertyOrFieldExpr;

			var memberInfo = findPropertyOrField(instance.Type, name);
			if (memberInfo == null)
				return this.dynamicSetMember(instance, name, newValue);

			switch (memberInfo.MemberType)
			{
				case MemberTypes.Field:
					propertyOrFieldExpr = Expression.Field(instance, (FieldInfo)memberInfo);
					break;
				case MemberTypes.Property:
					propertyOrFieldExpr = Expression.Property(instance, (PropertyInfo)memberInfo);
					break;
				default:
					throw new UnhandledCaseException();
			}
			return Expression.Assign(propertyOrFieldExpr, newValue);
		}

		static ExpressionType ToExpressionType(Operator node)
		{
			ExpressionType expType;

			switch(node.Operation)
			{
			case OperationKind.Add:
				expType = ExpressionType.Add;
				break;
			case OperationKind.Subtract:
				expType = ExpressionType.Subtract;
				break;
			case OperationKind.Divide:
				expType = ExpressionType.Divide;
				break;
			case OperationKind.Multiply:
				expType = ExpressionType.Multiply;
				break;
			case OperationKind.Mod:
				expType = ExpressionType.Modulo;
				break;
			case OperationKind.LogicalAnd:
				expType = ExpressionType.AndAlso;
				break;
			case OperationKind.LogicalOr:
				expType = ExpressionType.OrElse;
				break;
			case OperationKind.Xor:
				expType = ExpressionType.ExclusiveOr;
				break;
			case OperationKind.Equal:
				expType = ExpressionType.Equal;
				break;
			case OperationKind.Greater:
				expType = ExpressionType.GreaterThan;
				break;
			case OperationKind.Less:
				expType = ExpressionType.LessThan;
				break;
			case OperationKind.GreaterThanOrEqual:
				expType = ExpressionType.GreaterThanOrEqual;
				break;
			case OperationKind.LessThanOrEqual:
				expType = ExpressionType.LessThanOrEqual;
				break;
			case OperationKind.NotEqual:
				expType = ExpressionType.NotEqual;
				break;
			case OperationKind.Assign:
				expType = ExpressionType.Assign;
				break;
			case OperationKind.Not:
				expType = ExpressionType.Not;
				break;
			case OperationKind.BitwiseAnd:
				expType = ExpressionType.And;
				break;
			case OperationKind.BitwiseOr:
				expType = ExpressionType.Or;
				break;
			case OperationKind.Index:
				expType = ExpressionType.Index;
				break;
			case OperationKind.MemberAccess:
				expType = ExpressionType.MemberAccess;
				break;
			default:
				throw new UnhandledCaseException(node.Operation.ToString());
			}
			return expType;
		}
	}
}

