using System;
using System.ServiceModel;
using System.Threading;

namespace Artefacts.Service
{
	/// <summary>
	/// Async result.
	/// </summary>
	public class QueryAsyncResult : IAsyncResult
	{
		/// <summary>
		/// Query asynchronous state enumerable
		/// </summary>
		public enum QueryAsyncState
		{
			Created,
			Counted,
			Queried
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.AsyncResult"/> class.
		/// </summary>
		public QueryAsyncResult(object queryId, string query)
		{
			SessionId = OperationContext.Current.SessionId;
			OperationId = OperationContext.Current.IncomingMessageHeaders.MessageId.ToString();
			Action = OperationContext.Current.IncomingMessageHeaders.Action;
			AsyncState = QueryAsyncState.Created;
//			AsyncWaitHandle = new ManualResetEventSlim(false);
			QueryId = queryId;
			Query = query;
		}

		/// <summary>
		/// Gets the operation identifier.
		/// </summary>
		/// <value>The operation identifier.</value>
		public string OperationId {
			get;
			private set;
		}

		/// <summary>
		/// Gets the action.
		/// </summary>
		/// <value>The action.</value>
		public string Action {
			get;
			private set;
		}

		/// <summary>
		/// Gets the arguments.
		/// </summary>
		/// <value>The arguments.</value>
		public string SessionId {
			get;
			private set;
		}

		/// <summary>
		/// Gets the query identifier.
		/// </summary>
		/// <value>The query identifier.</value>
		public object QueryId {
			get;
			private set;
		}

		/// <summary>
		/// Gets the query.
		/// </summary>
		/// <value>The query.</value>
		public string Query {
			get;
			private set;
		}

		#region IAsyncResult implementation
		/// <summary>
		/// Gets the state of the async.
		/// </summary>
		/// <value>The state of the async.</value>
		public object AsyncState {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the async wait handle.
		/// </summary>
		/// <value>The async wait handle.</value>
		public WaitHandle AsyncWaitHandle {
			get;
			protected set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Artefacts.Service.AsyncResult"/> completed synchronously.
		/// </summary>
		/// <value><c>true</c> if completed synchronously; otherwise, <c>false</c>.</value>
		public bool CompletedSynchronously {
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is completed.
		/// </summary>
		public bool IsCompleted {
			get { return ((QueryAsyncState)AsyncState) == QueryAsyncState.Queried; }
		}
		#endregion
	}
}

