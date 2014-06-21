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
	[DataContract]	//(IsReference = true)] Disk={Disk}
	[ArtefactFormat("[Drive: Partition={Partition} Label={Label} Format={Format} Type={Type} Size={Size} FreeSpace={FreeSpace} AvailableFreeSpace={AvailableFreeSpace}]")]
	public class Drive : Artefact
	{
		#region Static members
//		public static Type[] GetArtefactTypes() { return Artefact.GetArtefactTypes(); }

		/// <summary>
		/// Gets the partition mount paths.
		/// </summary>
		/// <value>The partition mount paths.</value>
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
		private static IDictionary<string, string> _partitionMountPaths = null;

		/// <summary>
		/// Gets or sets the repository.
		/// </summary>
		/// <value>The repository.</value>
		public static IRepository Repository { get; set; }
		
		/// <summary>
		/// Gets the drives.
		/// </summary>
		/// <returns>The drives.</returns>
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
							Repository.Update(drive.Update());
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
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets or sets the disk.
		/// </summary>
		/// <value>The disk.</value>
		[DataMember]
		public virtual Disk Disk { get; set; }
		
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DataMember]
		public virtual string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the partition.
		/// </summary>
		/// <value>The partition.</value>
		[DataMember]
		public virtual string Partition { get; set; }
		
		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>The label.</value>
		[DataMember]
		public virtual string Label { get; set; }

		/// <summary>
		/// Gets or sets the format.
		/// </summary>
		/// <value>The format.</value>
		[DataMember]
		public virtual string Format { get; set; }

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[DataMember]
		public virtual DriveType Type { get; set; }

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>The size.</value>
		[DataMember]
		public virtual long Size { get; set; }

		/// <summary>
		/// Gets or sets the free space.
		/// </summary>
		/// <value>The free space.</value>
		[DataMember]
		public virtual long FreeSpace { get; set; }

		/// <summary>
		/// Gets or sets the available free space.
		/// </summary>
		/// <value>The available free space.</value>
		[DataMember]
		public virtual long AvailableFreeSpace { get; set; }
		
		/// <summary>
		/// Gets or sets the drive info.
		/// </summary>
		/// <value>The drive info.</value>
		public DriveInfo DriveInfo {
			get { return _driveInfo; }
			set
			{
				_driveInfo = value;
				Name = _driveInfo.Name;
				Label = _driveInfo.VolumeLabel;
				Format = _driveInfo.DriveFormat;
				Type = _driveInfo.DriveType;
				Size = _driveInfo.TotalSize;
				FreeSpace = _driveInfo.TotalFreeSpace;
				AvailableFreeSpace = _driveInfo.AvailableFreeSpace;
				if (Drive.PartitionMountPaths != null)
					Partition = PartitionMountPaths.ContainsKey(Label) ? PartitionMountPaths[Label] : string.Empty;
			}
		}
		private DriveInfo _driveInfo;	
		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.Drive"/> class.
		/// </summary>
		/// <param name="driveName">Drive name.</param>
		public Drive(string driveName)
		{
			DriveInfo = new DriveInfo(driveName);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.Drive"/> class.
		/// </summary>
		/// <param name="driveInfo">Drive info.</param>
		protected Drive(DriveInfo driveInfo)
		{
			DriveInfo = driveInfo;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.Drive"/> class.
		/// </summary>
		protected Drive() {}
				
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Artefacts.FileSystem.Drive"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Artefacts.FileSystem.Drive"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Artefacts.FileSystem.Drive"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>
		///	Removed conditions:
		/// 				Disk == drive.Disk && Partition == drive.Partition
		///		&& Label == drive.Label && Format == drive.Format && Type == drive.Type && Size == drive.Size
		///		&& FreeSpace == drive.FreeSpace && AvailableFreeSpace == drive.AvailableFreeSpace;
		/// </remarks>
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			Drive drive = (Drive)obj;
			return Label == drive.Label;
		}
		
		/// <summary>
		/// Serves as a hash function for a <see cref="Artefacts.FileSystem.Drive"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode()
		{
			return string.Concat(GetType().FullName, ":",
				Disk != null ? Disk.GetHashCode().ToString() :
				Host.Current != null ? Host.Current.GetHashCode().ToString() :
				Guid.NewGuid().ToString(), ":", Name).GetHashCode();		// 			return Convert.ToInt32(Name);
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		public override Artefact Update()
		{
			DriveInfo = new DriveInfo(Name);
			return base.Update();
		}
		
		/// <summary>
		/// Copies the members from.
		/// </summary>
		/// <param name="source">Source.</param>
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
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Artefacts.FileSystem.Drive"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Artefacts.FileSystem.Drive"/>.</returns>
		public override string ToString()
		{
			return string.Concat(string.Format(
				"[Drive: Partition={0} Disk={7} Label={1} Format={2} Type={3} Size={4} FreeSpace={5} AvailableFreeSpace={6}]\n",
				Partition, Label, Format, Type, Size, FreeSpace, AvailableFreeSpace,
				Disk != null ? Disk.Id.HasValue ? Disk.Id.ToString() : "#idError!" : "(null)"), base.ToString().Indent());
		}
	}
}

