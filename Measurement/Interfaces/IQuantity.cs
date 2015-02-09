using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// An IQuantity provides an interface that represents a specific measured Value and
	/// its associated Dimensions. Implementations will allow Quantities to be created,
	/// converted to other dimensions,  have mathematical operations performed on them,
	/// be simplified, and be formatted.
	/// </summary>
	/// <example>
	/// A simple measured value of five metres is a Quantity. It has a Value of 5
	/// and a single Dimension with a power of 1 which is a distance with a Unit of metres.
	/// </example>
	public interface IQuantity<TNumber, TNumberImpl, TQuantity> : IValueExtended<TQuantity, TNumber>,
		ISerializable, IFormatter, IFormattable, ICopyable<TQuantity>, IObjectReference
			where TQuantity : IQuantity<TNumber, TNumberImpl, TQuantity>
			where TNumberImpl : INumber<TNumberImpl> {

		#region Properties

		/// <summary>
		/// The Value of a Quantity is an unqualified, dimensionless measurement.
		/// </summary>
		TNumber Value { get; set; }

		/// <summary>
		/// The Dimensions of a Quantity are the combination of units of measurement
		/// that qualify this quantity in the real world.
		/// </summary>
		List<Dimension> Dimensions { get; }

		#endregion

		#region Conversion

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unitName">The name of the unit to convert</param>
		/// <returns>The converted quantity</returns>
		TQuantity Convert(string unitName);

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unit">The unit to convert</param>
		/// <returns>The converted quantity</returns>
		TQuantity Convert(Unit unit);

		/// <summary>
		/// Converts a quantity based on another quantity's dimensions.
		/// </summary>
		/// <param name="quantity">The other quantity whose dimensions to converge on</param>
		/// <returns>The converted quantity</returns>
		TQuantity Convert(TQuantity quantity);

		/// <summary>
		/// Converts the quantity into the base units of each dimension.
		/// </summary>
		/// <returns>The converted quantity</returns>
		TQuantity ConvertToBase();

		/// <summary>
		/// Converts a quantity in based on a unit.
		/// Assumes that the dimensions that have the same system as the provided unit are in a base unit.
		/// </summary>
		/// <param name="unit">The unit to convert into</param>
		/// <param name="prefix">The prefix to convert into</param>
		/// <returns>The converted quantity</returns>
		TQuantity ConvertFromBase(Unit unit, Prefix prefix = null);

		#endregion

		#region General Operations

		/// <summary>
		/// Simplifies a quantity to the most simple set of dimensions possible.
		/// Dimension order is preserved during the simplification in order to choose
		/// the appropriate units.
		/// </summary>
		/// <example>
		/// 5 m^2.in.ft^-1.s^-1 => 897930.494 m^2.s^-1
		/// </example>
		/// <returns>The simplified quantity</returns>
		TQuantity Simplify();

		/// <summary>
		/// Tidies the prefixes associated with the quantity.
		/// - Ignores dimensionless quantities
		/// - Tries to settle on a prefix for only one dimension
		/// - Moves the dimension with the prefix to the start
		/// </summary>
		/// <returns>A quantity with tidied prefixes</returns>
		TQuantity TidyPrefixes();

		/// <summary>
		/// Checks if the quantity is dimensionless.
		/// </summary>
		/// <example>
		/// A quantity of 5 is dimensionless, a quantity of 5 metres is not
		/// </example>
		/// <returns>True if quantity has no dimensions, else false</returns>
		bool IsDimensionless();

		/// <summary>
		/// Checks if the quantity is commensurable with another quantity.
		/// Two commensurable quantities share dimensions with equivalent powers
		/// and systems when simplified.
		/// </summary>
		/// <example>
		/// 5 m^3 is commensurable with 2 km^2.ft or 9000 L but not 5 m^2.s or 3.452 m^3.ft^-1
		/// </example>
		/// <param name="quantity">The quantity to compare with</param>
		/// <returns>True if they are commensurable, else false</returns>
		bool IsCommensurable(TQuantity quantity);

		#endregion

	}
}
