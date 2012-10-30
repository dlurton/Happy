﻿namespace HappyTemplate.Compiler
{
	public enum HappyTokenKind
	{
		Verbatim,
		Identifier,
		Bang,
		Comma,
		LiteralString,
		LiteralDecimalInt32,
		LiteralHexInt32,
		LiteralHexInt64,
		LiteralDouble,
		LiteralChar,
		LiteralBool,
		LiteralNull,
		KeywordIf,
		KeywordElse,
		KeywordLookup,
		KeywordDefault,
		KeywordCase,
		KeywordFunction,
		KeywordIn,
		KeywordWhile,
		KeywordFor,
		KeywordBetween,
		KeywordWhere,
		KeywordReturn,
		KeywordDef,	
		KeywordLoad,
		KeywordUse,
		KeywordNew,
		KeywordContinue,
		KeywordBreak,
		KeywordSwitch,
		OpenBrace,
		CloseBrace,
		EndOfInput,
		InvalidCharacter,
		OpenVerbatim,
		CloseVerbatim,
		EndOfStatement,
		OperatorOpenParen,
		OperatorCloseParen,
		OperatorOpenBracket,
		OperatorCloseBracket,
		OperatorAssign,
		OperatorAdd,
		OperatorSubtract,
		OperatorDivide,
		OperatorMultiply,
		OperatorMod,
		OperatorEqual,
		OperatorNotEqual,
		OperatorGreaterThan,
		OperatorLessThan,
		OperatorGreaterThanOrEqual,
		OperatorLessThanOrEqual,
		OperatorXor,
		OperatorLogicalAnd,
		OperatorBitwiseAnd,
		OperatorLogicalOr,
		OperatorDot,
		OperatorNot,
		Output,
		OperatorBitwiseOr,
		BeginTemplate,
		EndTemplate,
		VerbatimOutputExpressionDelimiter,
		Colon,
	}
}