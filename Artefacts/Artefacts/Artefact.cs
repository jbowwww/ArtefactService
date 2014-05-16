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
	[DataContract, KnownType("GetArtefactTypes")]
	[ArtefactFormat("[Artefact: Id={Id}]")]	// TimeCreated={TimeCreated} TimeUpdated={TimeUpdated} TimeChecked={TimeChecked}]")]
	public abstract class Artefact : IArtefact
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
		/// Backing store for <see cref="Artefact.GetTypeHeirarchy"/>
		/// </summary>
		private static Dictionary<Type, Type[]> _typeHeirarchies = null;
		
		/// <summary>
		/// Gets a type's inheritance heirarchy
		/// </summary>
		/// <returns>A <see cref="System.Type"/> array representing the type heirarchy</returns>
		/// <param name="artefactType">Artefact type</param>
		public static Type[] GetTypeHeirarchy(Type artefactType)
		{
			if (_typeHeirarchies == null)
				_typeHeirarchies = new Dictionary<Type, Type[]>();
			if (_typeHeirarchies.ContainsKey(artefactType))
				return _typeHeirarchies[artefactType];
			List<Type> heirarchy = new List<Type>();
			for (Type T = artefactType; T.BaseType != null; T = T.BaseType)
				heirarchy.Add(T);
			heirarchy.Reverse();
			return _typeHeirarchies[artefactType] = heirarchy.ToArray();
		}
		
		/// <summary>
		/// Gets the inheritance level of the type <paramref name="artefactType"/> relative to <c>typeof<see cref="Artefact"/></c>
		/// </summary>
		/// <typeparam name="TArtefact"><see cref="Artefacts.Artefact"/> type</typeparam>
		/// <exception cref="ArgumentException">Is thrown when an argument passed to a method is invalid</exception>
		public static int GetInheritanceLevel<TArtefact>()
			where TArtefact : Artefact
		{
//			if (!artefactType.IsSubclassOf(typeof(Artefact)))
//				throw new ArgumentException("Not derived from class Artefact", "artefactType");
			return GetTypeHeirarchy(typeof(TArtefact)).Length - 2;
		}
		#endregion
		
		#region Properties
		public virtual bool IsTransient {
			get { return !this.Id.HasValue; }
		}
		
		public virtual TimeSpan CreatedAge {
			get { return DateTime.Now - TimeCreated; }
		}
		
		public virtual TimeSpan UpdateAge {
			get { return DateTime.Now - TimeUpdated; }
		}
		
		public virtual TimeSpan CheckedAge {
			get { return DateTime.Now - TimeChecked; }
		}
		
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
		#endregion
		
		public Artefact()
		{
			TimeCreated = TimeUpdated = TimeChecked = DateTime.Now;
		}
		
		public virtual Artefact Update()
		{
			TimeUpdated = DateTime.Now;
			return this;
		}
		
		public virtual void CopyMembersFrom(Artefact source)
		{
//			TimeCreated = source.TimeCreated;
			TimeChecked = source.TimeChecked;
			TimeUpdated = source.TimeUpdated;
		}
		
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (System.Object.ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return true;
		}

				public override int GetHashCode()
		{
						return Convert.ToInt32(TimeCreated.Ticks) + Convert.ToInt32(TimeUpdated.Ticks) + Convert.ToInt32(TimeChecked.Ticks);
		}
//			Artefact artefact = (Artefact)obj;
//			return TimeCreated == artefact.TimeCreated
//				&& TimeUpdated == artefact.TimeUpdated
//				&& TimeChecked == artefact.TimeChecked;
		
		public override string ToString()
		{
			return ArtefactFormatAttribute.GetString(this);
		}
	}
}

