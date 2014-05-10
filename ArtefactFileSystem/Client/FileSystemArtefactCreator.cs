using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Reflection;

using Artefacts.Services;

namespace Artefacts.FileSystem
{
	public class FileSystemArtefactCreator : CreatorBase
	{
		#region Thread-static singleton instance
		[ThreadStatic]
				public static FileSystemArtefactCreator Singleton;
		#endregion
		
		#region Properties & fields
		public int RecursionLimit = -1;
		public Uri BaseUri { get; private set; }
		public string BasePath {
			get { return BaseUri.LocalPath; }
			set { BaseUri = new UriBuilder(BaseUri.Scheme, BaseUri.Host, BaseUri.Port, value, BaseUri.Query).Uri; }
		}
		public string Match {
			get { return BaseUri.Query; }
			set { BaseUri = new UriBuilder(BaseUri.Scheme, BaseUri.Host, BaseUri.Port, BaseUri.LocalPath, value).Uri; }
		}
		
		public IRepository<Artefact> Repository { get; private set; }
//		public IQueryable Artefacts { get; private set; }
		public IQueryable FileEntries {
			get { return Repository.Queryables[typeof(FileSystemEntry)]; }
			private set { Repository.Queryables[typeof(FileSystemEntry)] = value; }
		}
		public IQueryable Files {
			get { return Repository.Queryables[typeof(File)]; }
			private set { Repository.Queryables[typeof(File)] = value; }
		}
		public IQueryable Directories {
			get { return Repository.Queryables[typeof(Directory)]; }
			private set { Repository.Queryables[typeof(Directory)] = value; }
		}
		public IQueryable Drives {
			get { return Repository.Queryables[typeof(Drive)]; }
			private set { Repository.Queryables[typeof(Drive)] = value; }
		}
		public IQueryable Disks {
			get { return Repository.Queryables[typeof(Disk)]; }
			private set { Repository.Queryables[typeof(Disk)] = value; }
		}
		public Host ThisHost = new Host(true);
		#endregion
		
		#region Constructors & Initialization
		public FileSystemArtefactCreator(IRepository<Artefact> repository)
		{
			if (Singleton != null)
				throw new InvalidOperationException("FileSystemArtefactCreator.c'tor: Singleton is not null");
			Singleton = this;
			
			BaseUri = new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "/", "?*").Uri;
			
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;
			
//			Artefacts = Repository.Artefacts.OfType
			FileEntries = QueryBase<FileSystemEntry>();
			Files = QueryBase<File>();
			Directories = QueryBase<Directory>();
			Drives = QueryBase<Drive>();
			Disks = QueryBase<Disk>();			
		}
//			BuildTypedQueryable<Artefacts.FileSystem.FileSystemEntry>();
//			BuildTypedQueryable<Artefacts.FileSystem.File>();
//			BuildTypedQueryable<Artefacts.FileSystem.Directory>();
//			BuildTypedQueryable<Artefacts.FileSystem.Drive>();
//			BuildTypedQueryable<Artefacts.FileSystem.Disk>();
			                          
		internal IQueryable QueryBase<TArtefact>()
		{
			IQueryable q = Repository.Artefacts.OfType<TArtefact>().AsQueryable();
			return q;
		}
//			const BindingFlags bf = BindingFlags.Public | BindingFlags.Static;
//			Expression expression =
//				Expression.Call(typeof(NHibernate.Linq.LinqExtensionMethods), "Query", new Type[] { typeof(TArtefact) },
//				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", bf).GetGetMethod()));
//			return Repository.Artefacts.Provider.CreateQuery(expression);				//			Repository.Queryables.Add(typeof(TArtefact),
//		}
		#endregion
		
		#region implemented abstract members of Artefacts.CreatorBase
		public override void Run(object param)
		{
			// Initialise
			Drive.Repository = Repository;
			int recursionDepth = -1;
			Queue<Uri> subDirectories = new Queue<Uri>(new Uri[] { BaseUri });
			
			// Using the initial BaseUri(s), create queries for directories and files underneath the directory path,
			// and get the Drive(s) that the initial directory(s) are positioned on
			
//						Disks = new query
			
			// Recurse subdirectories
			while (subDirectories.Count > 0)
			{
								Disk[] Disks = Disk.Disks.ToArray();

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

