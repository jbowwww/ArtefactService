using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Antlr.Runtime.Tree;
using System.Reflection;
using System.Globalization;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;
using NHibernate.Mapping;
using System.ServiceModel;

namespace Artefacts.Service
{
	/// <summary>
	/// Client query provider.
	/// </summary>
	/// <remarks>
	/// TODO: This is so tightly coupled with RepositoryClientProxy, and they are logically very closley
	/// related, so I'm thinking that class should implement IQueryProvider and provide the implementations
	/// currently in this class. In this particular case I think rationalizing/condesnsing the two into
	/// one makes sense, as they have a fairly "mutual" (can't think of a better word") relationship and
	/// logically speaking, the "RepositoryClientProxy" class does kind of exist to "provide" the "client"
	/// with the ability to run "queries"
	/// 
	/// Removed code from somewhere c'tor I think
	/// 
	/// //			if (!typeof(TElement).IsAssignableFrom(expression.Type.GetElementType()))
	///				throw new ArgumentOutOfRangeException("expression", expression,
	///					string.Format("Should assignable to System.Linq.IQueryable<{0}>", typeof(TElement).FullName));
	///			Expression newExpression = _expressionVisitor.Visit(expression);//			object expressionId = Repository.CreateQuery(newExpression.ToExpressionNode());
	///			Expression expressionClientSide = //Expression.MakeIndex(
	///				Expression.Property(
	///					Expression.Property(
	///						Expression.Variable(
	///							typeof(ArtefactRepository),
	///							"ArtefactRepository"),
	///						typeof(ArtefactRepository).GetProperty(
	///							"QueryCache",
	///							BindingFlags.Public | BindingFlags.Instance,
	///							typeof(IDictionary<object, IQueryable>))),
	///					"Item",
	///					Expression.Constant(expressionId));
	///				typeof(IDictionary<object, IQueryable>).GetProperty(), ,);
	/// </remarks>
	internal abstract class ClientQueryProvider : IQueryProvider
	{
		private static RepositoryClientProxy _clientProxy = null;

		/// <summary>
		/// The repository.
		/// </summary>
		public IRepository Repository {
			get; private set;
		}

//		protected int Query [ int Index ] {
//			get { }
//			set { }
//		}

		/// <summary>
		/// Gets the query cache.
		/// </summary>
		/// <remarks>
		/// If you have trouble converting items to IQueryable, try deriving Queryable`1[TArtefact] from a new base class Queryable
		/// that has no type parameters and use that as the element type for this dictionary
		/// </remarks>
		protected IDictionary<Expression, IQueryable> _queryCache;
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.ClientQueryProvider`1"/> class.
		/// </summary>
		/// <param name="repository">Repository.</param>
		/// <param name="visitor">Visitor.</param>
		public ClientQueryProvider(IRepository repository, ExpressionVisitor visitor = null)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;
			_clientProxy = new RepositoryClientProxy(new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3334/ArtefactRepository");

		}


	}
}

