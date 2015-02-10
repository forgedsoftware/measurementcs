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

		public static DimensionDefinition FindDimension(string dimensionName) {
			if (string.IsNullOrWhiteSpace(dimensionName)) {
				throw new ArgumentException("dimensionName must be a string at least one character of non-whitespace");
			}
			return Match(Dimensions, dimensionName, u => u.Key)
					?? Match(Dimensions, dimensionName, u => u.Name)
					?? Match(Dimensions, dimensionName, u => u.Symbol)
					?? MatchList(Dimensions, dimensionName, u => u.OtherNames)
					?? MatchList(Dimensions, dimensionName, u => u.OtherSymbols);
		}

		public static DimensionDefinition FindDimensionPartial(string dimensionName) {
			return FindDimension(dimensionName)
					?? MatchPartial(Dimensions, dimensionName, u => u.Key)
					?? MatchPartial(Dimensions, dimensionName, u => u.Name)
					?? MatchPartial(Dimensions, dimensionName, u => u.Symbol)
					?? MatchListPartial(Dimensions, dimensionName, u => u.OtherNames)
					?? MatchListPartial(Dimensions, dimensionName, u => u.OtherSymbols);
		}

		public static Unit FindBaseUnit(string dimensionName) {
			DimensionDefinition dimension = FindDimension(dimensionName);
			return (dimension != null) ? dimension.BaseUnit : null;
		}

		public static Unit FindBaseUnitPartial(string dimensionName) {
			DimensionDefinition dimension = FindDimensionPartial(dimensionName);
			return (dimension != null) ? dimension.BaseUnit : null;
		}

		public static Unit FindUnit(string unitName, DimensionDefinition dimension = null) {
			if (string.IsNullOrWhiteSpace(unitName)) {
				throw new ArgumentException("unitName must be a string at least one character of non-whitespace");
			}
			List<Unit> allUnits = (dimension == null) ? Dimensions.SelectMany(s => s.Units).ToList() : dimension.Units;
			return Match(allUnits, unitName, u => u.Key)
					?? Match(allUnits, unitName, u => u.Name)
					?? Match(allUnits, unitName, u => u.Plural)
					?? Match(allUnits, unitName, u => u.Symbol)
					?? MatchList(allUnits, unitName, u => u.OtherNames)
					?? MatchList(allUnits, unitName, u => u.OtherSymbols);
		}

		public static Unit FindUnitPartial(string unitName, DimensionDefinition dimension = null) {
			List<Unit> allUnits = (dimension == null) ? Dimensions.SelectMany(s => s.Units).ToList() : dimension.Units;
			return FindUnit(unitName, dimension)
					?? MatchPartial(allUnits, unitName, u => u.Key)
					?? MatchPartial(allUnits, unitName, u => u.Name)
					?? MatchPartial(allUnits, unitName, u => u.Plural)
					?? MatchPartial(allUnits, unitName, u => u.Symbol)
					?? MatchListPartial(allUnits, unitName, u => u.OtherNames)
					?? MatchListPartial(allUnits, unitName, u => u.OtherSymbols);
		}

		public static Unit FindUnit(string unitName, string dimensionName) {
			return FindUnit(unitName, FindDimension(dimensionName));
		}

		public static Unit FindUnitPartial(string unitName, string dimensionName) {
			return FindUnitPartial(unitName, FindDimensionPartial(dimensionName));
		}

		#endregion

		#region Prefixes

		public static Prefix FindPrefix(string prefixName) {
			if (string.IsNullOrWhiteSpace(prefixName)) {
				throw new ArgumentException("prefixName must be a string at least one character of non-whitespace");
			}
			return Match(Prefixes, prefixName, p => p.Name)
					?? Match(Prefixes, prefixName, p => p.Symbol);
		}

		public static Prefix FindPrefixPartial(string prefixName) {
			return FindPrefix(prefixName)
					?? MatchPartial(Prefixes, prefixName, p => p.Name)
					?? MatchPartial(Prefixes, prefixName, p => p.Symbol);
		}

		#endregion

		#region Systems

		public static MeasurementSystem FindSystem(string systemName) {
			if (string.IsNullOrWhiteSpace(systemName)) {
				throw new ArgumentException("systemName must be a string at least one character of non-whitespace");
			}
			return Match(AllSystems, systemName, p => p.Key)
					?? Match(AllSystems, systemName, p => p.Name);
		}

		public static MeasurementSystem FindSystemPartial(string systemName) {
			return FindSystem(systemName)
					?? MatchPartial(AllSystems, systemName, p => p.Key)
					?? MatchPartial(AllSystems, systemName, p => p.Name);
		}

		#endregion

		#endregion

		#region Helper Functions

		private static T Match<T>(IEnumerable<T> items, string itemName, Func<T, string> accessor) {
			return items.FirstOrDefault(u => (!string.IsNullOrWhiteSpace(accessor(u))) && accessor(u).Equals(itemName, StringComparison.OrdinalIgnoreCase));
		}

		private static T MatchPartial<T>(IEnumerable<T> items, string itemName, Func<T, string> accessor) {
			return items.FirstOrDefault(u => (!string.IsNullOrWhiteSpace(accessor(u))) && accessor(u).IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static T MatchList<T>(IEnumerable<T> items, string itemName, Func<T, List<string>> accessor) {
			return items.FirstOrDefault(u => accessor(u).Any(y => y.Equals(itemName, StringComparison.OrdinalIgnoreCase)));
		}

		private static T MatchListPartial<T>(IEnumerable<T> items, string itemName, Func<T, List<string>> accessor) {
			return items.FirstOrDefault(u => accessor(u).Any(y => y.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0));
		}

		#endregion

	}
}
