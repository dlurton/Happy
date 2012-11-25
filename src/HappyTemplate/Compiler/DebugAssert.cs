/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

#if DEBUG
#define BREAK_ON_ASSERT_FAIL
#endif

using System;

namespace HappyTemplate
{
	[Serializable]
	public class AssertException : Exception
	{
		public AssertException() { }
		public AssertException(string message) : base(message) { }
		public AssertException(string message, Exception inner) : base(message, inner) { }
		public AssertException(string msg, params object[] args)
			: base(Util.Format(msg, args)) { }
	}

	static class DebugAssert
	{
		private static void FailAssertion(string msg, params object[] args)
		{
#if BREAK_ON_ASSERT_FAIL
			System.Diagnostics.Debugger.Break();
#endif
			throw new AssertException(msg, args);
		}

		private static void FailAssertion()
		{
#if BREAK_ON_ASSERT_FAIL
			System.Diagnostics.Debugger.Break();
#endif
			throw new AssertException();
		}
		public static void IsNotNull(object o)
		{
			if (o == null)
				FailAssertion();
		}
		public static void IsNull(object o, string msg, params object[] args)
		{
			if (o != null)
				FailAssertion(msg, args);
		}
		public static void IsNotNull(object o, string msg, params object[] args)
		{
			if (o == null)
				FailAssertion(msg, args);
		}
		public static void IsInstanceOfType(Type t, object o, string msg = null, params object[] args)
		{
			Type was = o.GetType();
			if (t == was) 
				return;

			string message = String.IsNullOrEmpty(msg) ? "" : Util.Format(msg + "  ", args);
			FailAssertion(Util.Format("{0}Expected type:  '{1}' but was '{2}'", message, t.Name, was.Name));
		}

		public static void AreNotEqual(object a, object b, string fmt, params object[] args)
		{
			if (a.Equals(b))
				FailAssertion(Util.Format(fmt, args));
		}
		public static void AreEqual(object a, object b, string fmt, params object[] args)
		{
			if (!a.Equals(b))
				FailAssertion(Util.Format(fmt, args));
		}

		public static void AreSameObject(object a, object b, string fmt, params object[] args)
		{
			if(a != b)
				FailAssertion(Util.Format(fmt, args));
		}

		public static void IsTrue(bool val, string fmt, params object[] args)
		{
			if(val != true)
				FailAssertion(Util.Format(fmt, args));
			
		}
		public static void IsFalse(bool val, string fmt, params object[] args)
		{
			if (val)
				FailAssertion(Util.Format(fmt, args));

		}
		public static void IsGreater(int less, int greater, string fmt, params object[] args)
		{
			if (greater <= less)
				FailAssertion(Util.Format(fmt, args));

		}

		internal static void Fail()
		{
			FailAssertion();	
		}
		internal static void Fail(string fmt, params object[] args)
		{
			FailAssertion(Util.Format(fmt, args));
		}

		public static void IsNonZero(int n, string fmt, params object[] args)
		{
			IsTrue(n != 0, fmt, args);
		}
		public static void IsZero(int n, string fmt, params object[] args)
		{
			IsTrue(n == 0, fmt, args);
		}

		public static void IsGreaterOrEqual(int greater, int than, string fmt, params object[] args)
		{
			if(!(greater >= than))
				FailAssertion(Util.Format(fmt, args));
		}

		public static void IsLessOrEqual(int less, int than, string fmt, params object[] args)
		{
			if (!(less <= than))
				FailAssertion(Util.Format(fmt, args));
		}
	}
}

