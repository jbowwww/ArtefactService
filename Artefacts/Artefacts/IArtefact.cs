using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts
{
	public interface IArtefact
	{
		int? Id { get; set; }
		DateTime TimeCreated { get; set; }
		DateTime TimeUpdated { get; set; }
		DateTime TimeChecked { get; set; }
	}
}

