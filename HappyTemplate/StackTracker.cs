using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyTemplate
{
	class StackTracker<T> : IDisposable
	{
		Stack<T> _stack;
		public StackTracker(Stack<T> stack)
		{
			_stack = stack;
		}

		public void Dispose()
		{
			_stack.Pop();
		}
		
	}

	static class StackTrackerExtension
	{
		public static StackTracker<T> TrackPush<T>(this Stack<T> stack, T variable)
		{
			DebugAssert.IsNotNull(variable, "Value being pushed onto the stack was null");
			stack.Push(variable);
			return new StackTracker<T>(stack);
		}

	}
}
