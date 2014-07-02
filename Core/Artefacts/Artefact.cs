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
		#region Type-related stuff
		/// <summary>
		/// Collection of <see cref="Artefact"/> types for use as known types in WCF service's <see cref="DataContractSerializer"/> 
		/// </summary>
		public static readonly List<Type> ArtefactTypes = new List<Type>();
//		 {
//			get { return _artefactTypes ?? _artefactTypes = new List<Type>(); }
//		}
//		private static List<Type> _artefactTypes = null;
		
		/// <summary>
		/// Backing store for <see cref="Artefact.GetTypeHeirarchy"/>
		/// </summary>
		public static readonly Dictionary<Type, Type[]> TypeHeirarchies = new Dictionary<Type, Type[]>();
		
		/// <summary>
		/// Gets the known artefact types for the <see cref="DataContractSerializer"/> in WCF services
		/// </summary>
		/// <returns>The artefact types.</returns>
		public static Type[] GetArtefactTypes()
		{
			return ArtefactTypes.ToArray();				//		return GetArtefactTypes(null);
		}

		/// <summary>
		/// Returns the <see cref="Artefact"/> stored in <see cref="ArtefactTypes"/>.
		/// Called by WCF services' <see cref="DataContractResolver"/> as a KnownType or ServiceKnownType
		/// </summary>
//		public static Type[] GetArtefactTypes(ICustomAttributeProvider provider = null)
//		{
//			Type[] artefactTypes = new Type[ArtefactTypes.Count];
//			ArtefactTypes.CopyTo(artefactTypes, 0);
//			return artefactTypes;
//		}
		
		/// <summary>
		/// Gets a type's inheritance heirarchy
		/// </summary>
		/// <param name="artefactType">Artefact type</param>
		/// <returns>A <see cref="System.Type"/> array representing the type heirarchy</returns>
		public static Type[] GetTypeHeirarchy(Type artefactType)
		{
//			if (TypeHeirarchies == null)
//				TypeHeirarchies = new Dictionary<Type, Type[]>();
			if (TypeHeirarchies.ContainsKey(artefactType))
				return TypeHeirarchies[artefactType];
			Stack<Type> heirarchy = new Stack<Type>();
			for (Type T = artefactType; T.BaseType != null; T = T.BaseType)
				heirarchy.Push(T);
			TypeHeirarchies[artefactType] = heirarchy.ToArray();
			return TypeHeirarchies[artefactType];
		}
		
		/// <summary>
		/// Gets the inheritance level of the type <paramref name="artefactType"/> relative to <c>typeof<see cref="Artefact"/></c>
		/// </summary>
		/// <typeparam name="TArtefact"><see cref="Artefacts.Artefact"/> type</typeparam>
		/// <exception cref="ArgumentException">Is thrown when an argument passed to a method is invalid</exception>
		public static int GetInheritanceLevel<TArtefact>() where TArtefact : Artefact
		{
			return GetTypeHeirarchy(typeof(TArtefact)).Length - 2;
		}
		#endregion
		
		/// <summary>
		/// The longest <see cref="TimeSpan"/> that an <see cref="Artefact"/> may use cached values.
		/// After that it must be updated from <see cref="Repository"/>.
		/// </summary>
		public static TimeSpan UpdateAgeLimit = new TimeSpan(0, 1, 0);
		#endregion
		
		#region Properties
		private readonly Dictionary<Type, object> _aspects = new Dictionary<Type, object>();
		
		#region IArtefact implementation
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public virtual int? Id { get; set; }

		[DataMember(IsRequired = true)]
		public virtual Uri Uri { get; private set; }

		[DataMember(IsRequired = false)]
		public virtual DateTime TimeCreated { get; set; }

		[DataMember(IsRequired = false)]
		public virtual DateTime TimeUpdated { get; set; }

		[DataMember(IsRequired = false)]
		public virtual DateTime TimeChecked { get; set; }

		public virtual DateTime TimeUpdatesCommitted { get; set; }
		#endregion
		
		public virtual bool IsTransient {
			get { return !this.Id.HasValue; }
		}
		
		public virtual bool IsOutdated {
			get { return UpdateAge >= Artefact.UpdateAgeLimit; }
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
		#endregion
		
		protected Artefact()
		{
			TimeCreated = TimeUpdated = TimeChecked = DateTime.Now;
			TimeUpdatesCommitted = DateTime.MinValue;
		}
		
		public virtual TAspect GetAspect<TAspect>()
		{
			Type _TAspect = typeof(TAspect);
			if (!_aspects.ContainsKey(_TAspect))
				throw new ApplicationException(string.Format("Aspect type \"{0}\" does not exist for {1} artefact with Id #{2}",
					_TAspect.FullName, GetType().FullName, Id.HasValue ? Id.Value.ToString() : "(null)"));
			return (TAspect)_aspects[_TAspect];
		}
		
//		public virtual TAspect AddAspect<TAspect>()
//		{
//			// TODO
//		}
		
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
//			return ArtefactFormatAttribute.GetString(this);
			return string.Format(
				"[Artefact: Id={0} TimeCreated={1} TimeChecked={2} TimeUpdated={3}]",
				Id, TimeCreated, TimeChecked, TimeUpdated);
		}
	}
}

