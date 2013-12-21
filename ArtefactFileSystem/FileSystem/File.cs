using System;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	public class File : FileSystemEntry
	{
		public static Type[] GetArtefactTypes ()
		{
			return Artefact.ArtefactTypes.ToArray();
		}

		[DataMember]
		public virtual long Size { get; set; }

		public virtual string Name {
			get { return System.IO.Path.GetFileName(Path); }
		}

		public virtual string NameWithoutExtension {
			get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
		}

		public virtual string Extension {
			get { return System.IO.Path.GetExtension(Path); }
		}

		public File (FileInfo fInfo, Drive drive) :
			base(fInfo, drive)
		{
			Size = fInfo.Length;
		}

		public File() {}
	}
}

