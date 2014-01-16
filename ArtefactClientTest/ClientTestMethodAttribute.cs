using System;

namespace ArtefactClientTest
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ClientTestMethodAttribute : Attribute
	{
		public string Name { get; set; }
		
		public int Order { get; set; }
		
		public ClientTestMethodAttribute()
		{
		}
	}
}

