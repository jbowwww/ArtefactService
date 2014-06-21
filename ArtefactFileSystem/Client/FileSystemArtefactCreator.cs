using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Reflection;
using Artefacts.Service;
using System.Diagnostics;

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

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.FileSystem.FileSystemArtefactCreator"/> class.
		/// </summary>
		/// <param name="repository">Repository.</param>
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

		/// <summary>
		/// Run the specified param.
		/// </summary>
		/// <param name="param">Parameter.</param>
		/// <remarks>Artefacts.CreatorBase implementation</remarks>
		public override void Run(object param)
		{
			int recursionDepth = -1;
			Drive drive;
			Queue<Uri> subDirectories = new Queue<Uri>(new Uri[] { BaseUri });
			Uri currentUri;
			string absPath;
			
			// Recurse subdirectories
			while (subDirectories.Count > 0)
			{
				currentUri = subDirectories.Dequeue();
				drive = Drives.FirstOrDefault((dr) => currentUri.LocalPath.StartsWith(dr.Label));

				foreach (string relPath in EnumerateFiles(currentUri))
				{
					absPath = Path.Combine(currentUri.LocalPath, relPath);
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
						absPath = Path.Combine(currentUri.LocalPath, relPath);
						Directory dir = Directories.FirstOrDefault((d) => d.Path == absPath);
						if (dir == null)
							Repository.Add(new Directory(new System.IO.DirectoryInfo(absPath)));
						else if (dir.UpdateAge > TimeSpan.FromMinutes(1))
							Repository.Update(dir.Update());
//								new Directory(new System.IO.DirectoryInfo(absPath))
//								{
//									Id = dir.Id,
//									TimeCreated = dir.TimeCreated
//								});
						subDirectories.Enqueue(new Uri(currentUri, relPath));
					}
				}
			}
		}

		#region Overridable file system entry  (files and directories) enumerators
		public virtual IEnumerable<Disk> EnumerateDisks(Host host)
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

		public virtual IEnumerable<string> EnumerateDirectories(Uri uri)
		{
			if (!uri.IsFile)
				throw new NotImplementedException("Only URIs with a file schema are currently supported");
			return System.IO.Directory.EnumerateDirectories(uri.LocalPath, "*", SearchOption.TopDirectoryOnly);
		}
		
		public virtual IEnumerable<string> EnumerateFiles(Uri uri)
		{
			if (!uri.IsFile)
				throw new NotImplementedException("Only URIs with a file schema are currently supported");
			return System.IO.Directory.EnumerateFiles(uri.LocalPath, "*", SearchOption.TopDirectoryOnly);
		}
		#endregion
	}
}

