using System;
using System.Runtime.Serialization;
using Artefacts.Service;

namespace Artefacts
{
	[DataContract]
	public class ArtefactProxy<TArtefact> : IArtefact
		where TArtefact : Artefact
	{
		public static implicit operator TArtefact(ArtefactProxy<TArtefact> proxy)
		{
			return (TArtefact)proxy.Repository.GetById(proxy.Id.Value);
		}

		public static implicit operator ArtefactProxy<TArtefact>(TArtefact artefact)
		{
			return new ArtefactProxy<TArtefact>(artefact);
		}

		public IRepository Repository { get { return Artefacts.Service.Repository.Context; } }

		#region IArtefact implementation
		[DataMember]
		public int? Id { get; set; }
		[DataMember]
		public DateTime TimeCreated { get; set; }
		[DataMember]
		public DateTime TimeUpdated { get; set; }
		[DataMember]
		public DateTime TimeChecked { get; set; }
		#endregion

		public ArtefactProxy(Artefact artefact)
		{
			Id = artefact.Id;
			TimeCreated = artefact.TimeCreated;
			TimeUpdated = artefact.TimeUpdated;
			TimeChecked = artefact.TimeChecked;

		}
	}
}

