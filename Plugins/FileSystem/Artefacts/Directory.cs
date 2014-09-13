using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[Directory: ]")]
	public class Directory : FileSystemEntry
	{
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }
		
		public virtual System.IO.DirectoryInfo DirectoryInfo {
			get { return (System.IO.DirectoryInfo)base.FileSystemInfo; }
			set
			{
				base.FileSystemInfo = value;
			}
		}
			
		public Directory(string path)
		{
			DirectoryInfo = new System.IO.DirectoryInfo(path);
		}
		
		public Directory(System.IO.DirectoryInfo directoryInfo)
		{
			DirectoryInfo = directoryInfo;
		}

		protected Directory() {}
		
		public override Artefact Update()
		{
			DirectoryInfo = new System.IO.DirectoryInfo(Path);
			return base.Update();
		}
		
		public override string ToString()
		{
			return string.Format("[Directory: ]\n" + base.ToString().Indent());
		}
	}
}

