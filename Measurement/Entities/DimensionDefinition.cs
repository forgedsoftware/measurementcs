using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ForgedSoftware.Measurement.Number;

namespace ForgedSoftware.Measurement.Entities {

	/// <summary>
	/// A dimension definition is the definition of a dimensioned,
	/// measurable value, that can be obtained via direct observation of the world.
	/// 
	/// A DimensionDefinition should not be confused with a Dimension. The former
	/// is the definition of something that can be measured in the world. The latter
	/// is a specific quanitifier for part of an actual observation or quantity.
	/// </summary>
	/// <example>
	/// Examples of dimensions are: length, mass, acceleration, time
	/// </example>
	public class DimensionDefinition : Entity {

		public DimensionDefinition() {
			Units = new List<Unit>();
			OtherNames = new List<string>();
			OtherSymbols = new List<string>();
			Derived = new List<Dimension>();
		}

		// Basic Properties
		public string Name { get; set; }
		public List<string> OtherNames { get; set; }
		public string Symbol { get; set; }
		public List<string> OtherSymbols { get; set; }
		public string BaseUnitName { get; set; }
		public bool Vector { get; set; }
		public bool IsDimensionless { get; set; }
		public string InheritedUnits { get; set; }

		// Calculated Properties
		public List<Unit> Units { get; private set; }
		public Unit BaseUnit { get; set; }

		#region Derived

		public string DerivedString { get; set; }
		public List<Dimension> Derived { get; private set; }

		public bool IsDerived() {
			return Derived.Count > 0;
		}

		public void UpdateDerived(MeasurementCorpus corpus) {
			Derived.Clear();
			if (string.IsNullOrEmpty(DerivedString)) {
				return;
			}
			foreach (Match match in Regex.Matches(DerivedString, @"(^\w+|([\*|/])(\w+))")) {
				if (match.Success) {
					string type;
					string systemName;
					if (!string.IsNullOrEmpty(match.Groups[3].Value)) {
						type = match.Groups[2].Value;
						systemName = match.Groups[3].Value;
					} else {
						type = "*";
						systemName = match.Groups[1].Value;
					}
					if (systemName != "1") {
						Unit baseUnit = corpus.FindBaseUnit(systemName);
						if (baseUnit == null) {
							throw new Exception("All derived entries must be the name of a dimension or the '1' placeholder");
						}
						if (type != "*" && type != "/") {
							throw new Exception("Derived divider is not valid - must be either '*' or '/'");
						}
						int power = (type == "*") ? 1 : -1;
						Derived.Add(new Dimension(baseUnit, power));
					}
				}
			}
			var computedValue = new DoubleWrapper(1);
			Derived = Derived.SimpleSimplify(ref computedValue);
		}

		#endregion

	}
}
