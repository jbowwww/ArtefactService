using System;

namespace Artefacts.Service
{
	/// <summary>
	/// Artefact service configuration options
	/// </summary>
	public class ArtefactServiceConfiguration
	{
		/// <summary>
		/// Writes exceptions to console (see <see cref="ArtefactRepository.Error"/>)
		/// </summary>
		public bool OutputExceptions = true;
	}
}

