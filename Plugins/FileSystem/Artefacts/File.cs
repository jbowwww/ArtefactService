using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[File: Size={Size}]")]
	public class File : FileSystemEntry
	{
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }

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
		
		public virtual FileInfo FileInfo {
			get
			{
				return (FileInfo)base.FileSystemInfo;
			}
			set
			{
				Size = value.Length;
				base.FileSystemInfo = value;
			}
		}
		
		public File(string path)
		{
			FileInfo = new FileInfo(path);
		}
		
		protected File (FileInfo fileInfo)
		{
			FileInfo = fileInfo;
		}

		protected File() {}
		
		public override Artefact Update()
		{
			if (UpdateAge > Artefacts.Service.Repository.ArtefactUpdateAgeLimit)
			{
				FileInfo = new FileInfo(Path);
				return base.Update();
			}
			return this;
		}
		
		public override string ToString()
		{
			return string.Concat(string.Format("[File: Size={0}, Name={1}, NameWithoutExtension={2}, Extension={3}]\n",
				Size, Name, NameWithoutExtension, Extension), base.ToString().Indent());
		}
	}
}

