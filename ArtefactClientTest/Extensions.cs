using System;

namespace Artefacts.TestClient
{
	public static class Extensions
	{
		public static Attribute[] GetCustomAttributes(this Type type)
		{
			Attribute[] attributes = type.GetCustomAttributes();
			return attributes;
			
		}
	}
}
