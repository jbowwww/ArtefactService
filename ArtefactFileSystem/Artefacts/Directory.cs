using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormatString("[Directory: ]")]
	public class Directory : FileSystemEntry
	{
		public static Type[] GetArtefactTypes ()
		{
			return Artefact.GetArtefactTypes();
		}

		public Directory (DirectoryInfo dInfo, Drive drive) :
			base(dInfo, drive)
		{
		}

		protected Directory() {}
	}
}

