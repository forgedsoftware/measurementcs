using System.Collections.Generic;

namespace ForgedSoftware.Measurement {

	public class MeasurementSystem {
		public List<Unit> Units { get; private set; }

		public MeasurementSystem() {
			Units = new List<Unit>();
		}

		public string Name { get; set; }
		public string Symbol { get; set; }
		public Unit BaseUnit { get; set; }

	}
}
