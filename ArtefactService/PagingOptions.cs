using System;
using System.Runtime.Serialization;

namespace Artefacts.Services
{
	/// <summary>
	/// Paging options for <see cref="IRepository"/> query results
	/// </summary>
	[Serializable]
	public class PagingOptions
	{
		/// <summary>
		/// Page size
		/// </summary>
		public int PageSize;
		
		/// <summary>
		/// Start index.
		/// </summary>
//		public int StartIndex;
	}
}
