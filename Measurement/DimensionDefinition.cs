using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ForgedSoftware.Measurement.Number;

namespace ForgedSoftware.Measurement {

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
	public class DimensionDefinition {

		public DimensionDefinition() {
			Units = new List<Unit>();
			OtherNames = new List<string>();
			OtherSymbols = new List<string>();
			Derived = new List<Dimension>();
		}

		// Basic Properties
		public string Key { get; set; }
		public string Name { get; set; }
		public List<string> OtherNames { get; private set; }
		public string Symbol { get; set; }
		public List<string> OtherSymbols { get; private set; }
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

		public void UpdateDerived() {
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
					Unit baseUnit = MeasurementFactory.FindBaseUnit(systemName);
					if (baseUnit != null) {
						switch (type) {
							case "*": {
								Derived.Add(new Dimension(baseUnit, 1));
								break;
							}
							case "/": {
								Derived.Add(new Dimension(baseUnit, -1));
								break;
							}
							default: {
								throw new Exception("Derived divider is not valid - must be either '*' or '/'");
							}
						}
					} else if (systemName != "1") {
						throw new Exception("All derived entries must be the name of a system or the '1' placeholder");
					}
				}
			}
			var computedValue = new DoubleWrapper(1);
			Derived = Derived.SimpleSimplify(ref computedValue);
		}

		#endregion

	}
}
