using System;
using System.Runtime.Serialization;
using FluentNHibernate.Automapping;

namespace Artefacts.Core
{
	public class AutoMapperConfiguration : DefaultAutomappingConfiguration
	{
		public override bool ShouldMap(Type type)
		{
			return type.IsSubclassOf(typeof(Artefact)); 	//typeof(Artefact).IsAssignableFrom(type);
		}
		
		public override bool ShouldMap(FluentNHibernate.Member member)
		{
			 return member.MemberInfo.IsDefined(typeof(DataMemberAttribute), false);
		}
		
//		public override 
	}
}

