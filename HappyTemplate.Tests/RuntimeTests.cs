﻿using System.Collections.Generic;
using System.Linq;
using HappyTemplate.Runtime;

using NUnit.Framework;

namespace HappyTemplate.Tests
{
	[TestFixture]
	public class RuntimeTests
	{
		[Test]
		public void SequenceIncremental()
		{
			List<int> numbers = Sequence.GetSequence(0, 4).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(0, numbers[0]);
			Assert.AreEqual(1, numbers[1]);
			Assert.AreEqual(2, numbers[2]);
			Assert.AreEqual(3, numbers[3]);
			Assert.AreEqual(4, numbers[4]);
		}

		[Test]
		public void SequenceDecremental()
		{
			List<int> numbers = Sequence.GetSequence(4, 0).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(4, numbers[0]);
			Assert.AreEqual(3, numbers[1]);
			Assert.AreEqual(2, numbers[2]);
			Assert.AreEqual(1, numbers[3]);
			Assert.AreEqual(0, numbers[4]);
		}

		[Test]
		public void SequenceIncrementalStep2()
		{
			List<int> numbers = Sequence.GetSequence(0, 8, 2).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(0, numbers[0]);
			Assert.AreEqual(2, numbers[1]);
			Assert.AreEqual(4, numbers[2]);
			Assert.AreEqual(6, numbers[3]);
			Assert.AreEqual(8, numbers[4]);
		}

		[Test]
		public void SequenceIncrementalStep2Odd()
		{
			List<int> numbers = Sequence.GetSequence(0, 9, 2).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(0, numbers[0]);
			Assert.AreEqual(2, numbers[1]);
			Assert.AreEqual(4, numbers[2]);
			Assert.AreEqual(6, numbers[3]);
			Assert.AreEqual(8, numbers[4]);
		}

		[Test]
		public void SequenceDecrementalStep2()
		{
			List<int> numbers = Sequence.GetSequence(8, 0, 2).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(8, numbers[0]);
			Assert.AreEqual(6, numbers[1]);
			Assert.AreEqual(4, numbers[2]);
			Assert.AreEqual(2, numbers[3]);
			Assert.AreEqual(0, numbers[4]);
		}

		[Test]
		public void SequenceDecrementalStep2Odd()
		{
			List<int> numbers = Sequence.GetSequence(9, 1, 2).ToList();
			Assert.AreEqual(5, numbers.Count);
			Assert.AreEqual(9, numbers[0]);
			Assert.AreEqual(7, numbers[1]);
			Assert.AreEqual(5, numbers[2]);
			Assert.AreEqual(3, numbers[3]);
			Assert.AreEqual(1, numbers[4]);
		}
	}
}
