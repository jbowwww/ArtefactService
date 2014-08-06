using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Artefacts.Service
{
	[DataContract]
	public class QueryResult<TArtefact> where TArtefact : Artefact
	{
		#region Fields
		[DataMember(Name="Results")]
		private int[] _results = null;
		#endregion
		
		#region Properties & Indexers
		public bool HasResults { get { return _results != null; } }
		public int Count { get { return _results == null ? -1 : _results.Length; } }
		public int this[int index]
		{
			get { return _results[index]; }
		}
		#endregion
		
		public QueryResult(IQueryable<TArtefact> query, int startIndex = 0, int count = -1)
		{
			_results = (count == -1 ? // TODO: Is NhQueryable's caching sufficient here or should I use Queryable<>
				query.Skip(startIndex) : // with a new custom server-side query provider, and implement caching??
				query.Skip(startIndex).Take(count)).Select((a) => a.Id.Value).ToArray();
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(string.Format("QueryResult(Count={0}): {{ ", Count));
			for (int i = 0; i < Count; i++)
			{
				sb.Append(_results[i]);
				if (i != Count - 1)
					sb.Append(", ");
			}
			sb.Append(" }}");
			return sb.ToString();
		}
	}
}

