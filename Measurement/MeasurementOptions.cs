using System.Collections.Generic;
using System.Globalization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Provides a set of options to be used by the Measurement library
	/// </summary>
	public class MeasurementOptions {

		/// <summary>
		/// Parameterless constructor that sets up all the default values of the options
		/// </summary>
		public MeasurementOptions() {
			// Defaults - General
			CanReorderDimensions = true;

			// Defaults - Systems & Units
			IgnoreDerivedSystems = false;
			AllowedUnitSystems = new List<string> { "imperial", "usCustomary", "metric" };
			// TODO - Need to tidy this up!!
			// Others: nautical, cgi, traditional Chinese, historical, [no system], oldImperial, astronomicalUnits, avoirdupois, jewellery, cgs, si
			IgnoredMeasurementSystems = new List<DimensionDefinition>();
			UseEstimatedUnits = false;
			UseRareUnits = false;
			UseHistoricalUnits = false;

			// Defaults - Prefixes
			UseAutomaticPrefixManagement = true;
			UseRarePrefixes = false;
			UseUnofficalPrefixes = false;
			AllowedRarePrefixCombinations = new List<KeyValuePair<Unit, Prefix>> {
				new KeyValuePair<Unit, Prefix>(
					MeasurementCorpus.FindUnit("metre"), MeasurementCorpus.FindPrefix("centi"))
			};
			PreferBinaryPrefixes = true;
			UpperPrefixValue = 1000;
			LowerPrefixValue = 1;
			HavingPrefixScoreOffset = 1;

			// Defaults - Formatting
			DefaultFormatOptions = new FormatOptions(CultureInfo.CurrentCulture);
		}

		#region General

		public bool CanReorderDimensions { get; set; }

		#endregion

		#region Systems & Units

		public bool IgnoreDerivedSystems { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public List<string> AllowedUnitSystems { get; private set; }

		// TODO - Documentation
		// TODO - Use it!
		public List<DimensionDefinition> IgnoredMeasurementSystems { get; private set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseEstimatedUnits { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseRareUnits { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseHistoricalUnits { get; set; } // TODO - Should historical be its own field on a unit?

		#endregion

		#region Prefixes

		// TODO - Documentation
		// TODO - Use it!
		public bool UseAutomaticPrefixManagement { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseRarePrefixes { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseUnofficalPrefixes { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public List<KeyValuePair<Unit, Prefix>> AllowedRarePrefixCombinations { get; private set; }

		/// <summary>
		/// An option that determines whether binary prefixes are used over standard
		/// SI prefixes for quantities involving information.
		/// </summary>
		// TODO - Use it!
		public bool PreferBinaryPrefixes { get; set; }

		// TODO - Documentation
		public double UpperPrefixValue { get; set; }

		// TODO - Documentation
		public double LowerPrefixValue { get; set; }

		// TODO - Documentation
		public double HavingPrefixScoreOffset { get; set; }

		#endregion

		#region Formatting

		/// <summary>
		/// A set of format options to be used by default when formatting quantities.
		/// </summary>
		// TODO - Use it!
		public FormatOptions DefaultFormatOptions { get; set; }

		#endregion

	}
}
