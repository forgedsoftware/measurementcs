using System.Collections.Generic;

namespace ForgedSoftware.Measurement {

	public class MeasurementSystem {
		public List<Unit> Units { get; private set; }

		public MeasurementSystem() {
			Units = new List<Unit>();
			OtherNames = new List<string>();
		}

		public string Name { get; set; }
		public List<string> OtherNames { get; private set; }
		public string Symbol { get; set; }
		public string Derived { get; set; }
		public Unit BaseUnit { get; set; }

	}
}
