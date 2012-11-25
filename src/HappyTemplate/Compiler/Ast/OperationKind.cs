/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace HappyTemplate.Compiler.Ast
{
	public enum OperationKind
	{
		Assign,
		Add,
		Subtract,
		Divide,
		Multiply,
		Mod,
		LogicalAnd,
		LogicalOr,
		Xor,
		Not,
		Equal,
		Greater,
		Less,
		GreaterThanOrEqual,
		LessThanOrEqual,
		NotEqual,
		MemberAccess,
		BitwiseOr,
		BitwiseAnd,
		Index
	}
}

