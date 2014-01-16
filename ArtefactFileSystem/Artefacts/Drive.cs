using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormatString("[Drive: Disk={DiskId} Partition={Partition} Label={Label} Format={Format} Type={Type} Size={Size} FreeSpace={FreeSpace} AvailableFreeSpace={AvailableFreeSpace}]")]
	public class Drive : Artefact
	{
		public static Type[] GetArtefactTypes()
		{
			return Artefact.GetArtefactTypes();
		}
		
		private static IDictionary<string, string> _partitionMountPaths = null;
		public static IDictionary<string, string> PartitionMountPaths {
			get
			{
				if (_partitionMountPaths == null)
				{
					_partitionMountPaths = new ConcurrentDictionary<string, string>();
				
					Process getMountProcess = Process.Start(
						new ProcessStartInfo("mount")
						{
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							UseShellExecute = false
						});
					getMountProcess.WaitForExit(1600);
					
					string mountOutput;
					string[] splitOutput;
					while (!string.IsNullOrEmpty(mountOutput = getMountProcess.StandardOutput.ReadLine()))
					{
						splitOutput = mountOutput.Split(' ');
						if (splitOutput.Length <= 5 || splitOutput[1] != "on" || splitOutput[3] != "type")
							throw new InvalidDataException("Unexpected output data from mount command");
						_partitionMountPaths[splitOutput[2]] = splitOutput[0];
					}
				}
				return _partitionMountPaths;				
			}
		}
		
		public virtual int? DiskId {
			get { return Disk == null ? null : Disk.Id; }
		}

		[DataMember]
		public virtual Disk Disk { get; set; }
		
		[DataMember]
		public virtual string Partition { get; set; }
		
		[DataMember]
		public virtual string Label { get; set; }

		[DataMember]
		public virtual string Format { get; set; }

		[DataMember]
		public virtual DriveType Type { get; set; }

		[DataMember]
		public virtual long Size { get; set; }

		[DataMember]
		public virtual long FreeSpace { get; set; }

		[DataMember]
		public virtual long AvailableFreeSpace { get; set; }

		public Drive(DriveInfo dInfo)
		{
			Label = dInfo.VolumeLabel;
			Format = dInfo.DriveFormat;
			Type = dInfo.DriveType;
			Size = dInfo.TotalSize;
			FreeSpace = dInfo.TotalFreeSpace;
			AvailableFreeSpace = dInfo.AvailableFreeSpace;
			if (Drive.PartitionMountPaths != null)
				Partition = PartitionMountPaths.ContainsKey(Label) ? PartitionMountPaths[Label] : string.Empty;
			if (!string.IsNullOrEmpty(Partition) && Partition.StartsWith("/dev/"))
				Disk = new Disk(Partition
					.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' })
					.Substring(Partition.LastIndexOfAny(new char[] { '/', '\\' }) + 1));
		}

		protected Drive()
		{
		}
		
//		public override string ToString()
//		{
//			return (this as IFormattable).ToString("", null);
//		}

//		#region IFormattable implementation
//		string IFormattable.ToString(string format, IFormatProvider formatProvider)
//		{
//			return string.Format("[Drive: Label={0}, Format={1}, Type={2}, Size={3}, FreeSpace={4}, AvailableFreeSpace={5}]",
//				Label, Format, Type, Size, FreeSpace, AvailableFreeSpace);
//		}
//		#endregion
	}
}

