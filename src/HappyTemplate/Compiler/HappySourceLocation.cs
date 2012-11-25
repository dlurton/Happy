/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
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
			ContractUtils.RequiresNotNull(unit, "unit");
			ContractUtils.RequiresNotNull(start, "start");
			ContractUtils.RequiresNotNull(end, "end");
			//ContractUtils.Requires(isBefore(start, end), "start and end must be well ordered");
			if(!isBefore(start, end))
				throw new ApplicationException("start and end must be well ordered");
			this.Unit = unit;
			this.Span = new SourceSpan(start, end);
		}

		

		HappySourceLocation(SourceUnit unit, SourceSpan span)
		{
			//ContractUtils.RequiresNotNull(unit, "unit");
			ContractUtils.RequiresNotNull(span, "span");
			this.Unit = unit;
			this.Span = span;
		}

		public override string ToString()
		{
			return this.Unit == null ? this.Unit.Path : "<unknown>" + this.Span;
		}

		public static HappySourceLocation Merge(HappySourceLocation start, HappySourceLocation end)
		{
			if(start == Invalid || end == Invalid)
				return Invalid;
			if (start == None || end == None)
				return None;

			DebugAssert.AreSameObject(start.Unit, end.Unit, "Start and end SourceUnits don't match.");
			return new HappySourceLocation(start.Unit, start.Span.Start, end.Span.End);
		}


		private static readonly HappySourceLocation _none = new HappySourceLocation(null, SourceSpan.None);
		public static HappySourceLocation None { get { return _none; } }

		private static readonly HappySourceLocation _invalid = new HappySourceLocation(null, SourceSpan.Invalid);
		public static HappySourceLocation Invalid { get { return _invalid; }		}
	
		//public static HappySourceLocation FindExtents(IEnumerable<HappySourceLocation> range)
		//{
		//	var rangeList = range.AsQueryable().ToList();

		//	if (rangeList.Count == 1)
		//		return rangeList[0];

		//	SourceLocation minStart = rangeList[0].Span.Start;
		//	SourceLocation maxEnd = rangeList[0].Span.End;
		//	SourceUnit unit = rangeList[0].Unit;

		//	foreach(var hsl in rangeList)
		//	{
		//		if(hsl.Unit != unit)
		//			throw new InvalidOperationException("Not all HappySourceLocations in the collection had the same source unit.");

		//		if (startsBefore(minStart, hsl))
		//			minStart = hsl.Span.Start;

		//		if (endsAfter(maxEnd, hsl))
		//			maxEnd = hsl.Span.End;
		//	}

		//	return new HappySourceLocation(unit, minStart, maxEnd);
		//}

		//static bool endsAfter(SourceLocation lvalue, HappySourceLocation rvalue)
		//{
		//	return lvalue.Line >= rvalue.Span.End.Line || (lvalue.Column == rvalue.Span.End.Column && lvalue.Column > rvalue.Span.End.Column);
		//}

		//static bool startsBefore(SourceLocation lvalue, HappySourceLocation rvalue)
		//{
		//	return lvalue.Line <= rvalue.Span.Start.Line || (lvalue.Column == rvalue.Span.Start.Column && lvalue.Column < rvalue.Span.Start.Column);
		//}

		static bool isBefore(SourceLocation lvalue, SourceLocation rvalue)
		{
			return lvalue.Line <= rvalue.Line || (lvalue.Column == rvalue.Column && lvalue.Column < rvalue.Column);
		}
	}
}

