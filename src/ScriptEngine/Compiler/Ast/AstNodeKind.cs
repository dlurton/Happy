/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace Happy.ScriptEngine.Compiler.Ast
{
	//TO DO:  make non-public (can be done after AstNodeBase is made non-public)
	public enum AstNodeKind
	{
		BinaryExpression,
		WhileStatement,
		ForStatement,
		ForWhereClause,
		IfStatement,
		LiteralExpression,
		NullExpression,
		Operator,
		SetStatement,
		Template,
		FunctionCallExpression,
		TemplateParameter,
		UnexpectedToken,
		IdentifierExpression,
		VerbatimSection,
		AnonymousTemplate,
		OutputWrite,
		ReturnStatement,
		VariableDef,
		DefStatement,
		LoadStatement,
		UseStatement,
		NewObjectExpression,
		UnaryExpression,
		ArgumentList,
		BreakStatement,
		ContinueStatement,
		Module,
		BlockStatement,
		FunctionParameterList,
		UseStatementList,
		SwitchStatement,
		SwitchCase,
	}
}

