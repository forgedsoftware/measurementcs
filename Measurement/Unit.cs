using System.Collections.Generic;
using System.Linq;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A unit represents a unit of measurement within a specific
	/// measurement system.
	/// </summary>
	/// <example>
	/// Examples of a unit are: metre, second, Newton, inch, foot
	/// </example>
	public class Unit {

		/// <summary>
		/// Main constructor for a unit
		/// </summary>
		public Unit() {
			Multiplier = 1;
			OtherNames = new List<string>();
			OtherSymbols = new List<string>();
			InheritedDimensionDefinitions = new List<DimensionDefinition>();
			MeasurementSystemNames = new List<string>();
			MeasurementSystems = new List<MeasurementSystem>();
		}

		public string Key { get; set; }
		public string Name { get; set; }
		public string Plural { get; set; }
		public UnitType Type { get; set; }
		public List<string> OtherNames { get; private set; } 
		public DimensionDefinition DimensionDefinition { get; set; }
		public List<DimensionDefinition> InheritedDimensionDefinitions { get; private set; }
		public string Symbol { get; set; }
		public List<string> OtherSymbols { get; private set; } 
		public bool IsRare { get; set; }
		public bool IsEstimation { get; set; }
		public double Multiplier { get; set; }
		public double Offset { get; set; }

		public List<string> MeasurementSystemNames { get; private set; }
		public List<MeasurementSystem> MeasurementSystems { get; private set; }

		public string PrefixName { get; set; }
		public string PrefixFreeName { get; set; }

		public bool IsBaseUnit() {
			return DimensionDefinition.BaseUnit.Equals(this);
		}

		public void UpdateMeasurementSystems() {
			MeasurementSystems.AddRange(MeasurementFactory.AllSystems.Where(s => MeasurementSystemNames.Contains(s.Key)));
			MeasurementSystems.ForEach(s => s.Units.Add(this));
		}

		public bool IsCompatible(Prefix prefix) {
			if (MeasurementFactory.Options.AllowedRarePrefixCombinations.Contains(new KeyValuePair<Unit, Prefix>(this, prefix))) {
				return true;
			}
			if (prefix.IsRare && !MeasurementFactory.Options.UseRarePrefixes) {
				return false;
			}
			switch (prefix.Type) {
				case PrefixType.Si: {
					return Type == UnitType.Si || (Type == UnitType.Binary && !MeasurementFactory.Options.PreferBinaryPrefixes);
				}
				case PrefixType.SiBinary: {
					return Type == UnitType.Binary && MeasurementFactory.Options.PreferBinaryPrefixes;
				}
				case PrefixType.SiUnofficial: {
					return Type == UnitType.Si && MeasurementFactory.Options.UseUnofficalPrefixes;
				}
				default: {
					return false;
				}
			}
		}
	}

	public enum UnitType {
		Si, Customary, Range, Binary, Fractional, Whole
	}
}
