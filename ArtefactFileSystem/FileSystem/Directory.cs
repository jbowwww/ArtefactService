using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	public class Directory : FileSystemEntry
	{
public static Type[] GetArtefactTypes ()
		{
			return Artefact.ArtefactTypes.ToArray();
		}

		public Directory (DirectoryInfo dInfo, Drive drive) :
			base(dInfo, drive)
		{
		}

		public Directory() {}
	}
}

