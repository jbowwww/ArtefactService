using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	public class Drive : Artefact
	{
		public static Type[] GetArtefactTypes ()
		{
			return Artefact.ArtefactTypes.ToArray();
		}

		[DataMember]
		public virtual string Label { get; set; }

		[DataMember]
		public virtual string Format { get; set; }

		[DataMember]
		public virtual DriveType Type { get; set; }

		[DataMember]
		public virtual long Size { get; set; }

		[DataMember]
		public virtual long FreeSpace { get; set; }

		[DataMember]
		public virtual long AvailableFreeSpace { get; set; }

		public Drive (DriveInfo dInfo)
		{
			Label = dInfo.VolumeLabel;
			Format = dInfo.DriveFormat;
			Type = dInfo.DriveType;
			Size = dInfo.TotalSize;
			FreeSpace = dInfo.TotalFreeSpace;
			AvailableFreeSpace = dInfo.AvailableFreeSpace;
		}

		public Drive() {}
	}
}

