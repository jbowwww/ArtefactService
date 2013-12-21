using System;
using System.Collections.Generic;
using System.IO;
using Artefacts.Services;

namespace Artefacts.FileSystem
{
	public class FileSystemArtefactCreator : CreatorBase
	{
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

		public FileSystemArtefactCreator ()
		{
			BaseUri = new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "", "").Uri;		//Uri.EscapeDataString("*")
			RecursionLimit = -1;
		}

		#region implemented abstract members of Artefacts.CreatorBase
		public override void Run(object param)
		{
			try
			{
//				IArtefactRepository<Artefact> proxy = (IArtefactRepository<Artefact>)param;
				
				DriveInfo[] driveInfos = DriveInfo.GetDrives();
				Drive[] drives = new Drive[driveInfos.Length];
				for (int i = 0; i < driveInfos.Length; i++)
//					proxy.Add(drives[i] = new Drive(driveInfos[i]));
					base.NotifyCreate((Artefact)(drives[i] = new Drive(driveInfos[i])));

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
//						proxy.Add(new File(new System.IO.FileInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));
						base.NotifyCreate(new File(new System.IO.FileInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));

					if (RecursionLimit < 0 || ++recursionDepth < RecursionLimit)
					{
						foreach (string relPath in EnumerateDirectories(currentUri))
						{
							subDirectories.Enqueue(new Uri(currentUri, relPath));
//							proxy.Add(new Directory(new DirectoryInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));
							base.NotifyCreate(new Directory(new DirectoryInfo(Path.Combine(currentUri.LocalPath, relPath)), drive));
						}
					}
				}
				NotifyComplete();
			}
			catch (Exception ex)
			{
				NotifyError(ex);
			}
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

