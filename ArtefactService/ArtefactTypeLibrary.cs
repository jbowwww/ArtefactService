using System;
using System.Collections.Generic;
using System.Reflection;

namespace Artefacts.Services
{
	/// <summary>
	/// Artefact Library
	/// ! TODO
	/// </summary>
	/// <remarks>
	/// This will contain collections or dictionaries of -
	/// 	- Assemblies that have been added to the library which contain one or more of types of items below
	/// 	- Types derived from <see cref="Artefact"/>
	/// 	- Types containing operations relating to <see cref="Artefact"/>s
	/// 		-	Could relate to <see cref="Artefact"/>s in general, and/or derived types
	///			contained in the same assembly and/or in other assemblies
	/// 		-	Have not yet devised how those operations will be defined and what classes will be needed to do so
	/// 		-	Possible operations could include but are not limited to -
	/// 			-	"Finders" that create <see cref="Artefact"/>s
	/// 			-	"Analyzers" that consider <see cref="Artefact"/>s either newly created and/or
	/// 				periodically for possible further operations
	/// 			-	"Listeners" may be functionally indistinct from "Analyzers" could consider all <see cref="Artefact"/>s
	/// 				or define some type of filter(s) that the <see cref="ArtefactRepository"/> can use to select
	/// 				<see cref="Artefact"/>s to send to the operators (analyzer/listener/whatev)
	/// </remarks>
	public class ArtefactTypeLibrary : IArtefactTypeLibrary
	{
		[ThreadStatic]
		private static ArtefactTypeLibrary _singleton = null;
		public static ArtefactTypeLibrary Singleton {
			get
			{
				return _singleton != null ? _singleton : _singleton = new ArtefactTypeLibrary();
			}
		}

		/// <summary>
		/// A list of types that can be added to manually or by using method(s) in this class
		/// </summary>
		public List<Assembly> ArtefactAssemblies { get; private set; }

		/// <summary>
		/// A list of types that can be added to manually or by using method(s) in this class
		/// </summary>
		public List<Type> ArtefactTypes { get { return Artefact.ArtefactTypes; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Services.ArtefactTypeLibrary"/> class.
		/// </summary>
		public ArtefactTypeLibrary ()
		{
			ArtefactAssemblies = new List<Assembly>();
//			ArtefactTypes = new List<Type>();
		}

		/// <summary>
		/// Adds types that implement <see cref="Artefacts.Artefact"/> to this <see cref="ArtefactTypeLibrary"/>.
		/// <see cref="ArtefactRepository"/> I *think* should follow the Singleton pattern. Because when a
		/// host program ... No wait, I'm confuised??? TODO: Think this through
		/// </summary>
		/// <param name="assembly">Assembly to get types from</param>
		public int AddAssembly (Assembly assembly)
		{
			int typesAdded = 0;
			if (!ArtefactAssemblies.Contains(assembly))
			{
				ArtefactAssemblies.Add(assembly);
				foreach (Type type in assembly.GetExportedTypes())
				{
					if (type.GetInterface("Artefacts.IArtefact") != null)
					{
						ArtefactTypes.Add(type);
						typesAdded++;
					}
				}
			}
			return typesAdded;
		}
	}
}

