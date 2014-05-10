using System;

namespace Artefacts.Services
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

