/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Runtime;
using Microsoft.Scripting;

namespace HappyTemplate
{
	public class HappyErrorSink : ErrorSink
	{
		private readonly HappyLanguageContext _languageContext;

		public int ErrorCount { get; private set; }
		
		public HappyErrorSink(HappyLanguageContext languageContext)
		{
			_languageContext = languageContext;
		}

		public override void Add(SourceUnit source, string message, SourceSpan span, int errorCode, Severity severity)
		{
			this.ErrorCount++;

			message = FormatMessage(source != null ? source.Path : "<none>", span.Start.Line, span.Start.Column, message);
			_languageContext.ErrorOutput.WriteLine(message);
			base.Add(source, message, span, errorCode, severity);
		}

		static string FormatMessage(string path, int line, int col, string message)
		{
			return string.Format("{0} ({1}, {2}): {3}", path, line, col, message);
		}
	}
}

