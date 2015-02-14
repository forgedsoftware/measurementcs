using System;

namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// An extension of the IValue interface, enforcing that the number described must also be of type
	/// INumber.
	/// </summary>
	/// <typeparam name="TNumber">The type of the implementing concrete class</typeparam>
	public interface INumber<TNumber> : IValueExtended<TNumber, double>, IFormattable, ICloneable<TNumber>
			where TNumber : INumber<TNumber> {

		/// <summary>
		/// An approximate value of the underlying INumber. It can be used for
		/// comparisons.
		/// </summary>
		double EquivalentValue { get; }
	}
}
