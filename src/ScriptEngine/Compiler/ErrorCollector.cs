/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using Happy.ScriptEngine.Compiler.Ast;
using Microsoft.Scripting;

namespace Happy.ScriptEngine.Compiler
{
	internal class ErrorCollector
	{
		readonly ErrorSink _sourceErrors;

		public ErrorCollector(ErrorSink sourceErrors)
		{
			_sourceErrors = sourceErrors;
		}

		void Add(HappySourceLocation loc, Enum message, params object[] args)
		{
			string messageName = message.ToString();

			string msgfmt = Resources.CompileErrorMessages.ResourceManager.GetString(messageName);
			DebugAssert.IsNotNull(msgfmt, "Error message \"{0}\" not found in resources", messageName.ToString());
			_sourceErrors.Add(loc.Unit, Util.Format(msgfmt, args), loc.Span, message.GetHashCode(), Severity.FatalError);
		}
		
		internal void UnexpectedEndOfInputInComment(HappySourceLocation loc)
		{
			this.Add(loc, ErrorCode.UnexpectedEndOfInputInComment);
		}

		public void InvalidCharacter(HappySourceLocation location, char c)
		{
			this.Add(location, ErrorCode.InvalidCharacter_Character_AsciiCode, c, (ushort)c);
		}

		public void UnexpectedToken(Token t) 
		{
			this.Add(t.Location, ErrorCode.UnexpectedToken_TokenText, t.Text);
		}

		public void Expected(string expected, Token actual)
		{
			this.Add(actual.Location, ErrorCode.Expected_Expected_Actual, expected, actual.Text);
		}

		public void NewLineInStringLiteral(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.NewLineInLiteralString);
		}

		public void MultipartIdentifierNotAllowedInThisContext(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.MultipartIdentiferNotAllowedInThisContext);
		}

		public void DuplicateInputValue(Token value)
		{
			this.Add(value.Location, ErrorCode.DuplicateInputValue_Value, value.Text);
		}

		public void DefaultValueMayOnlyBeSpecifiedOnce(HappySourceLocation location) 
		{
			this.Add(location, ErrorCode.DefaultValueMayOnlyBeSpecifiedOnce);
		}


		public void UnexpectedEndOfInputWhileParsingSection(HappySourceLocation location) 
		{
			this.Add(location, ErrorCode.UnexpectedEndOfInputWhileParsingSection);
		}	

		public void InvalidEscapeSequence(HappySourceLocation location, char invalidChar)
		{
			this.Add(location, ErrorCode.InvalidEscapeSequence_InvalidCharacter, invalidChar);
		}

		public void LiteralCharsMustBeOneCharLong(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.LiteralCharsMustBeOneCharLong);
		}

		public void MismatchedCloseParen(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.MismatchedCloseParen);
		}

		public void MismatchedOpenParen(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.MismatchedOpenParen);
		}

		public void ExpectedOperator(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.ExpectedOperator);
		}

		public void InvalidContextForAssignmentOperator(HappySourceLocation location) 
		{ 
			this.Add(location, ErrorCode.InvalidContextForAssignmentOperator);
		}

		public void UndefinedVariable(Identifier identifier)
		{
			this.Add(identifier.Location, ErrorCode.UndefinedVariable_Identifier, identifier.Text);
		}

		public void InvalidLValueForAssignment(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.InvalidLValueForAssignment);
		}

		public void MismatchedEndVerbatim(HappySourceLocation location) 
		{ 
			this.Add(location, ErrorCode.MismatchedEndVerbatim);
		}

		public void BeginTemplateStatementBlockNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.BeginTemplateStatementBlockNotAllowedHere);
		}

		public void FunctionAlreadyDefined(HappySourceLocation location, string name)
		{
			this.Add(location, ErrorCode.FunctionAlreadyDefined_FuntionName, name);
		}

		public void EndTemplateNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.EndTemplateNotAllowedHere);
		}

		public void VerbatimOutputExpressionDelimiterNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.VerbatimOutputExpressionDelimiterNotAllowedHere);
		}

		public void MismatchedVerbatimOutputExpressionDelimiter(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.MismatchedVerbatimOutputExpressionDelimiter);
		}

		public void OperatorIsNotUnary(HappySourceLocation location, string operatorString)
		{
			this.Add(location, ErrorCode.OperatorIsNotUnary_Operator, operatorString);
		}

		public void OperatorIsNotBinary(HappySourceLocation location, string operatorString)
		{
			this.Add(location, ErrorCode.OperatorIsNotBinary_Operator, operatorString);
		}

		public void SyntaxErrorInExpression(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.SyntaxErrorInExpression);
		}

		public void TooManyCharactersInHexLiteral(HappySourceLocation startedAt)
		{
			this.Add(startedAt, ErrorCode.TooManyCharactersInHexLiteral);
		}

		public void BreakNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.BreakNotAllowedHere);
		}

		public void ContinueNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.ContinueNotAllowedHere);
		}

		public void DuplicateFunctionParameterName(Identifier name)
		{
			this.Add(name.Location, ErrorCode.DuplicateFunctionParameterName_Name, name.Text);
		}

		public void VariableOfSameNameDefinedInForLoopScope(Identifier loopVariable)
		{
			this.Add(loopVariable.Location, ErrorCode.VariableOfSameNameDefinedInForLoopScope_Name, loopVariable.Text);
		}

		public void UseStatementDoesNotEvaluateToANamespace(UseStatement node, string segment)
		{
			this.Add(node.Location, ErrorCode.UseStatementDoesNotEvaluateToANamespace_Segment_Namespace, node, String.Join(".", node.NamespaceSegments), node.NamespaceSegments);
		}

		public void VariableAlreadyDefined(Identifier name)
		{
			this.Add(name.Location, ErrorCode.VariableAlreadyDefined_Name, name.Text);
		}

		public void EndTemplateStatementBlockNotAllowedHere(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.EndTemplateStatementBlockNotAllowedHere);
		}

		public void CannotNestTemplatesWithinTemplateOutputExpression(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.CannotNestTemplatesWithinTemplateOutputExpression);
		}

		public void SwitchCaseMissingBreak(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.SwitchCaseMissingBreak);
		}

		public void DefaultCaseSpecifiedMoreThanOnce(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.DefaultCaseSpecifiedMoreThanOnce);
		}

		public void WhereMayOnlyAppearOnceInFor(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.WhereMayOnlyAppaearOnceInFor);
		}

		public void BetweenMayOnlyAppearOnceInFor(HappySourceLocation location)
		{
			this.Add(location, ErrorCode.BetweenMayOnlyAppearOnceInFor);
		}
	}
}

