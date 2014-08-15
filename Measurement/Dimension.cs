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
	public class Dimension : ISerializable, IFormatter, IFormattable, ICopyable<Dimension> {

		#region Constructors

		private Dimension() {
			Power = 1;
		}

		public Dimension(Unit unit, int power, Prefix prefix = null)
			: this() {
			Unit = unit;
			Power = power;
			Prefix = prefix;
		}

		public Dimension(string unitName, string prefix = null)
			: this() {
			Unit = MeasurementFactory.FindUnit(unitName);
			Prefix = MeasurementFactory.FindPrefix(prefix);
		}

		public Dimension(string unitName, string systemName, string prefix = null)
			: this() {
			Unit = MeasurementFactory.FindUnit(unitName, systemName);
			Prefix = MeasurementFactory.FindPrefix(prefix);
		}

		public Dimension(string unitName, int power, string prefix = null)
			: this(unitName, prefix) {
			Power = power;
		}

		public Dimension(string unitName, string systemName, int power, string prefix = null)
			: this(unitName, systemName, prefix) {
			Power = power;
		}

		// Copy Constructor
		protected Dimension(Dimension dim) {
			Unit = dim.Unit;
			Power = dim.Power;
			Prefix = dim.Prefix;
		}

		#endregion

		[DataMember(Name = "power")] // TODO - Only emit if != 1
		public int Power { get; set; }
		
		public Unit Unit { get; set; }

		public Prefix Prefix { get; private set; }

		[DataMember(Name = "unitName")]
		public string UnitName {
			get { return Unit.Name; }
			private set { }
		}

		[DataMember(Name = "systemName")]
		public string SystemName {
			get { return Unit.System.Name; }
			private set { }
		}

		[DataMember(Name = "prefix", IsRequired = false, EmitDefaultValue = false)]
		public string PrefixName {
			get { return (Prefix != null) ? Prefix.Name : null; }
			private set { }
		}

		#region Convert

		public KeyValuePair<Dimension, double> Convert(double value, Unit unit, Prefix prefix) {
			KeyValuePair<Dimension, double> baseDimension = ConvertToBase(value);
			return baseDimension.Key.ConvertFromBase(baseDimension.Value, unit, prefix);
		}

		public KeyValuePair<Dimension, double> ConvertToBase(double value) {
			if (Unit.IsBaseUnit() && Prefix == null) {
				return new KeyValuePair<Dimension, double>(Copy(), value);
			}

			Unit baseUnit = Unit.System.BaseUnit;
			if (baseUnit == null) {
				throw new Exception("Base unit could not be found!");
			}
			return new KeyValuePair<Dimension, double>(new Dimension(baseUnit, Power), DoConvert(value, Unit, Prefix, true));
		}

		public KeyValuePair<Dimension, double> ConvertFromBase(double value, Unit unit, Prefix prefix = null) {
			if (!Unit.IsBaseUnit()) {
				throw new Exception("Existing unit is not base unit");
			}
			if (Prefix != null) {
				throw new Exception("A dimension as a base may not have a prefix");
			}
			double convertedValue = unit.IsBaseUnit() ? value : DoConvert(value, unit, prefix, false);
			return new KeyValuePair<Dimension, double>(new Dimension(unit, Power, prefix), convertedValue);
		}

		private double DoConvert(double value, Unit unit, Prefix prefix, bool toBase) {
			double calculatedValue = value;
			for (int pow = 0; pow < Math.Abs(Power); pow++) {
				if (prefix != null) {
					calculatedValue = toBase ? prefix.Remove(calculatedValue) : Prefix.Apply(calculatedValue);
				}
				if (toBase ? (Power > 0) : (Power < 0)) {
					calculatedValue = (calculatedValue * unit.Multiplier) + unit.Offset; // TODO dimensionality with offsets may not work with compound dimensions.
				} else {
					calculatedValue = (calculatedValue - unit.Offset) / unit.Multiplier; // TODO dimensionality with offsets may not work with compound dimensions.
				}
			}
			return calculatedValue;
		}

		#endregion

		public bool IsCommensurableMatch(Dimension dimension) {
			return Unit.System.Name == dimension.Unit.System.Name && Power == dimension.Power;
		}

		public KeyValuePair<Dimension, double> Combine(double computedValue, Dimension dimension) {
			// Some validation
			if (Unit.System.Name != dimension.Unit.System.Name) {
				throw new Exception("Dimensions must have the same system to combine");
			}

			// Do conversion if necessary
			int aggregatePower;
			if (Unit.Name != dimension.Unit.Name) {
				KeyValuePair<Dimension, double> dimValuePair = dimension.Convert(computedValue, Unit, Prefix);
				computedValue = dimValuePair.Value;
				aggregatePower = Power + dimValuePair.Key.Power;
			} else {
				aggregatePower = Power + dimension.Power;
			}

			return new KeyValuePair<Dimension, double>(new Dimension(Unit, aggregatePower), computedValue);
		}

		#region Prefixes

		internal bool CanApplyPrefix() {
			return Unit.Type == UnitType.Binary || Unit.Type == UnitType.Si;
		}

		public KeyValuePair<Dimension, double> ApplyPrefix(double value) {
			Dimension d = Copy();
			if (d.Prefix != null) {
				KeyValuePair<Dimension, double> removedPrefixKeyValue = RemovePrefix(value);
				d = removedPrefixKeyValue.Key;
				value = removedPrefixKeyValue.Value;
			}
			Prefix p = d.FindPrefix(value);
			if (p != null) {
				d.Prefix = p;
				value = p.Apply(value);
			}
			return new KeyValuePair<Dimension, double>(d, value);
		}

		private Prefix FindPrefix(double value) {
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
		private double GetPrefixRating(double value, Prefix prefix = null) {
			double upper = MeasurementFactory.Options.UpperPrefixValue;
			double lower = MeasurementFactory.Options.LowerPrefixValue;
			if (upper <= lower) {
				throw new Exception("The UpperPrefixValue must be greater than the LowerPrefixValue");
			}
			double score = (value > upper || value < lower) ? upper : value;
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

		public KeyValuePair<Dimension, double> RemovePrefix(double value) {
			Dimension d = Copy();
			if (d.Prefix != null) {
				value = d.Prefix.Remove(value);
				d.Prefix = null;
			}
			return new KeyValuePair<Dimension, double>(d, value);
		}

		#endregion

		public Dimension Copy() {
			return new Dimension(this);
		}

		public Dimension Invert() {
			Dimension d = Copy();
			d.Power = -d.Power;
			return d;
		}

		string ISerializable.ToJson() {
			return this.ToJson();
		}

		#region Formatting

		public string Format() {
			return Format(new FormatOptions(CultureInfo.CurrentCulture));
		}

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
				if (options.ShowAllPowers || Power != 1) {
					string powerStr = (options.Ascii) ? "^" + Power : Power.ToSuperScript();
					dimensionString += powerStr;
				}
			}
			return dimensionString;
		}

		#endregion

		#region ToString

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
				default:
					throw new ArgumentException("Provided format string is not recognized");
			}

			return Format(options);
		}

		#endregion

	}
}
