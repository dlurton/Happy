/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using NUnit.Framework;

namespace HappyTemplate.Tests
{
	[TestFixture]
	public class ParserTests : TestFixtureBase
	{
		[Test]
		public void ParseSingleParameter()
		{
			Function function = ParseFunction("aparameter", "irrelevent;");
			Assert.AreEqual(1, function.ParameterList.Count);
			Assert.AreEqual("aparameter", function.ParameterList[0].Name.Text);
		}

		[Test]
		public void ParseMultipleParameters()
		{
			Function function = ParseFunction("p1, p2,p3", "irrelevent;");
			Assert.AreEqual(3, function.ParameterList.Count);
			Assert.AreEqual("p1", function.ParameterList[0].Name.Text);
			Assert.AreEqual("p2", function.ParameterList[1].Name.Text);
			Assert.AreEqual("p3", function.ParameterList[2].Name.Text);
		}

		[Test]
		public void ParseLiteralExpression()
		{
			Function function = ParseFunction("~\"test\";");
			Assert.AreEqual(1, function.Body.Statements.Length);
			LiteralExpression stringExpression = (LiteralExpression)((OutputExpression)function.Body[0]).ExpressionsToWrite[0];
			Assert.AreEqual("test", stringExpression.Value);
		}

		[Test]
		public void ParseVariableExpression()
		{
			Function function = ParseFunction("~test;");
			Assert.AreEqual(1, function.Body.Statements.Length);
			AstNodeBase temp = ((OutputExpression) function.Body[0]).ExpressionsToWrite[0];
			Assert.IsInstanceOf(typeof(IdentifierExpression), temp);
			IdentifierExpression expression = (IdentifierExpression)temp;
			Assert.AreEqual("test", expression.Identifier.Text);
		}

		[Test]
		public void ParseMultiVariableOuputExpression()
		{
			Function function = ParseFunction("~test test2 test3;");
			Assert.AreEqual(1, function.Body.Statements.Length);
			OutputExpression outputExpression = (OutputExpression)function.Body[0];

			AstNodeBase temp = outputExpression.ExpressionsToWrite[0];
			Assert.IsInstanceOf(typeof(IdentifierExpression), temp);
			IdentifierExpression expression = (IdentifierExpression)temp;
			Assert.AreEqual("test", expression.Identifier.Text);

			temp = outputExpression.ExpressionsToWrite[0];
			Assert.IsInstanceOf(typeof(IdentifierExpression), temp);
			expression = (IdentifierExpression)temp;
			Assert.AreEqual("test", expression.Identifier.Text);

			temp = outputExpression.ExpressionsToWrite[0];
			Assert.IsInstanceOf(typeof(IdentifierExpression), temp);
			expression = (IdentifierExpression)temp;
			Assert.AreEqual("test", expression.Identifier.Text);
		}


		[Test]
		public void ParseTemplateExpression()
		{
			Function function = ParseFunction("~template();");
			Assert.AreEqual(1, function.Body.Statements.Length);
			FunctionCallExpression expansion = (FunctionCallExpression)((OutputExpression)function.Body[0]).ExpressionsToWrite[0];
			Assert.AreEqual(0, expansion.Arguments.Length);
		}
		[Test]
		public void ParseTemplateExpressionWithSingleArgument()
		{
			Function function = ParseFunction("~template(arg1);");
			Assert.AreEqual(1, function.Body.Statements.Length);
			FunctionCallExpression expansion = (FunctionCallExpression)((OutputExpression)function.Body[0]).ExpressionsToWrite[0]; 
			Assert.AreEqual(1, expansion.Arguments.Length);
			NamedExpression arg1 = (NamedExpression)expansion.Arguments[0];
			Assert.AreEqual("arg1", arg1.Identifier.Text);
		}

		[Test]
		public void ParseTemplateExpressionWithMultipleArguments()
		{
			Function function = ParseFunction("~template(arg1, arg2,arg3);");
			Assert.AreEqual(1, function.Body.Statements.Length);
			AstNodeBase temp = ((OutputExpression)function.Body[0]).ExpressionsToWrite[0];
			Assert.IsInstanceOf(typeof(FunctionCallExpression), temp);
			FunctionCallExpression expansion = (FunctionCallExpression)temp;
			Assert.AreEqual(3, expansion.Arguments.Length);
			Assert.AreEqual("arg1", ((NamedExpression)expansion.Arguments[0]).Identifier.Text);
			Assert.AreEqual("arg2", ((NamedExpression)expansion.Arguments[1]).Identifier.Text);
			Assert.AreEqual("arg3", ((NamedExpression)expansion.Arguments[2]).Identifier.Text);
		}

		[Test]
		public void ParseFor()
		{
			Function t = ParseFunction("for(n in x) ~n; ");
			ForStatement fs = (ForStatement)t.Body[0];
			IdentifierExpression enumerable = (IdentifierExpression)fs.Enumerable;
			Assert.AreEqual("x", enumerable.Identifier.Text);
			Assert.AreEqual("n", fs.LoopVar.Text);
			Assert.AreEqual(1, fs.LoopBody.Statements.Length);
			Assert.AreEqual("n", ((IdentifierExpression)((OutputExpression)fs.LoopBody.Statements[0]).ExpressionsToWrite[0]).Identifier.Text);
		}

		[Test]
		public void ParseForWithBraces()
		{
			Function t = ParseFunction("for(n in x) { ~n; }");
			ForStatement fs = (ForStatement)t.Body[0];
			IdentifierExpression enumerable = (IdentifierExpression)fs.Enumerable;
			Assert.AreEqual("x", enumerable.Identifier.Text);
			Assert.AreEqual("n", fs.LoopVar.Text);
			Assert.AreEqual(1, fs.LoopBody.Statements.Length);
			Assert.AreEqual("n", ((IdentifierExpression)((OutputExpression)fs.LoopBody.Statements[0]).ExpressionsToWrite[0]).Identifier.Text);
		}


		[Test]
		public void ParseForWithBetween()
		{
			Function t = ParseFunction("for(n in x between \",\") ~n; ");
			ForStatement fes = (ForStatement)t.Body[0];
			IdentifierExpression enumerable = (IdentifierExpression)fes.Enumerable;
			Assert.AreEqual("x", enumerable.Identifier.Text);
			Assert.AreEqual("n", fes.LoopVar.Text);
			Assert.AreEqual(",", ((LiteralExpression)fes.Between).Value);
			Assert.AreEqual(1, fes.LoopBody.Statements.Length);
			Assert.AreEqual("n", ((IdentifierExpression)((OutputExpression)fes.LoopBody[0]).ExpressionsToWrite[0]).Identifier.Text);
		}

		[Test]
		public void ParseForWithBetweenAndWhere()
		{
			Function t = ParseFunction("for(n in x where true between \",\") ~n; ");
			ForStatement fes = (ForStatement)t.Body[0];
			IdentifierExpression enumerable = (IdentifierExpression)fes.Enumerable;
			Assert.AreEqual("x", enumerable.Identifier.Text);
			Assert.AreEqual("n", fes.LoopVar.Text);
			Assert.AreEqual(",", ((LiteralExpression)fes.Between).Value);
			Assert.AreEqual(1, fes.LoopBody.Statements.Length);
			Assert.AreEqual("n", ((IdentifierExpression)((OutputExpression)fes.LoopBody[0]).ExpressionsToWrite[0]).Identifier.Text);
		}

	}
}

