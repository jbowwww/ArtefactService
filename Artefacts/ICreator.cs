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
