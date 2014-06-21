using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

		public static string ToString(this object[] array)
		{
			StringBuilder sb = new StringBuilder(array.GetType().FullName).Append(" { ");
			foreach (object item in array)
				sb.AppendFormat("{0} [{1}], ",
					item != null ? item.ToString() : "(null)",
					item != null ? item.GetType().FullName : "(System.Object)");
			sb.Remove(sb.Length - 2, 2).Append(" }");
			return sb.ToString();
		}
//		public static IDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(KeyValuePair<TKey, TValue>[] initialPairs)
//		{
//			IDictionary<TKey, TValue> r = new Dictionary<TKey, TValue>();
//			if (initialPairs != null)
//				foreach (KeyValuePair<TKey, TValue> kvp in initialPairs)
//					r.Add(kvp);
//			return r;
//		}

		public static Host GetOrCreateCurrentHost(this IQueryable<Host> hosts)
		{
			string hostId = Host.GetHostId();
			if (string.IsNullOrEmpty(hostId))
				throw new ApplicationException("Host.GetHostId() returned null or empty!");
			Host dbHost = hosts.Where((h) => h.HostId == hostId).FirstOrDefault();
			if (dbHost == null)
				dbHost = new Host(true);
			return dbHost;
		}
	}
}

