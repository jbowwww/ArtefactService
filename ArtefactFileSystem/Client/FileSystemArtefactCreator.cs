using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Artefacts.Services;

namespace Artefacts.FileSystem
{
	public class FileSystemArtefactCreator : CreatorBase
	{
		private RepositoryClientProxy<Artefact> _repoProxy = null;
		private int recursionDepth;

		public int RecursionLimit;

		public Uri BaseUri;

		public string Pathmatch {
			get { return BaseUri.Query; }
			set {
				BaseUri =  BaseUri == null ?
					new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "", "").Uri		//	Uri.EscapeDataString("*")
				:	new UriBuilder(BaseUri.Scheme, BaseUri.Host, 0, BaseUri.LocalPath, value).Uri;
			}
		}

		public FileSystemArtefactCreator(RepositoryClientProxy<Artefact> repoProxy)
		{
			_repoProxy = repoProxy;
			BaseUri = new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "", "").Uri;		//Uri.EscapeDataString("*")
			RecursionLimit = -1;
		}

		#region implemented abstract members of Artefacts.CreatorBase
		public override void Run(object param)
		{
//			try
//			{
			DriveInfo[] driveInfos = DriveInfo.GetDrives();
			Drive[] drives = new Drive[driveInfos.Length];
				
			for (int i = 0; i < driveInfos.Length; i++)
//					base.NotifyCreate((Artefact)(drives[i] = new Drive(driveInfos[i])));
			{
				IQueryable<Artefact> r =
					from a in _repoProxy.Artefacts
					where
						a.GetType() == typeof(Drive) &&
						(a as Drive).Label == driveInfos[i].VolumeLabel//&&
//						a.UpdateAge > TimeSpan.FromMinutes(1)
					select a;
//					_repoProxy.RunQuery(
//						(a) =>
//				{
//					if (a.GetType() == typeof(Drive))
//					{
//						Drive d = (Drive)a;
//						if (d.Disk.MostRecentHost.HostId == Host.Current.HostId && d.Label == driveInfos[i].VolumeLabel)
//							return true;
//								
//					}
//					return false;
//				});
//				r.ArtefactRepository = _repoProxy;
				drives[i] = (Drive)r.SingleOrDefault();
				if (drives[i] == null)
					_repoProxy.Add(drives[i] = new Drive(driveInfos[i]));
				else if (drives[i].UpdateAge > TimeSpan.FromMinutes(1))
					_repoProxy.Update(drives[i] = new Drive(driveInfos[i]) { Id = drives[i].Id, TimeCreated = drives[i].TimeCreated });
			}

			recursionDepth = -1;
			Queue<Uri> subDirectories = new Queue<Uri>(new Uri[] { BaseUri });
			while (subDirectories.Count > 0)
			{
				Uri currentUri = subDirectories.Dequeue();
				Drive drive = null;
				foreach (Drive d in drives)
					if (currentUri.LocalPath.StartsWith(d.Label))
						drive = d;

				foreach (string relPath in EnumerateFiles(currentUri))
//						base.NotifyCreate(new File(new System.IO.FileInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));
				{
					string absPath = Path.Combine(currentUri.LocalPath, relPath);
					IQueryable<Artefact> r = from a in _repoProxy.Artefacts
						where
							a.GetType() == typeof(Artefacts.FileSystem.File) &&
							(a as Artefacts.FileSystem.File).Path == absPath
						select a;
//						_repoProxy.RunQuery(
//							(a) =>
//					{
//						if (a.GetType() == typeof(File))
//						{
//							File f = (File)a;
//							if (f.Path == absPath)
//								return true;
//						}
//						return false;
//					});
//					r.ArtefactRepository = _repoProxy;
					File file = (File)r.SingleOrDefault();
					if (file == null)
						_repoProxy.Add(new File(new System.IO.FileInfo(absPath), drive));
					else if (file.UpdateAge > TimeSpan.FromMinutes(1))
						_repoProxy.Update(new File(new System.IO.FileInfo(absPath), drive) { Id = file.Id, TimeCreated = file.TimeCreated });
				}

				if (RecursionLimit < 0 || ++recursionDepth < RecursionLimit)
				{
					foreach (string relPath in EnumerateDirectories(currentUri))
					{
//							base.NotifyCreate(new Directory(new DirectoryInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));
						string absPath = Path.Combine(currentUri.LocalPath, relPath);
						IQueryable<Artefact> r = from a in _repoProxy.Artefacts
							where
								a.GetType() == typeof(Artefacts.FileSystem.Directory) &&
								(a as Artefacts.FileSystem.Directory).Path == absPath
							select a;
//							_repoProxy.RunQuery(
//								(a) =>
//						{
//							if (a.GetType() == typeof(Directory))
//							{
//								Directory d = (Directory)a;
//								if (d.Path == absPath)
//									return true;
//							}
//							return false;
//						});
//						r.ArtefactRepository = _repoProxy;
						Directory dir = (Directory)r.SingleOrDefault();
						if (dir == null)
							_repoProxy.Add(new Directory(new System.IO.DirectoryInfo(absPath), drive));
						else if (dir.UpdateAge > TimeSpan.FromMinutes(1))
							_repoProxy.Update(new Directory(new System.IO.DirectoryInfo(absPath), drive) { Id = dir.Id, TimeCreated = dir.TimeCreated });
						subDirectories.Enqueue(new Uri(currentUri, relPath));
					}
				}
			}
//				NotifyComplete();
//			}
//			catch (FaultException<ExceptionDetail> ex)
//			{
////				NotifyError(ex);
//			}
//			catch (Exception ex)
//			{
////				NotifyError(ex);
//			}
		}
		#endregion

		public virtual IEnumerable<string> EnumerateFiles(Uri uri)
		{
			if (!uri.IsFile)
				throw new NotImplementedException("Only URIs with a file schema are currently supported");
			return System.IO.Directory.EnumerateFiles(uri.LocalPath, "*", SearchOption.TopDirectoryOnly);
		}

		public virtual IEnumerable<string> EnumerateDirectories(Uri uri)
		{
			if (!uri.IsFile)
				throw new NotImplementedException("Only URIs with a file schema are currently supported");
			return System.IO.Directory.EnumerateDirectories(uri.LocalPath, "*", SearchOption.TopDirectoryOnly);
		}
	}
}

