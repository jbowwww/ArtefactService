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

		public WCFTypeResolver()
		{
		}

		#region implemented abstract members of System.Runtime.Serialization.DataContractResolver
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeof(System.Linq.IQueryable).IsAssignableFrom(declaredType))
			{
				string fullTypeName = typeNamespace + "." + typeName;
				Type type = Type.GetType(fullTypeName);
				if (type != null)
					return type;
			}
			else if (typeName.StartsWith("ExpressionNode`1["))
			{
				string tn = typeNamespace + "." + typeName.Substring(0, typeName.IndexOf('['));
				Type t = Type.GetType(tn);
				return t;
			}
			return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		public override bool TryResolveType(
			Type type, Type declaredType, DataContractResolver knownTypeResolver,
			out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (declaredType.IsSubclassOf(typeof(System.Linq.IQueryable)))
			{
				typeName = _typeDictionary.Add(type.Name);
				typeNamespace = _typeDictionary.Add(type.Namespace);
				return true;
			}
			else if (type.Name.StartsWith("ExpressionNode`1"))
			{
				typeName = _typeDictionary.Add("ExpressionNode`1[" + type.GetGenericArguments()[0].Name + "]");
				typeNamespace = _typeDictionary.Add(type.Namespace);
				return true;
			}
			return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}
		#endregion

	}
}

