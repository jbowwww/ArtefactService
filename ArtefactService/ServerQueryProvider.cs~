using System;
using System.Collections.Generic;
using System.Linq;

using Serialize.Linq;
using Serialize.Linq.Serializers;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;

namespace Artefacts.Services
{
	public class ServerQueryProvider<TArtefact> :
		IQueryProvider
		where TArtefact : Artefact
	{
		private IRepository<TArtefact> _repo = null;
		
		public ServerQueryProvider(IRepository<TArtefact> repo)
		{
			_repo = repo;
		}

		#region IQueryProvider implementation
		public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
		{
			return new Queryable(null, expression);
		}

		public object Execute(System.Linq.Expressions.Expression expression)
		{
			ExpressionNode en = expression.ToExpressionNode();
			Artefact[] result = RepoProxy.RunLinq(en);
			return result;
		}

		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
		{
			if (!typeof(Artefact).IsAssignableFrom(typeof(TElement)))
				throw new ArgumentOutOfRangeException("TElement", typeof(TElement), "TElement should derive from Artefact");
			return (IQueryable<TElement>)new Queryable(null, expression);
		}

		TResult IQueryProvider.Execute<TResult>(System.Linq.Expressions.Expression expression)
		{
//			if (!typeof(Artefact).IsAssignableFrom(typeof(TElement)))
//				throw new ArgumentOutOfRangeException("TElement", typeof(TElement), "TElement should derive from Artefact");
			ExpressionNode en = expression.ToExpressionNode();
			return _repo.Query(en);				//Convert.ChangeType( , typeof(TResult))
		}
		#endregion
	}
}

