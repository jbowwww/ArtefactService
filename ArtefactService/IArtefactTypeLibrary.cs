using System;
using System.Collections.Generic;
using System.Reflection;

namespace Artefacts.Services
{
	/// <summary>
	/// I artefact library.
	/// </summary>
	/// <remarks>
	/// Interface defines service implemented by <see cref="ArtefactLibrary"/> and exposes to clients
	/// </remarks>
	public interface IArtefactTypeLibrary
	{
		List<Assembly> ArtefactAssemblies { get; }

		List<Type> ArtefactTypes { get; }

		int AddAssembly(Assembly assembly);
	}
}

