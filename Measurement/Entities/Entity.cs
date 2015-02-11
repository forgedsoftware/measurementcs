
namespace ForgedSoftware.Measurement.Entities {

	/// <summary>
	/// An entity is a loaded data item that has a unique key to describe it.
	/// </summary>
	public abstract class Entity {

		/// <summary>
		/// A unique key for the entity.
		/// </summary>
		public string Key { get; set; }

	}
}
