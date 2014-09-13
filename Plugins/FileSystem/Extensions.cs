using System;
using System.Linq;

namespace Artefacts.FileSystem
{
	public static class Extensions
	{
		public static FileSystemEntry FromPath(this IQueryable<FileSystemEntry> fsEntries, string rootPath)
		{
			if (fsEntries == null)
				throw new NullReferenceException("drives");
			return (from fse in fsEntries
				where
					fse.Path.Length < rootPath.TrimEnd('/', '\\').Length
					&& rootPath.StartsWith(fse.Path)
				orderby fse.Path.Length descending
				select fse).FirstOrDefault();
		}
		
		public static Drive FromPath(this IQueryable<Drive> drives, string rootPath)
		{
			if (drives == null)
				throw new NullReferenceException("drives");
			return (from d in drives
				where rootPath.StartsWith(d.Label)		//					.Where((d) => rootPath.StartsWith(d.Label))
				orderby d.Label.Length descending		//					.OrderByDescending((d) => d.Label.Length)
				select d).FirstOrDefault();
		}
	}
}

