

namespace ForgedSoftware.Measurement {

	public class Unit {

		public Unit() {
			Multiplier = 1;
		}

		public string Name { get; set; }
		public MeasurementSystem System { get; set; }
		public string Symbol { get; set; }
		public double Multiplier { get; set; }
		public double Offset { get; set; }

		public bool IsBaseUnit() {
			return System.BaseUnit == this;
		}
	}
}
