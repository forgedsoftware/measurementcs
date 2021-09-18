using System.Collections.Generic;
using System.Linq;

namespace ForgedSoftware.Measurement.Entities {

	/// <summary>
	/// A unit represents a unit of measurement within a specific
	/// measurement system.
	/// </summary>
	/// <example>
	/// Examples of a unit are: metre, second, Newton, inch, foot
	/// </example>
	public class Unit : Entity {

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

		public string Name { get; init; }
		public string Plural { get; init; }
		public UnitType Type { get; init; }
		public List<string> OtherNames { get; init; } 
		public DimensionDefinition DimensionDefinition { get; set; } // TODO - fix tests so can use init
		public List<DimensionDefinition> InheritedDimensionDefinitions { get; private set; }
		public string Symbol { get; init; }
		public List<string> OtherSymbols { get; init; } 
		public bool IsRare { get; set; } // TODO - fix tests so can use init;
		public bool IsEstimation { get; set; } // TODO - fix tests so can use init;
		public double Multiplier { get; init; }
		public double Offset { get; init; }

		public List<string> MeasurementSystemNames { get; init; }
		public List<MeasurementSystem> MeasurementSystems { get; private set; }

		public string PrefixName { get; init; }
		public string PrefixFreeName { get; init; }

		public bool IsBaseUnit() {
			return DimensionDefinition.BaseUnit.Equals(this);
		}

		public void UpdateMeasurementSystems(IList<MeasurementSystem> systems) {
			MeasurementSystems.AddRange(systems.Where(s => MeasurementSystemNames.Contains(s.Key)));
			MeasurementSystems.ForEach(s => s.Units.Add(this));
		}

		public bool IsCompatible(Prefix prefix) {
			if (MeasurementCorpus.Corpus.Options.AllowedRarePrefixCombinations.Contains(new KeyValuePair<Unit, Prefix>(this, prefix))) {
				return true;
			}
			if (prefix.IsRare && !MeasurementCorpus.Corpus.Options.UseRarePrefixes) {
				return false;
			}
			switch (prefix.Type) {
				case PrefixType.Si: {
					return Type == UnitType.Si || (Type == UnitType.Binary && !MeasurementCorpus.Corpus.Options.PreferBinaryPrefixes);
				}
				case PrefixType.SiBinary: {
					return Type == UnitType.Binary && MeasurementCorpus.Corpus.Options.PreferBinaryPrefixes;
				}
				case PrefixType.SiUnofficial: {
					return Type == UnitType.Si && MeasurementCorpus.Corpus.Options.UseUnofficalPrefixes;
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
