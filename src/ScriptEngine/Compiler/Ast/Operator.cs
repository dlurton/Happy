/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;
using Happy.ScriptEngine.Exceptions;

namespace Happy.ScriptEngine.Compiler.Ast
{
	internal class Operator : AstNodeBase
	{
		internal override AstNodeKind NodeKind { get { return AstNodeKind.Operator; } }
		readonly string _stringRep;
		internal override void Accept(AstVisitorBase visitor)
		{
			throw new InternalSourceException(this.Location, "Operators are not visited directly");
			//visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine(this.Operation.ToString());
		}

		public const int PrecedenceModifierStep = 1000;
		public OperationKind Operation;
		public int PrecedenceLevelModifier;
		public bool IsUnary
		{
			get 
			{
				switch(this.Operation)
				{
				case OperationKind.Not:
					return true;
				default:
					return false;
				}
			}
		}

		public int PrecedenceLevel
		{
			get
			{
				int retval;
				
				//TODO:  adjust this for partiy with Javascript's or C#'s operator precedence
				switch (this.Operation)
				{
				case OperationKind.MemberAccess:
					retval = 30;
					break;
				case OperationKind.Index:
					retval = 26;
					break;
				case OperationKind.Not:
					retval = 25;
					break;
				case OperationKind.Divide:
				case OperationKind.Multiply:
				case OperationKind.Mod:
					retval = 20;
					break;
				case OperationKind.Add:
				case OperationKind.Subtract:
					retval = 15;
					break;
				case OperationKind.Greater:
				case OperationKind.Less:
				case OperationKind.GreaterThanOrEqual:
				case OperationKind.LessThanOrEqual:
					retval = 10;
					break;
				case OperationKind.Equal:
				case OperationKind.NotEqual:
					retval = 7;
					break;
				case OperationKind.LogicalOr:
				case OperationKind.Xor:
				case OperationKind.LogicalAnd:
					retval = 5;
					break;
				case OperationKind.Assign:
					retval = 1;
					break;
				default:
					throw new UnhandledCaseSourceException(this.Location);
					//case OperationKind :	retval = ;	break;
					//case OperationKind :	retval = ;	break;
				}
				return retval + this.PrecedenceLevelModifier;
			}
		}
		
		public Operator(Token token)
			: base(token.Location)
		{
			_stringRep = token.Text;
			switch (token.HappyTokenKind)
			{
			case HappyTokenKind.OperatorAdd:
				this.Operation = OperationKind.Add;
				break;
			case HappyTokenKind.OperatorSubtract:
				this.Operation = OperationKind.Subtract;
				break;
			case HappyTokenKind.OperatorMultiply:
				this.Operation = OperationKind.Multiply;
				break;
			case HappyTokenKind.OperatorDivide:
				this.Operation = OperationKind.Divide;
				break;
			case HappyTokenKind.OperatorAssign:
				this.Operation = OperationKind.Assign;
				break;
			case HappyTokenKind.OperatorMod:
				this.Operation = OperationKind.Mod;
				break;
			case HappyTokenKind.OperatorEqual:
				this.Operation = OperationKind.Equal;
				break;
			case HappyTokenKind.OperatorNotEqual:
				this.Operation = OperationKind.NotEqual;
				break;
			case HappyTokenKind.OperatorGreaterThan:
				this.Operation = OperationKind.Greater;
				break;
			case HappyTokenKind.OperatorLessThan:
				this.Operation = OperationKind.Less;
				break;
			case HappyTokenKind.OperatorGreaterThanOrEqual:
				this.Operation = OperationKind.GreaterThanOrEqual;
				break;
			case HappyTokenKind.OperatorLessThanOrEqual:
				this.Operation = OperationKind.LessThanOrEqual;
				break;
			case HappyTokenKind.OperatorDot:
				this.Operation = OperationKind.MemberAccess;
				break;
			case HappyTokenKind.UnaryOperatorNot:
				this.Operation = OperationKind.Not;
				break;
			case HappyTokenKind.OperatorLogicalOr:
				this.Operation = OperationKind.LogicalOr;
				break;
			case HappyTokenKind.OperatorLogicalAnd:
				this.Operation = OperationKind.LogicalAnd;
				break;
			case HappyTokenKind.OperatorXor:
				this.Operation = OperationKind.Xor;
				break;
			case HappyTokenKind.OperatorBitwiseOr:
				this.Operation = OperationKind.BitwiseOr;
				break;
			case HappyTokenKind.OperatorBitwiseAnd:
				this.Operation = OperationKind.BitwiseAnd;
				break;
			case HappyTokenKind.OperatorOpenBracket:
				this.Operation = OperationKind.Index;
				break;
			default:
				throw new UnhandledCaseSourceException(token.Location, "HappyTokenKind." + token.HappyTokenKind);
			}
		}

		public override string ToString()
		{
			return _stringRep;
		}

		//public object Operate(object lvalue, object rvalue)
        //{
        //    return OmniOperate.Operate(this.Operation, lvalue, rvalue);
        //}
	}
}

