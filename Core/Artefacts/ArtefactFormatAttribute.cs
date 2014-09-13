using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Artefacts
{
	/// <summary>
	/// Attribute to specify a format string for a <see cref="Artefact"/>
	/// </summary>
	/// <exception cref="ArgumentNullException">
	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
	/// </exception>
	public class ArtefactFormatAttribute : Attribute
	{
		private static Dictionary<Type, Type[]> _typeHeirarchyCache = new Dictionary<Type, Type[]>();
		
		/// <summary>
		/// Gets the artefact string.
		/// </summary>
		/// <returns>The formatted artefact string</returns>
		/// <param name="artefact">The <see cref="Artefact"/> instance</param>
		public static string GetString(Artefact artefact)
		{
			StringBuilder sb = new StringBuilder(1024);
			string artefactString = string.Empty;
			bool moreTypes = true;
			for (int i = 0; moreTypes; artefactString = ArtefactFormatAttribute.GetString(artefact, i, out moreTypes))
				if (artefactString.Length > 0)
					sb.Append('\n').Append(' ', i * 2).Append(artefactString);
			return sb.ToString();	
		}
		
		/// <summary>
		/// Gets the artefact string.
		/// </summary>
		/// <returns>The formatted artefact string</returns>
		/// <param name="artefact">The <see cref="Artefact"/> instance</param>
		/// <param name="baseLevel">The distance from the most derived type, of the type to format the artefact string for</param>
		/// <param name="levelValid"><c>out</c> parameter that indicates if <paramref name="baseLevel"/> was valid (not higher than type inheritance depth)</param>
		public static string GetString(Artefact artefact, int baseLevel, out bool levelValid)
		{
			Type TArtefact = artefact.GetType();
			Type[] typeHeirarchy = _typeHeirarchyCache.ContainsKey(TArtefact) ?
				_typeHeirarchyCache[TArtefact]
				: _typeHeirarchyCache[TArtefact] = Artefact.GetTypeHeirarchy(TArtefact);
			levelValid = baseLevel < typeHeirarchy.Length ? true : false;
			return levelValid ? GetString(artefact, typeHeirarchy[baseLevel]) : string.Empty;
		}
		
		/// <param name="artefact">The <see cref="Artefact"/> instance</param>
		/// <param name="baseLevel">The type to format the artefact string for</param>
		public static string GetString(Artefact artefact, Type artefactType)
		{
			object[] attrs = artefactType.GetCustomAttributes(typeof(ArtefactFormatAttribute), false);
			ArtefactFormatAttribute afsAttr = attrs.Length > 0 ?
				(ArtefactFormatAttribute)attrs[0] : new ArtefactFormatAttribute();
			return afsAttr.GetArtefactString(artefact);
		}
		
		public string Format = string.Empty;
		
		public ArtefactFormatAttribute()
		{
		}

		public ArtefactFormatAttribute(string format)
		{
			Format = format;
		}

		/// <summary>
		/// Gets the artefact string.
		/// </summary>
		/// <returns>
		/// The artefact string.
		/// </returns>
		/// <param name='artefact'>
		/// Artefact.
		/// </param>
		/// <exception cref="ArgumentNullException">
	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
	/// </exception>
	public string GetArtefactString(Artefact artefact)
		{
			if (artefact == null)
				throw new ArgumentNullException("artefact");
			
			if (string.IsNullOrEmpty(Format))
				return string.Empty;
			
			Type T = artefact.GetType();
//			StringBuilder sb = new StringBuilder(ArtefactFormat, 256);
			string sb = Format;
			int i, i2 = 0;
			
			string memberName;
			MemberInfo mi;
			object member;
			string memberString;
			BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
				BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty;
			i = sb.IndexOf('{');
			while (i >= 0 && i < sb.Length - 1)
			{
				i = sb.IndexOf('{', i);
				if (i >= 0)
				{
					i2 = sb.IndexOf('}', i + 1);
					if (i2 >= 0)
					{
						memberName = sb.Substring(i + 1, i2 - i - 1);
						mi = T.GetMember(memberName)[0];
						member = T.InvokeMember(memberName, bf, null, artefact, new object[] {});
						sb = sb.Remove(i, i2 - i + 1);
						memberString = member == null ? "(null)" : member.ToString();
						sb = sb.Insert(i, memberString);
						i += memberString.Length;					
					}
					else
						break;
				}
				else
					break;
			}
			
			return sb.ToString();
		}
	}
}

