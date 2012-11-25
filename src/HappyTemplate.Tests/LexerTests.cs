/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/


using System;
using HappyTemplate.Compiler;

using Microsoft.Scripting;

using NUnit.Framework;

namespace HappyTemplate.Tests
{
	[TestFixture]
	public class LexerTests : TestFixtureBase
	{
		internal Lexer TestLexer(string text)
		{
			return new Lexer(LanguageContext.CreateSnippet(text, SourceCodeKind.AutoDetect), LanguageContext);
		}

		internal Lexer TestVerbatimLexer(string text)
		{
			Lexer retval = new Lexer(LanguageContext.CreateSnippet("<|" + text + "|>", SourceCodeKind.AutoDetect), LanguageContext);
			NextTokenAssert(retval, HappyTokenKind.BeginTemplate);
			Assert.AreEqual(Lexer.LexerMode.LexingVerbatimText, retval.Mode);
			return retval;
		}

		#region Lexer Error Conditions
		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void UnexpectedEndOfInputInStringLiteral()
		{
			AssertCompileErrorInModule(ErrorCode.UnexpectedEndOfInputWhileParsingSection, "def aVar = \"");
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void FailIfEof_ShouldNotFail()
		{
			Lexer l = TestLexer("token");
			Token t = l.PeekToken(1);
			Assert.AreEqual(HappyTokenKind.EndOfInput, t.HappyTokenKind);

			//Should not fail
			l.FailIfEof(HappySourceLocation.None);
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void FailIfEof_ShouldFail()
		{
			Lexer l = TestLexer("token");
			Token t = l.NextToken();
			Assert.AreEqual(HappyTokenKind.Identifier, t.HappyTokenKind);

			//Should fail
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInputWhileParsingSection, () => l.FailIfEof(HappySourceLocation.None));
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void UnexpectedEndOfInputInComment()
		{
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInputInComment, () => TestLexer("/*").NextToken());
		}

		#region Lexer State Transitions (ensure that $ is terminated by $ and |% by %| and <| by |>)
		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void MismatchedVerbatimOutputExpressionDelimiter()
		{
			Lexer lexer = TestLexer("<|test1$testb|>");
			NextTokenAssert(lexer, HappyTokenKind.BeginTemplate);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "test1");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "testb");

