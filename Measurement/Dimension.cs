using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	[DataContract]
	public class Dimension : ISerializable, IFormatter, IFormattable, ICopyable<Dimension> {

		#region Constructors

		private Dimension() {
			Power = 1;
		}

		public Dimension(Unit unit, int power): this() {
			Power = power;
			Unit = unit;
		}

		public Dimension(string unitName) : this() {
			Unit = MeasurementFactory.FindUnit(unitName);
		}

		public Dimension(string unitName, string systemName): this() {
			Unit = MeasurementFactory.FindUnit(unitName, systemName);
		}

		public Dimension(string unitName, int power): this(unitName) {
			Power = power;
		}

		public Dimension(string unitName, string systemName, int power): this(unitName, systemName) {
			Power = power;
		}

		// Copy Constructor
		protected Dimension(Dimension dim) {
			Unit = dim.Unit;
			Power = dim.Power;
		}

		#endregion

		[DataMember(Name = "power")]
		public int Power { get; set; }
		
		public Unit Unit { get; set; }

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

		public KeyValuePair<Dimension, double> ConvertToBase(double value) {
			if (Unit.IsBaseUnit()) {
				return new KeyValuePair<Dimension, double>(Copy(), value);
			}

			Unit baseUnit = Unit.System.BaseUnit;
			if (baseUnit == null) {
				throw new Exception("Base unit could not be found!");
			}
			return new KeyValuePair<Dimension, double>(new Dimension(baseUnit, Power), DoConvert(value, Unit, true));
		}

		public KeyValuePair<Dimension, double> ConvertFromBase(double value, Unit unit) {
			if (!Unit.IsBaseUnit()) {
				throw new Exception("Existing unit is not base unit");
			}
			return new KeyValuePair<Dimension, double>(new Dimension(unit, Power), DoConvert(value, unit, false));
		}

		private double DoConvert(double value, Unit unit, bool toBase) {
			double calculatedValue = value;
			for (int pow = 0; pow < Math.Abs(Power); pow++) {
				if (toBase ? (Power > 0) : (Power < 0)) {
					calculatedValue = (calculatedValue * unit.Multiplier) + unit.Offset; // TODO dimensionality with offsets may not work with compound dimensions.
				} else {
					calculatedValue = (calculatedValue - unit.Offset) / unit.Multiplier; // TODO dimensionality with offsets may not work with compound dimensions.
				}
			}
			return calculatedValue;
		}

		public Dimension Copy() {
			return new Dimension(this);
		}

		string ISerializable.ToJson() {
			return this.ToJson();
		}

		#region Formatting

		public string Format() {
			return Format(new FormatOptions(CultureInfo.CurrentCulture));
		}

		public string Format(FormatOptions options) {
			string dimensionString;

			if (options.FullName) {
				var dimParts = new List<string>();
				if (Power < 0) {
					dimParts.Add("per");
				}
				dimParts.Add(Unit.Name); // TODO - plurals??
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
				dimensionString = Unit.Symbol;
				if (options.ShowAllPowers || Power != 1) {
					string powerStr = (options.Ascii) ? "^" + Power : Power.ToSuperScript();
					dimensionString += powerStr;
				}
			}
			return dimensionString;
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
				default:
					throw new ArgumentException("Provided format string is not recognized");
			}

			return Format(options);
		}

	}
}
