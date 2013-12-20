using System;
using System.Collections.Concurrent;

namespace Artefacts
{
	/// <summary>
	/// Implemented by workers that create <see cref="Artefacts.Artefact"/>s
	/// </summary>
	public interface ICreator : IWorker, IObservable<Artefact>
	{
	}
}

//		public interface IObserver<in T>
//		// Methods
//		void OnCompleted ();
//		void OnError (Exception error);
//		void OnNext (T value);
