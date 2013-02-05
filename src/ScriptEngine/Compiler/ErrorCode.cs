/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace Happy.ScriptEngine.Compiler
{
// ReSharper disable InconsistentNaming
	internal enum ErrorCode
	{
		UnexpectedEndOfInputInComment = 100,
		InvalidCharacter_Character_AsciiCode,
		UnexpectedToken_TokenText,
		Expected_Expected_Actual,
		NewLineInLiteralString,
		MultipartIdentiferNotAllowedInThisContext,
		DuplicateInputValue_Value,
		DefaultValueMayOnlyBeSpecifiedOnce,
		UnexpectedEndOfInputWhileParsingSection,
		InvalidEscapeSequence_InvalidCharacter,
		LiteralCharsMustBeOneCharLong,
		MismatchedCloseParen,
		MismatchedOpenParen,
		ExpectedOperator,
		InvalidContextForAssignmentOperator,
		AlreadyExistsInTheCurrentScope_Identifier,
		UndefinedVariable_Identifier,
		InvalidLValueForAssignment,
		MismatchedEndVerbatim,
		BeginTemplateStatementBlockNotAllowedHere,
		FunctionAlreadyDefined_FuntionName,
		EndTemplateNotAllowedHere,
		VerbatimOutputExpressionDelimiterNotAllowedHere,
		MismatchedVerbatimOutputExpressionDelimiter,
		OperatorIsNotUnary_Operator,
		OperatorIsNotBinary_Operator,
		SyntaxErrorInExpression,
		TooManyCharactersInHexLiteral,
		BreakNotAllowedHere,
		ContinueNotAllowedHere,
		DuplicateFunctionParameterName_Name,
		VariableOfSameNameDefinedInForLoopScope_Name,
		UseStatementDoesNotEvaluateToANamespace_Segment_Namespace,
		InvalidLiteral,
		VariableAlreadyDefined_Name,
		EndTemplateStatementBlockNotAllowedHere,
		CannotNestTemplatesWithinTemplateOutputExpression,
		SwitchCaseMissingBreak,
		DefaultCaseSpecifiedMoreThanOnce,
		WhereMayOnlyAppaearOnceInFor,
		BetweenMayOnlyAppearOnceInFor
	}
// ReSharper restore InconsistentNaming
}

