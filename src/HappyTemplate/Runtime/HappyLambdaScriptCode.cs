/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace HappyTemplate.Runtime
{
	public class HappyLambdaScriptCode : ScriptCode
	{
		readonly Delegate _compiledLambda;

		public HappyLambdaScriptCode(SourceUnit sourceUnit, Delegate compiledLambda)
			: base(sourceUnit)
		{
			_compiledLambda = compiledLambda;
		}

		public override object Run(Scope scope)
		{
			return _compiledLambda; 
		}
	}
}

