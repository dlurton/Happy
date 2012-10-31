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

			_languageContext.ErrorOutput.WriteLine(FormatMessage(source != null ? source.Path : "<none>", span.Start.Line, span.Start.Column, message));
			base.Add(source, message, span, errorCode, severity);
		}

		private static string FormatMessage(string path, int line, int col, string message)
		{
			return string.Format("{0} ({1}, {2}): {3}", path, line, col, message);
		}

		public override void Add(string message, string path, string code, string line, SourceSpan span, int errorCode, Severity severity)
		{
			this.ErrorCount++;
			_languageContext.ErrorOutput.WriteLine(FormatMessage(path, span.Start.Line, span.Start.Column, message));
			base.Add(message, path, code, line, span, errorCode, severity);
		}
	}
}
