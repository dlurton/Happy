/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler;
using NUnit.Framework;

namespace Happy.ScriptEngine.Tests
{
	[TestFixture]
	public class SemanticCheckingTests : TestFixtureBase
	{

		#region Symbol Table Error Conditions
		[Test]
		public void GlobalVariableAlreadyDefined()
		{
			AssertCompileErrorInModule(ErrorCode.VariableAlreadyDefined_Name, "def aVar; def aVar;");
		}

		[Test]
		public void LocalVariableAlreadyDefined()
		{
			AssertCompileErrorInModule(ErrorCode.VariableAlreadyDefined_Name, "function aFunc() { def aVar; def aVar; }");
		}

		[Test]
		public void UseStatementDoesNotEvaluateToANamespace()
		{
			AssertCompileErrorInModule(ErrorCode.UseStatementDoesNotEvaluateToANamespace_Segment_Namespace, "load \"mscorlib\"; use System.Int32; ");
		}

		#endregion

		#region Sematnic Error Conditions

		[Test]
		public void DuplicateFunctionParameterName()
		{
			AssertCompileErrorInModule(ErrorCode.DuplicateFunctionParameterName_Name, "function testFunc(arga, argb, argb, arbc) { return true; }");
		}

		[Test]
		[Ignore("Not sure this error condition can ever be met.")]
		public void OperatorIsNotBinary()
		{
			AssertCompileErrorInModule(ErrorCode.OperatorIsNotBinary_Operator, "function testFunc() { return 1 ! 2; } ");
		}

		#endregion 


	}
}