			AssertSyntaxErrorException(ErrorCode.MismatchedVerbatimOutputExpressionDelimiter, () => lexer.NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void EndTemplateStatementBlockNotAllowedHere_WithinStatement()
		{
			AssertSyntaxErrorException(ErrorCode.EndTemplateStatementBlockNotAllowedHere, () => Console.WriteLine(TestLexer("%|").NextToken().Text));
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void EndTemplateStatementBlockNotAllowedHere_AfterVerbatimOutputExpressionDelimiter()
		{
			//Within a template, cannot close $ with %|
			Lexer l = TestLexer("<| $ %| |>");

			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			AssertSyntaxErrorException(ErrorCode.EndTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void BeginTemplateStatementBlockNotAllowedHere_WithinTemplate()
		{
			Lexer l = TestLexer("<||%|%");
			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			AssertSyntaxErrorException(ErrorCode.BeginTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void BeginTemplateStatementBlockNotAllowedHere_InStatement()
		{
			Lexer l = TestLexer("|%");
			AssertSyntaxErrorException(ErrorCode.BeginTemplateStatementBlockNotAllowedHere, () => l.NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void VerbatimOutputExpressionDelimiterNotAllowedHere()
		{
			AssertSyntaxErrorException(ErrorCode.VerbatimOutputExpressionDelimiterNotAllowedHere, () => TestLexer("$").NextToken());
		}

		/// <summary>
		/// The doing this after a nested template is neccessary to ensure that
		/// the Lexer's state stack is handled correctly.
		/// </summary>
		[Test, Category(TestCategories.SyntaxErrorHandling)]
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


		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void CannotNestTemplatesWithinTemplateOutputExpression_AfterVerbatimOutputExpressionDelimiterAndNestedTemplate()
		{
			Lexer l = TestLexer("<| $ <| ");

			NextTokenAssert(l, HappyTokenKind.BeginTemplate);
			NextTokenAssert(l, HappyTokenKind.Verbatim);
			NextTokenAssert(l, HappyTokenKind.VerbatimOutputExpressionDelimiter);

			AssertSyntaxErrorException(ErrorCode.CannotNestTemplatesWithinTemplateOutputExpression, () => l.NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void InvalidEscapeSequence_InvalidCharacter()
		{
			AssertSyntaxErrorException(ErrorCode.InvalidEscapeSequence_InvalidCharacter, () => TestLexer("\" \\q \"").NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void NewLineInLiteralString()
		{
			AssertSyntaxErrorException(ErrorCode.NewLineInLiteralString, () => TestLexer("\" \n").NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void LiteralCharsMustBeOneCharLong()
		{
			AssertSyntaxErrorException(ErrorCode.LiteralCharsMustBeOneCharLong, () => TestLexer("'ab'").NextToken());
		}

		[Test, Category(TestCategories.SyntaxErrorHandling)]
		public void InvalidCharacter()
		{
			AssertSyntaxErrorException(ErrorCode.InvalidCharacter_Character_AsciiCode, () => TestLexer("#").NextToken());
		}

		#endregion

		[Test]
		public void LexSimpleVerbatim()
		{
			Lexer lexer = TestLexer("<|   $$verbatim$$   |>");

			NextTokenAssert(lexer, HappyTokenKind.BeginTemplate);
			Assert.AreEqual(Lexer.LexerMode.LexingVerbatimText, lexer.Mode);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "   $verbatim$   ");
			NextTokenAssert(lexer, HappyTokenKind.EndTemplate);
			Assert.AreEqual(Lexer.LexerMode.LexingStatement, lexer.Mode);
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);

			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		public void LexIdentifierWithoutWhitespace()
		{
			Lexer lexer = TestLexer("test");

			NextTokenAssert(lexer, HappyTokenKind.Identifier, "test");
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}


		[Test]
		public void LexIdentifierWithWhitespace()
		{
			Lexer lexer = TestLexer("    test   ");

			NextTokenAssert(lexer, HappyTokenKind.Identifier, "test");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}

		[Test]
		public void LexLiteralString()
		{
			Lexer lexer = TestLexer("    \"test\"   ");

			NextTokenAssert(lexer, HappyTokenKind.LiteralString, "test");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();

		}

		[Test]
		public void LexLiteralInt32()
		{
			Lexer lexer = TestLexer(" 123 -456 ");

			NextTokenAssert(lexer, HappyTokenKind.LiteralDecimalInt32, "123");
			NextTokenAssert(lexer, HappyTokenKind.LiteralDecimalInt32, "-456");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof); 

			AssertNoErrors();
		}

		[Test]
		public void LexLiteralIntAsHex()
		{
			Lexer lexer = TestLexer(" 0x12345678 0x123456789 0x1234567890123456");

			NextTokenAssert(lexer, HappyTokenKind.LiteralHexInt32, "12345678");
			NextTokenAssert(lexer, HappyTokenKind.LiteralHexInt64, "123456789");
			NextTokenAssert(lexer, HappyTokenKind.LiteralHexInt64, "1234567890123456");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void LexLiteralIntTooManyCharacters()
		{
			Lexer lexer = TestLexer("0x12345678901234567");
			lexer.NextToken();
		}

		[Test]
		public void LexLiteralChar()
		{
			Lexer lexer = TestLexer(" 'a' 'b' 'c' ");

			NextTokenAssert(lexer, HappyTokenKind.LiteralChar, "a");
			NextTokenAssert(lexer, HappyTokenKind.LiteralChar, "b");
			NextTokenAssert(lexer, HappyTokenKind.LiteralChar, "c");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		public void LexLiteralDouble()
		{
			Lexer lexer = TestLexer(" 123.456 456.789 ");

			NextTokenAssert(lexer, HappyTokenKind.LiteralDouble, "123.456");
			NextTokenAssert(lexer, HappyTokenKind.LiteralDouble, "456.789");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		public void LexLiteralNull()
		{
			Lexer lexer = TestLexer(" null ");
			NextTokenAssert(lexer, HappyTokenKind.LiteralNull);
		}

		[Test]
		public void LexLiteralStringWithEscapeSequences()
		{
			//" \" \t \n " \
			Lexer lexer = TestLexer("\" \\\" \\t \\n \\\\ \\r\"");

			NextTokenAssert(lexer, HappyTokenKind.LiteralString, " \" \t \n \\ \r");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}

		[Test]
		public void LexOperators()
		{
			Lexer lexer = TestLexer(" ( ) [ ] = { } ~ ; & && | || ^ + - * / % :");
			NextTokenAssert(lexer, HappyTokenKind.OperatorOpenParen);
			NextTokenAssert(lexer, HappyTokenKind.OperatorCloseParen);
			NextTokenAssert(lexer, HappyTokenKind.OperatorOpenBracket);
			NextTokenAssert(lexer, HappyTokenKind.OperatorCloseBracket);
			NextTokenAssert(lexer, HappyTokenKind.OperatorAssign);
			NextTokenAssert(lexer, HappyTokenKind.OpenBrace);
			NextTokenAssert(lexer, HappyTokenKind.CloseBrace);
			NextTokenAssert(lexer, HappyTokenKind.Output);
			NextTokenAssert(lexer, HappyTokenKind.EndOfStatement);
			NextTokenAssert(lexer, HappyTokenKind.OperatorBitwiseAnd);
			NextTokenAssert(lexer, HappyTokenKind.OperatorLogicalAnd);
			NextTokenAssert(lexer, HappyTokenKind.OperatorBitwiseOr);
			NextTokenAssert(lexer, HappyTokenKind.OperatorLogicalOr);
			NextTokenAssert(lexer, HappyTokenKind.OperatorXor);
			NextTokenAssert(lexer, HappyTokenKind.OperatorAdd);
			NextTokenAssert(lexer, HappyTokenKind.OperatorSubtract);
			NextTokenAssert(lexer, HappyTokenKind.OperatorMultiply);
			NextTokenAssert(lexer, HappyTokenKind.OperatorDivide);
			NextTokenAssert(lexer, HappyTokenKind.OperatorMod);
			NextTokenAssert(lexer, HappyTokenKind.Colon);
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
		}

		[Test]
		public void LexKeywords()
		{
			Lexer lexer = TestLexer(" if else for in between where def use load function break continue switch case default");
			NextTokenAssert(lexer, HappyTokenKind.KeywordIf);
			NextTokenAssert(lexer, HappyTokenKind.KeywordElse);
			NextTokenAssert(lexer, HappyTokenKind.KeywordFor);
			NextTokenAssert(lexer, HappyTokenKind.KeywordIn);
			NextTokenAssert(lexer, HappyTokenKind.KeywordBetween);
			NextTokenAssert(lexer, HappyTokenKind.KeywordWhere);
			NextTokenAssert(lexer, HappyTokenKind.KeywordDef);
			NextTokenAssert(lexer, HappyTokenKind.KeywordUse);
			NextTokenAssert(lexer, HappyTokenKind.KeywordLoad);
			NextTokenAssert(lexer, HappyTokenKind.KeywordFunction);
			NextTokenAssert(lexer, HappyTokenKind.KeywordBreak);
			NextTokenAssert(lexer, HappyTokenKind.KeywordContinue);
			NextTokenAssert(lexer, HappyTokenKind.KeywordSwitch);
			NextTokenAssert(lexer, HappyTokenKind.KeywordCase);
			NextTokenAssert(lexer, HappyTokenKind.KeywordDefault);

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			//Assert.IsTrue(lexer.Eof);
		}

		[Test]
		public void LexSimpleVerbatimAndStatement()
		{
			Lexer lexer = TestVerbatimLexer("verbatim$test$mitaberv");

			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "verbatim");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "test");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "mitaberv");
			NextTokenAssert(lexer, HappyTokenKind.EndTemplate);
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}

		[Test]
		public void LexSingleLineComments()
		{
			Lexer lexer = TestLexer("//some comment\nfoo//some more comments\nbar\n");
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "foo");
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "bar");
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		public void LexSingleLineCommentsEof()
		{
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInputInComment, () => TestLexer("\n\n  //").NextToken());
		}

		[Test]
		public void LexMultilineComments()
		{
			Lexer lexer = TestLexer("/* ignore *me**/foo/************************ignoreme*/ bar");
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "foo");
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "bar");
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);
			AssertNoErrors();
		}

		[Test]
		public void LexMultilineCommentsEof()
		{
			AssertSyntaxErrorException(ErrorCode.UnexpectedEndOfInputInComment, () => TestLexer("\n\n  /* ignore *me*").NextToken());
		}

		[Test]
		public void LexAnonymousTemplate()
		{
			Lexer lexer = TestLexer("<|test|>");

			NextTokenAssert(lexer, HappyTokenKind.BeginTemplate);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "test");
			NextTokenAssert(lexer, HappyTokenKind.EndTemplate);
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}

		[Test]
		public void LexAnonymousTemplateWithOutputExpression()
		{
			Lexer lexer = TestLexer("<|test1$testb$test2|>");

			NextTokenAssert(lexer, HappyTokenKind.BeginTemplate);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "test1");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Identifier, "testb");
			NextTokenAssert(lexer, HappyTokenKind.VerbatimOutputExpressionDelimiter);
			NextTokenAssert(lexer, HappyTokenKind.Verbatim, "test2");
			NextTokenAssert(lexer, HappyTokenKind.EndTemplate);
			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
		}

		

	}
}

