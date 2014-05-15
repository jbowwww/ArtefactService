using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Diagnostics;

using Artefacts.Service;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[Drive: Disk={Disk} Partition={Partition} Label={Label} Format={Format} Type={Type} Size={Size} FreeSpace={FreeSpace} AvailableFreeSpace={AvailableFreeSpace}]")]
	public class Drive : Artefact
	{
		#region Static members
		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }
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
							UseShellExecute = false,
						});
					getMountProcess.WaitForExit(1600);
					string mountOutput;
					while (!string.IsNullOrEmpty(mountOutput = getMountProcess.StandardOutput.ReadLine()))
					{
						string[] splitOutput = mountOutput.Split(' ');
						if (splitOutput.Length <= 5 || splitOutput[1] != "on" || splitOutput[3] != "type")
							throw new InvalidDataException("Unexpected output data from mount command");
						_partitionMountPaths[splitOutput[2]] = splitOutput[0];
					}
				}
				return _partitionMountPaths;				
			}
		}
		public static IRepository<Artefact> Repository { get; set; }
		
		public static IQueryable<Drive> GetDrives()
		{
			List<Drive> drives = new List<Drive>();
			foreach (DriveInfo dInfo in DriveInfo.GetDrives())
			{
				Drive drive = null;
				try
				{
					if (Repository != null)
					{
						drive = ((IQueryable<Drive>)Repository.Queryables[typeof(Drive)]).FirstOrDefault((d) => d.Label == dInfo.VolumeLabel);
						if (drive == null)
							Repository.Add(drive = new Drive(dInfo));
						else
							Repository.Update(drive.Update(dInfo));
					}
					else
						drive = new Drive(dInfo);
					if (drive == null)
						throw new NullReferenceException("drive is null");
					drives.Add(drive);
				}
				catch (UnauthorizedAccessException ex)
				{
					// this is OK to continue from, just ignore & omit that drive - for now output for debug though
					Console.WriteLine("\n{0}\n", ex.ToString());
				}
			}
			return drives.AsQueryable();
		}
		
		public static Drive GetDrive(string rootPath)
		{
//						var q =c
//								from dr in FileSystemArtefactCreator.Singleton.Drives
//									orderby dr.Label.Length descending
//										where rootPath.StartsWith(dr.Label)
//				select dr;
						IQueryable<Drive> q = Repository.Artefacts
								.Where((arg) => arg.GetType().FullName == typeof(Drive).FullName)
								.Select((arg) => (Drive)arg)
								.OrderByDescending((d) => d.Label.Length);
//								.Where((d) => rootPath.StartsWith(d.Label));
//						FileSystemArtefactCreator.Singleton.Drives
						Drive dr = q.FirstOrDefault();
						return dr;
//								orderby dr.Label.Length descending
//										where rootPath.StartsWith(dr.Label)
//								select dr;
//						return q.FirstOrDefault();
		}
//			return (from dr in Drive.GetDrives().AsEnumerable()
		
		public static Drive GetOrCreate(string driveName)
		{
						throw new NotImplementedException();
		}
		#endregion
		
		#region Properties
//		public virtual int? DiskId {
//			get { return Disk == null ? null : Disk.Id; }
//		}

		[DataMember]
		public virtual Disk Disk { get; set; }
		
		[DataMember]
		public virtual string Name { get; set; }
		
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
		#endregion
		
		public Drive(string driveName)
		{
			Init(new DriveInfo(driveName));
		}
		
		protected Drive(DriveInfo dInfo)
		{
			Init(dInfo);
		}

		protected Drive() {}
		
		protected virtual void Init(DriveInfo dInfo)
		{
			Name = dInfo.Name;
			Label = dInfo.VolumeLabel;
			Format = dInfo.DriveFormat;
			Type = dInfo.DriveType;
			Size = dInfo.TotalSize;
			FreeSpace = dInfo.TotalFreeSpace;
			AvailableFreeSpace = dInfo.AvailableFreeSpace;
			if (Drive.PartitionMountPaths != null)
				Partition = PartitionMountPaths.ContainsKey(Label) ? PartitionMountPaths[Label] : string.Empty;
//			if (!string.IsNullOrEmpty(Partition) && Partition.StartsWith("/dev/"))
//			{
//				Disk disk = new Disk(Partition
//					.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' })
//					.Substring(Partition.LastIndexOfAny(new char[] { '/', '\\' }) + 1));
//				if (Disk == null)
//					Disk = disk;
//				else
//					Disk.CopyMembersFrom(disk);
//			}			
		}
		
		public override Artefact Update()
		{
			base.Update();
			Init(new DriveInfo(Name));
			return this;
		}
		
		protected virtual Artefact Update(DriveInfo dInfo)
		{
			base.Update();
			Init(dInfo);
			return this;
		}
		
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			Drive drive = (Drive)obj;
			return Label == drive.Label;
		}
//				Disk == drive.Disk && Partition == drive.Partition
//				&& Label == drive.Label && Format == drive.Format && Type == drive.Type && Size == drive.Size
//				&& FreeSpace == drive.FreeSpace && AvailableFreeSpace == drive.AvailableFreeSpace;
		
				public override int GetHashCode()
		{
						return Convert.ToInt32(Name);
		}

		public override void CopyMembersFrom(Artefact source)
		{
			base.CopyMembersFrom(source);
			Drive srcDrive = (Drive)source;
			Disk = srcDrive.Disk;
			Partition = srcDrive.Partition;
			Label = srcDrive.Label;
			Format = srcDrive.Format;
			Type = srcDrive.Type;
			Size = srcDrive.Size;
			FreeSpace = srcDrive.FreeSpace;
			AvailableFreeSpace = srcDrive.AvailableFreeSpace;
		}
	}
}

