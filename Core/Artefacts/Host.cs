using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Artefacts
{
	[DataContract]	//(IsReference = true)]
	[ArtefactFormat("[Host: HostId={HostId}]")]
	public class Host : Artefact
	{
		public static Type[] GetArtefactTypes()
		{
			return Artefact.GetArtefactTypes();
		}
		
		private static Host _current = null;
		public static Host Current {
			get; set;
//			get
//			{
//				if (_current == null)
//				{
//					Host tempNewHost = new Host();
//					_current = 
//					
//				}
//				return _current;
//			}
		}
		
		public static string GetHostId()
		{
			string hostId;
		Process getDiskSerialProcess = Process.Start(
				new ProcessStartInfo("hostid")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				});
				getDiskSerialProcess.WaitForExit(1111);
				hostId = getDiskSerialProcess.StandardOutput.ReadLine();
				if (string.IsNullOrEmpty(hostId))
					throw new InvalidDataException("Unexpected output data from hostid command");
					return hostId;
		}
		
		[DataMember]
		public virtual string HostId { get; set; }

		public Host()
		{
			HostId = GetHostId();
		}

		public override Artefact Update()
		{
//			if (UpdateAge > Artefacts.Service.Repository.ArtefactUpdateAgeLimit)
				return base.Update();
//			return this;
		}		

		public override void CopyMembersFrom(Artefact source)
		{
			base.CopyMembersFrom(source);
			HostId = ((Host)source).HostId;
		}		
		
		public override bool Equals(object obj)
		{
			if (System.Object.ReferenceEquals(this, obj))
				return true;
			if (obj == null || obj.GetType() != this.GetType())
				return false;
			Host h = (Host)obj;
			return base.Equals(obj) && HostId == h.HostId;
		}

				public override int GetHashCode()
		{
						return Convert.ToInt32(HostId);
		}
	}
}

