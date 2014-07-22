using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Linq;

namespace ForgedSoftware.Measurement {
	public static class MeasurementFactory {
		public static List<MeasurementSystem> Systems = new List<MeasurementSystem>();

		static MeasurementFactory() {
			LoadSystems();
		}

		private static void LoadSystems() {
			using (var r = new StreamReader(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "./common/systems.json")))
			{
				var serializer = new JavaScriptSerializer();
				string json = r.ReadToEnd();

				var items = (Dictionary<string, object>)serializer.DeserializeObject(json);
				var systemsJson = (Dictionary<string, object>)items["systems"];
				foreach (KeyValuePair<string, object> systemKeyValuePair in systemsJson) {
					var systemJson = (Dictionary<string, object>) systemKeyValuePair.Value;
					var system = new MeasurementSystem {
						Name = systemKeyValuePair.Key,
						Symbol = Parse<string>(systemJson, "symbol")
						// TODO - add more properties here
					};
					string baseUnitName = Parse<string>(systemJson, "baseUnit");
					foreach (KeyValuePair<string, object> unitKeyValuePair in (Dictionary<string, object>) systemJson["units"]) {
						var unitJson = (Dictionary<string, object>) unitKeyValuePair.Value;
						var unit = new Unit {
							Name = unitKeyValuePair.Key,
							System = system,
							Symbol = Parse<string>(unitJson, "symbol"),
							Multiplier = Parse<double>(unitJson, "multiplier"),
							Offset = Parse<double>(unitJson, "offset")
							// TODO - add more properties here
						};
						system.Units.Add(unit);
						if (baseUnitName == unit.Name) {
							system.BaseUnit = unit;
						}
					}
					Systems.Add(system);
				}
			}
		}

		private static T Parse<T>(Dictionary<string, object> values, string name) {
			object val;
			if (values.TryGetValue(name, out val)) {
				if (typeof(T) == typeof(double)) {
					return (T)(object) Convert.ToDouble(val);
				}
				return (T) val;
			}
			return default(T);
		}

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

		public static Unit FindUnit(string unitName) {
			return Systems.SelectMany(s => s.Units).First(u => u.Name == unitName);
		}

		public static Unit FindUnit(string unitName, string systemName) {
			return Systems.First(s => s.Name == systemName).Units.First(u => u.Name == unitName);
		}
	}
}
