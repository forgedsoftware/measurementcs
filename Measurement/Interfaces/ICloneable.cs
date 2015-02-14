using System;

namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// A simple type safe cloneable interface for cloning objects.
	/// Individual objects need to decide if they want to present a deep or shallow clone.
	/// </summary>
	/// <typeparam name="TClass">The type of the implementing class</typeparam>
	public interface ICloneable<TClass> : ICloneable where TClass : ICloneable<TClass> {

		/// <summary>
		/// Returns a typed clone of the object.
		/// </summary>
		/// <returns>Typed clone of the object</returns>
		new TClass Clone();
	}
}
