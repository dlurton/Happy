using System;
using HappyTemplate.Compiler;
using Microsoft.Scripting;
using NUnit.Framework;

namespace HappyTemplate.Tests
{
	[TestFixture]
	public class SemanticCheckingTests : TestFixtureBase
	{
		#region Lexer Error Conditions
		[Test]
		public void UnexpectedEndOfInputInStringLiteral()
		{
			ExpectCompileError(ErrorCode.UnexpectedEndOfInputWhileParsingSection, "def aVar = \"");
		}

		[Test] 
		public void FailIfEof_ShouldNotFail()
		{
			Lexer l = base.TestLexer("token");
			Token t = l.PeekToken(1);
			Assert.AreEqual(HappyTokenKind.EndOfInput, t.HappyTokenKind);
			
			//Should not fail
			l.FailIfEof(HappySourceLocation.None);
		}

		[Test]
		public void FailIfEof_ShouldFail()
		{
			Lexer l = base.TestLexer("token");
			Token t = l.NextToken();
			Assert.AreEqual(HappyTokenKind.Identifier, t.HappyTokenKind);

			//Should fail
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInputWhileParsingSection, () => l.FailIfEof(HappySourceLocation.None));
		}

		[Test]
		public void UnexpectedEndOfInputInComment()
		{
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInput, () => TestLexer("/*").NextToken());
		}

		#region Lexer State Transitions (ensure that $ is terminated by $ and |% by %| and <| by |>)
		[Test]
		public void MismatchedVerbatimOutputExpressionDelimiter()
		{
			Lexer lexer = TestLexer("<|test1$testb|>");
			NextTokenAssert(lexer, HappyTokenKind.BeginTemplate);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "test1");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "testb");

			AssertSyntaxErrorException(ErrorCode.MismatchedVerbatimOutputExpressionDelimiter, () => lexer.NextToken());
		}

		[Test]
		public void EndTemplateStatementBlockNotAllowedHere_WithinStatement()
		{
			AssertSyntaxErrorException(ErrorCode.EndTemplateStatementBlockNotAllowedHere, () => Console.WriteLine(TestLexer("%|").NextToken().Text));
		}

		[Test]
		public void EndTemplateStatementBlockNotAllowedHere_AfterVerbatimOutputExpressionDelimiter()
		{
			//Within a template, cannot close $ with %|
			Lexer l = TestLexer("<| $ %| |>");

			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			AssertSyntaxErrorException(ErrorCode.EndTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		

		[Test]
		public void BeginTemplateStatementBlockNotAllowedHere_WithinTemplate()
		{
			Lexer l = TestLexer("<||%|%");
			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			AssertSyntaxErrorException(ErrorCode.BeginTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		[Test]
		public void BeginTemplateStatementBlockNotAllowedHere_InStatement()
		{
			Lexer l = TestLexer("|%");
			AssertSyntaxErrorException(ErrorCode.BeginTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		[Test]
		public void VerbatimOutputExpressionDelimiterNotAllowedHere()
		{
			AssertSyntaxErrorException(ErrorCode.VerbatimOutputExpressionDelimiterNotAllowedHere, () => TestLexer("$").NextToken());
		}

		/// <summary>
		/// The doing this after a nested template is neccessary to ensure that
		/// the Lexer's state stack is handled correctly.
		/// </summary>
		[Test]
		public void VerbatimOutputExpressionDelimiterNotAllowedHere_AfterNestedTemplate()
		{
			Lexer l = TestLexer("<| |% <| |> $ |>");

			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.EndTemplate);

			AssertSyntaxErrorException(ErrorCode.VerbatimOutputExpressionDelimiterNotAllowedHere, () => l.NextToken());
		}

		#endregion


		[Test]
		public void CannotNestTemplatesWithinTemplateOutputExpression_AfterVerbatimOutputExpressionDelimiterAndNestedTemplate()
		{
			Lexer l = TestLexer("<| $ <| ");

			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.VerbatimOutputExpressionDelimiter);

			AssertSyntaxErrorException(ErrorCode.CannotNestTemplatesWithinTemplateOutputExpression, () => l.NextToken());
		}

		[Test]
		public void InvalidEscapeSequence_InvalidCharacter()
		{
			AssertSyntaxErrorException(ErrorCode.InvalidEscapeSequence_InvalidCharacter, () => TestLexer("\" \\q \"").NextToken());
		}

		[Test]
		public void NewLineInLiteralString()
		{
			AssertSyntaxErrorException(ErrorCode.NewLineInLiteralString, () => TestLexer("\" \n").NextToken());
		}

		[Test]
		public void LiteralCharsMustBeOneCharLong()
		{
			AssertSyntaxErrorException(ErrorCode.LiteralCharsMustBeOneCharLong, () => TestLexer("'ab'").NextToken());
		}

		[Test]
		public void InvalidCharacter()
		{
			AssertSyntaxErrorException(ErrorCode.InvalidCharacter_Character_AsciiCode, () => TestLexer("#").NextToken());
		}

		#endregion

		#region Parser Error Conditions

		#endregion

		#region Symbol Table Error Conditions
		[Test]
		public void GlobalVariableAlreadyDefined()
		{
			ExpectCompileError(ErrorCode.VariableAlreadyDefined_Name, "def aVar; def aVar;");
		}

		[Test]
		public void LocalVariableAlreadyDefined()
		{
			ExpectCompileError(ErrorCode.VariableAlreadyDefined_Name, "function aFunc() { def aVar; def aVar; }");
		}

		[Test]
		public void UseStatementDoesNotEvaluateToANamespace()
		{
			ExpectCompileError(ErrorCode.UseStatementDoesNotEvaluateToANamespace_Segment_Namespace, "load \"mscorlib\"; use System.Int32; ");
		}

		#endregion

		#region Sematnic Error Conditions

		[Test]
		public void DuplicateFunctionParameterName()
		{
			ExpectCompileError(ErrorCode.DuplicateFunctionParameterName_Name, "function testFunc(arga, argb, argb, arbc) { return true; }");
		}

		[Test]
		[Ignore("Not sure this error condition can ever be met.")]
		public void OperatorIsNotBinary()
		{
			ExpectCompileError(ErrorCode.OperatorIsNotBinary_Operator, "function testFunc() { return 1 ! 2; } ");
		}

		#endregion 


	}
}
