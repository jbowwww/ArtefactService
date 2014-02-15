using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormatString("[FileSystemEntry: Drive={Drive} Path={Path} Attributes={Attributes} CreationTime={CreationTime} AccessTime={AccessTime} ModifyTime={ModifyTime}]")]
	public class FileSystemEntry : Artefact
	{
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }

//		public virtual int? DriveId { 
//			get { return Drive == null ? -1 : Drive.Id; }
//		}
		
		[DataMember]
		public virtual Drive Drive { get; set; }

		[DataMember]
		public virtual string Path { get; set; }

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
		
		protected FileSystemEntry(FileSystemInfo fsInfo, Drive drive = null)
		{
			Init(fsInfo, drive);
		}
		
		protected FileSystemEntry() {}
		
		protected virtual void Init(FileSystemInfo fsInfo, Drive drive = null)
		{
			Drive = drive;
			Path = fsInfo.FullName;
			Attributes = fsInfo.Attributes;
			CreationTime = fsInfo.CreationTime;
			AccessTime = fsInfo.LastAccessTime;
			ModifyTime = fsInfo.LastWriteTime;
		}
		
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			FileSystemEntry fse = (FileSystemEntry)obj;
			return Drive == fse.Drive && Path == fse.Path;
		}
	}
}

//		public override string ToString()
//		{
//			int r = 0;
//			return ToString(ref r);
//		}

//		public virtual string ToString(ref int subClassLevel)
//		{
			
//			Type T = GetType();
			// = "[" + subClassLevel > 0 ? base.ToString(subClassLevel + 1);
			
//			string r = base.ToString(ref subClassLevel);
//			subClassLevel++;
//			for (int i = 0; i < subClassLevel; i++)
//				r += "  ";
//			r += string.Format("[" + GetType().Name + ": Drive={0} Path=\"{1}\" ModifyTime={2} ... ",
//					Drive.Id, Path, ModifyTime.ToShortTimeString());
//			//= base.ToString(10 - subClassLevel) + GetType().Name;
//			
////			r += (string)(subClassLevel > 0 ? "" : 
//			r += "]\n";
//			subClassLevel = subClassLevel + 1;
//			
//			return r;
//		}
//	}
//}

