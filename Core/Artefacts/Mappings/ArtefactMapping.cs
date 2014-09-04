using System;
using FluentNHibernate.Mapping;

namespace Artefacts.Core
{
	public class ArtefactMapping : ClassMap<Artefact>
	{
		public ArtefactMapping()
		{
			Id(x => x.Id).GeneratedBy.Increment();
			Map(x => x.TimeCreated);
			Map(x => x.TimeChecked);
			Map(x => x.TimeUpdated);
			
		}
	}
}

