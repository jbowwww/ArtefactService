using System;
using System.Runtime.Serialization;
using System.Reflection;
namespace Artefacts.Service
{
	public class QuerySerializationBinder :
		SerializationBinder
	{
		public QuerySerializationBinder()
		{
		}

		#region implemented abstract members of System.Runtime.Serialization.SerializationBinder
		public override Type BindToType(string assemblyName, string typeName)
		{
			System.Reflection.AssemblyName[] entryNames = System.Reflection.Assembly.GetEntryAssembly().GetReferencedAssemblies();
			System.Reflection.AssemblyName[] execNames = System.Reflection.Assembly.GetEntryAssembly().GetReferencedAssemblies();
			string qName = Assembly.CreateQualifiedName(assemblyName, typeName);
			Type type = Type.GetType(qName, true);
			return type;
		}
		#endregion
	}
}

