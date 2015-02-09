using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A dimension is an representation of the application of a unit of measurement
	/// to the measurement itself. It includes the unit, a prefix asssociated with that unit,
	/// and the power to which that unit is applied.
	/// </summary>
	/// <example>
	/// An example of a dimension could be: kilometres squared (km^2)
	/// Such a dimension has the unit "metre", the prefix "kilo", and a power of 2.
	/// </example>
	[DataContract]
	public class Dimension : ISerializable, IFormatter, IFormattable, ICopyable<Dimension>, IObjectReference {

		private const int DEFAULT_POWER = 1;
		private const double EPSILON = 1E-15;

		#region Constructors

		/// <summary>
		/// Default private constructor
		/// </summary>
		private Dimension() {
			Power = DEFAULT_POWER;
		}

		/// <summary>
		/// Full constructor for exactly specifying unit, power, and prefix.
		/// </summary>
		/// <param name="unit">The unit to use for this dimension</param>
		/// <param name="power">The power of the dimension</param>
		/// <param name="prefix">The optional prefix of the dimension</param>
		public Dimension(Unit unit, int power, Prefix prefix = null)
			: this() {
			if (unit == null) {
				throw new ArgumentException("A Dimension may not have a Unit that is null");
			}
			Unit = unit;
			Power = power;
			Prefix = prefix;
		}

		/// <summary>
		/// A helper constructor with a unit name, power, and optional prefix name.
		/// </summary>
		/// <param name="unitName">The name of the unit</param>
		/// <param name="power">The power of the dimension</param>
		/// <param name="prefixName">The optional name of the prefix</param>
		public Dimension(string unitName, int power, string prefixName = null)
			: this(MeasurementFactory.FindUnit(unitName), power,
				MeasurementFactory.FindPrefix(prefixName)) {
		}

		/// <summary>
		/// Helper constructor with a unit name and optional prefix name.
		/// </summary>
		/// <param name="unitName">The name of the unit</param>
		/// <param name="prefixName">The optional name of the prefix</param>
		public Dimension(string unitName, string prefixName = null)
			: this(unitName, DEFAULT_POWER, prefixName) {
		}

		/// <summary>
		/// Helper constructor with unit name, system name, power, and prefix name so
		/// the unit can be fully specified.
		/// </summary>
		/// <param name="unitName">The unit name</param>
		/// <param name="systemName">The name of the system the unit is in</param>
		/// <param name="power">The power of the dimension</param>
		/// <param name="prefixName">The optional name of the prefix</param>
		public Dimension(string unitName, string systemName, int power, string prefixName = null)
			: this(MeasurementFactory.FindUnit(unitName, systemName), power,
				MeasurementFactory.FindPrefix(prefixName)) {
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="dim">Existing dimension to copy</param>
		public Dimension(Dimension dim) {
			Unit = dim.Unit;
			Power = dim.Power;
			Prefix = dim.Prefix;
		}

		#endregion

		/// <summary>
		/// The power of the dimension.
		/// It has a protected setter as it should not be necessary to change the power outside of this class.
		/// </summary>
		public int Power { get; protected set; }

		/// <summary>
		/// The unit of the dimension.
		/// It has a protected setter as it should not be necessary to change the unit outside of this class.
		/// </summary>
		public Unit Unit { get; protected set; }

		/// <summary>
		/// The prefix of the dimension.
		/// It has a protected setter as it should not be necessary to change the prefix outside of this class.
		/// </summary>
		public Prefix Prefix { get; protected set; }

		#region Conversion

		/// <summary>
		/// A basic convert function for the dimension.
		/// Converts value into a base unit, then 
		/// </summary>
		/// <param name="value">The value to be converted</param>
		/// <param name="unit">The unit to be converted into</param>
		/// <param name="prefix">The prefix to be converted into</param>
		/// <returns>A converted copy of the dimension</returns>
		public Dimension Convert<TNumber>(ref TNumber value, Unit unit, Prefix prefix = null) where TNumber : INumber<TNumber> {
			Dimension baseDimension = ConvertToBase(ref value);
			return baseDimension.ConvertFromBase(ref value, unit, prefix);
		}

		/// <summary>
		/// Converts a value and the dimension to the base unit.
		/// </summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The dimension with the base unit and no prefix</returns>
		public Dimension ConvertToBase<TNumber>(ref TNumber value) where TNumber : INumber<TNumber> {
			if (Unit.IsBaseUnit() && Prefix == null) {
				return Copy();
			}

			Unit baseUnit = Unit.DimensionDefinition.BaseUnit;
			if (baseUnit == null) {
				throw new Exception("Base unit could not be found!");
			}
			value = DoConvert(value, Unit, Prefix, true);
			return new Dimension(baseUnit, Power);
		}

		/// <summary>
		/// Converts a dimension with a base unit into a specified unit and prefix.
		/// Existing dimension must have a base unit and no prefix.
		/// </summary>
		/// <param name="value">The value to be converted</param>
		/// <param name="unit">The unit to convert into</param>
		/// <param name="prefix">The optional prefix to convert into</param>
		/// <returns>The converted dimension with unit and prefix</returns>
		public Dimension ConvertFromBase<TNumber>(ref TNumber value, Unit unit, Prefix prefix = null) where TNumber : INumber<TNumber> {
			if (!Unit.IsBaseUnit()) {
				throw new Exception("Existing unit is not base unit");
			}
			if (Prefix != null) {
				throw new Exception("A dimension as a base may not have a prefix");
			}
			value = unit.IsBaseUnit() ? value : DoConvert(value, unit, prefix, false);
			return new Dimension(unit, Power, prefix);
		}

		/// <summary>
		/// Underlying convert method
		/// </summary>
		private TNumber DoConvert<TNumber>(TNumber value, Unit unit, Prefix prefix, bool toBase) where TNumber : INumber<TNumber> {
			// Ignore offset allows us to avoid adding/subtracting scalars where it is not reasonable to do so.
			// For example, conversion of vectors. If offset is required, this will still throw an error.
			bool ignoreOffset = (!value.OperationsAllowed(v => v.Add(1), v => v.Subtract(1)) && Math.Abs(unit.Offset) < EPSILON);
			TNumber calculatedValue = value;
			for (int pow = 0; pow < Math.Abs(Power); pow++) {
				if (prefix != null) {
					calculatedValue = toBase ? prefix.Remove(calculatedValue) : Prefix.Apply(calculatedValue);
				}
				if (toBase ? (Power > 0) : (Power < 0)) {
					calculatedValue = calculatedValue.Multiply(unit.Multiplier);
					if (!ignoreOffset) {
						calculatedValue = calculatedValue.Add(unit.Offset);
							// TODO dimensionality with offsets may not work with compound dimensions.
					}
				} else {
					if (!ignoreOffset) {
						calculatedValue = calculatedValue.Subtract(unit.Offset);
					}
					calculatedValue = calculatedValue.Divide(unit.Multiplier);
					// TODO dimensionality with offsets may not work with compound dimensions.
				}
			}
			return calculatedValue;
		}

		#endregion

		#region General Operations

		/// <summary>
		/// Checks if both dimensions are commensurable. Dimensions are commensurable
		/// if the system that their units belong to is equivalent and they have the same power.
		/// </summary>
		/// <param name="dimension">The dimension to check against</param>
		/// <returns>True if they are commensurable, else false</returns>
		public bool IsCommensurableMatch(Dimension dimension) {
			return Unit.DimensionDefinition.Name == dimension.Unit.DimensionDefinition.Name && Power == dimension.Power;
		}

		/// <summary>
		/// Combine two dimensions. These dimensions must have the same measurement system.
		/// </summary>
		/// <param name="value">The value to be converted while combining</param>
		/// <param name="dimension">The dimension to combine</param>
		/// <returns>The combined dimension</returns>
		public Dimension Combine<TNumber>(ref TNumber value, Dimension dimension)
				where TNumber : INumber<TNumber> {
			// Some validation
			if (Unit.DimensionDefinition.Name != dimension.Unit.DimensionDefinition.Name) {
				throw new Exception("Dimensions must have the same system to combine");
			}

			// Do conversion if necessary
			int aggregatePower;
			if (Unit.Name != dimension.Unit.Name) {
				Dimension dim = dimension.Convert(ref value, Unit, Prefix);
				aggregatePower = Power + dim.Power;
			} else {
				aggregatePower = Power + dimension.Power;
			}

			return new Dimension(Unit, aggregatePower);
		}

		public List<Dimension> ToBaseSystems<TNumber>(ref TNumber copy2)
				where TNumber : INumber<TNumber> {
			// TODO this currently assumes the derived dimension has base units...
			if (!Unit.DimensionDefinition.IsDerived()) {
				return new List<Dimension> { Copy() };
			}
			List<Dimension> baseSystems = Unit.DimensionDefinition.Derived.CopyList();
			baseSystems.ForEach(d => d.Power = d.Power * Power);
			return baseSystems;
		}

		public bool MatchDimensions(List<Dimension> neededSystems) {
			bool exactMatchFound = false;
			// find matching dimension and power in derived system
			var neededSystemsToRemove = new List<Dimension>();
			foreach (Dimension needed in neededSystems) {
				if (needed.Unit.DimensionDefinition.Name == Unit.DimensionDefinition.Name) {
					if (Math.Sign(needed.Power) == Math.Sign(Power) &&
						Math.Abs(needed.Power) <= Math.Abs(Power)) {
						neededSystemsToRemove.Add(needed);
						if (needed.Power == Power) {
							exactMatchFound = true;
						} else {
							Power = Math.Sign(needed.Power)*(Math.Abs(Power) - Math.Abs(needed.Power));
						}
					} else {
						break;
					}
				}
			}
			neededSystemsToRemove.ForEach(d => neededSystems.Remove(d));
			return exactMatchFound;
		}

		#endregion

		#region Prefixes

		/// <summary>
		/// Determines if this dimension has a unit that a prefix can be applied to.
		/// Only certain types of units can have prefixes applied - Si, Binary
		/// </summary>
		/// <returns>True if a prefix can be applied, else false</returns>
		internal bool CanApplyPrefix() {
			return Unit.Type == UnitType.Binary || Unit.Type == UnitType.Si;
		}

		/// <summary>
		/// Finds and applies a prefix to the dimension
		/// </summary>
		/// <param name="value">The value that the addition of the prefix should be applied to</param>
		/// <returns>A copy of the dimension with the found prefix applied, if a useful prefix could be found</returns>
		public Dimension ApplyPrefix<TNumber>(ref TNumber value) where TNumber : INumber<TNumber> {
			Dimension d = Copy();
			if (d.Prefix != null) {
				d = RemovePrefix(ref value);
			}
			Prefix p = d.FindPrefix(value);
			if (p != null) {
				d.Prefix = p;
				value = p.Apply(value);
			}
			return d;
		}

		/// <summary>
		/// Finds the best usable prefix 
		/// </summary>
		/// <param name="value">The value that the prefix will be applied to</param>
		/// <returns>If no usable prefix that is better than no prefix, returns null, else the prefix</returns>
		private Prefix FindPrefix<TNumber>(TNumber value) where TNumber : INumber<TNumber> {
			IEnumerable<Prefix> possiblePrefixes = MeasurementFactory.Prefixes.Where(p => Unit.IsCompatible(p));
			KeyValuePair<Prefix, double> bestPrefixKv = possiblePrefixes
				.Select(p => new KeyValuePair<Prefix, double>(p, GetPrefixRating(p.Apply(value), p)))
				.OrderBy(kv => kv.Value)
				.First();
			return (bestPrefixKv.Value < GetPrefixRating(value)) ? bestPrefixKv.Key : null;
		}

		/// <summary>
		/// This function produces a score for a given prefix or no prefix for a particular value.
		/// It aims to produce a sensible score for human consumption of a value and prefix combination.
		/// The score mainly trys to calculate a score between an upper and lower bound,
		/// as well as preferring no prefix and dealing with edge cases like the kilogramme.
		/// A lower rating is 'better'.
		/// TODO - This function may need tweaking...
		/// </summary>
		/// <example>
		/// 750, 'giga' => 1750; 0.750, 'tera' => 2000; therefore 'giga' should be preferred for this value.
		/// </example>
		/// <param name="value">The value after the prefix has been applied</param>
		/// <param name="prefix">The prefix that has been applied, or null if no prefix</param>
		/// <returns>A rating of the 'fitness' of the prefix for the value, lower is better</returns>
		private double GetPrefixRating<TNumber>(TNumber value, Prefix prefix = null) where TNumber : INumber<TNumber> {
			double upper = MeasurementFactory.Options.UpperPrefixValue;
			double lower = MeasurementFactory.Options.LowerPrefixValue;
			if (upper <= lower) {
				throw new Exception("The UpperPrefixValue must be greater than the LowerPrefixValue");
			}
			double score = (value.EquivalentValue > upper || value.EquivalentValue < lower) ? upper : value.EquivalentValue;
			// Prefer no prefix
			if (prefix != null) {
				score += MeasurementFactory.Options.HavingPrefixScoreOffset;
			}
			// If a unit has a prefix applied by default, we should apply that prefix if possible.
			// This deals with the edge case of preferring kilogramme over gramme.
			if (prefix != null && prefix.Name == Unit.PrefixName) {
				score = 0;
			}
			return score;
		}

		/// <summary>
		/// Removes an applied prefix to a dimension
		/// </summary>
		/// <param name="value">The value that the removal of the prefix should be applied to</param>
		/// <returns>A copy of the dimension with the prefix removed</returns>
		public Dimension RemovePrefix<TNumber>(ref TNumber value) where TNumber : INumber<TNumber> {
			Dimension d = Copy();
			if (d.Prefix != null) {
				value = d.Prefix.Remove(value);
				d.Prefix = null;
			}
			return d;
		}

		#endregion

		#region Copyable

		/// <summary>
		/// A helper method that provides a copy of the dimension
		/// </summary>
		/// <returns>The copy of the dimension</returns>
		public Dimension Copy() {
			return new Dimension(this);
		}

		/// <summary>
		/// Inverts the dimension's power
		/// </summary>
		/// <returns>An inverted copy of the dimension</returns>
		public Dimension Invert() {
			Dimension d = Copy();
			d.Power = -d.Power;
			return d;
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Provides a serialized version of the dimension
		/// </summary>
		/// <returns>A string of the serialized json</returns>
		string ISerializable.ToJson() {
			return this.ToJson();
		}

		/// <summary>
		/// Static method to deserialize a json string into a dimension.
		/// </summary>
		/// <param name="json">The json serialization of a dimension</param>
		/// <returns>The deserialized dimension</returns>
		public static Dimension FromJson(string json) {
			return json.FromJson<Dimension>();
		}

		/// <summary>
		/// Explicit implementation of IObjectReference to make sure dimension
		/// is instantiated properly during deserialization.
		/// </summary>
		object IObjectReference.GetRealObject(StreamingContext context) {
			return new Dimension {
				Unit = MeasurementFactory.FindUnit(_unitName, _systemName),
				Power = _power ?? DEFAULT_POWER,
				Prefix = MeasurementFactory.FindPrefix(_prefixName)
			};
		}

		#region Readonly Properties for Serializing

		private string _unitName;
		private string _systemName;
		private int? _power;
		private string _prefixName;

		[DataMember(Name = "unitName")]
		public string UnitName {
			get { return Unit.Name; }
			private set { _unitName = value; }
		}

		[DataMember(Name = "systemName")]
		public string SystemName {
			get { return Unit.DimensionDefinition.Name; }
			private set { _systemName = value; }
		}

		[DataMember(Name = "power", IsRequired = false, EmitDefaultValue = false)]
		public int? PowerValue {
			get { return (Power != DEFAULT_POWER) ? (int?)Power : null; }
			private set { _power = value; }
		}

		[DataMember(Name = "prefix", IsRequired = false, EmitDefaultValue = false)]
		public string PrefixName {
			get { return (Prefix != null) ? Prefix.Name : null; }
			private set { _prefixName = value; }
		}

		#endregion

		#endregion

		#region Formatting

		/// <summary>
		/// A simple format method that formats the dimension with the
		/// default format options and the current culture.
		/// </summary>
		/// <returns>The formatted string</returns>
		public string Format() {
			return Format(new FormatOptions(CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Provides a formatted version of the dimension given a provided
		/// set of format options.
		/// </summary>
		/// <seealso cref="ForgedSoftware.Measurement.FormatOptions"/>
		/// <param name="options">The format options</param>
		/// <returns>The formatted string</returns>
		public string Format(FormatOptions options) {
			string dimensionString = "";

			if (options.FullName) {
				var dimParts = new List<string>();
				if (Power < 0) {
					dimParts.Add("per");
				}
				string name = Unit.Name;
				if (Prefix != null) {
					name = Prefix.Name + name;
				}
				dimParts.Add(name); // TODO - plurals??
				int absPower = Math.Abs(Power);
				if (absPower == 2) {
					dimParts.Add("squared");
				} else if (absPower == 3) {
					dimParts.Add("cubed");
				} else if (absPower > 3 || absPower == 0) {
					dimParts.Add("to the power of " + absPower);
				}
				dimensionString = dimParts.Aggregate((current, next) => current + " " + next);
			} else {
				if (Prefix != null) {
					dimensionString += Prefix.Symbol;
				}
				dimensionString += Unit.Symbol;
				if (options.ShowAllPowers || Power != DEFAULT_POWER) {
					string powerStr = (options.Ascii) ? "^" + Power : Power.ToSuperScript();
					dimensionString += powerStr;
				}
			}
			return dimensionString;
		}

		#endregion

		#region ToString

		/// <summary>
		/// Overrides ToString to provide a formatted representation
		/// of the dimension with the default options and the current culture.
		/// </summary>
		/// <returns>The formatted string</returns>
		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Provides a ToString method with a couple of format options.
		/// A specific IFormatProvider can be specified, else the current culture is used.
		/// 
		/// The avaliable format strings are:
		/// - "G": General format, uses the default format options
		/// - "S": Scientific format is equivalent to the General format
		/// - "R": Raw format is an rough, ascii only format
		/// </summary>
		/// <param name="format">The format letter</param>
		/// <param name="provider">An optional format provider</param>
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
				default:
					throw new ArgumentException("Provided format string is not recognized");
			}

			return Format(options);
		}

		#endregion
	}
}
