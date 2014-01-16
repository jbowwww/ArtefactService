using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormatString("[Host: HostId={HostId}]")]
	public class Host : Artefact
	{
		public static Type[] GetArtefactTypes()
		{
			return Artefact.GetArtefactTypes();
		}
		
		private static Host _current = null;
		
		public static Host Current {
			get
			{
				if (_current == null)
					_current = new Host(true);
				return _current;
			}
		}
		
		[DataMember]
		public virtual string HostId { get; set; }
		
		public Host(bool createNew = true)
		{
			if (createNew)
			{
//				System.AppDomain.CurrentDomain.DomainManager.EntryAssembly.HostContext;
				Process getDiskSerialProcess = Process.Start(
				new ProcessStartInfo("hostid")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				});
				getDiskSerialProcess.WaitForExit(1111);
				HostId = getDiskSerialProcess.StandardOutput.ReadLine();
				if (string.IsNullOrEmpty(HostId))
					throw new InvalidDataException("Unexpected output data from hostid command");
//					{
//						Data = Extensions.CreateDictionary<string, object>(
//							new KeyValuePair<string, object>[]
//						{ new KeyValuePair<string, object>("HostId", HostId) })
//					};
			}
		}
		
		protected Host()
		{			
		}
	}
}

