/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

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

