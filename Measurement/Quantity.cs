using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	[DataContract]
	public class Quantity : ISerializable, IFormatter, IFormattable {

		private Quantity() {
			Dimensions = new List<Dimension>();
		}

		public Quantity(double value)
			: this() {
			Value = value;
		}

		public Quantity(double value, IEnumerable<string> unitNames)
			: this(value) {
			foreach (string unitName in unitNames) {
				Dimensions.Add(new Dimension(unitName));
			}
		}

		public Quantity(double value, IEnumerable<Dimension> dimensions)
			: this(value) {
			Dimensions.AddRange(dimensions);
		}

		[DataMember(Name = "value")]
		public double Value { get; private set; }
		[DataMember(Name = "dimensions")]
		public List<Dimension> Dimensions { get; private set; }

		public bool IsDimensionless {
			get { return Dimensions.Count == 0; }
		}

		#region Conversion

		public Quantity Convert(string unitName) {
			return Convert(MeasurementFactory.FindUnit(unitName));
		}

		public Quantity Convert(Unit unit) {
			Quantity quantityAsBase = ConvertToBase();
			return quantityAsBase.ConvertFromBase(unit);
		}

		public Quantity Convert(Quantity quantity) {
			Quantity quantityAsBase = ConvertToBase();
			if (!IsCommensurable(quantity)) {
				throw new Exception("Quantities must have commensurable dimensions in order to convert between them");
			}
			quantity.Dimensions.ForEach(d => quantityAsBase = quantityAsBase.ConvertFromBase(d.Unit));
			return quantityAsBase;
		}

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

		protected Quantity ConvertFromBase(Unit unit) {
			double convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				if (dimension.Unit.System.Units.Contains(unit)) {
					KeyValuePair<Dimension, double> result = dimension.ConvertFromBase(convertedValue, unit);
					convertedValue = result.Value;
					newDimensions.Add(result.Key);
				} else {
					newDimensions.Add(dimension.Copy());
				}
			}
			return new Quantity(convertedValue, newDimensions);
		}

		#endregion

		#region Simplification

		public Quantity Simplify() {
			var newDimensions = new List<Dimension>();
			var processedDimensions = new List<int>();
			double computedValue = Value;

			for (int index = 0; index < Dimensions.Count; index++) {
				Dimension dimension = Dimensions[index];
				if (dimension.Power != 0 && !processedDimensions.Contains(index)) {
					for (int i = index  + 1; i < Dimensions.Count; i++) {
						if (dimension.Unit.System.Name == Dimensions[i].Unit.System.Name) {
							KeyValuePair<Dimension, double> dimValuePair = dimension.Combine(computedValue, Dimensions[i]);
							dimension = dimValuePair.Key;
							computedValue = dimValuePair.Value;
							processedDimensions.Add(i);
						}
					}
					if (dimension.Power != 0) {
						newDimensions.Add(dimension);
					}
					processedDimensions.Add(index);
				}
			}
			return new Quantity(computedValue, newDimensions);
		}

		public bool IsCommensurable(Quantity quantity) {

			// Dimensionless
			if (IsDimensionless && quantity.IsDimensionless) {
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

		#region Basic Math

		#region Dimensionless

		public Quantity Multiply(double value) {
			return new Quantity(Value*value, Dimensions.CopyList());
		}

		public Quantity Divide(double value) {
			return new Quantity(Value/value, Dimensions.CopyList());
		}

		/// <summary>
		/// Adds an arbitrary value to the original quantity.
		/// Assumes that value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be added</param>
		/// <returns>A new quantity with the value added</returns>
		public Quantity Add(double value) {
			return new Quantity(Value + value, Dimensions.CopyList());
		}

		/// <summary>
		/// Subtracts an arbitrary value from the original quanity.
		/// Assumes that the value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="value">The value to be subtracted</param>
		/// <returns>A new quantity with the value subtracted</returns>
		public Quantity Subtract(double value) {
			return new Quantity(Value - value, Dimensions.CopyList());
		}

		#endregion

		public Quantity Multiply(Quantity q) {
			List<Dimension> clonedDimensions = Dimensions.CopyList();
			clonedDimensions.AddRange(q.Dimensions.CopyList());
			var newQuantity = new Quantity(Value * q.Value, clonedDimensions);
			return newQuantity.Simplify();
		}

		public Quantity Divide(Quantity q) {
			List<Dimension> clonedDimensions = Dimensions.CopyList();
			clonedDimensions.AddRange(q.Dimensions.CopyList().Select(d => d.Invert()));
			var newQuantity = new Quantity(Value / q.Value, clonedDimensions);
			return newQuantity.Simplify();
		}

		public Quantity Add(Quantity q) {
			// Convert value into same units
			Quantity convertedQuantity = q.Convert(this);
			return new Quantity(Value + convertedQuantity.Value, Dimensions.CopyList());
		}

		public Quantity Subtract(Quantity q) {
			// Convert value into same units
			Quantity convertedQuantity = q.Convert(this);
			return new Quantity(Value - convertedQuantity.Value, Dimensions.CopyList());
		}

		#endregion

		#region Extended Math

		// Extensions of System.Math functions

		public Quantity Abs() { return new Quantity(Math.Abs(Value), Dimensions.CopyList()); }
		public Quantity Acos() { return new Quantity(Math.Acos(Value), Dimensions.CopyList()); }
		public Quantity Asin() { return new Quantity(Math.Asin(Value), Dimensions.CopyList()); }
		public Quantity Atan() { return new Quantity(Math.Atan(Value), Dimensions.CopyList()); }
		public Quantity Ceiling() { return new Quantity(Math.Ceiling(Value), Dimensions.CopyList()); }
		public Quantity Cos() { return new Quantity(Math.Cos(Value), Dimensions.CopyList()); }
		public Quantity Exp() { return new Quantity(Math.Exp(Value), Dimensions.CopyList()); }
		public Quantity Floor() { return new Quantity(Math.Floor(Value), Dimensions.CopyList()); }
		public Quantity Log() { return new Quantity(Math.Log(Value), Dimensions.CopyList()); }
		public Quantity Log10() { return new Quantity(Math.Log10(Value), Dimensions.CopyList()); }
		public Quantity Round() { return new Quantity(Math.Round(Value), Dimensions.CopyList()); }
		public Quantity Sin() { return new Quantity(Math.Sin(Value), Dimensions.CopyList()); }
		public Quantity Sqrt() { return new Quantity(Math.Sqrt(Value), Dimensions.CopyList()); }
		public Quantity Tan() { return new Quantity(Math.Tan(Value), Dimensions.CopyList()); }

		// Extra functions not avaliable in JS version

		public Quantity Cosh() { return new Quantity(Math.Cosh(Value), Dimensions.CopyList()); }
		public Quantity Sinh() { return new Quantity(Math.Sinh(Value), Dimensions.CopyList()); }
		public Quantity Tanh() { return new Quantity(Math.Tanh(Value), Dimensions.CopyList()); }

		// Functions requiring a parameter

		/// <summary>
		/// A standard atan2 function where this provides the x coordinate and a provided value provides the y.
		/// It's assumed that any value has the same dimensions as the original quantity.
		/// </summary>
		/// <param name="y">A raw y value for the atan2 function</param>
		/// <returns>The atan2 result</returns>
		public Quantity Atan2(double y) {
			return new Quantity(Math.Atan2(y, Value), Dimensions.CopyList());
		}

		/// <summary>
		/// A standard atan2 function where this provides the y coordinate and the provided quantity provides the y.
		/// Both quantities must be commensurable. The parameter quantity is converted to make sure the coordinates are equivalent.
		/// </summary>
		/// <param name="y">A quantity that describes the y coordinate</param>
		/// <returns>The atan2 result with the dimensions of the x value</returns>
		public Quantity Atan2(Quantity y) {
			return Atan2(y.Convert(this).Value);
		}

		/// <summary>
		/// A basic power function where this acts as the base value.
		/// </summary>
		/// <param name="y">The raw power value</param>
		/// <returns>The base raised to the provided power</returns>
		public Quantity Pow(double y) {
			return new Quantity(Math.Pow(Value, y), Dimensions.CopyList());
		}

		/// <summary>
		/// A basic power function taking in the power value as a quantity.
		/// The power parameter must be dimensionless.
		/// </summary>
		/// <exception cref="System.Exception">Thrown when the power parameter is not dimensionless</exception>
		/// <param name="y">The dimensionless quantity</param>
		/// <returns>The base raised to the provided power</returns>
		public Quantity Pow(Quantity y) {
			if (!y.IsDimensionless) {
				throw new Exception("The power must be dimensionless");
			}
			return Pow(y.Value);
		}

		#region Max

		public Quantity Max(double y) { return new Quantity(Math.Max(Value, y), Dimensions.CopyList()); }
		public Quantity Max(Quantity y) { return Max(y.Convert(this).Value); }

		public Quantity Max(params double[] values) {
			List<double> vals = values.ToList();
			vals.Add(Value);
			return new Quantity(vals.Max(), Dimensions.CopyList());
		}

		public Quantity Max(params Quantity[] values) {
			return Max(values.ToList().Select(q => q.Convert(this).Value).ToArray());
		}

		#endregion

		#region Min

		public Quantity Min(double y) { return new Quantity(Math.Min(Value, y), Dimensions.CopyList()); }
		public Quantity Min(Quantity y) { return Min(y.Convert(this).Value); }

		public Quantity Min(params double[] values) {
			List<double> vals = values.ToList();
			vals.Add(Value);
			return new Quantity(vals.Min(), Dimensions.CopyList());
		}

		public Quantity Min(params Quantity[] values) {
			return Min(values.ToList().Select(q => q.Convert(this).Value).ToArray());
		}

		#endregion

		#endregion

		string ISerializable.ToJson() {
			return this.ToJson();
		}

		#region Formatting

		public string Format() {
			return Format(new FormatOptions(CultureInfo.CurrentCulture));
		}

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
			if (options.GroupSeparator.Length > 0) {
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

		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		public string ToString(string format) {
			return ToString(format, CultureInfo.CurrentCulture);
		}

		public string ToString(string format, IFormatProvider provider) {
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
	}
}
