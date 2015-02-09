using System.Collections.Generic;
using System.Linq;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A corpus of data that provides accessors to the measurement systems, dimension definitions, and prefixes.
	/// It provides accessor functions to simply usage as well as collections of data.
	/// It is loaded using the CorpusBuilder.
	/// </summary>
	public static class MeasurementCorpus {

		public static List<MeasurementSystem> AllSystems { get; private set; }
		public static List<MeasurementSystem> RootSystems { get; private set; }
		public static List<DimensionDefinition> Dimensions { get; private set; }
		public static List<Prefix> Prefixes { get; private set; }
		public static MeasurementOptions Options { get; private set; }

		#region Setup

		static MeasurementCorpus() {
			AllSystems = new List<MeasurementSystem>();
			RootSystems = new List<MeasurementSystem>();
			Dimensions = new List<DimensionDefinition>();
			Prefixes = new List<Prefix>();

			// Load Data
			new CorpusBuilder().PrepareMeasurement();

			// Create Default Options
			Options = new MeasurementOptions();
		}

		#endregion

		#region Options

		public static void ResetToDefaultOptions() {
			Options = new MeasurementOptions();
		}

		#endregion

		#region Quantity Factory Methods

		public static Quantity CreateQuantity(double val) {
			return new Quantity(val);
		}

		public static Quantity CreateQuantity(double val, string unitName) {
			return new Quantity(val, new List<string> {unitName });
		}

		public static Quantity CreateQuantity(double val, IEnumerable<string> unitNames) {
			return new Quantity(val, unitNames);
		}

		public static Quantity CreateQuantity(double val, IEnumerable<Dimension> dimensions) {
			return new Quantity(val, dimensions);
		}

		#endregion

		#region Find

		public static Unit FindBaseUnit(string systemName) {
			DimensionDefinition system = Dimensions.FirstOrDefault(s => s.Key == systemName);
			return (system != null) ? system.BaseUnit : null;
		}

		public static Unit FindUnit(string unitName) {
			return Dimensions.SelectMany(s => s.Units).FirstOrDefault(u => u.Name == unitName);
		}

		public static Unit FindUnit(string unitName, string systemName) {
			return Dimensions.First(s => s.Key == systemName).Units.FirstOrDefault(u => u.Name == unitName);
		}

		public static Prefix FindPrefix(string prefixName) {
			return Prefixes.FirstOrDefault(p => p.Name == prefixName);
		}

		#endregion

	}
}
