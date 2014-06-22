using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Diagnostics;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[Disk: Serial={Serial} MostRecentHost={MostRecentHostId}] DeviceName={DeviceName}")]
	public class Disk : Artefact
	{
		public static Type[] GetArtefactTypes()
		{
			return Artefact.GetArtefactTypes();
		}
		
				private static List<Disk> _disks;
				public static List<Disk> Disks {
						get { return _disks != null ? _disks : _disks =GetDisks(Host.Current); }
				}

				private static List<Disk> GetDisks(Host host)
				{
						List<Disk> disks = new List<Disk>();
						Process lsblkProcess = Process.Start(
								new ProcessStartInfo("lsblk")
								{
										RedirectStandardOutput = true,
										RedirectStandardError = true,
										UseShellExecute = false,
								});
						lsblkProcess.WaitForExit(1600);
						string lsblkOutput;
						while (!string.IsNullOrEmpty(lsblkOutput = lsblkProcess.StandardOutput.ReadLine()))
						{
								string[] tokens = lsblkOutput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								if (lsblkOutput.Contains("disk"))
									disks.Add(new Disk(tokens[0], host));
						}
						return disks;
				}

				public virtual void RefreshDisks()
				{
						_disks = null;
				}

		[DataMember]
		public virtual string Serial { get; set; }
		
		public virtual int? MostRecentHostId {
			get
			{
				if (MostRecentHost == null)
					throw new InvalidDataException("Disk.MostRecentHost==null, which should not happen");
//					MostRecentHost = Host.Current;
				return MostRecentHost.Id;
			}
		}
		
		[DataMember]
		public virtual Host MostRecentHost { get; set; }
		
		[DataMember]
		public virtual string DeviceName { get; set; }
		
		public Disk(string deviceName, Host mostRecentHost = null)
		{
			MostRecentHost = mostRecentHost != null ? mostRecentHost : Host.Current;	
			DeviceName = deviceName;
			Update();
		}
		
		protected Disk()
		{
			
		}
		
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;
			Disk d = (Disk)obj;
			return Serial == d.Serial;
		}

				public override int GetHashCode()
		{
						return Convert.ToInt32(Serial);
		}

//		public override Artefact Update()
//		{
////			if (UpdateAge > Artefacts.Service.Repository.ArtefactUpdateAgeLimit)
////			{
//				Process getDiskSerialProcess = Process.Start(
//					new ProcessStartInfo("udevadm", string.Format("info --query=property --name={0}", DeviceName))			//  | grep ID_SERIAL=
//				{
//					RedirectStandardOutput = true,
//					RedirectStandardError = true,
//					UseShellExecute = false
//				});
//				getDiskSerialProcess.WaitForExit(1111);
//				string serialOut = getDiskSerialProcess.StandardOutput.ReadToEnd().Trim().Grep("ID_SERIAL=");
//				if (string.IsNullOrEmpty(serialOut) || !serialOut.StartsWith("ID_SERIAL="))
//					throw new InvalidDataException("Unexpected output data from udevadm command");
//	//				{
//	//					Data = Extensions.CreateDictionary<string, object>(
//	//						new KeyValuePair<string, object>[]
//	//					{ new KeyValuePair<string, object>("serialOut", serialOut) })
//	//				};
//				Serial = serialOut.Substring(10).Trim();
//				return base.Update();
////			}
//			return this;
//		}

public override void CopyMembersFrom(Artefact source)
		{
			base.CopyMembersFrom(source);
			Disk disk = (Disk)source;
			DeviceName = disk.DeviceName;
			Serial = disk.Serial;
		}
		
		public override string ToString()
		{
			return string.Concat(string.Format(
				"[Disk: Serial={0}, MostRecentHost=#{1} (HostId=#{2}), DeviceName={3}]\n",
					Serial, MostRecentHost != null ? MostRecentHost.Id.ToString() ?? "(null)" : "(null)",
					MostRecentHost != null ? MostRecentHost.HostId : string.Empty,
					DeviceName), base.ToString().Indent());
		}
	}
}

