using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts.FileSystem
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormatString("[Disk: Serial={Serial} MostRecentHost={MostRecentHostId}] DeviceName={DeviceName}")]
	public class Disk : Artefact
	{
		public static Type[] GetArtefactTypes()
		{
			return Artefact.GetArtefactTypes();
		}
		
		[DataMember]
		public virtual string Serial { get; set; }
		
		public virtual int? MostRecentHostId {
			get
			{
				if (MostRecentHost == null)
					throw new InvalidDataException("Disk.MostRecentHost==null, which should not happen");
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
			Process getDiskSerialProcess = Process.Start(
				new ProcessStartInfo("udevadm", string.Format("info --query=property --name={0} | grep ID_SERIAL=", deviceName))
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			});
			getDiskSerialProcess.WaitForExit(1111);
			string serialOut = getDiskSerialProcess.StandardOutput.ReadLine();
			if (string.IsNullOrEmpty(serialOut) || !serialOut.StartsWith("ID_SERIAL="))
				throw new InvalidDataException("Unexpected output data from udevadm command");
//				{
//					Data = Extensions.CreateDictionary<string, object>(
//						new KeyValuePair<string, object>[]
//					{ new KeyValuePair<string, object>("serialOut", serialOut) })
//				};
			Serial = serialOut.Substring(10);
		}
		
		protected Disk()
		{
			
		}
	}
}

