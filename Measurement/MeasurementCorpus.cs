using System;
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

		#region Dimensions & Units

		public static DimensionDefinition FindDimension(string dimensionName, bool ignoreCase = true) {
			if (string.IsNullOrWhiteSpace(dimensionName)) {
				throw new ArgumentException("dimensionName must be a string at least one character of non-whitespace");
			}
			return Match(Dimensions, dimensionName, u => u.Key, ignoreCase)
					?? Match(Dimensions, dimensionName, u => u.Name, ignoreCase)
					?? Match(Dimensions, dimensionName, u => u.Symbol, ignoreCase)
					?? MatchList(Dimensions, dimensionName, u => u.OtherNames, ignoreCase)
					?? MatchList(Dimensions, dimensionName, u => u.OtherSymbols, ignoreCase);
		}

		public static DimensionDefinition FindDimensionPartial(string dimensionName, bool ignoreCase = true) {
			return FindDimension(dimensionName, ignoreCase)
					?? MatchPartial(Dimensions, dimensionName, u => u.Key, ignoreCase)
					?? MatchPartial(Dimensions, dimensionName, u => u.Name, ignoreCase)
					?? MatchPartial(Dimensions, dimensionName, u => u.Symbol, ignoreCase)
					?? MatchListPartial(Dimensions, dimensionName, u => u.OtherNames, ignoreCase)
					?? MatchListPartial(Dimensions, dimensionName, u => u.OtherSymbols, ignoreCase);
		}

		public static Unit FindBaseUnit(string dimensionName, bool ignoreCase = true) {
			DimensionDefinition dimension = FindDimension(dimensionName, ignoreCase);
			return (dimension != null) ? dimension.BaseUnit : null;
		}

		public static Unit FindBaseUnitPartial(string dimensionName, bool ignoreCase = true) {
			DimensionDefinition dimension = FindDimensionPartial(dimensionName, ignoreCase);
			return (dimension != null) ? dimension.BaseUnit : null;
		}

		public static Unit FindUnit(string unitName, DimensionDefinition dimension = null, bool ignoreCase = true) {
			if (string.IsNullOrWhiteSpace(unitName)) {
				throw new ArgumentException("unitName must be a string at least one character of non-whitespace");
			}
			List<Unit> allUnits = (dimension == null) ? Dimensions.SelectMany(s => s.Units).ToList() : dimension.Units;
			return Match(allUnits, unitName, u => u.Key, ignoreCase)
					?? Match(allUnits, unitName, u => u.Name, ignoreCase)
					?? Match(allUnits, unitName, u => u.Plural, ignoreCase)
					?? Match(allUnits, unitName, u => u.Symbol, ignoreCase)
					?? MatchList(allUnits, unitName, u => u.OtherNames, ignoreCase)
					?? MatchList(allUnits, unitName, u => u.OtherSymbols, ignoreCase);
		}

		public static Unit FindUnitPartial(string unitName, DimensionDefinition dimension = null, bool ignoreCase = true) {
			List<Unit> allUnits = (dimension == null) ? Dimensions.SelectMany(s => s.Units).ToList() : dimension.Units;
			return FindUnit(unitName, dimension, ignoreCase)
					?? MatchPartial(allUnits, unitName, u => u.Key, ignoreCase)
					?? MatchPartial(allUnits, unitName, u => u.Name, ignoreCase)
					?? MatchPartial(allUnits, unitName, u => u.Plural, ignoreCase)
					?? MatchPartial(allUnits, unitName, u => u.Symbol, ignoreCase)
					?? MatchListPartial(allUnits, unitName, u => u.OtherNames, ignoreCase)
					?? MatchListPartial(allUnits, unitName, u => u.OtherSymbols, ignoreCase);
		}

		public static Unit FindUnit(string unitName, string dimensionName, bool ignoreCase = true) {
			return FindUnit(unitName, FindDimension(dimensionName, ignoreCase), ignoreCase);
		}

		public static Unit FindUnitPartial(string unitName, string dimensionName, bool ignoreCase = true) {
			return FindUnitPartial(unitName, FindDimensionPartial(dimensionName, ignoreCase), ignoreCase);
		}

		#endregion

		#region Prefixes

		public static Prefix FindPrefix(string prefixName, bool ignoreCase = true) {
			if (string.IsNullOrWhiteSpace(prefixName)) {
				throw new ArgumentException("prefixName must be a string at least one character of non-whitespace");
			}
			return Match(Prefixes, prefixName, p => p.Name, ignoreCase)
					?? Match(Prefixes, prefixName, p => p.Symbol, ignoreCase);
		}

		public static Prefix FindPrefixPartial(string prefixName, bool ignoreCase = true) {
			return FindPrefix(prefixName, ignoreCase)
					?? MatchPartial(Prefixes, prefixName, p => p.Name, ignoreCase)
					?? MatchPartial(Prefixes, prefixName, p => p.Symbol, ignoreCase);
		}

		#endregion

		#region Systems

		public static MeasurementSystem FindSystem(string systemName, bool ignoreCase = true) {
			if (string.IsNullOrWhiteSpace(systemName)) {
				throw new ArgumentException("systemName must be a string at least one character of non-whitespace");
			}
			return Match(AllSystems, systemName, p => p.Key, ignoreCase)
					?? Match(AllSystems, systemName, p => p.Name, ignoreCase);
		}

		public static MeasurementSystem FindSystemPartial(string systemName, bool ignoreCase = true) {
			return FindSystem(systemName, ignoreCase)
					?? MatchPartial(AllSystems, systemName, p => p.Key, ignoreCase)
					?? MatchPartial(AllSystems, systemName, p => p.Name, ignoreCase);
		}

		#endregion

		#endregion

		#region Helper Functions

		private static T Match<T>(IEnumerable<T> items, string itemName, Func<T, string> accessor, bool ignoreCase = true) {
			return items.FirstOrDefault(u => (!string.IsNullOrWhiteSpace(accessor(u)))
				&& accessor(u).Equals(itemName, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
		}

		private static T MatchPartial<T>(IEnumerable<T> items, string itemName, Func<T, string> accessor, bool ignoreCase = true) {
			return items.FirstOrDefault(u => (!string.IsNullOrWhiteSpace(accessor(u)))
				&& accessor(u).IndexOf(itemName, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0);
		}

		private static T MatchList<T>(IEnumerable<T> items, string itemName, Func<T, List<string>> accessor, bool ignoreCase = true) {
			return items.FirstOrDefault(u => accessor(u).Any(y => y.Equals(itemName, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)));
		}

		private static T MatchListPartial<T>(IEnumerable<T> items, string itemName, Func<T, List<string>> accessor, bool ignoreCase = true) {
			return items.FirstOrDefault(u => accessor(u).Any(y => y.IndexOf(itemName, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0));
		}

		#endregion

	}
}
