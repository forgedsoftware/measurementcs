﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Linq;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A factory that provides accessors to the measurement systems and prefixes.
	/// Additionally it also preloads those systems and prefixes from a json file.
	/// </summary>
	public static class MeasurementFactory {

		public static List<MeasurementSystem> Systems { get; private set; }
		public static List<Prefix> Prefixes { get; private set; }
		public static MeasurementOptions Options { get; private set; }

		#region Setup

		static MeasurementFactory() {
			Systems = new List<MeasurementSystem>();
			Prefixes = new List<Prefix>();
			LoadSystemsAndPrefixes();
			Options = new MeasurementOptions();
		}

		private static void LoadSystemsAndPrefixes() {
			using (var r = new StreamReader(GetPath())) {
				var serializer = new JavaScriptSerializer();
				string json = r.ReadToEnd();

				var items = (Dictionary<string, object>)serializer.DeserializeObject(json);
				var systemsJson = (Dictionary<string, object>)items["systems"];
				ParseSystems(systemsJson);

				var prefixesJson = (Dictionary<string, object>)items["prefixes"];
				ParsePrefixes(prefixesJson);
			}
		}

		private static string GetPath() {
			var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
			if (directoryInfo == null || directoryInfo.Parent == null) {
				throw new Exception("Directory containing json does not exist");
			}
			return Path.Combine(directoryInfo.Parent.FullName, "./common/systems.json");
		}

		private static void ParseSystems(Dictionary<string, object> systemsJson) {
			foreach (KeyValuePair<string, object> systemKeyValuePair in systemsJson) {
				var systemJson = (Dictionary<string, object>) systemKeyValuePair.Value;
				var system = new MeasurementSystem {
					Name = systemKeyValuePair.Key,
					Symbol = Parse<string>(systemJson, "symbol"),
					DerivedString = Parse<string>(systemJson, "derived")
				};
				system.OtherNames.AddRange(ParseArray<string>(systemJson, "otherNames"));

				ParseUnits(systemJson, system);
				Systems.Add(system);
			}
			Systems.ForEach(s => s.UpdateDerived());
		}

		private static void ParseUnits(Dictionary<string, object> systemJson, MeasurementSystem system) {
			string baseUnitName = Parse<string>(systemJson, "baseUnit");
			foreach (KeyValuePair<string, object> unitKeyValuePair in (Dictionary<string, object>) systemJson["units"]) {
				var unitJson = (Dictionary<string, object>) unitKeyValuePair.Value;
				var unit = new Unit {
					Name = unitKeyValuePair.Key,
					System = system,
					Type = Parse<UnitType>(unitJson, "type"),
					Symbol = Parse<string>(unitJson, "symbol"),
					Multiplier = Parse<double>(unitJson, "multiplier"),
					Offset = Parse<double>(unitJson, "offset"),
					IsRare = Parse<bool>(unitJson, "rare"),
					IsEstimation = Parse<bool>(unitJson, "estimation"),
					PrefixName = Parse<string>(unitJson, "prefixName"),
					PrefixFreeName = Parse<string>(unitJson, "prefixFreeName")
				};
				unit.UnitSystems.AddRange(ParseArray<string>(unitJson, "systems"));
				unit.OtherNames.AddRange(ParseArray<string>(unitJson, "otherNames"));
				unit.OtherSymbols.AddRange(ParseArray<string>(unitJson, "otherSymbols"));
				system.Units.Add(unit);
				if (baseUnitName == unit.Name) {
					system.BaseUnit = unit;
				}
			}
		}

		private static void ParsePrefixes(Dictionary<string, object> prefixesJson) {
			foreach (KeyValuePair<string, object> prefixKeyValuePair in prefixesJson) {
				var prefixJson = (Dictionary<string, object>)prefixKeyValuePair.Value;
				var prefix = new Prefix {
					Name = prefixKeyValuePair.Key,
					Symbol = Parse<string>(prefixJson, "symbol"),
					Type = Parse<PrefixType>(prefixJson, "type"),
					IsRare = Parse<bool>(prefixJson, "isRare"),
					Multiplier = Parse<double>(prefixJson, "multiplier"),
					Power = Parse<double>(prefixJson, "power"),
					Base = Parse<double>(prefixJson, "base")
				};
				Prefixes.Add(prefix);
			}
		}

		private static T Parse<T>(Dictionary<string, object> values, string name) {
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

		private static IEnumerable<T> ParseArray<T>(Dictionary<string, object> values, string name) {
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
			MeasurementSystem system = Systems.FirstOrDefault(s => s.Name == systemName);
			return (system != null) ? system.BaseUnit : null;
		}

		public static Unit FindUnit(string unitName) {
			return Systems.SelectMany(s => s.Units).FirstOrDefault(u => u.Name == unitName);
		}

		public static Unit FindUnit(string unitName, string systemName) {
			return Systems.First(s => s.Name == systemName).Units.FirstOrDefault(u => u.Name == unitName);
		}

		public static Prefix FindPrefix(string prefixName) {
			return Prefixes.FirstOrDefault(p => p.Name == prefixName);
		}

		#endregion

	}
}
