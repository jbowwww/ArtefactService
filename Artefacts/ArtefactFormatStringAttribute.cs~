using System;
using System.Text;
using System.Reflection;

namespace Artefacts
{
	public class ArtefactFormatStringAttribute :
		Attribute
	{
		public string ArtefactFormat = string.Empty;
		
		public ArtefactFormatStringAttribute()
		{
		}

		public ArtefactFormatStringAttribute(string artefactFormat)
		{
			ArtefactFormat = artefactFormat;
		}

		public string GetArtefactString(Artefact artefact)
		{
			if (artefact == null)
				throw new ArgumentNullException("artefact");
			
			if (string.IsNullOrEmpty(ArtefactFormat))
				return string.Empty;
			
			Type T = artefact.GetType();
//			StringBuilder sb = new StringBuilder(ArtefactFormat, 256);
			string sb = ArtefactFormat;
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

