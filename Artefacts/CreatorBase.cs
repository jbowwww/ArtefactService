using System;
using System.Collections.Generic;
using System.Threading;

namespace Artefacts
{
	public abstract class CreatorBase : ICreator
	{
		internal class CreatorSubscription : IDisposable
		{
			private CreatorBase _creator;
			private IObserver<Artefact> _observer;

			public CreatorSubscription(CreatorBase creator, IObserver<Artefact> observer)
			{
				_creator = creator;
				_observer = observer;
			}

			#region IDisposable implementation
			public void Dispose ()
			{
				_creator._observers.Remove(_observer);
			}
			#endregion
		}

		private List<IObserver<Artefact>> _observers;

		public Thread AsyncThread { get; private set; }

		public CreatorBase ()
		{
			_observers = new List<IObserver<Artefact>>();
		}

		protected virtual void NotifyCreate(Artefact artefact)
		{
			foreach (IObserver<Artefact> observer in _observers)
				observer.OnNext(artefact);
		}

		protected virtual void NotifyComplete ()
		{
			foreach (IObserver<Artefact> observer in _observers)
				observer.OnCompleted();
		}

		protected virtual void NotifyError(Exception ex)
		{
			foreach (IObserver<Artefact> observer in _observers)
				observer.OnError(ex);
		}

		#region IWorker implementation
		public Thread RunAsync (object param)
		{
			if (AsyncThread != null)
			{
				if (AsyncThread.ThreadState == ThreadState.Background
					|| AsyncThread.ThreadState == ThreadState.Running
					|| AsyncThread.ThreadState == ThreadState.Suspended)
					throw new InvalidOperationException("Something something RunAsync");
			}
			if (AsyncThread == null || AsyncThread.ThreadState != ThreadState.Unstarted)
				AsyncThread = new Thread(Run);
			AsyncThread.Start(param);
			return AsyncThread;
		}

		public abstract void Run (object param);
//		{
//			throw new System.NotImplementedException();
//		}
		#endregion

		#region IObservable implementation
		public IDisposable Subscribe (IObserver<Artefact> observer)
		{
			_observers.Add(observer);
			return new CreatorSubscription(this, observer);
		}
		#endregion
	}
}

