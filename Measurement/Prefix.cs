
using System;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A prefix is a scale modifier used in certain systems of measurement,
	/// particularly the metric system.
	/// </summary>
	/// <example>
	/// kilo-, centi-, kili-, tera-, pico- are all examples of prefixes.
	/// </example>
	public class Prefix {

		public string Name { get; set; }
		public string Symbol { get; set; }
		public PrefixType Type { get; set; }
		public bool IsRare { get; set; }
		public double Multiplier { get; set; }
		public double Power { get; set; }
		public double Base { get; set; }

		/// <summary>
		/// Applies a prefix to a value
		/// </summary>
		/// <example>
		/// For example, 5012.23 m, applying the prefix 'kilo' returns 5.01223 km
		/// </example>
		/// <param name="value">The value to apply the prefix to</param>
		/// <returns>The value after the prefix has been applied</returns>
		public double Apply(double value) {
			if (!Power.Equals(default(double)) && !Base.Equals(default(double))) {
				return value/Math.Pow(Base, Power);
			}
			if (!Multiplier.Equals(default(double))) {
				return value/Multiplier;
			}
			return value;
		}

		/// <summary>
		/// Removes a prefix from a value
		/// </summary>
		/// <example>
		/// For example, 5.01223 km, removing the prefix 'kilo' returns 5012.23 m
		/// </example>
		/// <param name="value">The value to remove the prefix from</param>
		/// <returns>The value after the prefix has been removed</returns>
		public double Remove(double value) {
			if (!Power.Equals(default(double)) && !Base.Equals(default(double))) {
				return value * Math.Pow(Base, Power);
			}
			if (!Multiplier.Equals(default(double))) {
				return value * Multiplier;
			}
			return value;
		}
	}

	/// <summary>
	/// A PrefixType is an enum that represents the type of a particular Prefix.
	/// </summary>
	public enum PrefixType {
		Si, SiBinary, SiUnofficial
	}
}
