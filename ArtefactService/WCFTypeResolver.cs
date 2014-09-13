using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Artefacts.Service
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
		private Dictionary<string, Type> _typeNames = new Dictionary<string, Type>();
		
		public WCFTypeResolver()
		{
		}

		#region implemented abstract members of System.Runtime.Serialization.DataContractResolver
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
//			if (typeof(System.Linq.IQueryable).IsAssignableFrom(declaredType))
//			{
//				string fullTypeName = typeNamespace + "." + typeName;
//				Type type = Type.GetType(fullTypeName);
//				if (type != null)
//					return type;
//			}
//			else
//			{
			string typeKey = typeName + "." + typeNamespace;
			
			return _typeNames.ContainsKey(typeKey) ? _typeNames[typeKey] :
				knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		public override bool TryResolveType(
			Type type, Type declaredType, DataContractResolver knownTypeResolver,
			out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
//			bool r = false;
//			if (declaredType.IsSubclassOf(typeof(System.Linq.IQueryable)))
//			{
//				typeName = _typeDictionary.Add(type.Name);
//				typeNamespace = _typeDictionary.Add(type.Namespace);
//				r = true;
//			}
//			else
//			{
			if (!type.IsPrimitive)
			{
				typeName = _typeDictionary.Add(GetTypeName(type));
				typeNamespace = _typeDictionary.Add(type.Namespace);
//				r = true;
//			}
//			if (r)
//			{
				string typeKey = typeName + "." + typeNamespace;
				if (!_typeNames.ContainsKey(typeKey))
					_typeNames.Add(typeKey, type);
				return true;
			}
			else 
				return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}
		#endregion
		
		private string GetTypeName(Type type)
		{
			if (type.IsGenericType)  //.StartsWith("ExpressionNode`1"))
			{
				string tn = type.FullName + "`" + type.GetGenericArguments().Length + "[";
				foreach (Type GT in type.GetGenericArguments())
					tn += GetTypeName(GT);
				tn += "]";
				return tn;
			}
			else
				return type.FullName;
		}
	}
}

