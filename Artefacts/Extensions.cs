using System;
using System.Collections.Generic;
using System.Text;

namespace Artefacts
{
	public static class Extensions
	{
		public static string Grep(this string s, string sGrep)
		{
			string[] sLines = s.Split('\n');
			List<string> sOutLines = new List<string>();
			foreach (string sLine in sLines)
				if (sLine.Contains(sGrep))
					sOutLines.Add(sLine.Trim());
			StringBuilder sb = new StringBuilder();
			foreach (string so in sOutLines)
				sb.AppendLine(so);
			return sb.ToString();
		}

		public static string Indent(this string s, string indent = "  ")
		{
			return string.Concat(indent, s.Replace("\n", string.Concat("\n", indent)));
		}

//		public static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(KeyValuePair<TKey, TValue>[] initialPairs)
//		{
//			IDictionary<TKey, TValue> r = new Dictionary<TKey, TValue>();
//			if (initialPairs != null)
//				foreach (KeyValuePair<TKey, TValue> kvp in initialPairs)
//					r.Add(kvp);
//			return r;
//		}
	}
}

