using System.Collections.Generic;
using HappyTemplate.Exceptions;

namespace HappyTemplate.Runtime
{
	static class Sequence
	{
		public static IEnumerable<int> GetSequence(int from, int to)
		{
			return GetSequence(from, to, 1);
		}

		public static IEnumerable<int> GetSequence(int from, int to, int step)
		{
			if(step < 0)
				throw new HappyRuntimeException(Resources.RuntimeMessages.SequenceStepMustBePositive);

			if(from < to)
			{
				for(int i = from; i <= to; i += step)
					yield return i;
			}
			else
			{
				for(int i = from; i >= to; i -= step)
					yield return i;
			}
		
			yield break;
		}	
	}
}
