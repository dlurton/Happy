using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace HappyTemplate.Runtime.Binding
{
	public class HappyBinder : DefaultBinder
	{

		public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
		{
			return toType.IsAssignableFrom(fromType);
		}

		public override Candidate PreferConvert(Type t1, Type t2)
		{
			throw new NotImplementedException();
		}

	}
}