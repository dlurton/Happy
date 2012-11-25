/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using Microsoft.Scripting;

namespace HappyTemplate.Compiler
{
	public class Lexer
	{
		readonly InputReader _reader;
		readonly List<Token> _readAhead = new List<Token>();
		readonly ErrorCollector _errorCollector;
		readonly Dictionary<string, HappyTokenKind> _keywords = new Dictionary<string, HappyTokenKind>();

		readonly Stack<LexerState> _stateStack = new Stack<LexerState>();

		//public SourceUnit SourceUnit { get { return _reader.SourceUnit; } }
		

		class LexerState
		{
			public LexerMode Mode { get; private set; }
			public int VerbatimOuputExpressionDelimiterCount;
			
			public LexerState(LexerMode mode)
			{
				this.Mode = mode;
			}
		}

		public enum LexerMode
		{
			LexingVerbatimText,
			LexingStatement,
			LexingVerbatimOutputExpression
		}


		public LexerMode Mode { get { return _stateStack.Peek().Mode;  } }
		int VerbatimOuputExpressionDelimiterCount { get { return _stateStack.Peek().VerbatimOuputExpressionDelimiterCount; } set { _stateStack.Peek().VerbatimOuputExpressionDelimiterCount = value; } }


		internal Lexer(SourceUnit unit, HappyLanguageContext languageContext)
		{
			_reader = new InputReader(unit);
			_errorCollector = new ErrorCollector(languageContext.ErrorSink);
			PushState(LexerMode.LexingStatement);

			_keywords["if"] = HappyTokenKind.KeywordIf;
			_keywords["else"] = HappyTokenKind.KeywordElse;
			_keywords["while"] = HappyTokenKind.KeywordWhile;
			_keywords["for"] = HappyTokenKind.KeywordFor;
			_keywords["in"] = HappyTokenKind.KeywordIn;
			_keywords["between"] = HappyTokenKind.KeywordBetween;
			_keywords["where"] = HappyTokenKind.KeywordWhere;
			_keywords["lookup"] = HappyTokenKind.KeywordLookup;
			_keywords["default"] = HappyTokenKind.KeywordDefault;
			_keywords["true"] = HappyTokenKind.LiteralBool;
			_keywords["false"] = HappyTokenKind.LiteralBool;
			_keywords["return"] = HappyTokenKind.KeywordReturn;
			_keywords["def"] = HappyTokenKind.KeywordDef;
			_keywords["null"] = HappyTokenKind.LiteralNull;
			_keywords["load"] = HappyTokenKind.KeywordLoad;
			_keywords["use"] = HappyTokenKind.KeywordUse;
			_keywords["new"] = HappyTokenKind.KeywordNew;
			_keywords["function"] = HappyTokenKind.KeywordFunction;
			_keywords["break"] = HappyTokenKind.KeywordBreak;
			_keywords["continue"] = HappyTokenKind.KeywordContinue;
			_keywords["switch"] = HappyTokenKind.KeywordSwitch;
			_keywords["case"] = HappyTokenKind.KeywordCase;
		}

		public bool Eof
		{
			get
			{
				return this.PeekToken().HappyTokenKind == HappyTokenKind.EndOfInput || _reader.Eof;
			}
		}

		private void PushState(LexerMode ls)
		{
			_stateStack.Push(new LexerState(ls));
		}

		void PopState()
		{
			_stateStack.Pop();
			DebugAssert.IsNonZero(_stateStack.Count, "Last state was popped off state stack.  There should always be at least one state in the stack.");
		}


		public Token PeekToken()
		{
			return this.PeekToken(0);
		}
		public Token PeekToken(int lookAheadDepth)
		{
			while (_readAhead.Count <= lookAheadDepth)
				_readAhead.Add(this.ExtractToken());

			return _readAhead[lookAheadDepth];
		}

		public Token NextToken()
		{
			Token retval;
			if (_readAhead.Count > 0)
			{
				retval = _readAhead[0];
				_readAhead.RemoveAt(0);
			}
			else
			{
				return this.ExtractToken();
			}
			return retval;
		}

		private void ReadAppendWord(StringBuilder build)
		{
			while (Char.IsLetterOrDigit(_reader.Peek()) || _reader.Peek() == '_')
				build.Append(_reader.Read());
		}


		public void FailIfEof(HappySourceLocation startingLocation)
		{
			if (_reader.Eof)
			{
				foreach (Token t in _readAhead)
					if (t.HappyTokenKind != HappyTokenKind.EndOfInput)
						return;

				_errorCollector.UnexpectedEndOfInputWhileParsingSection(startingLocation);
			}

		}

