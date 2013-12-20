using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Artefacts.Services
{
	/// <summary>
	/// This bloody thing doesn't even get CALLED!!! despite it being added to the operation behaviours (DataContractSerialzerOperationBehaviour)
	/// WCF is a lying HOEBAGSLUT.
	/// Guess I'm just looking for another alternative now
	/// </summary>
	public class WCFTypeResolver :
		DataContractResolver
	{
		private XmlDictionary _typeDictionary = new XmlDictionary();

		public WCFTypeResolver ()
		{
		}

		#region implemented abstract members of System.Runtime.Serialization.DataContractResolver
		public override Type ResolveName (string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeof(System.Linq.IQueryable).IsAssignableFrom(declaredType))
			{
				string fullTypeName = typeNamespace + "." + typeName;
				Type type = Type.GetType(fullTypeName);
				if (type != null)
					return type;
			}
			return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		public override bool TryResolveType (
			Type type, Type declaredType, DataContractResolver knownTypeResolver,
			out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (declaredType.IsSubclassOf(typeof(System.Linq.IQueryable)))
			{
				typeName = _typeDictionary.Add(type.Name);
				typeNamespace = _typeDictionary.Add(type.Namespace);
				return true;
			}
			return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}
		#endregion

	}
}

