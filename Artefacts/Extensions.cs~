using System;
using System.Collections.Generic;

namespace Artefacts
{
	public static class Extensions
	{
		public static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(KeyValuePair<TKey, TValue>[] initialPairs)
		{
			IDictionary<TKey, TValue> r = new Dictionary<TKey, TValue>();
			if (initialPairs != null)
				foreach (KeyValuePair<TKey, TValue> kvp in initialPairs)
					r.Add(kvp);
			return r;
		}
	}
}

