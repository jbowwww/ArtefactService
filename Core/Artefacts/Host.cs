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

// TODO: Connection closing/timeout
		public virtual int ConnectionId { get; set; }

		public virtual bool Connected { get { return ConnectionId >= 0; } }
		
		public virtual DateTime ConnectTime { get; set; }
		
		public virtual TimeSpan ConnectionAge { get { return ConnectTime == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now - ConnectTime; } }
		
		public Host()
		{
			ConnectionId = -1;
			ConnectTime = DateTime.MinValue;
			HostId = GetHostId();
		}

/// <summary>
/// Not suyre this oiperation entirely makes sense for a Host - what will it ever update?? Besides a timestamp?
/// </summary>
		public override Artefact Update()
		{
			if (UpdateAge > Artefact.UpdateAgeLimit)
			{
				if (GetHostId().CompareTo(HostId) != 0)
					throw new ApplicationException(string.Format("HostId has somehow changed!! From {0} to {1}", HostId, GetHostId()));
				return base.Update();
			}
			return this;
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
		
		public override string ToString()
		{
			return string.Format("[Host: HostId={0}]", HostId);
		}
	}
}

