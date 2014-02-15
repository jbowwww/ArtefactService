using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;

using Artefacts.Services;

namespace Artefacts.FileSystem
{
	public class FileSystemArtefactCreator : CreatorBase
	{
		#region Properties & fields
		public int RecursionLimit = -1;
		public IRepository<Artefact> Repository { get; private set; }
		public Uri BaseUri { get; private set; }
		public string BasePath {
			get { return BaseUri.LocalPath; }
			set { BaseUri = new UriBuilder(BaseUri.Scheme, BaseUri.Host, BaseUri.Port, value, BaseUri.Query).Uri; }
		}
		public string Match {
			get { return BaseUri.Query; }
			set { BaseUri = new UriBuilder(BaseUri.Scheme, BaseUri.Host, BaseUri.Port, BaseUri.LocalPath, value).Uri; }
		}
		#endregion
		
		public FileSystemArtefactCreator(IRepository<Artefact> repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;
			BaseUri = new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "/", "?*").Uri;
		}

		#region implemented abstract members of Artefacts.CreatorBase
		public override void Run(object param)
		{
			Drive.Repository = Repository;
			int recursionDepth = -1;
			Queue<Uri> subDirectories = new Queue<Uri>(new Uri[] { BaseUri });
			while (subDirectories.Count > 0)
			{
				Uri currentUri = subDirectories.Dequeue();
//				Queryable<Artefact> q =
//					(Queryable<Artefact>)
				var q =
					(from a in Repository.Artefacts.AsEnumerable().OfType<Drive>()
					where currentUri.LocalPath.StartsWith(a.Label)			/*	a is Drive && 	a.GetType() == typeof(Drive) a is Drive && */
					select a);
				Drive drive = (Drive)q.FirstOrDefault();
//			IQueryable<Drive> qDrives = Drive.GetDrives();
//				Drive drive = (from d in Repository.Artefacts.OfType<Drive>()				// Drive.GetDrives() 				//qDrives
//					orderby d.Label.Length
//					where currentUri.LocalPath.StartsWith(d.Label)
//					select d).FirstOrDefault();

				foreach (string relPath in EnumerateFiles(currentUri))
				{
					string absPath = Path.Combine(currentUri.LocalPath, relPath);
					File file = (from f in Repository.Artefacts.AsEnumerable().OfType<File>()
						where f.Path == absPath
						select f).FirstOrDefault();
					if (file == null)
						Repository.Add(new File(absPath));
					else 		// if (file.UpdateAge > TimeSpan.FromMinutes(1))
						Repository.Update(file.Update());
				}

				if (RecursionLimit < 0 || ++recursionDepth < RecursionLimit)
				{
					foreach (string relPath in EnumerateDirectories(currentUri))
					{
						string absPath = Path.Combine(currentUri.LocalPath, relPath);
						Directory dir = (from d in Repository.Artefacts.AsEnumerable().OfType<Directory>()
							where d.Path == absPath
							select d).FirstOrDefault();
						if (dir == null)
							Repository.Add(new Directory(new System.IO.DirectoryInfo(absPath), drive));
						else if (dir.UpdateAge > TimeSpan.FromMinutes(1))
							Repository.Update(new Directory(new System.IO.DirectoryInfo(absPath), drive) { Id = dir.Id, TimeCreated = dir.TimeCreated });
						subDirectories.Enqueue(new Uri(currentUri, relPath));
					}
				}
			}
		}
		#endregion
		
		#region Overridable file system entry  (files and directories) enumerators
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
		#endregion
	}
}

