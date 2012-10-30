using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Exceptions;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	static class AnalyzerUtil
	{
		public static ExpressionType ToExpressionType(Operator node)
		{
			ExpressionType expType;

			switch (node.Operation)
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
			default:
				throw new UnhandledCaseException(node.Operation.ToString());
			}
			return expType;
		}
		
	}
}
