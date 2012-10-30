using System;
using HappyTemplate.Compiler;

using Microsoft.Scripting;

using NUnit.Framework;

namespace HappyTemplate.Tests
{
	[TestFixture]
	public class LexerTests : TestFixtureBase
	{
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
		public void LexCommments()
		{
			//0123456789012345678901234567890123456789012345678901234567890
			Lexer lexer = TestLexer("/* ignore me */ test/*ignoreme*/");

			NextTokenAssert(lexer, HappyTokenKind.Identifier, "test");

			NextTokenAssert(lexer, HappyTokenKind.EndOfInput);
			Assert.IsTrue(lexer.Eof);

			AssertNoErrors();
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