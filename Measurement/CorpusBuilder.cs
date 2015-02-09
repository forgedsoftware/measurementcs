using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Builds MeasurementCorpus object and all data objects using data
	/// in the measurementcommon sub repo.
	/// </summary>
	public class CorpusBuilder {

		/// <summary>
		/// Main builder function
		/// </summary>
		public void PrepareMeasurement() {
			using (var r = new StreamReader(GetPath())) {
				var serializer = new JavaScriptSerializer();
				string json = r.ReadToEnd();
				var items = (Dictionary<string, object>)serializer.DeserializeObject(json);

				var systemsJson = (Dictionary<string, object>)items["systems"];
				ParseSystems(systemsJson);
				PrepareTree();

				var dimensionsJson = (Dictionary<string, object>)items["dimensions"];
				ParseDimensions(dimensionsJson);

				var prefixesJson = (Dictionary<string, object>)items["prefixes"];
				ParsePrefixes(prefixesJson);
			}
		}

		private string GetPath() {
			var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
			if (directoryInfo == null || directoryInfo.Parent == null) {
				throw new Exception("Directory containing json does not exist");
			}
			return Path.Combine(directoryInfo.Parent.FullName, "./common/systems.json");
		}

		private void ParseSystems(Dictionary<string, object> systemsJson) {
			foreach (KeyValuePair<string, object> systemKeyValuePair in systemsJson) {
				var systemJson = (Dictionary<string, object>) systemKeyValuePair.Value;
				var system = new MeasurementSystem {
					Key = systemKeyValuePair.Key,
					Name = Parse<string>(systemJson, "name"),
					IsHistorical = Parse<bool>(systemJson, "historical"),
					Inherits = Parse<string>(systemJson, "inherits")
				};
				MeasurementCorpus.AllSystems.Add(system);
			}
		}

		private void PrepareTree() {
			foreach (MeasurementSystem system in MeasurementCorpus.AllSystems) {
				if (string.IsNullOrWhiteSpace(system.Inherits)) {
					MeasurementCorpus.RootSystems.Add(system);
				} else {
					foreach (MeasurementSystem parentSystem in MeasurementCorpus.AllSystems) {
						if (parentSystem.Key == system.Inherits) {
							parentSystem.Children.Add(system);
							system.Parent = parentSystem;
						}
					}
				}
			}
		}

		private void ParseDimensions(Dictionary<string, object> dimensionsJson) {
			foreach (KeyValuePair<string, object> systemKeyValuePair in dimensionsJson) {
				var dimensionJson = (Dictionary<string, object>) systemKeyValuePair.Value;
				var dimension = new DimensionDefinition {
					Key = systemKeyValuePair.Key,
					Name = Parse<string>(dimensionJson, "name"),
					Symbol = Parse<string>(dimensionJson, "symbol"),
					DerivedString = Parse<string>(dimensionJson, "derived"),
					BaseUnitName = Parse<string>(dimensionJson, "baseUnit"),
					Vector = Parse<bool>(dimensionJson, "vector"),
					IsDimensionless = Parse<bool>(dimensionJson, "dimensionless"),
					InheritedUnits = Parse<string>(dimensionJson, "inheritedUnits")
				};
				dimension.OtherNames.AddRange(ParseArray<string>(dimensionJson, "otherNames"));
				dimension.OtherSymbols.AddRange(ParseArray<string>(dimensionJson, "otherSymbols"));

				ParseUnits(dimensionJson, dimension);
				MeasurementCorpus.Dimensions.Add(dimension);
			}
			MeasurementCorpus.Dimensions.ForEach(s => s.UpdateDerived());
			List<DimensionDefinition> inheritingDims = MeasurementCorpus.Dimensions.Where(d => !string.IsNullOrWhiteSpace(d.InheritedUnits)).ToList();
			inheritingDims.ForEach(a => a.Units.AddRange(MeasurementCorpus.Dimensions.First(d => d.Key == a.InheritedUnits).Units));
			inheritingDims.ForEach(a => a.Units.ForEach(u => u.InheritedDimensionDefinitions.Add(a)));
			inheritingDims.ForEach(a => a.BaseUnit = a.Units.First(u => u.Key == a.BaseUnitName));
		}

		private void ParseUnits(Dictionary<string, object> dimensionJson, DimensionDefinition dimension) {
			foreach (KeyValuePair<string, object> unitKeyValuePair in (Dictionary<string, object>)dimensionJson["units"]) {
				var unitJson = (Dictionary<string, object>) unitKeyValuePair.Value;
				var unit = new Unit {
					Key = unitKeyValuePair.Key,
					Name = Parse<string>(unitJson, "name"),
					Plural = Parse<string>(unitJson, "plural"),
					DimensionDefinition = dimension,
					Type = Parse<UnitType>(unitJson, "type"),
					Symbol = Parse<string>(unitJson, "symbol"),
					Multiplier = Parse<double>(unitJson, "multiplier"),
					Offset = Parse<double>(unitJson, "offset"),
					IsRare = Parse<bool>(unitJson, "rare"),
					IsEstimation = Parse<bool>(unitJson, "estimation"),
					PrefixName = Parse<string>(unitJson, "prefixName"),
					PrefixFreeName = Parse<string>(unitJson, "prefixFreeName")
				};
				unit.MeasurementSystemNames.AddRange(ParseArray<string>(unitJson, "systems"));
				unit.OtherNames.AddRange(ParseArray<string>(unitJson, "otherNames"));
				unit.OtherSymbols.AddRange(ParseArray<string>(unitJson, "otherSymbols"));
				unit.UpdateMeasurementSystems();
				dimension.Units.Add(unit);
				if (dimension.BaseUnitName == unit.Key) {
					dimension.BaseUnit = unit;
				}
			}
		}

		private void ParsePrefixes(Dictionary<string, object> prefixesJson) {
			foreach (KeyValuePair<string, object> prefixKeyValuePair in prefixesJson) {
				var prefixJson = (Dictionary<string, object>)prefixKeyValuePair.Value;
				var prefix = new Prefix {
					Name = prefixKeyValuePair.Key,
					Symbol = Parse<string>(prefixJson, "symbol"),
					Type = Parse<PrefixType>(prefixJson, "type"),
					IsRare = Parse<bool>(prefixJson, "rare"),
					Multiplier = Parse<double>(prefixJson, "multiplier"),
					Power = Parse<double>(prefixJson, "power"),
					Base = Parse<double>(prefixJson, "base")
				};
				MeasurementCorpus.Prefixes.Add(prefix);
			}
		}

		#region Helper Functions

		private T Parse<T>(Dictionary<string, object> values, string name) {
			object val;
			if (values.TryGetValue(name, out val)) {
				if (typeof(T) == typeof(double)) {
					return (T)(object) Convert.ToDouble(val);
				}
				if (typeof (T).IsEnum) {
					return (T) Enum.Parse(typeof(T), (string) val, true);
				}
				return (T) val;
			}
			return default(T);
		}

		private IEnumerable<T> ParseArray<T>(Dictionary<string, object> values, string name) {
			var result = new List<T>();
			object val;
			if (values.TryGetValue(name, out val)) {
				var arrayValues = (object[]) val;
				if (arrayValues != null) {
					result.AddRange(arrayValues.Select(value => (T) value));
				}
			}
			return result;
		}

		#endregion

	}
}
