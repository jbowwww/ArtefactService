using System;

namespace Artefacts
{
	/// <summary>
	/// Very general definition for now
	/// </summary>
	public interface IWorker
	{
		System.Threading.Thread RunAsync (object param);
		void Run(object param);
	}
}

