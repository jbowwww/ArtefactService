using System;

namespace Artefacts.TestClient
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
	public class ClientTestMethodAttribute : Attribute
	{
		public string Name { get; set; }
		
		public int Order { get; set; }
		
		public ClientTestMethodAttribute()
		{
		}
	}
}

