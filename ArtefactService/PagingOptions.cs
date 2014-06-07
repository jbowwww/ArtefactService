using System;
using System.Runtime.Serialization;

namespace Artefacts.Service
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
		public int PageSize = 5;
	}
}
