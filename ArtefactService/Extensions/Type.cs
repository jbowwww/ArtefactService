using System;
using System.Collections.Generic;

namespace Artefacts.Service
{
	/// <summary>
	/// <see cref="System.Type" /> extension methods
	/// </summary>
	internal static class Type_Extension
	{
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <returns>The element type.</returns>
		/// <param name="seqType">Seq type.</param>
		public static Type GetElementType(this Type seqType)
		{
			Type ienum = FindIEnumerable(seqType);
			if (ienum == null)
				return seqType;
			return ienum.GetGenericArguments()[0];
		}

		/// <summary>
		/// Finds the I enumerable.
		/// </summary>
		/// <returns>The IEnumerable.</returns>
		/// <param name="seqType">Seq type.</param>
		public static Type FindIEnumerable(this Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
				return null;
			if (seqType.IsArray)
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
			if (seqType.IsGenericType)
			{
				foreach (Type arg in seqType.GetGenericArguments())
				{
					Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
					if (ienum.IsAssignableFrom(seqType))
						return ienum;
				}
			}
			Type[] ifaces = seqType.GetInterfaces();
			if (ifaces != null && ifaces.Length > 0)
			{
				foreach (Type iface in ifaces)
				{
					Type ienum = FindIEnumerable(iface);
					if (ienum != null)
						return ienum;
				}
			}
			if (seqType.BaseType != null && seqType.BaseType != typeof(object))
				return FindIEnumerable(seqType.BaseType);
			return null;
		}
	}
}

