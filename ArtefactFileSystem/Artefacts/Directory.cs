using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[Directory: ]")]
	public class Directory : FileSystemEntry
	{
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }
		
		public Directory(string path)
		{
			
		}
		
		public Directory(DirectoryInfo dInfo, Drive drive)
		{
			Init(dInfo, drive);
		}

		protected Directory() {}
		
		protected virtual void Init(DirectoryInfo dInfo, Drive drive)
		{
			base.Init(dInfo, drive);
		}
		
		public override Artefact Update()
		{
			base.Update();
			Init(new DirectoryInfo(Path), Drive.GetDrive(Path));		// TODO: Warning!! shouldn't be null - need to think your strategy/architecture through better for these operations
			return this;
		}
	}
}

