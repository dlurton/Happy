using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Hosting;

namespace HappyTemplate.Runtime.Trackers
{
	interface IHappyTracker 
	{
		string Name { get; }
	}
}