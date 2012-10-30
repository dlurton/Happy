using Microsoft.Scripting;
using HappyTemplate.Exceptions;
using Microsoft.Scripting.Utils;

namespace HappyTemplate.Compiler
{
	public class HappySourceLocation
	{
		public SourceUnit Unit { get; private set; }
		public SourceSpan Span { get; private set; }

		public HappySourceLocation(SourceUnit unit, SourceLocation start, SourceLocation end)
		{
			ContractUtils.RequiresNotNull(start, "start");
			ContractUtils.RequiresNotNull(end, "end");
			this.Unit = unit;
			this.Span = new SourceSpan(start, end);
			
		}

		public HappySourceLocation(SourceUnit unit, SourceSpan span)
		{
			ContractUtils.RequiresNotNull(span, "span");
			this.Unit = unit;
			this.Span = span;
		}

		public static HappySourceLocation Combine(HappySourceLocation start, HappySourceLocation end)
		{
			DebugAssert.AreSameObject(start.Unit, end.Unit, "Start and end SourceUnits don't match.");

			return new HappySourceLocation(start.Unit, start.Span.Start, end.Span.End);
		}


		private static HappySourceLocation _none = new HappySourceLocation(null, SourceSpan.None);
		public static HappySourceLocation None { get { return _none; } }

		private static HappySourceLocation _invalid = new HappySourceLocation(null, SourceSpan.Invalid);
		public static HappySourceLocation Invalid
		{
			get { return _invalid; }
			
		}
	}
}
