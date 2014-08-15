using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A Quantity represents a specific measured Value and its associated Dimensions.
	/// This object will allow Quantities to be created, converted to other dimensions,
	/// have mathematical operations performed on them, be simplified, and be formatted.
	/// </summary>
	/// <example>
	/// A simple measured value of five metres is a Quantity. It has a Value of 5
	/// and a single Dimension with a power of 1 which is a distance with a Unit of metres.
	/// </example>
	[DataContract]
	public class Quantity : ISerializable, IFormatter, IFormattable, ICopyable<Quantity> {

		#region Constructors

		/// <summary>
		/// Default basic constructor
		/// </summary>
		private Quantity() {
			Dimensions = new List<Dimension>();
		}

		/// <summary>
		/// Dimensionless constructor with prefix
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		public Quantity(double value)
			: this() {
			Value = value;
		}

		/// <summary>
		/// Helper constructor for quantity with a single dimension
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitName">The common name of the unit of the dimension</param>
		public Quantity(double value, string unitName)
			: this(value) {
			Dimensions.Add(new Dimension(unitName));
		}

		/// <summary>
		/// Helper constructor for quantity with multiple simple dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitNames">A set of unit names to turn into dimensions</param>
		public Quantity(double value, IEnumerable<string> unitNames)
			: this(value) {
			foreach (string unitName in unitNames) {
				Dimensions.Add(new Dimension(unitName));
			}
		}

		/// <summary>
		/// Helper constructor for quantity with single complex dimension
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="dimension">Pre-existing dimension to use with this quantity</param>
		public Quantity(double value, Dimension dimension)
			: this(value) {
			Dimensions.Add(dimension);
		}

		/// <summary>
		/// A full constructor for multiple, complex dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="dimensions">Pre-existing dimensions to use with this quantity</param>
		public Quantity(double value, IEnumerable<Dimension> dimensions)
			: this(value) {
			Dimensions.AddRange(dimensions);
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="quantity">The quantity to copy</param>
		private Quantity(Quantity quantity) {
			Value = quantity.Value;
			Dimensions = quantity.Dimensions.CopyList();
		}

		#endregion

		/// <summary>
		/// The Value of a Quantity is an unqualified, dimensionless measurement.
		/// In this case it is a scalar.
		/// </summary>
		// TODO - Separate out this value as a generic 'V' ????????
		// Structure: QuantityType <-- Scalar
		//						   <-- Vector
		//						   <-- Tensor ??
		[DataMember(Name = "value")]
		public double Value { get; private set; }

		/// <summary>
		/// The Dimensions of a Quantity are the combination of units of measurement
		/// that qualify this quantity in the real world.
		/// </summary>
		[DataMember(Name = "dimensions")]
		public List<Dimension> Dimensions { get; private set; }

		#region Conversion

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unitName">The name of the unit to convert</param>
		/// <returns>The converted quantity</returns>
		public Quantity Convert(string unitName) {
			return Convert(MeasurementFactory.FindUnit(unitName));
		}

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unit">The unit to convert</param>
		/// <returns>The converted quantity</returns>
		public Quantity Convert(Unit unit) {
			Quantity quantityAsBase = ConvertToBase();
			return quantityAsBase.ConvertFromBase(unit);
		}

		/// <summary>
		/// Converts a quantity based on another quantity's dimensions.
		/// </summary>
		/// <param name="quantity">The other quantity whose dimensions to converge on</param>
		/// <returns>The converted quantity</returns>
		public Quantity Convert(Quantity quantity) {
			Quantity quantityAsBase = ConvertToBase();
			if (!IsCommensurable(quantity)) {
				throw new Exception("Quantities must have commensurable dimensions in order to convert between them");
			}
			quantity.Dimensions.ForEach(d => quantityAsBase = quantityAsBase.ConvertFromBase(d.Unit));
			return quantityAsBase;
		}

		/// <summary>
		/// Converts the quantity into the base units of each dimension.
		/// </summary>
		/// <returns>The converted quantity</returns>
		public Quantity ConvertToBase() {
			double convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				KeyValuePair<Dimension, double> result = dimension.ConvertToBase(convertedValue);
				convertedValue = result.Value;
				newDimensions.Add(result.Key);
			}
			return new Quantity(convertedValue, newDimensions);
		}

		/// <summary>
		/// Converts a quantity in based on a unit.
		/// Assumes that the dimensions that have the same system as the provided unit are in a base unit.
		/// </summary>
		/// <param name="unit">The unit to convert into</param>
		/// <param name="prefix">The prefix to convert into</param>
		/// <returns>The converted quantity</returns>
		protected Quantity ConvertFromBase(Unit unit, Prefix prefix = null) {
			double convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				if (dimension.Unit.System.Units.Contains(unit)) {
					KeyValuePair<Dimension, double> result = dimension.ConvertFromBase(convertedValue, unit, prefix);
					convertedValue = result.Value;
					newDimensions.Add(result.Key);
				} else {
					newDimensions.Add(dimension.Copy());
				}
			}
			return new Quantity(convertedValue, newDimensions);
		}

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
		public Quantity Simplify() {
			double computedValue = Value;
			List<Dimension> simplifiedDimensions = Dimensions.Simplify(ref computedValue);
			var resultingQuantity = new Quantity(computedValue, simplifiedDimensions);

			if (MeasurementFactory.Options.UseAutomaticPrefixManagement) {
				resultingQuantity = resultingQuantity.TidyPrefixes();
			}
			return resultingQuantity;
		}

		/// <summary>
		/// Tidies the prefixes associated with the quantity.
		/// - Ignores dimensionless quantities
		/// - Tries to settle on a prefix for only one dimension
		/// - Moves the dimension with the prefix to the start
		/// TODO - Prefer positive powers to negative powers of dimensions when applying prefixes
		/// TODO - Too long! Split into smaller methods
		/// </summary>
		/// <returns>A quantity with tidied prefixes</returns>
		public Quantity TidyPrefixes() {
			Quantity quantity = Copy();
			if (IsDimensionless()) {
				return quantity;
			}
			int numberOfPrefixes = quantity.Dimensions.Count(d => d.Prefix != null);

			// Try add a prefix to prefixless dimensions
			if (numberOfPrefixes == 0) {
				for (int i = 0; i < quantity.Dimensions.Count; i++) {
					Dimension dimension = quantity.Dimensions[i];
					if (dimension.CanApplyPrefix()) {
						KeyValuePair<Dimension, double> prefixAppliedKeyValue = dimension.ApplyPrefix(quantity.Value);
						quantity.Dimensions[i] = prefixAppliedKeyValue.Key;
						quantity.Value = prefixAppliedKeyValue.Value;
						if (quantity.Dimensions.First().Prefix != null) {
							numberOfPrefixes++;
						}
						break;
					}
				}
			}
			
			// Remove all but first prefix
			if (numberOfPrefixes > 1) {
				bool seenPrefix = false;
				for (int j = 0; j < quantity.Dimensions.Count; j++) {
					Dimension dimension = quantity.Dimensions[j];
					if (dimension.Prefix != null) {
						if (seenPrefix) {
							// Remove prefix
							KeyValuePair<Dimension, double> updatedDimensionKeyValue = dimension.RemovePrefix(quantity.Value);
							quantity.Dimensions[j] = updatedDimensionKeyValue.Key;
							quantity.Value = updatedDimensionKeyValue.Value;
						} else {
							seenPrefix = true;
						}
					}
				}
			}

			// Move prefixed dimension to start
			// Assumption: There is a max of one dimension with a prefix
			if (MeasurementFactory.Options.CanReorderDimensions) {
				int index = quantity.Dimensions.FindIndex(d => d.Prefix != null);
				if (index > 0) {
					Dimension prefixedDimension = quantity.Dimensions[index];
					quantity.Dimensions.RemoveAt(index);
					quantity.Dimensions.Insert(0, prefixedDimension);
				}
			}
			return quantity;
		}

		/// <summary>
		/// Checks if the quantity is dimensionless.
		/// </summary>
		/// <example>
		/// A quantity of 5 is dimensionless, a quantity of 5 metres is not
		/// </example>
		/// <returns>True if quantity has no dimensions, else false</returns>
		public bool IsDimensionless() {
			return Dimensions.Count == 0;
		}

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
		public bool IsCommensurable(Quantity quantity) {

			// Dimensionless
			if (IsDimensionless() && quantity.IsDimensionless()) {
				return true;
			}

			Quantity simplifiedThis = Simplify();
			Quantity simplifiedQuantity = quantity.Simplify();

			if (simplifiedThis.Dimensions.Count != simplifiedQuantity.Dimensions.Count) {
				return false;
			}

			bool allHaveMatch = true;
			simplifiedThis.Dimensions.ForEach(d1 => {
				bool foundMatch = false;
				simplifiedQuantity.Dimensions.ForEach(d2 => {
					if (d1.IsCommensurableMatch(d2)) {
						foundMatch = true;
					}
				});
				if (!foundMatch) {
					allHaveMatch = false;
				}
			});
			return allHaveMatch;
		}

		#endregion

		#region Basic Math - Dimensionless

		/// <summary>
		/// Multiplies the quantity as the multiplicand by a dimensionless value.
		/// </summary>
		/// <param name="value">The value that is the multiplier</param>
		/// <returns>A new quantity that is the product of the two original values</returns>
		public Quantity Multiply(double value) {
			return new Quantity(Value * value, Dimensions.CopyList());
		}

		/// <summary>
		/// Divides the quantity as a dividend by a dimensionless value.
		/// </summary>
		/// <param name="value">The value that is the divisor</param>
		/// <returns>A new quantity that is the quotient</returns>
		public Quantity Divide(double value) {
			return new Quantity(Value / value, Dimensions.CopyList());
		}

		/// <summary>
		/// Adds an arbitrary value to the original quantity.
		/// Assumes that value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be added</param>
		/// <returns>A new quantity with the sum of the values</returns>
		public Quantity Add(double value) {
			return new Quantity(Value + value, Dimensions.CopyList());
		}

		/// <summary>
		/// Subtracts an arbitrary value from the original quanity.
		/// Assumes that the value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be subtracted</param>
		/// <returns>A new quantity with the difference of the values</returns>
		public Quantity Subtract(double value) {
			return new Quantity(Value - value, Dimensions.CopyList());
		}

		#endregion

		#region Basic Math - Quantity Based

		/// <summary>
		/// Multiplies this quantity with another and combines the existing dimensions.
		/// Simplifies the result afterwards.
		/// </summary>
		/// <param name="q">The quantity being multiplied by</param>
		/// <returns>A resulting quantity that is the product of the two quantities</returns>
		public Quantity Multiply(Quantity q) {
			List<Dimension> clonedDimensions = Dimensions.CopyList();
			clonedDimensions.AddRange(q.Dimensions.CopyList());
			var newQuantity = new Quantity(Value * q.Value, clonedDimensions);
			return newQuantity.Simplify();
		}

		/// <summary>
		/// Divides this quantity with another, returning a quantity with dimensions
		/// that are the difference of the dividend and the divisor.
		/// Simplifies the result afterwards.
		/// </summary>
		/// <param name="q">The quantity that is the divisor</param>
		/// <returns>A resulting quantity that is the quotient of the two quantities</returns>
		public Quantity Divide(Quantity q) {
			List<Dimension> clonedDimensions = Dimensions.CopyList();
			clonedDimensions.AddRange(q.Dimensions.CopyList().Select(d => d.Invert()));
			var newQuantity = new Quantity(Value / q.Value, clonedDimensions);
			return newQuantity.Simplify();
		}

		/// <summary>
		/// Adds this quantity with another that has commensurable dimensions.
		/// Takes care of converting the parameter quantity into the same units.
		/// </summary>
		/// <param name="q">The quantity being added</param>
		/// <returns>A resulting quantity that is the sum of the two quantities</returns>
		public Quantity Add(Quantity q) {
			// Convert value into same units
			Quantity convertedQuantity = q.Convert(this);
			return new Quantity(Value + convertedQuantity.Value, Dimensions.CopyList());
		}

		/// <summary>
		/// Subtracts this quantity from another that has commensurable dimensions.
		/// Takes care of converting the parameter quantity into the same units.
		/// </summary>
		/// <param name="q">The quantity being subtracted</param>
		/// <returns>A resulting quantity that is the difference of the two quantities</returns>
		public Quantity Subtract(Quantity q) {
			// Convert value into same units
			Quantity convertedQuantity = q.Convert(this);
			return new Quantity(Value - convertedQuantity.Value, Dimensions.CopyList());
		}

		#endregion

		#region Extended Math

		// Extensions of System.Math functions

		/// <seealso cref="System.Math.Abs(double)"/>
		public Quantity Abs() {
			return new Quantity(Math.Abs(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Acos(double)"/>
		public Quantity Acos() {
			return new Quantity(Math.Acos(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Asin(double)"/>
		public Quantity Asin() {
			return new Quantity(Math.Asin(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Atan(double)"/>
		public Quantity Atan() {
			return new Quantity(Math.Atan(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Ceiling(double)"/>
		public Quantity Ceiling() {
			return new Quantity(Math.Ceiling(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Cos(double)"/>
		public Quantity Cos() {
			return new Quantity(Math.Cos(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Exp(double)"/>
		public Quantity Exp() {
			return new Quantity(Math.Exp(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Floor(double)"/>
		public Quantity Floor() {
			return new Quantity(Math.Floor(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Log(double)"/>
		public Quantity Log() {
			return new Quantity(Math.Log(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Log10(double)"/>
		public Quantity Log10() {
			return new Quantity(Math.Log10(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Round(double)"/>
		public Quantity Round() {
			return new Quantity(Math.Round(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Sin(double)"/>
		public Quantity Sin() {
			return new Quantity(Math.Sin(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Sqrt(double)"/>
		public Quantity Sqrt() {
			return new Quantity(Math.Sqrt(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Tan(double)"/>
		public Quantity Tan() {
			return new Quantity(Math.Tan(Value), Dimensions.CopyList());
		}

		// Extra functions not avaliable in JS version

		/// <seealso cref="System.Math.Cosh(double)"/>
		public Quantity Cosh() {
			return new Quantity(Math.Cosh(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Sinh(double)"/>
		public Quantity Sinh() {
			return new Quantity(Math.Sinh(Value), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Tanh(double)"/>
		public Quantity Tanh() {
			return new Quantity(Math.Tanh(Value), Dimensions.CopyList());
		}

		// Functions requiring a parameter

		/// <summary>
		/// A standard atan2 function where this provides the x coordinate and a provided value provides the y.
		/// It's assumed that any value has the same dimensions as the original quantity.
		/// </summary>
		/// <seealso cref="System.Math.Atan2(double, double)"/>
		/// <param name="y">A raw y value for the atan2 function</param>
		/// <returns>The atan2 result</returns>
		public Quantity Atan2(double y) {
			return new Quantity(Math.Atan2(y, Value), Dimensions.CopyList());
		}

		/// <summary>
		/// A standard atan2 function where this provides the y coordinate and the provided quantity provides the y.
		/// Both quantities must be commensurable. The parameter quantity is converted to make sure the coordinates are equivalent.
		/// </summary>
		/// <seealso cref="System.Math.Atan2(double, double)"/>
		/// <param name="y">A quantity that describes the y coordinate</param>
		/// <returns>The atan2 result with the dimensions of the x value</returns>
		public Quantity Atan2(Quantity y) {
			return Atan2(y.Convert(this).Value);
		}

		/// <summary>
		/// A basic power function where this acts as the base value.
		/// </summary>
		/// <seealso cref="System.Math.Pow(double, double)"/>
		/// <param name="y">The raw power value</param>
		/// <returns>The base raised to the provided power</returns>
		public Quantity Pow(double y) {
			return new Quantity(Math.Pow(Value, y), Dimensions.CopyList());
		}

		/// <summary>
		/// A basic power function taking in the power value as a quantity.
		/// The power parameter must be dimensionless.
		/// </summary>
		/// <seealso cref="System.Math.Pow(double, double)"/>
		/// <exception cref="System.Exception">Thrown when the power parameter is not dimensionless</exception>
		/// <param name="y">The dimensionless quantity</param>
		/// <returns>The base raised to the provided power</returns>
		public Quantity Pow(Quantity y) {
			if (!y.IsDimensionless()) {
				throw new Exception("The power must be dimensionless");
			}
			return Pow(y.Value);
		}

		#region Max

		/// <seealso cref="System.Math.Max(double, double)"/>
		public Quantity Max(double y) {
			return new Quantity(Math.Max(Value, y), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Max(double, double)"/>
		public Quantity Max(Quantity y) {
			return Max(y.Convert(this).Value);
		}

		/// <summary>
		/// A varargs function for calculating the max value of a set of values including this.
		/// All values are assumed to have the same dimensions as the initial quantity.
		/// </summary>
		/// <param name="values">The varargs of values to test</param>
		/// <returns>The largest value as a quantity</returns>
		public Quantity Max(params double[] values) {
			List<double> vals = values.ToList();
			vals.Add(Value);
			return new Quantity(vals.Max(), Dimensions.CopyList());
		}

		/// <summary>
		/// A varargs function for calculating the largest quantity of a set of quantities including this.
		/// All quantities must be commensurable. The dimensions of the first quantity are preserved.
		/// </summary>
		/// <exception cref="System.Exception">Thrown when one or more quantities are not commensurable</exception>
		/// <param name="values">The varargs of quantities to test</param>
		/// <returns>The largest value as a quantity</returns>
		public Quantity Max(params Quantity[] values) {
			// TODO option controlled whether to keep base units or use new units
			return Max(values.ToList().Select(q => q.Convert(this).Value).ToArray());
		}

		#endregion

		#region Min

		/// <seealso cref="System.Math.Min(double, double)"/>
		public Quantity Min(double y) {
			return new Quantity(Math.Min(Value, y), Dimensions.CopyList());
		}

		/// <seealso cref="System.Math.Min(double, double)"/>
		public Quantity Min(Quantity y) {
			return Min(y.Convert(this).Value);
		}

		/// <summary>
		/// A varargs function for calculating the min value of a set of values including this.
		/// All values are assumed to have the same dimensions as the initial quantity.
		/// </summary>
		/// <param name="values">The varargs of values to test</param>
		/// <returns>The smallest value as a quantity</returns>
		public Quantity Min(params double[] values) {
			List<double> vals = values.ToList();
			vals.Add(Value);
			return new Quantity(vals.Min(), Dimensions.CopyList());
		}

		/// <summary>
		/// A varargs function for calculating the smallest quantity of a set of quantities including this.
		/// All quantities must be commensurable. The dimensions of the first quantity are preserved.
		/// </summary>
		/// <exception cref="System.Exception">Thrown when one or more quantities are not commensurable</exception>
		/// <param name="values">The varargs of quantities to test</param>
		/// <returns>The smallest value as a quantity</returns>
		public Quantity Min(params Quantity[] values) {
			// TODO option controlled whether to keep base units or use new units
			return Min(values.ToList().Select(q => q.Convert(this).Value).ToArray());
		}

		#endregion

		#endregion

		#region Copyable

		/// <summary>
		/// Helper method that utilises the copy constructor to copy the existing quantity.
		/// </summary>
		/// <returns>A copy of this quantity</returns>
		public Quantity Copy() {
			return new Quantity(this);
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Serializes the quantity as a json string.
		/// </summary>
		/// <returns>The serialized quantity</returns>
		string ISerializable.ToJson() {
			return this.ToJson();
		}

		#endregion

		#region Formatting

		/// <summary>
		/// A simple format function that formats the quantity
		/// with the default options and the current culture.
		/// </summary>
		/// <returns>The formatted string</returns>
		public string Format() {
			return Format(new FormatOptions(CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Formats the quantity based on a series of options to provide
		/// various human readable alternatives.
		/// TODO - Too Long!! Separate into smaller methods
		/// </summary>
		/// <seealso cref="ForgedSoftware.Measurement.FormatOptions"/>
		/// <param name="options">An object detailing the different format options</param>
		/// <returns>The formatted string.</returns>
		public string Format(FormatOptions options) {
			string valueStr = "";

			// Precision/Fixed
			if (options.Fixed >= 0) {
				valueStr += Value.ToString("F" + options.Fixed);
			} else if (options.Precision > 0) {
				valueStr += Value.ToString("G" + options.Fixed);
			} else {
				valueStr += Value.ToString("G");
			}

			// Separator/Decimal
			int numLength = valueStr.IndexOf(".", StringComparison.InvariantCulture);
			if (numLength == -1) {
				numLength = valueStr.Length;
			}
			var separatorPos = numLength - options.GroupSize;
			valueStr = valueStr.Replace(".", options.DecimalSeparator);
			if (options.GroupSeparator.Length > 0 && !double.IsInfinity(Value) && !double.IsNaN(Value)) {
				while (separatorPos > 0) {
					valueStr = valueStr.Insert(separatorPos, options.GroupSeparator);
					separatorPos -= options.GroupSize;
				}
			}

			// Exponents
			if (options.ExpandExponent) {
				int eIndex = valueStr.IndexOf("E", StringComparison.InvariantCulture);
				if (eIndex >= 0) {
					double exponent = Math.Floor(Math.Log(Value) / Math.Log(10));
					valueStr = valueStr.Substring(0, eIndex);
					string exponentStr = (options.Ascii) ? "^" + exponent : exponent.ToSuperScript();
					valueStr += " x 10" + exponentStr;
				}
			}

			// Dimensions
			List<Dimension> clonedDimensions = Dimensions.CopyList();
			if (options.Sort) {
				//clonedDimensions.Sort((d1, d2) => (d1.Power == d2.Power) ? -2 : d2.Power - d1.Power);
				clonedDimensions = clonedDimensions.OrderBy(d => -d.Power).ToList();
			}
			IEnumerable<string> dimensionStrings = clonedDimensions.Select(d => d.Format(options));

			string joiner = (options.FullName) ? " " : options.UnitSeparator;
			string dimStr = dimensionStrings.Aggregate((current, next) => current + joiner + next);

			// Returning
			if (options.Show == FormatOptions.QuantityParts.DimensionsOnly) {
				return dimStr;
			}
			if (options.Show == FormatOptions.QuantityParts.ValueOnly) {
				return valueStr;
			}
			if (dimStr.Length > 0) {
				valueStr += " ";
			}
			return valueStr + dimStr;
		}

		#endregion

		#region ToString

		/// <summary>
		/// Basic to string override that provides a simple formatted
		/// version of the string using default options and the current culture.
		/// </summary>
		/// <returns>The formatted string</returns>
		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Provides a ToString method with various standard format options.
		/// A specific IFormatProvider can be specified, else the current culture is used.
		/// 
		/// The avaliable format strings are:
		/// - "G": General format, uses the default format options
		/// - "S": Scientific format is equivalent to the General format
		/// - "R": Raw format is an rough, ascii only format
		/// - "N": Number format just provides the formatted value of the quantity
		/// - "U": Unit format just provides the formatted units of the quantity as specified by the dimensions
		/// </summary>
		/// <param name="format">The format letter</param>
		/// <param name="provider">A format provider</param>
		/// <returns>The formatted string</returns>
		public string ToString(string format, IFormatProvider provider = null) {
			if (String.IsNullOrEmpty(format)) {
				format = "G";
			}
			format = format.Trim().ToUpperInvariant();

			if (provider == null) {
				provider = CultureInfo.CurrentCulture;
			}

			FormatOptions options;

			switch (format) {
				case "G":
				case "S":
					options = FormatOptions.Default(provider);
					break;
				case "R":
					options = FormatOptions.Raw(provider);
					break;
				case "N":
					options = FormatOptions.ValueOnly(provider);
					break;
				case "U":
					options = FormatOptions.DimensionsOnly(provider);
					break;
				default:
					throw new ArgumentException("Provided format string is not recognized");
			}

			return Format(options);
		}

		#endregion

	}
}
