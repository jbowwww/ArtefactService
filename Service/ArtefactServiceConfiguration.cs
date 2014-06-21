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
		
		/// <summary>
		/// The update age threshold - if artefacts have not been updated for longer than this, they get updated
		/// </summary>
		public TimeSpan UpdateAgeThreshold = new TimeSpan(12, 0, 0);
	}
}

