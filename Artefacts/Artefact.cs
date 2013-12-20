using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts
{
	[DataContract(IsReference = true)]
//	[MessageContract]
//	[KnownType(typeof(Artefacts.FileSystem.Drive))]
//	[KnownType(typeof(Artefacts.FileSystem.File))]
//	[KnownType(typeof(Artefacts.FileSystem.Directory))]
	[KnownType("GetArtefactTypes")]
	public abstract class Artefact : IArtefact
	{
		[ThreadStatic]
		private static List<Type> _artefactTypes = null;
		public static List<Type> ArtefactTypes {
			get
			{
				return _artefactTypes != null ? _artefactTypes : _artefactTypes = new List<Type>();
			}
		}

		public static Type[] GetArtefactTypes ()
		{
			return ArtefactTypes.ToArray();
		}

		#region IArtefact implementation
		[DataMember]
//		[MessageBodyMember]
		public virtual int? Id { get; set; }

		[DataMember]
//		[MessageBodyMember]
		public virtual DateTime TimeCreated { get; set; }

		[DataMember]
//		[MessageBodyMember]
		public virtual DateTime TimeUpdated { get; set; }

		[DataMember]
//		[MessageBodyMember]
		public virtual DateTime TimeChecked { get; set; }
		#endregion

		public Artefact ()
		{
			TimeCreated = TimeUpdated = TimeChecked = DateTime.Now;
		}

		public override string ToString ()
		{
			return string.Format ("[Artefact: Id={0}, TimeCreated={1}, TimeUpdated={2}, TimeChecked={3}]",
				Id == null ? "(null)" : Id.ToString(), TimeCreated, TimeUpdated, TimeChecked);
		}
	}
}

