/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors.Analyzer;
using Happy.ScriptEngine.Runtime;

namespace Happy.ScriptEngine.Compiler.AstVisitors
{
	class LocationScanner : AstAnalyzer
	{
		public LocationScanner(HappyLanguageContext languageContext) : base(languageContext)
		{
			
		}


	}
}

