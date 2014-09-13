using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace Artefacts
{
	public static partial class Extensions
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

// TODO: Might be nice for diagnostic etc formatting. Indents and also wraps (at spaces between words) to a specified width
//		public static string IndentWrap(this string s, int width, string indent = "  ", string wrap = "\n")
//		{
//			int iNL = -1;
//			string sw = s;
//			for (string sw = sw.Insert(iNL + 1, indent); sw.Length > width; iNL = sw.LastIndexOf(" ", iNL + width, width))
//				sw.Remove(iNL).Insert(iNL, string.Concat(wrap, indent))
//			
//		}
		
		public static string ToString(this object o)
		{
			if (o == null)
				return "(null)";
			Type T = o.GetType();
			if (T.HasElementType)
			{
				StringBuilder sb = new StringBuilder(string.Concat(o.ToString(), " { "));
				object[] array = ((IEnumerable<object>)o).ToArray();
				foreach (object element in array)
					sb.Append(string.Concat(element.ToString(), ", "));
				if (array.Length > 0)
					sb.Remove(sb.Length - 2, 2);
				return sb.ToString();
			}
			return o.ToString();
		}
		
		public static string ToString(this object[] array)
		{
			StringBuilder sb = new StringBuilder(array.Length * 4);
			sb.AppendFormat("{0}[{1}] { ", array.GetType().GetElementType().FullName, array.Length);
			foreach (object item in array)
				sb.AppendFormat("{0} [{1}], ",
					item != null ? item.ToString() : "(null)",
					item != null ? item.GetType().FullName : "(System.Object)");
			sb.Remove(sb.Length - 2, 2).Append(" }");
			return sb.ToString();
		}
		
		public static string ToString<T>(this T[] array)
		{
			StringBuilder sb = new StringBuilder(array.Length * 4);
			sb.AppendFormat("{0}[{1}] { ", array.GetType().GetElementType().FullName, array.Length);
			foreach (T item in array)
				sb.AppendFormat("{0}, ",		// [{1}], ",
					item != null ? item.ToString() : "(null)");
					//item != null ? item.GetType().FullName : "(System.Object)");
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

//		public static Host GetOrCreateCurrentHost(this IQueryable<Host> hosts)
//		{
//			string hostId = Host.GetHostId();
//			if (string.IsNullOrEmpty(hostId))
//				throw new ApplicationException("Host.GetHostId() returned null or empty!");
//			Host dbHost = hosts.Where((h) => h.HostId == hostId).FirstOrDefault();
//			if (dbHost == null)
//				dbHost = new Host(true);
//			return dbHost;
//		}
	}
}

