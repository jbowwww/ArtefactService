using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract(IsReference = true)]
	public class FileSystemEntry : Artefact
	{
		public static Type[] GetArtefactTypes ()
		{
			return Artefact.ArtefactTypes.ToArray();
		}

		[DataMember]
		public virtual string Path { get; set; }

		[DataMember]
		public virtual Drive Drive { get; set; }

		[DataMember]
		public virtual FileAttributes Attributes { get; set; }

		[DataMember]
		public virtual DateTime CreationTime { get; set; }

		[DataMember]
		public virtual DateTime AccessTime { get; set; }

		[DataMember]
		public virtual DateTime ModifyTime { get; set; }

		public virtual string Directory {
			get { return System.IO.Path.GetDirectoryName(Path); }
		}

		public FileSystemEntry (FileSystemInfo fsInfo, Drive drive = null)
		{
			Drive = drive;
			Path = fsInfo.FullName;
			Attributes = fsInfo.Attributes;
			CreationTime = fsInfo.CreationTime;
			AccessTime = fsInfo.LastAccessTime;
			ModifyTime = fsInfo.LastWriteTime;
		}

		public FileSystemEntry() {}
	}
}

