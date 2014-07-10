using System;
using System.Collections.Generic;

namespace ForgedSoftware.Measurement {

	public class Dimension : ICopyable<Dimension> {

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

		public int Power { get; set; }
		public Unit Unit { get; set; }

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
	}
}
