using System;
//using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Linq;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[FileSystemEntry: Drive={Drive} Path={Path} Attributes={Attributes} CreationTime={CreationTime} AccessTime={AccessTime} ModifyTime={ModifyTime}]")]
	public class FileSystemEntry : Artefact
	{
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }

		[DataMember]
		public virtual Drive Drive { get; set; }

		[DataMember]													// think I need this for NHibernate/serialization so it can use default parameterless constructor and then set
		public virtual string Path { get; set; }	// the property however confirm this, check if maybe NH uses a c'tor with SerializationInfo & context params
		
		[DataMember]
		public virtual System.IO.FileAttributes Attributes { get; set; }

		[DataMember]
		public virtual DateTime CreationTime { get; set; }

		[DataMember]
		public virtual DateTime AccessTime { get; set; }

		[DataMember]
		public virtual DateTime ModifyTime { get; set; }

		[DataMember]
		public virtual Directory Directory { get; set; }
		
		public virtual System.IO.FileSystemInfo FileSystemInfo {
			get { return _fileSystemInfo; }
			set
			{
				if (_fileSystemInfo != null && !_fileSystemInfo.FullName.Equals(value.FullName))
					throw new InvalidOperationException("Cannot set FileSystemEntry.FileSystemInfo to an instance with a different Path value due to hash code requirements");
				_fileSystemInfo = value;
				Path = _fileSystemInfo.FullName;
				Attributes = _fileSystemInfo.Attributes;
				CreationTime = _fileSystemInfo.CreationTime;
				AccessTime = _fileSystemInfo.LastAccessTime;
				ModifyTime = _fileSystemInfo.LastWriteTime;
				if (FileSystemArtefactCreator.Singleton != null)
				{
					if (FileSystemArtefactCreator.Singleton.Drives != null)
						Drive = FileSystemArtefactCreator.Singleton.Drives.FromPath(Path);
					if (FileSystemArtefactCreator.Singleton.Directories != null)
						Directory = (Directory)FileSystemArtefactCreator.Singleton.Directories.FromPath(Path);
				}
			}
		}
		private System.IO.FileSystemInfo _fileSystemInfo;
		
		protected FileSystemEntry(System.IO.FileSystemInfo fileSystemInfo)
		{
			FileSystemInfo = fileSystemInfo;
		}
		
		protected FileSystemEntry() {}		
		
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			FileSystemEntry fse = (FileSystemEntry)obj;
			return /*Drive == fse.Drive && */ Path.Equals(fse.Path);
		}

		public override int GetHashCode()
		{
			return string.Concat(GetType().FullName, ":", Path).GetHashCode();			// Convert.ToInt32(Path);
		}			/*  _hashCode.HasValue ? _hashCode.Value : (_hashCode = .... ).Value */

		public override string ToString()
		{
			return string.Concat(string.Format(string.Concat(
				"[FileSystemEntry: Drive=#{0} Path={1} Directory=#{2} Attributes={3} CreationTime={4} AccessTime={5} ModifyTime={6}]\n"),
				Drive != null ? Drive.Id.HasValue ? Drive.Id.Value.ToString() : "int?" : "(null)", Path,
				Directory != null ? Directory.Id.ToString() : "(null)", Attributes, CreationTime, AccessTime, ModifyTime), base.ToString().Indent());
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

