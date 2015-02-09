using System.Collections.Generic;

namespace ForgedSoftware.Measurement {
	/// <summary>
	/// A measurement system is a man-made construction that provides a series
	/// of units for providing a quantified view of the world.
	/// </summary>
	/// <example>
	/// Examples of measurement systems are: metric system, imperial system
	/// </example>
	public class MeasurementSystem {

		public MeasurementSystem() {
			Children = new List<MeasurementSystem>();
			Units = new List<Unit>();
		}

		// Basic Properties
		public string Key { get; set; }
		public string Name { get; set; }
		public bool IsHistorical { get; set; }
		public string Inherits { get; set; }

		// Calculated Properties
		public MeasurementSystem Parent { get; set; }
		public List<MeasurementSystem> Children { get; private set; }
		public List<Unit> Units { get; private set; }

		public bool IsRootSystem() {
			return (Parent == null);
		}
	}
}
