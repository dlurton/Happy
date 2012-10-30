using System;
using System.Collections.Generic;
using System.Globalization;
using HappyTemplate.Exceptions;
using HappyTemplate.Resources;
using HappyTemplate.Runtime;
using Microsoft.Scripting;
using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler
{
	public class Parser
	{
		readonly Lexer _lexer;
		readonly ErrorCollector _errorCollector;

		internal Parser(Lexer lexer, HappyLanguageContext languageContext)
		{
			_lexer = lexer;
			_errorCollector = new ErrorCollector(languageContext.ErrorSink);
		}

		public static Module ParseModule(SourceUnit sourceUnit, HappyLanguageContext languageContext)
		{
			Lexer lexer = new Lexer(sourceUnit, languageContext);
			Parser parser = new Parser(lexer, languageContext);
			Module retval = null;
			try
			{
				retval = parser.Parse();
			}
			catch(AbortParseException )
			{
				//Left blank intentionally because AbortParseExceptions should never
				//be allowed to bubble up to the caller.
			}
			return retval;
		}


		internal Module Parse()
		{
			List<Function> functions = new List<Function>();
			List<DefStatement> globalDefStmts = new List<DefStatement>();
			List<LoadDirective> loadStatements = new List<LoadDirective>();
			List<UseStatement> useStatements = new List<UseStatement>();

			while(!_lexer.Eof)
			{
				switch(this._lexer.PeekToken().HappyTokenKind)
				{
				case HappyTokenKind.KeywordDef:
					globalDefStmts.Add(this.ParseDef());
					break;
				case HappyTokenKind.KeywordLoad:
					loadStatements.Add(this.ParseLoadStatement());
					break;
				case HappyTokenKind.KeywordUse:
					useStatements.Add(this.ParseUseStatement());
					break;
				case HappyTokenKind.KeywordFunction:
					functions.Add(this.ParseFunction());
					break;
				default:
					this._errorCollector.UnexpectedToken(this._lexer.PeekToken());
					break;
				}
			}

			return new Module(loadStatements.ToArray(), new UseStatementList(useStatements.ToArray()), globalDefStmts.ToArray(), functions.ToArray());
		}

		LoadDirective ParseLoadStatement()
		{
			this.AssertExpect(HappyTokenKind.KeywordLoad);
			Token assemblyName = this.Expect(HappyTokenKind.LiteralString, MiscMessages.LiteralString);
			this.ExpectEOS();
			return new LoadDirective(assemblyName.Location, assemblyName.Text);
		}

		UseStatement ParseUseStatement()
		{
			Token useKeyword = this.AssertExpect(HappyTokenKind.KeywordUse);
			List<string> namespaceSegments = new List<string>();

			do
			{
				namespaceSegments.Add(this.Expect(HappyTokenKind.Identifier, MiscMessages.AnIdentifier).Text);
			} while (this.EatNextTokenIf(HappyTokenKind.OperatorDot));
			this.ExpectEOS();
			return new UseStatement(useKeyword.Location, namespaceSegments.ToArray());
		}

		Function ParseFunction()
		{
			this.AssertExpect(HappyTokenKind.KeywordFunction);
			
			Token identifier = this.Expect(HappyTokenKind.Identifier, MiscMessages.TemplateIdentifier);
			List<FunctionParameter> parameters = new List<FunctionParameter>();

			FunctionParameterList parameterList = ParseParameterList(parameters);

			StatementBlock stmts = this.ParseBlock();
			return new Function(identifier.ToIdentifier(), parameterList, stmts);
		}

		FunctionParameterList ParseParameterList(List<FunctionParameter> parameters) 
		{
			HappySourceLocation sectionStartsAt = this.Expect(HappyTokenKind.OperatorOpenParen, "(").Location;
			//Parse parameters
			bool parseMore = !this.EatNextTokenIf(HappyTokenKind.OperatorCloseParen);
			while (parseMore)
			{
				_lexer.FailIfEof(sectionStartsAt);
				Token tmp = this.Expect(HappyTokenKind.Identifier, MiscMessages.ParameterIdentifier);
				parameters.Add(new FunctionParameter(tmp.ToIdentifier()));

				if (this.Expect(HappyTokenKind.OperatorCloseParen, HappyTokenKind.Comma, MiscMessages.CommaOrCloseParen).HappyTokenKind == HappyTokenKind.OperatorCloseParen)
					parseMore = false;
			}
			return new FunctionParameterList(sectionStartsAt, parameters.ToArray());
		}


		AstNodeBase ParseStatement()
		{
			Token temp;
			AstNodeBase retval;
			Token peekedToken = _lexer.PeekToken();
			switch (peekedToken.HappyTokenKind)
			{
			case HappyTokenKind.KeywordIf:
				retval = this.ParseIf();
				break;
			case HappyTokenKind.KeywordWhile:
				retval = this.ParseWhile();
				break;
			case HappyTokenKind.KeywordFor:
				retval = this.ParseFor();
				break;
			case HappyTokenKind.Verbatim:
				temp = _lexer.NextToken();
				retval = new VerbatimSection(temp.Location, temp.Text);
				break;
			case HappyTokenKind.KeywordReturn:
				retval = ParseReturn();
				this.ExpectEOS();
				break;
			case HappyTokenKind.KeywordDef:
				retval = ParseDef();
				break;
			case HappyTokenKind.Output:
				retval = this.ParseOutputExpression();
				break;
			case HappyTokenKind.KeywordBreak:
				_lexer.NextToken();
				retval = new BreakStatement(peekedToken.Location);
				this.ExpectEOS();
				break;
			case HappyTokenKind.KeywordContinue:
				_lexer.NextToken();
				retval = new ContinueStatement(peekedToken.Location);
				this.ExpectEOS();
				break;
			case HappyTokenKind.Identifier:
			case HappyTokenKind.LiteralString:
			case HappyTokenKind.LiteralDecimalInt32:
			case HappyTokenKind.LiteralBool:
			case HappyTokenKind.LiteralDouble:
			case HappyTokenKind.OperatorOpenParen:
				retval = this.ParseExpression(ExpressionContext.Expression);
				this.ExpectEOS();
				break;
			case HappyTokenKind.BeginTemplate:
				retval = this.ParseExpression(ExpressionContext.Expression);
				break;
			case HappyTokenKind.VerbatimOutputExpressionDelimiter:
				{
					_lexer.NextToken();
					ExpressionNodeBase exp = this.ParseExpression(ExpressionContext.Expression);
					retval = new OutputStatement(exp.Location, new[] { exp });
					this.Expect(HappyTokenKind.VerbatimOutputExpressionDelimiter, "$");
				}
				break;
			case HappyTokenKind.KeywordSwitch:
				retval = this.ParseSwitch();
				break;
			default:
				temp = _lexer.NextToken();
				retval = new UnexpectedToken(temp.Location);
				_errorCollector.UnexpectedToken(temp);
				break;
			}	
			DebugAssert.IsNotNull(retval, "return value is null");
			return retval;
		}

		AstNodeBase ParseReturn()
		{
			AstNodeBase retval;
			HappySourceLocation location = this.AssertExpect(HappyTokenKind.KeywordReturn).Location;
			ExpressionNodeBase returnExpression = null;

			if(_lexer.PeekToken().HappyTokenKind != HappyTokenKind.EndOfStatement)
				returnExpression = this.ParseExpression(ExpressionContext.Expression);
			
			retval = new ReturnStatement(location, returnExpression);
			return retval;
		}


		Token Expect(HappyTokenKind tokenType1, HappyTokenKind tokenType2, string whatWasExpected)
		{
			Token next = _lexer.NextToken();
			if (next.HappyTokenKind == tokenType1 || next.HappyTokenKind == tokenType2)
				return next;

			_errorCollector.Expected(whatWasExpected, next);
			throw new AbortParseException(next.Location);
		}

		Token Expect(HappyTokenKind tokenKind, string whatWasExpected)
		{
			Token next = _lexer.NextToken();
			if(next.HappyTokenKind == tokenKind) 
				return next;

			_errorCollector.Expected(whatWasExpected, next);
			throw new AbortParseException(next.Location);
		}

		void ExpectEOS()
		{
			Expect(HappyTokenKind.EndOfStatement, ";");
		}

		Token AssertExpect(HappyTokenKind tokenKind)
		{
			Token next = _lexer.NextToken();
			if (next.HappyTokenKind != tokenKind)
				throw new InternalSourceException(next.Location, "Assertion Failed:  expected HappyTokenKind." + tokenKind + " but found HappyTokenKind." + next.HappyTokenKind);
			return next;
		}

		bool EatNextTokenIf(HappyTokenKind tokenKind)
		{
			if (_lexer.PeekToken().HappyTokenKind == tokenKind)
			{
				_lexer.NextToken();
				return true;
			}

			return false;
		}

		bool EatNextTokenIf(HappyTokenKind tokenKind, out HappySourceLocation location)
		{
			if (_lexer.PeekToken().HappyTokenKind == tokenKind)
			{
				location = _lexer.NextToken().Location;
				return true;
			}

			location = HappySourceLocation.None;
			return false;
		}

		DefStatement ParseDef()
		{
			Token defKeyword = this.AssertExpect(HappyTokenKind.KeywordDef);
			List<VariableDef> variableDefs = new List<VariableDef>();
			do
			{
				Token ident = Expect(HappyTokenKind.Identifier, Resources.MiscMessages.AnIdentifier);
				ExpressionNodeBase expressionNode =
					this.EatNextTokenIf(HappyTokenKind.OperatorAssign) ? this.ParseExpression(ExpressionContext.Expression) : null;

				variableDefs.Add(new VariableDef(ident.ToIdentifier(), expressionNode));

			} while (EatNextTokenIf(HappyTokenKind.Comma));
			this.ExpectEOS();

			return new DefStatement(defKeyword.Location, variableDefs.ToArray());
		}

		StatementBlock ParseBlock()
		{
			List<AstNodeBase> block = new List<AstNodeBase>();
			HappySourceLocation startsAt;
			if(_lexer.PeekToken().HappyTokenKind != HappyTokenKind.OpenBrace)
			{
				AstNodeBase stmt = this.ParseStatement();
				startsAt = stmt.Location;
				block.Add(stmt);
			}
			else
			{
				startsAt = _lexer.NextToken().Location;
				while(_lexer.PeekToken().HappyTokenKind != HappyTokenKind.CloseBrace)
				{
					block.Add(this.ParseStatement());
				}
				//Eat trailing }
				_lexer.NextToken();
			}
			return new StatementBlock(startsAt, block.ToArray());
		}

		IfStatement ParseIf()
		{
			this.Expect(HappyTokenKind.KeywordIf, "if");
			this.Expect(HappyTokenKind.OperatorOpenParen, "(");
			ExpressionNodeBase value = this.ParseExpression(ExpressionContext.NestedParens);
			this.Expect(HappyTokenKind.OperatorCloseParen, ")");

			StatementBlock trueStatementBlock = this.ParseBlock();
			StatementBlock falseStatementBlock = null;
			
			if (EatNextTokenIf(HappyTokenKind.KeywordElse))
				falseStatementBlock = this.ParseBlock();

			return new IfStatement(value, trueStatementBlock, falseStatementBlock);
		}

		WhileStatement ParseWhile()
		{
			var location = this.Expect(HappyTokenKind.KeywordWhile, "while").Location;
			this.Expect(HappyTokenKind.OperatorOpenParen, "(");
			ExpressionNodeBase value = this.ParseExpression(ExpressionContext.NestedParens);
			this.Expect(HappyTokenKind.OperatorCloseParen, ")");
			StatementBlock block = this.ParseBlock();
			return new WhileStatement(location, value, block);
		}

		ForStatement ParseFor()
		{
			this.AssertExpect(HappyTokenKind.KeywordFor);
			this.Expect(HappyTokenKind.OperatorOpenParen, "(");
			Token loopIdentifier = this.Expect(HappyTokenKind.Identifier, MiscMessages.AnIdentifier);
			this.Expect(HappyTokenKind.KeywordIn, "in");
			ExpressionNodeBase enumerable = this.ParseExpression(ExpressionContext.NestedParens);
			ExpressionNodeBase between = null;
			ExpressionNodeBase where = null;

			if(this.EatNextTokenIf(HappyTokenKind.KeywordWhere))
				where = this.ParseExpression(ExpressionContext.NestedParens);

			if(this.EatNextTokenIf(HappyTokenKind.KeywordBetween))
				between = this.ParseExpression(ExpressionContext.NestedParens);
			
			this.Expect(HappyTokenKind.OperatorCloseParen, ")");

			StatementBlock statementBlock = this.ParseBlock();

			return new ForStatement(loopIdentifier.Location, loopIdentifier.ToIdentifier(),
			                            enumerable, between, where, statementBlock);
		}


		AstNodeBase ParseSwitch()
		{
			StatementBlock defaultStatementBlock = null;
			this.AssertExpect(HappyTokenKind.KeywordSwitch);
			this.Expect(HappyTokenKind.OperatorOpenParen, "(");

			ExpressionNodeBase switchExpr = this.ParseExpression(ExpressionContext.ArgumentList);
			this.Expect(HappyTokenKind.OperatorCloseParen, ")");

			this.Expect(HappyTokenKind.OpenBrace, "{");

			List<SwitchCase> cases = new List<SwitchCase>();

			while(_lexer.PeekToken().HappyTokenKind != HappyTokenKind.CloseBrace)
			{
				defaultStatementBlock = ParseSwitchCase(defaultStatementBlock, cases);
			}

			this.AssertExpect(HappyTokenKind.CloseBrace);

			return new SwitchStatement(switchExpr, cases.ToArray(), defaultStatementBlock);
		}

		StatementBlock ParseSwitchCase(StatementBlock defaultStatementBlock, List<SwitchCase> cases)
		{
			List<ExpressionNodeBase> caseValues = new List<ExpressionNodeBase>();
			List<AstNodeBase> statements = new List<AstNodeBase>();
			Token keyword = this.Expect(HappyTokenKind.KeywordCase, HappyTokenKind.KeywordDefault, "case or default");
			if (keyword.HappyTokenKind == HappyTokenKind.KeywordCase)
			{
				do
				{
					caseValues.Add(this.ParseExpression(ExpressionContext.Expression));
					this.Expect(HappyTokenKind.Colon, ":");
				} while(this.EatNextTokenIf(HappyTokenKind.KeywordCase));
			}
			else
			{
				this.Expect(HappyTokenKind.Colon, ":");
				if (defaultStatementBlock != null)
					_errorCollector.DefaultCaseSpecifiedMoreThanOnce(keyword.Location);
			}

			while(_lexer.PeekToken().HappyTokenKind != HappyTokenKind.KeywordBreak)
			{
				statements.Add(this.ParseStatement());

				if (_lexer.PeekToken().HappyTokenKind == HappyTokenKind.CloseBrace)
					_errorCollector.SwitchCaseMissingBreak(_lexer.PeekToken().Location);
			}

			this.AssertExpect(HappyTokenKind.KeywordBreak);
			this.Expect(HappyTokenKind.EndOfStatement, ";");

			if(keyword.HappyTokenKind == HappyTokenKind.KeywordCase)
				cases.Add(new SwitchCase(caseValues.ToArray(), new StatementBlock(keyword.Location, statements.ToArray())));
			else
				defaultStatementBlock = new StatementBlock(keyword.Location, statements.ToArray());
			return defaultStatementBlock;
		}

		private enum ExpressionContext
		{
			ArgumentList,
			Indexer,
			NestedParens,
			Expression
		}

		private ExpressionNodeBase ParseExpression(ExpressionContext context)
		{
			ExpressionNodeBase retval;
			Stack<AstNodeBase> expStack = new Stack<AstNodeBase>();
			Stack<Operator> operatorStack = new Stack<Operator>();
			int parenCount = 0;

			Token currToken = _lexer.PeekToken();
			HappySourceLocation startLocation = currToken.Location;

			Action<Operator> insertOperatorAndUpdateStacks = (oper) =>
			                                           {
			                                           	while (operatorStack.Count > 0 &&
			                                           	       operatorStack.Peek().PrecedenceLevel >= oper.PrecedenceLevel)
			                                           		expStack.Push(operatorStack.Pop());
			                                           	operatorStack.Push(oper);
			                                           };

			if (currToken.HappyTokenKind == HappyTokenKind.LiteralNull)
			{
				_lexer.NextToken();
				retval = this.PromoteToExpression(currToken);
			}
			else
			{
				//Rules for existing the while loop below:
				//  Always stop parsing when a keyword other than new is encountered.
				//  If the languageContext is argument list, stop parsing when "," or ")" that is not matched by a corresponding "(" is encountered 
				//  if the languageContext is nested parens, stop parsing when ")" that is not matched by a corresponding "(" is encountered
				//  if the languageContext is new statement, stop parsing when "(" is encountered
				while (
					(!currToken.IsKeyword || currToken.HappyTokenKind == HappyTokenKind.KeywordNew)
					&& !(context == ExpressionContext.ArgumentList && currToken.HappyTokenKind == HappyTokenKind.Comma)
					&& !((context == ExpressionContext.NestedParens || context == ExpressionContext.ArgumentList) && parenCount == 0 && currToken.HappyTokenKind == HappyTokenKind.OperatorCloseParen)
					&& !(context == ExpressionContext.Indexer && (currToken.HappyTokenKind == HappyTokenKind.OperatorCloseBracket || currToken.HappyTokenKind == HappyTokenKind.Comma)))
				{

					//Check exit conditions first:
					bool lastWasArithOperator;
					_lexer.NextToken();
					if (currToken.IsOperator)
					{
						Operator oper;	
						lastWasArithOperator = false;
						switch(currToken.HappyTokenKind)
						{
						case HappyTokenKind.OperatorOpenParen:
							parenCount++;
							break;
						case HappyTokenKind.OperatorCloseParen:
							parenCount--;
							if (parenCount < 0)
							{
								HappySourceLocation location = HappySourceLocation.Combine(startLocation, this._lexer.PeekToken().Location);
								this._errorCollector.MismatchedCloseParen(location);
								throw new AbortParseException(location);
							}
							break;
						case HappyTokenKind.OperatorOpenBracket:
							List<ExpressionNodeBase> arguments = new List<ExpressionNodeBase> { this.ParseExpression(ExpressionContext.Indexer) };

							while (this.EatNextTokenIf(HappyTokenKind.Comma))
								arguments.Add(this.ParseExpression(ExpressionContext.Indexer));

							this.Expect(HappyTokenKind.OperatorCloseBracket, "]");

							var indexOperator = new Operator(currToken);

							insertOperatorAndUpdateStacks(indexOperator);
								
							expStack.Push(new ArgumentList(currToken.Location, arguments.ToArray()));

							
							break;
						default:
							lastWasArithOperator = true;

							oper = new Operator(currToken) { PrecedenceLevelModifier = parenCount * Operator.PrecedenceModifierStep };

							insertOperatorAndUpdateStacks(oper);
							break;
						}
					}
					else if(currToken.HappyTokenKind == HappyTokenKind.KeywordNew)
					{
						Expect(HappyTokenKind.OperatorOpenParen, "(");
						ExpressionNodeBase exp = this.ParseExpression(ExpressionContext.ArgumentList);
						List<ExpressionNodeBase> constructorArgs = new List<ExpressionNodeBase>();
						
						if (Expect(HappyTokenKind.OperatorCloseParen, HappyTokenKind.Comma, MiscMessages.CommaOrCloseParen).HappyTokenKind == HappyTokenKind.Comma)
							do
							{
								constructorArgs.Add(this.ParseExpression(ExpressionContext.ArgumentList));
							} while (this.Expect(HappyTokenKind.Comma, HappyTokenKind.OperatorCloseParen,
							                     MiscMessages.CommaOrCloseParen).HappyTokenKind != HappyTokenKind.OperatorCloseParen);

						ExpressionNodeBase newExpression = new NewObjectExpression(exp, constructorArgs.ToArray());
						expStack.Push(newExpression);
						lastWasArithOperator = false;
					}
					else
					{
						expStack.Push(this.PromoteToExpression(currToken));
						lastWasArithOperator = false;
					}

					currToken = _lexer.PeekToken();


					//TODO:  ^ ??
					if (!lastWasArithOperator && (!currToken.IsOperator)// ^ currToken.HappyTokenKind == HappyTokenKind.OpenParen)
					    && parenCount <= 0)
						break;
				}

				if (parenCount > 0)
				{
					HappySourceLocation location = HappySourceLocation.Combine(startLocation, currToken.Location);
					_errorCollector.MismatchedOpenParen(location);
					throw new AbortParseException(location);
				}

				while (operatorStack.Count > 0)
					expStack.Push(operatorStack.Pop());

				retval = this.RecurseExp(expStack);
			}

			return retval;
		}


		private ExpressionNodeBase RecurseExp(Stack<AstNodeBase> expStack)
		{
			ExpressionNodeBase retval;

			//If there was only one item in the stack, then we must be parsing 
			//an expression (or part of an expression) with only a single term.
			if (expStack.Count == 1)
				retval = (ExpressionNodeBase)expStack.Pop();
			else
			{
				ExpressionNodeBase rval;
				AstNodeBase node = expStack.Pop();
				if (!(node is Operator))
					_errorCollector.ExpectedOperator(node.Location);

				Operator oper = (Operator)node;
				if (expStack.Peek() is Operator)
					rval = this.RecurseExp(expStack);
				else
					rval = expStack.Pop() as ExpressionNodeBase;

				if (oper.IsUnary)
					retval = new UnaryExpression(rval, oper);
				else
				{
					//DebugAssert.IsNonZero(expStack.Count, "expStack.Count == 0");
					if(expStack.Count == 0)
						_errorCollector.SyntaxErrorInExpression(rval.Location);

					ExpressionNodeBase lval;
					if(expStack.Peek() is Operator)
						lval = this.RecurseExp(expStack);
					else
						lval = expStack.Pop() as ExpressionNodeBase;

					retval = new BinaryExpression(lval, oper, rval);
				}
			}
			return retval;
		}

		private ExpressionNodeBase PromoteToExpression(Token firstToken)
		{
			
			ExpressionNodeBase retval;
			int intValue;
			switch(firstToken.HappyTokenKind)
			{
				case HappyTokenKind.Identifier:
					if (this.EatNextTokenIf(HappyTokenKind.OperatorOpenParen))
					{
						List<ExpressionNodeBase> arguments = new List<ExpressionNodeBase>();
						bool parseMore = !this.EatNextTokenIf(HappyTokenKind.OperatorCloseParen);
						while (parseMore)
						{
							arguments.Add(this.ParseExpression(ExpressionContext.ArgumentList));

							parseMore = this.Expect(HappyTokenKind.Comma, HappyTokenKind.OperatorCloseParen, 
							                        MiscMessages.CommaOrCloseParen).HappyTokenKind != HappyTokenKind.OperatorCloseParen;
						}
						retval = new FunctionCallExpression(firstToken.ToIdentifier(), arguments.ToArray());
					}
					else 
					    retval = new IdentifierExpression(firstToken.ToIdentifier());

					break;
				case HappyTokenKind.LiteralBool:
					switch(firstToken.Text)
					{
						case "true":
							retval = new LiteralExpression(firstToken.Location, true);
							break;
						case "false":
							retval = new LiteralExpression(firstToken.Location, false);
							break;
						default:
							throw new UnhandledCaseSourceException(firstToken.Location);
					}
					break;
				case HappyTokenKind.LiteralDecimalInt32:
					if(!Int32.TryParse(firstToken.Text, out intValue))
						throw new InternalSourceException(firstToken.Location, "Failed to parse an Int32 from \"{0}\" _tokenKind is LiteralInt32?!?!", firstToken.Text);
					retval = new LiteralExpression(firstToken.Location, intValue);
					break;
				case HappyTokenKind.LiteralHexInt32:
					if(!Int32.TryParse(firstToken.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out intValue))
						throw new InternalSourceException(firstToken.Location, "Failed to parse an Int32 from \"{0}\" _tokenKind is LiteralHexInt32?!?!", firstToken.Text);
					retval = new LiteralExpression(firstToken.Location, intValue);
					break;
				case HappyTokenKind.LiteralHexInt64:
					long longValue;
					if (!Int64.TryParse(firstToken.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out longValue))
						throw new InternalSourceException(firstToken.Location, "Failed to parse an Int32 from \"{0}\" _tokenKind is LiteralHexInt64?!?!", firstToken.Text);
					retval = new LiteralExpression(firstToken.Location, longValue);
					break;
				case HappyTokenKind.LiteralDouble:
					double doubleValue;
					try
					{
						doubleValue = Double.Parse(firstToken.Text);	
					} catch(Exception e)
					{
						throw new InternalSourceException(firstToken.Location, e,
						                                  "Failed to parse a Double from \"{0}\" but _tokenKind is LiteralDouble?!?!", firstToken.Text);
					}
					retval = new LiteralExpression(firstToken.Location, doubleValue);
					break;
				case HappyTokenKind.LiteralString:
					retval = new LiteralExpression(firstToken.Location, firstToken.Text);
					break;
				case HappyTokenKind.LiteralNull:
					retval = new NullExpression(firstToken.Location);
					break;
				case HappyTokenKind.BeginTemplate:
					retval = this.ParseAnonymousTemplateExpression(firstToken.Location);
					break;
				default:
					_errorCollector.UnexpectedToken(firstToken);
					throw new AbortParseException(firstToken.Location);
			}
			return retval;
		}

		
		OutputStatement ParseOutputExpression()
		{
			this.AssertExpect(HappyTokenKind.Output);
			List<ExpressionNodeBase> outputs = new List<ExpressionNodeBase>();

			do
			{
				outputs.Add(this.ParseExpression(ExpressionContext.Expression));
			}
			while(!EatNextTokenIf(HappyTokenKind.EndOfStatement));
			
			return new OutputStatement(outputs[0].Location, outputs.ToArray());
		}

		AnonymousTemplate ParseAnonymousTemplateExpression(HappySourceLocation startsAt)
		{
			List<AstNodeBase> sections = new List<AstNodeBase>();

			//the <| already parsed during call to ParseExpression()
			//SourceLocation sectionStartsAt = Expect(_tokenKind.OpenBrace, "{").location;
			bool keepParsing = true;
			HappySourceLocation endingLocation = HappySourceLocation.Invalid;
			while(keepParsing)
			{
				sections.Add(this.ParseStatement());

				if (this.EatNextTokenIf(HappyTokenKind.EndTemplate, out endingLocation))
					keepParsing = false;
			}

			return new AnonymousTemplate(new StatementBlock(HappySourceLocation.Combine(startsAt, endingLocation), sections.ToArray()));
		}
	}
}