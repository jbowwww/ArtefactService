using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Artefacts
{
	/// <summary>
	/// Implemented by workers that receive <see cref="Artefacts.Artefact"/>s
	/// </summary>
	public interface IReceiver : IObserver<Artefact>
	{
	}
}

	// IObserver members
//	void OnCompleted ();
//	void OnError (Exception error);
//	void OnNext (T value);

