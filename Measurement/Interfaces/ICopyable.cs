using System;

namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// A simple interface to provide copy functionality. This should be implemented as a deep copy.
	/// </summary>
	/// <typeparam name="TClass">The type or supertype of the implementing class</typeparam>
	[Obsolete("Use ICloneable<TClass> instead")]
	public interface ICopyable<TClass> where TClass : ICopyable<TClass> {

		/// <summary>
		/// Copies the current object.
		/// </summary>
		/// <returns>A copy of the current object</returns>
		TClass Copy();
	}
}
