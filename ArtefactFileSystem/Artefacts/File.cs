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
//		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }

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
		
		public File(string path)
		{
			Init(new FileInfo(path), Drive.GetDrive(path));	// TODO: Warning!! shouldn't be null - need to think your strategy/architecture through better for these operations
		}
		
		protected File (FileInfo fInfo, Drive drive)
		{
			Init(fInfo, drive);
		}

		protected File() {}
		
		protected virtual void Init(FileInfo fInfo, Drive drive)
		{
			Size = fInfo.Length;
			base.Init(fInfo, drive);
		}
		
		public override Artefact Update()
		{
			base.Update();
			Init(new FileInfo(Path), Drive.GetDrive(Path));	// TODO: Warning!! shouldn't be null - need to think your strategy/architecture through better for these operations
			return this;
		}
	}
}

