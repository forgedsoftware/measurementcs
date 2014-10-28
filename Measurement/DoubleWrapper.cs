using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// This is a wrapper of a double to provide the standard set of INumber functions.
	/// </summary>
	[DataContract]
	public struct DoubleWrapper : INumber<DoubleWrapper> {

		#region Constructors

		/// <summary>
		/// A basic constructor
		/// </summary>
		/// <param name="value">The double value to use</param>
		public DoubleWrapper(double value)
				: this() {
			Value = value;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The double value being wrapped
		/// </summary>
		public double Value { get; private set; }

		/// <summary>
		/// An approximate equivalent value of the double as a double.
		/// </summary>
		public double EquivalentValue {
			get { return Value; }
		}

		#endregion

		#region Bacic Math

		/// <summary>
		/// Adds two wrapped doubles together
		/// </summary>
		/// <param name="add">The wrapped double to add</param>
		/// <returns>The sum of the two doubles</returns>
		public DoubleWrapper Add(DoubleWrapper add) {
			return new DoubleWrapper(Value + add.Value);
		}

		/// <summary>
		/// Subtracts one wrapped double from another
		/// </summary>
		/// <param name="subtract">The wrapped double to subtract</param>
		/// <returns>The difference between the two doubles</returns>
		public DoubleWrapper Subtract(DoubleWrapper subtract) {
			return new DoubleWrapper(Value - subtract.Value);
		}

		/// <summary>
		/// Multiplies two wrapped doubles together
		/// </summary>
		/// <param name="multiply">The wrapped double to multiply by</param>
		/// <returns>The product of the two doubles</returns>
		public DoubleWrapper Multiply(DoubleWrapper multiply) {
			return new DoubleWrapper(Value * multiply.Value);
		}

		/// <summary>
		/// Divides one wrapped double by another
		/// </summary>
		/// <param name="divide">The wrapped double to divide by</param>
		/// <returns>The quotient of the two doubles</returns>
		public DoubleWrapper Divide(DoubleWrapper divide) {
			return new DoubleWrapper(Value / divide.Value);
		}

		/// <summary>
		/// Adds a raw double value to a wrapped double
		/// </summary>
		/// <param name="add">The raw double value to add</param>
		/// <returns>The sum of the two doubles</returns>
		public DoubleWrapper Add(double add) {
			return new DoubleWrapper(Value + add);
		}

		/// <summary>
		/// Subtracts a raw double from a wrapped double
		/// </summary>
		/// <param name="subtract">The double to subtract by</param>
		/// <returns>The difference of the two doubles</returns>
		public DoubleWrapper Subtract(double subtract) {
			return new DoubleWrapper(Value - subtract);
		}

		/// <summary>
		/// Multiplies a raw double with a wrapped double
		/// </summary>
		/// <param name="multiply">The double to multiply by</param>
		/// <returns>The product of the two doubles</returns>
		public DoubleWrapper Multiply(double multiply) {
			return new DoubleWrapper(Value * multiply);
		}

		/// <summary>
		/// Divides the wrapped double by a raw double
		/// </summary>
		/// <param name="divide">The double to divide by</param>
		/// <returns>The quotient of the two doubles</returns>
		public DoubleWrapper Divide(double divide) {
			return new DoubleWrapper(Value / divide);
		}

		/// <summary>
		/// Returns the negation of the wrapped double
		/// </summary>
		/// <returns>The negated double</returns>
		public DoubleWrapper Negate() {
			return new DoubleWrapper(-Value);
		}

		#endregion

		#region Extended Math

		public DoubleWrapper Abs() {
			return new DoubleWrapper(Math.Abs(Value));
		}

		public DoubleWrapper Ceiling() {
			return new DoubleWrapper(Math.Ceiling(Value));
		}

		public DoubleWrapper Floor() {
			return new DoubleWrapper(Math.Floor(Value));
		}

		public DoubleWrapper Pow(double power) {
			return new DoubleWrapper(Math.Pow(Value, power));
		}

		public DoubleWrapper Round() {
			return new DoubleWrapper(Math.Round(Value));
		}

		public DoubleWrapper Sqrt() {
			return new DoubleWrapper(Math.Sqrt(Value));
		}

		public DoubleWrapper Max(DoubleWrapper other) {
			return new DoubleWrapper(Math.Max(Value, other.Value));
		}

		public DoubleWrapper Max(params DoubleWrapper[] values) {
			var doubles = new List<DoubleWrapper>(values) { this };
			return new DoubleWrapper(doubles.Max(d => d.Value));
		}

		public DoubleWrapper Min(DoubleWrapper other) {
			return new DoubleWrapper(Math.Min(Value, other.Value));
		}

		public DoubleWrapper Min(params DoubleWrapper[] values) {
			var doubles = new List<DoubleWrapper>(values) { this };
			return new DoubleWrapper(doubles.Min(d => d.Value));
		}

		#endregion

		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		public string ToString(string format, IFormatProvider formatProvider) {
			return Value.ExtendedToString(format, formatProvider);
		}
	}
}
