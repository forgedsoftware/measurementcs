using System.Collections.Generic;

namespace ForgedSoftware.Measurement {

	public class Unit {

		public Unit() {
			Multiplier = 1;
			UnitSystems = new List<string>();
			OtherNames = new List<string>();
			OtherSymbols = new List<string>();
		}

		public string Name { get; set; }
		public UnitType Type { get; set; }
		public List<string> OtherNames { get; private set; } 
		public MeasurementSystem System { get; set; }
		public string Symbol { get; set; }
		public List<string> OtherSymbols { get; private set; } 
		public bool IsRare { get; set; }
		public bool IsEstimation { get; set; }
		public double Multiplier { get; set; }
		public double Offset { get; set; }
		public List<string> UnitSystems { get; private set; }

		public string PrefixName { get; set; }
		public string PrefixFreeName { get; set; }

		public bool IsBaseUnit() {
			return System.BaseUnit.Equals(this);
		}
	}

	public enum UnitType {
		Si, Customary, Range, Binary, Fractional, Whole
	}
}