		//public void FailIfEof()
		//{
		//    if (_reader.Eof)
		//    {
		//        foreach (Token t in _readAhead)
		//            if (t.HappyTokenKind != HappyTokenKind.EndOfInput)
		//                return;

		//        SourceSpan span = new SourceSpan(_enteredLastStatementGroupAt, _reader.CurrentLocation);
		//        if (_state == LexerState.LexingStatement)
		//            _errorCollector.UnexpectedEndOfInputWhileParsingSection(_reader.GetHappySourceLocation());
		//        else
		//            _errorCollector.UnexpectedEndOfInputInComment(_reader.GetHappySourceLocation());

		//        throw new AbortParseException(_reader.GetHappySourceLocation());
		//    }
		//}

		Token ExtractToken()
		{
			Token retval;

			_reader.ResetStartLocation();

			if (_reader.Eof)
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.EndOfInput, Resources.MiscMessages.EndOfInput);
			else
			{
				switch (this.Mode)
				{
				case LexerMode.LexingVerbatimText:
					retval = ExtractVerbatimText();
					break;
				case LexerMode.LexingVerbatimOutputExpression:
				case LexerMode.LexingStatement:
					//read until an end of statement is encountered
					retval = ExtractStatementToken();
					break;
				default:
					throw new UnhandledCaseSourceException(_reader.GetHappySourceLocation());
				}
			}

			DebugAssert.IsFalse(_reader.Peek() == 0xFFFF && !_reader.Eof, "_reader.Peek() returned 0xFFFF yet _reader.Eof is false");
			DebugAssert.IsNotNull(retval, "retval cannot be null");
			return retval;
		}

