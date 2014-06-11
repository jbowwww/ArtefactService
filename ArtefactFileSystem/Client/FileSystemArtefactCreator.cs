using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Reflection;
using Artefacts.Service;

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

		public RepositoryClientProxy Repository { get; private set; }
//		public SynchronizedReadOnlyCollection<FileSystemEntry> files = new SynchronizedReadOnlyCollection<FileSystemEntry>(this, )
		//		public IQueryable Artefacts { get; private set; }
		public IQueryable<FileSystemEntry> FileEntries;
//		 {
//			get { return (IQueryable<FileSystemEntry>)Repository.Queryables[typeof(FileSystemEntry)]; }
//			private set { Repository.Queryables[typeof(FileSystemEntry)] = value; }
//		}

		public IQueryable<File> Files;
		// {
//			get { return (IQueryable<File>)Repository.Queryables[typeof(File)]; }
//			private set { Repository.Queryables[typeof(File)] = value; }
//		}

		public IQueryable<Directory> Directories;
		//  {
//			get { return (IQueryable<Directory>)Repository.Queryables[typeof(Directory)]; }
//			private set { Repository.Queryables[typeof(Directory)] = value; }
//		}

		public IQueryable<Drive> Drives;
		// {
//			get { return (IQueryable<Drive>)Repository.Queryables[typeof(Drive)]; }
//			private set { Repository.Queryables[typeof(Drive)] = value; }
//		}

		public IQueryable<Disk> Disks;
		//  {
//			get { return (IQueryable<Disk>)Repository.Queryables[typeof(Disk)]; }
//			private set { Repository.Queryables[typeof(Disk)] = value; }
//		}

		public Host ThisHost = new Host(true);
		#endregion

		#region Constructors & Initialization
		public FileSystemArtefactCreator(RepositoryClientProxy repository)
		{
			if (Singleton != null)
				throw new InvalidOperationException("FileSystemArtefactCreator.c'tor: Singleton is not null");
			Singleton = this;
			
			BaseUri = new UriBuilder(Uri.UriSchemeFile, "localhost", 0, "/", "?*").Uri;
			
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;

			FileEntries = Repository.BuildBaseQuery<FileSystemEntry>();

			Files = Repository.BuildBaseQuery<File>();
			Directories = Repository.BuildBaseQuery<Directory>();
			Drives = Repository.BuildBaseQuery<Drive>();
			Disks = Repository.BuildBaseQuery<Disk>();
		}
		//			BuildTypedQueryable<Artefacts.FileSystem.FileSystemEntry>();
		//			BuildTypedQueryable<Artefacts.FileSystem.File>();
		//			BuildTypedQueryable<Artefacts.FileSystem.Directory>();
		//			BuildTypedQueryable<Artefacts.FileSystem.Drive>();
		//			BuildTypedQueryable<Artefacts.FileSystem.Disk>();
//		internal IQueryable<TArtefact> QueryBase<TArtefact>() where TArtefact : Artefact
//		{
//						var q = Repository.Artefacts.OfType<TArtefact>().Select((arg) => (TArtefact)arg);//Where((a) => a is TArtefact).Select((a) => (TArtefact)a);
//						//;Where((arg) => arg.GetType().FullName == typeof(Artefact).FullName)//.AsQueryable();
//						                                //.Where((arg) => arg.GetType().FullName == typeof(Artefact).FullName);
//						                                //
//			return q;
			const BindingFlags bf = BindingFlags.Public | BindingFlags.Static;
//			Expression expression =
//				Expression.Call(typeof(NHibernate.Linq.LinqExtensionMethods), "Query", new Type[] { typeof(TArtefact) },
//					Expression.Call(typeof(ArtefactRepository).GetProperty("Session", bf).GetGetMethod()));
//			Expression expression = Expression.Parameter(typeof(IQueryable<TArtefact>), string.Concat("Artefacts:", typeof(TArtefact).FullName));
//			Expression expression = Expression.Call(typeof(System.Linq.Queryable), "OfType", new Type[] { typeof(TArtefact) },
//				Expression.Parameter(typeof(IQueryable<Artefact>), "Artefacts"));
//			return Repository.Artefacts.Provider.CreateQuery<TArtefact>(expression);				//			Repository.Queryables.Add(typeof(TArtefact),
//			return Repository.Artefacts.OfType<TArtefact>();
//		}
		#endregion

		#region implemented abstract members of Artefacts.CreatorBase
		public override void Run(object param)
		{
			// Initialise 
			Drive.Repository = Repository;
			int recursionDepth = -1;
			Queue<Uri> subDirectories = new Queue<Uri>(new Uri[] { BaseUri });
			Disk[] Disks = Disk.Disks.ToArray();

			// Recurse subdirectories
			while (subDirectories.Count > 0)
			{
				Uri currentUri = subDirectories.Dequeue();
//				Drive drive = Drives.FirstOrDefault((d) => currentUri.LocalPath.StartsWith(d.Label));
				var q = from dr in Drives
				where currentUri.LocalPath.StartsWith(dr.Label)
				select dr;
				Drive drive;
//				drive = q.FirstOrDefault();
				drive = q.Take(1).ToArray().FirstOrDefault();

				foreach (string relPath in EnumerateFiles(currentUri))
				{
					string absPath = Path.Combine(currentUri.LocalPath, relPath);
					File file = Files.FirstOrDefault((f) => f.Path == absPath);
					if (file == null)
						Repository.Add(new File(absPath));
					else
						Repository.Update(file.Update());
				}

				if (RecursionLimit < 0 || ++recursionDepth < RecursionLimit)
				{
					foreach (string relPath in EnumerateDirectories(currentUri))
					{
						string absPath = Path.Combine(currentUri.LocalPath, relPath);
						Directory dir = Directories.FirstOrDefault((d) => d.Path == absPath);
						if (dir == null)
							Repository.Add(new Directory(new System.IO.DirectoryInfo(absPath), drive));
						else if (dir.UpdateAge > TimeSpan.FromMinutes(1))
							Repository.Update(
								new Directory(
									new System.IO.DirectoryInfo(absPath),
									drive)
								{
									Id = dir.Id,
									TimeCreated = dir.TimeCreated
								});
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

