namespace HappyTemplate.Compiler
{
// ReSharper disable InconsistentNaming
	internal enum ErrorCode
	{
		UnexpectedEndOfInput = 100,
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
		DefaultCaseSpecifiedMoreThanOnce
	}
// ReSharper restore InconsistentNaming
}