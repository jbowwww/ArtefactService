using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Reflection;

namespace Artefacts
{
	/// <summary>
	/// The base abstract Artefact class
	/// </summary>
	[DataContract]	//(IsReference = true)]
	[KnownType("GetArtefactTypes")]
	[ArtefactFormatString("[Artefact: Id={Id} TimeCreated={TimeCreated} TimeUpdated={TimeUpdated} TimeChecked={TimeChecked}]")]
	public abstract class Artefact :
		IArtefact
	{
		#region Static members (store and return Type arrays for WCF service known types)
		/// <summary>
		/// Backing store for <see cref="ArtefactTypes"/>
		/// </summary>
		private static List<Type> _artefactTypes = null;
		
		/// <summary>
		/// Collection of <see cref="Artefact"/> types
		/// </summary>
		public static List<Type> ArtefactTypes {
			get { return _artefactTypes != null ? _artefactTypes : _artefactTypes = new List<Type>(); }
		}
		
		/// <summary>
		/// Returns the <see cref="Artefact"/> stored in <see cref="ArtefactTypes"/>.
		/// Called by WCF services' <see cref="DataContractResolver"/> as a KnownType or ServiceKnownType
		/// </summary>
		public static Type[] GetArtefactTypes()
		{
			Type[] artefactTypes = new Type[ArtefactTypes.Count];
			ArtefactTypes.CopyTo(artefactTypes, 0);
			return artefactTypes;
		}
		
		/// <summary>
		/// Gets the inheritance level of the type <paramref name="artefactType"/> relative to <c>typeof<see cref="Artefact"/></c>
		/// </summary>
		/// <param name="artefactType">Artefact type</param>
		/// <exception cref="ArgumentException">Is thrown when an argument passed to a method is invalid</exception>
		public static int GetInheritanceLevel(Type artefactType)
		{
			if (!artefactType.IsSubclassOf(typeof(Artefact)))
				throw new ArgumentException("Not derived from class Artefact", "artefactType");
			
			int i;
			for (i = 0; artefactType != typeof(Artefact); artefactType = artefactType.BaseType)
				i++;
			return i;
		}
		#endregion

		#region IArtefact implementation
		[DataMember]
		public virtual int? Id { get; set; }

		[DataMember]
		public virtual DateTime TimeCreated { get; set; }

		[DataMember]
		public virtual DateTime TimeUpdated { get; set; }

		[DataMember]
		public virtual DateTime TimeChecked { get; set; }
		#endregion

		public Artefact()
		{
			TimeCreated = TimeUpdated = TimeChecked = DateTime.Now;
		}
		
		public virtual TimeSpan UpdateAge {
			get { return DateTime.Now - TimeUpdated; }
		}

		private Type[] _typeHierarchy = null;
		public virtual Type[] TypeHeirarchy {
			get
			{
				if (_typeHierarchy != null)
					return _typeHierarchy;
				List<Type> heirarchy = new List<Type>();
				for (Type T = this.GetType(); T != typeof(System.Object); T = T.BaseType)
					heirarchy.Add(T);
				heirarchy.Reverse();
				return _typeHierarchy = heirarchy.ToArray();
			}	
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			string artefactString = string.Empty;
			for (int i = 0; i < TypeHeirarchy.Length; i++)
			{
				if (/* i > 0 && */ artefactString.Length > 0)
					sb.Append('\n').Append(' ', i * 2);
				artefactString = GetArtefactString(TypeHeirarchy[i]);
				sb.Append(artefactString);
			}
			return sb.ToString();
		}
		
		private string GetArtefactString(Type artefactType)
		{
			object[] attrs = artefactType.GetCustomAttributes(typeof(ArtefactFormatStringAttribute), false);
			ArtefactFormatStringAttribute afsAttr = attrs.Length > 0 ?
				(ArtefactFormatStringAttribute)attrs[0] : new ArtefactFormatStringAttribute();
			return afsAttr.GetArtefactString(this);
		}
	}
}