		Token ExtractStatementToken()
		{
			Token retval;
			this.EatWhite();
			if (_reader.Eof)
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.EndOfInput, Resources.MiscMessages.EndOfInput);
			else
			{
				if (Char.IsLetter(_reader.Peek()))
					retval = ExtractWord();
				else if (Char.IsNumber(_reader.Peek(0)) || (_reader.Peek(0) == '-' && Char.IsNumber(_reader.Peek(1))))
					retval = ExtractNumber();
				else 
					retval = ExtractOtherStatementToken();
			}
			return retval;
		}

		Token ExtractOtherStatementToken()
		{
			Token retval;
			//lexer state changes occur in the first few cases below.
			switch (_reader.Peek())
			{
			case '<':
				_reader.Read();
				switch (_reader.Peek(0))
				{
				case '=':
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorLessThanOrEqual, "<=");
					break;
				case '|':
					_reader.Read();

					if(this.Mode == LexerMode.LexingVerbatimOutputExpression)
						_errorCollector.CannotNestTemplatesWithinTemplateOutputExpression(_reader.GetHappySourceLocation());

					this.PushState(LexerMode.LexingVerbatimText);
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.BeginTemplate, "<|");
					break;
				default:
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorLessThan, "<");
					break;
				}
				break;
			case '%':
				_reader.Read();
				if (_reader.Peek(0) != '|')
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorMod, "%");
				else
				{
					_reader.Read();

					if(_stateStack.Count <= 1 || this.Mode == LexerMode.LexingVerbatimOutputExpression)
						_errorCollector.EndTemplateStatementBlockNotAllowedHere(_reader.GetHappySourceLocation());

					this.PopState();

					//I don't believe this case can ever happen...
					//if(this.Mode != LexerMode.LexingVerbatimText)
					//    _errorCollector.EndTemplateStatementBlockNotAllowedHere(_reader.GetHappySourceLocation());
					DebugAssert.AreEqual(this.Mode, LexerMode.LexingVerbatimText, "Unexpected case.  Debug me.");

					retval = this.ExtractToken();
				}
				break;
			case '|':
				_reader.Read();
				switch (_reader.Peek(0))
				{	
				case '>':

					if (this.Mode == LexerMode.LexingVerbatimOutputExpression)
						_errorCollector.MismatchedVerbatimOutputExpressionDelimiter(_reader.GetHappySourceLocation());

					//Do not PopState here as it is already popped in ExtractVerbatimText()
					//this.PopState();
					
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.EndTemplate, "|>");
					
					break;
				case '%':
					_reader.Read();
					_errorCollector.BeginTemplateStatementBlockNotAllowedHere(_reader.GetHappySourceLocation());
					throw new AbortParseException(_reader.GetHappySourceLocation());
				case '|':
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorLogicalOr, "||");
					break;
				default:
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorBitwiseOr, "|");
					break;
				}
				break;
			case '$':
				//First time this executes is the next extraction after the switch from LexingVerbatimText.
				//The state should just have been changed to LexingVerbatimOutputExpression
				//If not, we're in a state (LexingStatement, currently) where $ is not allowed.
				if(this.Mode != LexerMode.LexingVerbatimOutputExpression)
					_errorCollector.VerbatimOutputExpressionDelimiterNotAllowedHere(_reader.GetHappySourceLocation());

				_reader.Read();
				this.VerbatimOuputExpressionDelimiterCount++;
				if (this.VerbatimOuputExpressionDelimiterCount >= 2)
				{
					this.PopState();
					DebugAssert.AreEqual(LexerMode.LexingVerbatimText, this.Mode, "Previous lexer mode was not LexingVerbatimText.  That shoulnd't have happened.");
				}

				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.VerbatimOutputExpressionDelimiter, "$");
				break;
			case ';':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.EndOfStatement, ";");
				break;
			case '(':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorOpenParen, "(");
				break;
			case ')':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorCloseParen, ")");
				break;
			case '!':
				_reader.Read();
				if (_reader.Peek(0) == '=')
				{
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorNotEqual, "!=");
					_reader.Read();
				}
				else
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.UnaryOperatorNot, "!");
				break;
			case '&':
				_reader.Read();
				if(_reader.Peek() == '&')
				{
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorLogicalAnd, "&&");
				}
				else
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorBitwiseAnd, "&");
				break;
			case '^':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorXor, "^");
				break;
			case ',':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.Comma, ",");
				break;
			case '.':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorDot, ".");
				break;
			case '[':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorOpenBracket, "[");
				break;
			case ']':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorCloseBracket, "]");
				break;
			case '{':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OpenBrace, "{");
				break;
			case '}':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.CloseBrace, "}");
				break;
			case '>':
				_reader.Read();
				if (_reader.Peek(0) == '=')
				{
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorGreaterThanOrEqual, ">=");
				}
				else
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorGreaterThan, ">");
				break;
			case '=':
				_reader.Read();
				if (_reader.Peek(0) == '=')
				{
					_reader.Read();
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorEqual, "==");
				}
				else
					retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorAssign, "=");
				break;
			case '+':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorAdd, "+");
				break;
			case '-':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorSubtract, "-");
				break;
			case '/':
				_reader.Read();
				//if (_reader.Peek(0) == '*')
				//    do
				//        _reader.Read();
				//    while(_reader.Peek(0) != '*' && _reader.Peek(1) != '/');
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorDivide, "/");
				break;
			case '*':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.OperatorMultiply, "*");
				break;
			case '~':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.Output, "~");
				break;
			case ':':
				_reader.Read();
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.Colon, ":");
				break;
			case '\'':
				_reader.Read();
				this.FailIfEof(_reader.GetHappySourceLocation());
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.LiteralChar, _reader.Read().ToString());
				this.FailIfEof(_reader.GetHappySourceLocation());
				if (_reader.Read() != '\'')
					_errorCollector.LiteralCharsMustBeOneCharLong(_reader.GetHappySourceLocation());
				break;
			case '"':
				retval = ExtractLiteralString();
				break;
			default:
				char invalidChar = _reader.Read();
				_errorCollector.InvalidCharacter(_reader.GetHappySourceLocation(), invalidChar);
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.InvalidCharacter, Resources.MiscMessages.InvalidCharacter);
				break;
			}
			return retval;
		}


		Token	ExtractLiteralString()
		{
			StringBuilder sb;
			Token retval;
			sb = new StringBuilder();
			//Eat opening quote
			_reader.Read();
			while (_reader.Peek() != '"')
			{
				this.FailIfEof(_reader.GetHappySourceLocation());
				if (_reader.Peek() == '\\')
				{
					_reader.Read();
					char escapedChar = _reader.Read();
					switch (escapedChar)
					{
					case 't':
						sb.Append('\t');
						break;
					case 'r':
						sb.Append('\r');
						break;
					case 'n':
						sb.Append('\n');
						break;
					case '"':
						sb.Append('"');
						break;
					case '\\':
						sb.Append('\\');
						break;
					default:
						_errorCollector.InvalidEscapeSequence(_reader.GetHappySourceLocation(), escapedChar);
						break;
					}
				}
				else
					sb.Append(_reader.Read());

				if (_reader.Peek() == '\n')
					_errorCollector.NewLineInStringLiteral(_reader.GetHappySourceLocation());
									
			}
			//Eat closing quote
			_reader.Read();

			retval = new Token(_reader.GetHappySourceLocation(),
			                   HappyTokenKind.LiteralString, sb.ToString());
			return retval;
		}

		Token ExtractNumber()
		{
			StringBuilder sb;
			Token retval = null;
			HappySourceLocation startedAt = _reader.GetHappySourceLocation();
			sb = new StringBuilder();
			if(_reader.Peek(0) == '0' && _reader.Peek(1) == 'x')
			{
				_reader.Read();
				_reader.Read();

				char peeked;
				do
				{
					sb.Append(_reader.Read());
					peeked = _reader.Peek();
				} while (Char.IsNumber(peeked) || (peeked >= 'a' && peeked <= 'z' || peeked >='A' && peeked <='Z'));
				if(sb.Length <= 8)
					retval = new Token(startedAt, HappyTokenKind.LiteralHexInt32, sb.ToString());
				else if(sb.Length <= 16)
					retval = new Token(startedAt, HappyTokenKind.LiteralHexInt64, sb.ToString());
				else
					_errorCollector.TooManyCharactersInHexLiteral(startedAt);
			}
			else
			{
								
				do
				{
					sb.Append(_reader.Read());
				}
				while(Char.IsNumber(_reader.Peek(0)) || _reader.Peek(0) == '.');

				string aNumber = sb.ToString();

				if(aNumber.IndexOf('.') < 0)
					retval = new Token(startedAt, HappyTokenKind.LiteralDecimalInt32, aNumber);
				else
					retval = new Token(startedAt, HappyTokenKind.LiteralDouble, aNumber);
			}
			return retval;
		}

		Token ExtractWord()
		{
			StringBuilder sb;
			Token retval;
			sb = new StringBuilder();
			this.ReadAppendWord(sb);
			string word = sb.ToString();

			HappyTokenKind maybeKeyword;
			if (_keywords.TryGetValue(word, out maybeKeyword))
				retval = new Token(_reader.GetHappySourceLocation(), maybeKeyword, word);
			else
				retval = new Token(_reader.GetHappySourceLocation(), HappyTokenKind.Identifier, word);

			return retval;
		}

		Token ExtractVerbatimText()
		{
			StringBuilder sb;
			Token retval;
			bool keepReading = true;
			//read until a statement begin or end of template is encountered
			sb = new StringBuilder();

			while (keepReading)
			{
				char peeked = _reader.Peek();
				char peeked1 = _reader.Peek(1);
				if(peeked == '$')
				{
					if (peeked1 == '$')
					{
						_reader.Read();
						_reader.Read();
						sb.Append('$');
					}
					else
					{
						//Leave $ in the buffer so that it can be eaten next round.
						this.PushState(LexerMode.LexingVerbatimOutputExpression);
						keepReading = false;
					}
				}
				else if(peeked == '|' && (peeked1 == '%' || peeked1 == '>'))
				{
					keepReading = false;
					switch(peeked1)
					{
					case '%':
						_reader.Read();
						_reader.Read();
						this.PushState(LexerMode.LexingStatement);
						break;
					case '>':
						//Purposfully do NOT eat |> as it will be eaten in the LextingStatement state.
						this.PopState();
						break;
					}
				}
				else
				{
					if(_reader.Eof)
						_errorCollector.UnexpectedEndOfInputWhileParsingSection(_reader.GetHappySourceLocation());

					sb.Append(_reader.Read());
				}
			}

			//Entirely skip empty Verbatim sections; just parse the next token
			retval = sb.Length == 0 ? this.ExtractToken() : new Token(_reader.GetHappySourceLocation(), HappyTokenKind.Verbatim, sb.ToString());
			return retval;
		}

		private void EatWhite()
		{
			bool eatMore = true;

			while (eatMore)
			{
				if(_reader.PeekIsWhite())
					_reader.Read();
				else
				{
					_reader.ResetStartLocation();
					if(_reader.Peek(0) == '/')
					{
						char peek1 = _reader.Peek(1);
						//Eat up multiline comments.
						if(peek1 == '*')
						{
							//Eat the /*
							_reader.Read();
							_reader.Read();

							//Eat the text of the comment
							while(!(_reader.Peek(0) == '*' && _reader.Peek(1) == '/'))
							{
								_reader.Read();
								if(_reader.Eof)
									_errorCollector.UnexpectedEndOfInputInComment(_reader.GetHappySourceLocation());
							}

							//Eat the */
							_reader.Read();
							_reader.Read();
						}
						else if(peek1 == '/')
						{
							//Eat the //
							_reader.Read();
							_reader.Read();

							//Eat until the end of line
							while(_reader.Peek() != '\n')
							{
								_reader.Read();
								if(_reader.Eof)
									_errorCollector.UnexpectedEndOfInputInComment(_reader.GetHappySourceLocation());
							}
						}
						else
							eatMore = false;
					}
					else
						eatMore = false;
				}
			}
		}
	}
}

