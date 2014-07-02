using System;

namespace Artefacts.Core
{
	public interface IAspect
	{
		Type AspectType { get; }
		IArtefact Artefact { get; }
	}
}

