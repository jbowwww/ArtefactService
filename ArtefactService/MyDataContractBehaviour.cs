using System;
using System.ServiceModel.Description;
using System.Runtime.Serialization;

namespace Artefacts.Service
{
	public class MyDataContractBehaviour :
		DataContractSerializerOperationBehavior
	{
		public MyDataContractBehaviour(OperationDescription operation) :
			base(operation)
		{
		}
		
		public override System.Runtime.Serialization.XmlObjectSerializer CreateSerializer(Type type, string name, string ns, System.Collections.Generic.IList<Type> knownTypes)
		{
			
//			return new NetDataContractSerializer(name, ns, new StreamingContext(StreamingContextStates.All), this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject,
//				System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple, null);
//			knownTypes = new System.Collections.Generic.List<Type>();//new Type[] { typeof(Serialize.Linq.Nodes.ExpressionNode) });
//			knownTypes.Add(typeof(Serialize.Linq.Nodes.ExpressionNode));
//			if (type.Equals(typeof(Serialize.Linq.Nodes.ExpressionNode<>)))
//				name = "ExpressionNode`1[" + type.GetGenericArguments()[0].FullName + "]";
			DataContractSerializer ser = new DataContractSerializer(type, name, ns, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, true, this.DataContractSurrogate, this.DataContractResolver);
			return ser;
//			return base.CreateSerializer(type, name, ns, knownTypes);
		}
		public override System.Runtime.Serialization.XmlObjectSerializer CreateSerializer(Type type, System.Xml.XmlDictionaryString name, System.Xml.XmlDictionaryString ns, System.Collections.Generic.IList<Type> knownTypes)
		{
			return new DataContractSerializer(type, name, ns, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, true, this.DataContractSurrogate, this.DataContractResolver);
//			return base.CreateSerializer(type, name, ns, knownTypes);
		}
	}
}

