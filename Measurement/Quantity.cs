using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using ForgedSoftware.Measurement.Entities;
using ForgedSoftware.Measurement.Interfaces;
using ForgedSoftware.Measurement.Number;
using ISerializable = ForgedSoftware.Measurement.Interfaces.ISerializable;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A generics based approach to quantity that allows any type of number to be used
	/// providing a very broad range of functionality for manipulating quantities.
	/// </summary>
	/// <typeparam name="TNumber">The number type that this quantity manipulates</typeparam>
	[DataContract]
	public class Quantity<TNumber> : IQuantity<TNumber, TNumber, Quantity<TNumber>>
			where TNumber : INumber<TNumber> {

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public Quantity() {
			Dimensions = new List<Dimension>();
		}

		/// <summary>
		/// Dimensionless constructor
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		public Quantity(TNumber value) : this() {
			Value = value;
		}

		/// <summary>
		/// Helper constructor for quantity with a single dimension
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitName">The common name of the unit of the dimension</param>
		public Quantity(TNumber value, string unitName)
			: this(value) {
			Dimensions.Add(new Dimension(unitName));
		}

		/// <summary>
		/// Helper constructor for quantity with multiple simple dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitNames">A set of unit names to turn into dimensions</param>
		public Quantity(TNumber value, IEnumerable<string> unitNames)
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
		public Quantity(TNumber value, Dimension dimension)
			: this(value) {
			Dimensions.Add(dimension);
		}

		/// <summary>
		/// A full constructor for multiple, complex dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="dimensions">Pre-existing dimensions to use with this quantity</param>
		public Quantity(TNumber value, IEnumerable<Dimension> dimensions)
			: this(value) {
			Dimensions.AddRange(dimensions ?? new List<Dimension>());
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="quantity">The quantity to copy</param>
		public Quantity(Quantity<TNumber> quantity) {
			Value = quantity.Value;
			Dimensions = quantity.Dimensions.CloneList();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The Value of a Quantity is an unqualified, dimensionless measurement.
		/// </summary>
		[DataMember(Name = "value")]
		public TNumber Value { get; set; }

		/// <summary>
		/// The Dimensions of a Quantity are the combination of units of measurement
		/// that qualify this quantity in the real world.
		/// </summary>
		[DataMember(Name = "dimensions")]
		public List<Dimension> Dimensions { get; private set; }

		#endregion

		#region Conversion

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unitName">The name of the unit to convert</param>
		/// <returns>The converted quantity</returns>
		public Quantity<TNumber> Convert(string unitName) {
			return Convert(MeasurementCorpus.FindUnit(unitName));
		}

		/// <summary>
		/// Converts a quantity based on a unit that matches an existing dimension.
		/// </summary>
		/// <param name="unit">The unit to convert</param>
		/// <returns>The converted quantity</returns>
		public Quantity<TNumber> Convert(Unit unit) {
			Quantity<TNumber> quantityAsBase = ConvertToBase();
			return quantityAsBase.ConvertFromBase(unit);
		}

		/// <summary>
		/// Converts a quantity based on another quantity's dimensions.
		/// </summary>
		/// <param name="quantity">The other quantity whose dimensions to converge on</param>
		/// <returns>The converted quantity</returns>
		public Quantity<TNumber> Convert(Quantity<TNumber> quantity) {
			Quantity<TNumber> quantityAsBase = ConvertToBase();
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
		public Quantity<TNumber> ConvertToBase() {
			TNumber convertedValue = Value;
			List<Dimension> newDimensions = Dimensions
				.Select(dimension => dimension.ConvertToBase(ref convertedValue)).ToList();
			return new Quantity<TNumber>(convertedValue, newDimensions);
		}

		/// <summary>
		/// Converts a quantity in based on a unit.
		/// Assumes that the dimensions that have the same system as the provided unit are in a base unit.
		/// </summary>
		/// <param name="unit">The unit to convert into</param>
		/// <param name="prefix">The prefix to convert into</param>
		/// <returns>The converted quantity</returns>
		public Quantity<TNumber> ConvertFromBase(Unit unit, Prefix prefix = null) {
			TNumber convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				if (dimension.Unit.DimensionDefinition.Units.Contains(unit)) {
					newDimensions.Add(dimension.ConvertFromBase(ref convertedValue, unit, prefix));
				} else {
					newDimensions.Add(dimension.Clone());
				}
			}
			return new Quantity<TNumber>(convertedValue, newDimensions);
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
		public Quantity<TNumber> Simplify() {
			TNumber computedValue = Value;
			List<Dimension> simplifiedDimensions = Dimensions.Simplify(ref computedValue);
			var resultingQuantity = new Quantity<TNumber>(computedValue, simplifiedDimensions);

			if (MeasurementCorpus.Options.UseAutomaticPrefixManagement) {
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
		public Quantity<TNumber> TidyPrefixes() {
			Quantity<TNumber> quantity = Clone();
			if (IsDimensionless()) {
				return quantity;
			}
			int numberOfPrefixes = quantity.Dimensions.Count(d => d.Prefix != null);

			// Try add a prefix to prefixless dimensions
			if (numberOfPrefixes == 0) {
				for (int i = 0; i < quantity.Dimensions.Count; i++) {
					Dimension dimension = quantity.Dimensions[i];
					if (dimension.CanApplyPrefix()) {
						TNumber computedValue = quantity.Value;
						quantity.Dimensions[i] = dimension.ApplyPrefix(ref computedValue);
						quantity.Value = computedValue;
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
							TNumber computedValue = quantity.Value;
							quantity.Dimensions[j] = dimension.RemovePrefix(ref computedValue);
							quantity.Value = computedValue;
						} else {
							seenPrefix = true;
						}
					}
				}
			}

			// Move prefixed dimension to start
			// Assumption: There is a max of one dimension with a prefix
			if (MeasurementCorpus.Options.AllowReorderingDimensions) {
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
		public bool IsCommensurable(Quantity<TNumber> quantity) {

			// Dimensionless
			if (IsDimensionless() && quantity.IsDimensionless()) {
				return true;
			}

			Quantity<TNumber> simplifiedThis = Simplify();
			Quantity<TNumber> simplifiedQuantity = quantity.Simplify();

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
		public Quantity<TNumber> Multiply(TNumber value) {
			return new Quantity<TNumber>(Value.Multiply(value), Dimensions.CloneList());
		}

		/// <summary>
		/// Divides the quantity as a dividend by a dimensionless value.
		/// </summary>
		/// <param name="value">The value that is the divisor</param>
		/// <returns>A new quantity that is the quotient</returns>
		public Quantity<TNumber> Divide(TNumber value) {
			return new Quantity<TNumber>(Value.Divide(value), Dimensions.CloneList());
		}

		/// <summary>
		/// Adds an arbitrary value to the original quantity.
		/// Assumes that value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be added</param>
		/// <returns>A new quantity with the sum of the values</returns>
		public Quantity<TNumber> Add(TNumber value) {
			return new Quantity<TNumber>(Value.Add(value), Dimensions.CloneList());
		}

		/// <summary>
		/// Subtracts an arbitrary value from the original quanity.
		/// Assumes that the value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be subtracted</param>
		/// <returns>A new quantity with the difference of the values</returns>
		public Quantity<TNumber> Subtract(TNumber value) {
			return new Quantity<TNumber>(Value.Subtract(value), Dimensions.CloneList());
		}

		#endregion

		#region Basic Math - Quantity Based

		/// <summary>
		/// Multiplies this quantity with another and combines the existing dimensions.
		/// Simplifies the result afterwards.
		/// </summary>
		/// <param name="q">The quantity being multiplied by</param>
		/// <returns>A resulting quantity that is the product of the two quantities</returns>
		public Quantity<TNumber> Multiply(Quantity<TNumber> q) {
			List<Dimension> clonedDimensions = Dimensions.CloneList();
			clonedDimensions.AddRange(q.Dimensions.CloneList());
			var newQuantity = new Quantity<TNumber>(Value.Multiply(q.Value), clonedDimensions);
			return newQuantity.Simplify();
		}

		/// <summary>
		/// Divides this quantity with another, returning a quantity with dimensions
		/// that are the difference of the dividend and the divisor.
		/// Simplifies the result afterwards.
		/// </summary>
		/// <param name="q">The quantity that is the divisor</param>
		/// <returns>A resulting quantity that is the quotient of the two quantities</returns>
		public Quantity<TNumber> Divide(Quantity<TNumber> q) {
			List<Dimension> clonedDimensions = Dimensions.CloneList();
			clonedDimensions.AddRange(q.Dimensions.CloneList().Select(d => d.Invert()));
			var newQuantity = new Quantity<TNumber>(Value.Divide(q.Value), clonedDimensions);
			return newQuantity.Simplify();
		}

		/// <summary>
		/// Adds this quantity with another that has commensurable dimensions.
		/// Takes care of converting the parameter quantity into the same units.
		/// </summary>
		/// <param name="q">The quantity being added</param>
		/// <returns>A resulting quantity that is the sum of the two quantities</returns>
		public Quantity<TNumber> Add(Quantity<TNumber> q) {
			// Convert value into same units
			Quantity<TNumber> convertedQuantity = q.Convert(this);
			return new Quantity<TNumber>(Value.Add(convertedQuantity.Value), Dimensions.CloneList());
		}

		/// <summary>
		/// Subtracts this quantity from another that has commensurable dimensions.
		/// Takes care of converting the parameter quantity into the same units.
		/// </summary>
		/// <param name="q">The quantity being subtracted</param>
		/// <returns>A resulting quantity that is the difference of the two quantities</returns>
		public Quantity<TNumber> Subtract(Quantity<TNumber> q) {
			// Convert value into same units
			Quantity<TNumber> convertedQuantity = q.Convert(this);
			return new Quantity<TNumber>(Value.Subtract(convertedQuantity.Value), Dimensions.CloneList());
		}

		/// <summary>
		/// Negates this quantity by making the value negative and inverting the powers
		/// on the dimensions.
		/// </summary>
		/// <returns>A resulting quantity that is the negation of the original one</returns>
		public Quantity<TNumber> Negate() {
			return new Quantity<TNumber>(Value.Negate(), Dimensions.CloneList().Select(d => d.Invert()));
		}

		#endregion
		
		#region Extended Math

		public Quantity<TNumber> Abs() {
			return new Quantity<TNumber>(Value.Abs(), Dimensions.CloneList());
		}

		public Quantity<TNumber> Ceiling() {
			return new Quantity<TNumber>(Value.Ceiling(), Dimensions.CloneList());
		}

		public Quantity<TNumber> Floor() {
			return new Quantity<TNumber>(Value.Floor(), Dimensions.CloneList());
		}

		public Quantity<TNumber> Pow(double power) {
			return new Quantity<TNumber>(Value.Pow(power), Dimensions.CloneList());
		}

		public Quantity<TNumber> Round() {
			return new Quantity<TNumber>(Value.Round(), Dimensions.CloneList());
		}

		public Quantity<TNumber> Sqrt() {
			return new Quantity<TNumber>(Value.Sqrt(), Dimensions.CloneList());
		}

		public Quantity<TNumber> Max(Quantity<TNumber> other) {
			return Max(new[] {other});
		}

		public Quantity<TNumber> Max(params Quantity<TNumber>[] values) {
			var quantities = new List<Quantity<TNumber>>(values) {this};
			return new Quantity<TNumber>(quantities.Max(q => q.Convert(this)));
		}

		public Quantity<TNumber> Min(Quantity<TNumber> other) {
			return Min(new[] { other });
		}

		public Quantity<TNumber> Min(params Quantity<TNumber>[] values) {
			var quantities = new List<Quantity<TNumber>>(values) { this };
			return new Quantity<TNumber>(quantities.Min(q => q.Convert(this)));
		}

		#endregion

		#region Cloneable
		
		/// <summary>
		/// A simple clone of the quantity.
		/// </summary>
		/// <returns>A clone of this quantity</returns>
		object ICloneable.Clone() {
			return Clone();
		}

		/// <summary>
		/// Utilizes the copy constructor to make a clone of this quantity.
		/// </summary>
		/// <returns>A clone of this quantity</returns>
		public Quantity<TNumber> Clone() {
			return new Quantity<TNumber>(this);
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

		/// <summary>
		/// Static method to deserialize a json string into a quantity.
		/// </summary>
		/// <param name="json">The json serialization of a quantity</param>
		/// <returns>The deserialized quantity</returns>
		public static Quantity<TNumber> FromJson(string json) {
			return json.FromJson<Quantity<TNumber>>();
		}

		/// <summary>
		/// Explicit implementation of IObjectReference to make sure quantity
		/// is instantiated properly during deserialization.
		/// </summary>
		object IObjectReference.GetRealObject(StreamingContext context) {
			return new Quantity<TNumber> {
				Value = Value,
				Dimensions = Dimensions ?? new List<Dimension>()
			};
		}

		#endregion

		#region Formatting

		/// <summary>
		/// A simple format function that formats the quantity
		/// with the default options and the current format settings.
		/// </summary>
		/// <returns>The formatted string</returns>
		public string Format() {
			return Format(QuantityFormatInfo.CurrentInfo);
		}

		/// <summary>
		/// Formats the quantity based on a series of options to provide
		/// various human readable alternatives.
		/// </summary>
		/// <seealso cref="ForgedSoftware.Measurement.QuantityFormatInfo"/>
		/// <param name="info">An object detailing the different format options</param>
		/// <returns>The formatted string.</returns>
		public string Format(QuantityFormatInfo info) {
			info = info ?? QuantityFormatInfo.CurrentInfo;

			string formatType = (info.FormatParts == QuantityFormatInfo.QuantityParts.All) ? "Q" : "";
			if (info.ScientificExponent) {
				formatType += (info.AsciiOnly) ? "T" : "S";
			}
			formatType += info.DefaultDoubleFormat;

			string precision = (info.Precision < 0) ? "" : info.Precision.ToString();

			// Value
			string valueStr = Value.ToString(formatType + precision, info.NumberFormat);

			// Dimensions
			bool isPlural = (Math.Abs(Value.EquivalentValue - 1) > Double.Epsilon);
			string dimStr = FormatDimensions(info, isPlural);

			// Returning
			if (info.FormatParts == QuantityFormatInfo.QuantityParts.Dimensions) {
				return dimStr;
			}
			if (info.FormatParts == QuantityFormatInfo.QuantityParts.Value) {
				return valueStr;
			}
			return string.Format(valueStr, dimStr);
		}

		private string FormatDimensions(QuantityFormatInfo info, bool isPlural) {
			List<Dimension> clonedDimensions = Dimensions.CloneList();
			if (info.SortDimensions) {
				clonedDimensions = clonedDimensions.OrderBy(d => -d.Power).ToList();
			}
			IEnumerable<string> dimensionStrings = clonedDimensions.Select(d => d.Format(info, isPlural));

			string joiner = (info.TextualDescription) ? " " : info.UnitSeparator;
			return dimensionStrings.Aggregate((current, next) => current + joiner + next);
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
		/// <param name="provider">An optional format provider</param>
		/// <returns>The formatted string</returns>
		public string ToString(string format, IFormatProvider provider = null) {
			if (String.IsNullOrEmpty(format)) {
				format = "G";
			}
			format = format.Trim().ToUpperInvariant();

			QuantityFormatInfo options = QuantityFormatInfo.GetInstance(provider).Clone();

			switch (format) {
				case "G":
					break;
				case "S":
					options.FormatParts = QuantityFormatInfo.QuantityParts.All;
					break;
				case "R":
					options.AsciiOnly = true;
					options.ScientificExponent = false;
					break;
				case "N":
					options.FormatParts = QuantityFormatInfo.QuantityParts.Value;
					break;
				case "U":
					options.FormatParts = QuantityFormatInfo.QuantityParts.Dimensions;
					break;
				default:
					throw new ArgumentException("Provided format string is not recognized");
			}

			return Format(options);
		}

		#endregion

	}

	/// <summary>
	/// A basic quantity type, based around a double, provides basic quantity
	/// functionality and ease-of-use. It also provides some extended math functions.
	/// </summary>
	[DataContract]
	public class Quantity : IQuantity<double, DoubleWrapper, Quantity> {

		private readonly Quantity<DoubleWrapper> _q;

		// Properties for deserializing only
		private double _deserializedValue;
		private List<Dimension> _deserializedDimensions;

		#region Constructors

		/// <summary>
		/// Dimensionless constructor
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		public Quantity(double value) {
			_q = new Quantity<DoubleWrapper>(new DoubleWrapper(value));
		}

		/// <summary>
		/// Helper constructor for quantity with a single dimension
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitName">The common name of the unit of the dimension</param>
		public Quantity(double value, string unitName) {
			_q = new Quantity<DoubleWrapper>(new DoubleWrapper(value), unitName);
		}

		/// <summary>
		/// Helper constructor for quantity with multiple simple dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="unitNames">A set of unit names to turn into dimensions</param>
		public Quantity(double value, IEnumerable<string> unitNames) {
			_q = new Quantity<DoubleWrapper>(new DoubleWrapper(value), unitNames);
		}

		/// <summary>
		/// Helper constructor for quantity with single complex dimension
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="dimension">Pre-existing dimension to use with this quantity</param>
		public Quantity(double value, Dimension dimension) {
			_q = new Quantity<DoubleWrapper>(new DoubleWrapper(value), dimension);
		}

		/// <summary>
		/// A full constructor for multiple, complex dimensions
		/// </summary>
		/// <param name="value">The value of the quantity</param>
		/// <param name="dimensions">Pre-existing dimensions to use with this quantity</param>
		public Quantity(double value, IEnumerable<Dimension> dimensions) {
			_q = new Quantity<DoubleWrapper>(new DoubleWrapper(value), dimensions);
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="quantity">The quantity to copy</param>
		protected Quantity(Quantity quantity) {
			_q = new Quantity<DoubleWrapper>(quantity._q);
		}

		/// <summary>
		/// Helper constructor taking in the quantity to be wrapped
		/// </summary>
		/// <param name="wrappedQuantity">Quantity to be wrapped</param>
		private Quantity(Quantity<DoubleWrapper> wrappedQuantity) {
			_q = wrappedQuantity;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The Value of a Quantity is an unqualified, dimensionless measurement.
		/// In this case it is a scalar.
		/// </summary>
		[DataMember(Name = "value")]
		public double Value {
			get { return _q.Value.Value; }
			set { _deserializedValue = value; }
		}

		/// <summary>
		/// The Dimensions of a Quantity are the combination of units of measurement
		/// that qualify this quantity in the real world.
		/// </summary>
		[DataMember(Name = "dimensions")]
		public List<Dimension> Dimensions {
			get { return _q.Dimensions; }
			set { _deserializedDimensions = value; }
		}

		#endregion

		#region Conversion

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Convert(string)"/>
		/// </summary>
		public Quantity Convert(string unitName) {
			return new Quantity(_q.Convert(unitName));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Convert(Unit)"/>
		/// </summary>
		public Quantity Convert(Unit unit) {
			return new Quantity(_q.Convert(unit));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Convert(Quantity{DoubleWrapper})"/>
		/// </summary>
		public Quantity Convert(Quantity quantity) {
			return new Quantity(_q.Convert(quantity._q));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.ConvertToBase()"/>
		/// </summary>
		public Quantity ConvertToBase() {
			return new Quantity(_q.ConvertToBase());
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.ConvertFromBase(Unit, Prefix)"/>
		/// </summary>
		public Quantity ConvertFromBase(Unit unit, Prefix prefix = null) {
			return new Quantity(_q.ConvertFromBase(unit, prefix));
		}

		#endregion

		#region General Operations

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Simplify()"/>
		/// </summary>
		public Quantity Simplify() {
			return new Quantity(_q.Simplify());
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.TidyPrefixes()"/>
		/// </summary>
		public Quantity TidyPrefixes() {
			return new Quantity(_q.TidyPrefixes());
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.IsDimensionless()"/>
		/// </summary>
		public bool IsDimensionless() {
			return _q.IsDimensionless();
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.IsCommensurable(Quantity{DoubleWrapper})"/>
		/// </summary>
		public bool IsCommensurable(Quantity quantity) {
			return _q.IsCommensurable(quantity._q);
		}

		#endregion

		#region Basic Math - Quantity Based

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Add(Quantity{DoubleWrapper})"/>
		/// </summary>
		public Quantity Add(Quantity add) {
			return new Quantity(_q.Add(add._q));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Subtract(Quantity{DoubleWrapper})"/>
		/// </summary>
		public Quantity Subtract(Quantity subtract) {
			return new Quantity(_q.Subtract(subtract._q));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Multiply(Quantity{DoubleWrapper})"/>
		/// </summary>
		public Quantity Multiply(Quantity multiply) {
			return new Quantity(_q.Multiply(multiply._q));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Divide(Quantity{DoubleWrapper})"/>
		/// </summary>
		public Quantity Divide(Quantity divide) {
			return new Quantity(_q.Divide(divide._q));
		}

		#endregion

		#region Basic Math - Dimensionless

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Add(DoubleWrapper)"/>
		/// </summary>
		public Quantity Add(double add) {
			return new Quantity(_q.Add(new DoubleWrapper(add)));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Subtract(DoubleWrapper)"/>
		/// </summary>
		public Quantity Subtract(double subtract) {
			return new Quantity(_q.Subtract(new DoubleWrapper(subtract)));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Multiply(DoubleWrapper)"/>
		/// </summary>
		public Quantity Multiply(double multiply) {
			return new Quantity(_q.Multiply(new DoubleWrapper(multiply)));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Divide(DoubleWrapper)"/>
		/// </summary>
		public Quantity Divide(double divide) {
			return new Quantity(_q.Divide(new DoubleWrapper(divide)));
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Negate()"/>
		/// </summary>
		public Quantity Negate() {
			return new Quantity(_q.Negate());
		}

		#endregion

		#region Scalar Math Functions (Extensions of System.Math Functions)

		/// <seealso cref="System.Math.Acos(double)"/>
		public Quantity Acos() {
			return new Quantity(Math.Acos(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Asin(double)"/>
		public Quantity Asin() {
			return new Quantity(Math.Asin(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Atan(double)"/>
		public Quantity Atan() {
			return new Quantity(Math.Atan(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Cos(double)"/>
		public Quantity Cos() {
			return new Quantity(Math.Cos(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Exp(double)"/>
		public Quantity Exp() {
			return new Quantity(Math.Exp(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Log(double)"/>
		public Quantity Log() {
			return new Quantity(Math.Log(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Log10(double)"/>
		public Quantity Log10() {
			return new Quantity(Math.Log10(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Sin(double)"/>
		public Quantity Sin() {
			return new Quantity(Math.Sin(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Tan(double)"/>
		public Quantity Tan() {
			return new Quantity(Math.Tan(Value), Dimensions.CloneList());
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
			return new Quantity(Math.Atan2(y, Value), Dimensions.CloneList());
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

		#region Extra Functionality (Not Available in JS Version)

		/// <seealso cref="System.Math.Cosh(double)"/>
		public Quantity Cosh() {
			return new Quantity(Math.Cosh(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Sinh(double)"/>
		public Quantity Sinh() {
			return new Quantity(Math.Sinh(Value), Dimensions.CloneList());
		}

		/// <seealso cref="System.Math.Tanh(double)"/>
		public Quantity Tanh() {
			return new Quantity(Math.Tanh(Value), Dimensions.CloneList());
		}

		#endregion

		#endregion

		#region Extended Math

		public Quantity Abs() {
			return new Quantity(_q.Abs());
		}

		public Quantity Ceiling() {
			return new Quantity(_q.Ceiling());
		}

		public Quantity Floor() {
			return new Quantity(_q.Floor());
		}

		/// <summary>
		/// A basic power function where this acts as the base value.
		/// </summary>
		/// <seealso cref="System.Math.Pow(double, double)"/>
		/// <param name="y">The raw power value</param>
		/// <returns>The base raised to the provided power</returns>
		public Quantity Pow(double y) {
			return new Quantity(_q.Pow(y));
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

		public Quantity Round() {
			return new Quantity(_q.Round());
		}

		public Quantity Sqrt() {
			return new Quantity(_q.Sqrt());
		}

		#region Max

		public Quantity Max(Quantity y) {
			return new Quantity(_q.Max(y._q));
		}

		/// <seealso cref="System.Math.Max(double, double)"/>
		public Quantity Max(double y) {
			return new Quantity(_q.Max(new Quantity(y)._q));
		}

		/// <summary>
		/// A varargs function for calculating the largest quantity of a set of quantities including this.
		/// All quantities must be commensurable. The dimensions of the first quantity are preserved.
		/// </summary>
		/// <exception cref="System.Exception">Thrown when one or more quantities are not commensurable</exception>
		/// <param name="values">The varargs of quantities to test</param>
		/// <returns>The largest value as a quantity</returns>
		public Quantity Max(params Quantity[] values) {
			return new Quantity(_q.Max(values.Select(v => v._q).ToArray()));
		}

		#endregion

		#region Min

		/// <seealso cref="System.Math.Min(double, double)"/>
		public Quantity Min(Quantity y) {
			return new Quantity(_q.Min(y._q));
		}

		/// <seealso cref="System.Math.Min(double, double)"/>
		public Quantity Min(double y) {
			return new Quantity(_q.Min(new Quantity(y)._q));
		}

		/// <summary>
		/// A varargs function for calculating the smallest quantity of a set of quantities including this.
		/// All quantities must be commensurable. The dimensions of the first quantity are preserved.
		/// </summary>
		/// <exception cref="System.Exception">Thrown when one or more quantities are not commensurable</exception>
		/// <param name="values">The varargs of quantities to test</param>
		/// <returns>The smallest value as a quantity</returns>
		public Quantity Min(params Quantity[] values) {
			return new Quantity(_q.Max(values.Select(v => v._q).ToArray()));
		}

		#endregion

		#endregion

		#region Cloneable
		
		/// <summary>
		/// A simple clone of the quantity.
		/// </summary>
		/// <returns>A clone of this quantity</returns>
		object ICloneable.Clone() {
			return Clone();
		}

		/// <summary>
		/// Utilizes the copy constructor to make a clone of this quantity.
		/// </summary>
		/// <returns>A clone of this quantity</returns>
		public Quantity Clone() {
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

		/// <summary>
		/// Static method to deserialize a json string into a quantity.
		/// </summary>
		/// <param name="json">The json serialization of a quantity</param>
		/// <returns>The deserialized quantity</returns>
		public static Quantity FromJson(string json) {
			return json.FromJson<Quantity>();
		}

		/// <summary>
		/// Explicit implementation of IObjectReference to make sure quantity
		/// is instantiated properly during deserialization.
		/// </summary>
		public object GetRealObject(StreamingContext context) {
			var q1 = new Quantity(_deserializedValue, _deserializedDimensions);
			_deserializedValue = default(double);
			_deserializedDimensions = null;
			return q1;
		}

		#endregion

		#region Formatting

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Format()"/>
		/// </summary>
		public string Format() {
			return _q.Format();
		}


		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.Format(QuantityFormatInfo)"/>
		/// </summary>
		public string Format(QuantityFormatInfo info) {
			return _q.Format(info);
		}

		#endregion

		#region ToString

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.ToString()"/>
		/// </summary>
		public override string ToString() {
			return _q.ToString();
		}

		/// <summary>
		/// A wrapper for <see cref="Quantity{DoubleWrapper}.ToString(string, IFormatProvider)"/>
		/// </summary>
		public string ToString(string format, IFormatProvider formatProvider) {
			return _q.ToString(format, formatProvider);
		}

		#endregion

	}
}
