using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ForgedSoftware.Measurement {

	public class MeasurementSystem {

		public MeasurementSystem() {
			Units = new List<Unit>();
			OtherNames = new List<string>();
			Derived = new List<Dimension>();
		}

		public List<Unit> Units { get; private set; }
		public string Name { get; set; }
		public List<string> OtherNames { get; private set; }
		public string Symbol { get; set; }
		public Unit BaseUnit { get; set; }

		#region Derived

		public string DerivedString { get; set; }
		public List<Dimension> Derived { get; private set; }

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
			double computedValue = 1;
			Derived = Derived.Simplify(ref computedValue);
		}

		#endregion

	}
}
