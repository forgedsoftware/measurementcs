using System.Collections.Generic;
using System.Globalization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Provides a set of options to be used by the Measurement library
	/// </summary>
	public class MeasurementOptions {

		public MeasurementOptions() {
			// Defaults - Systems & Units
			AllowedUnitSystems = new List<string> { "imperial", "usCustomary", "metric" };
			// TODO - Need to tidy this up!!
			// Others: nautical, cgi, traditional Chinese, historical, [no system], oldImperial, astronomicalUnits, avoirdupois, jewellery, cgs
			IgnoredMeasurementSystems = new List<MeasurementSystem>();
			UseEstimatedUnits = false;
			UseRareUnits = false;
			UseHistoricalUnits = false;

			// Defaults - Prefixes
			UseAutomaticPrefixManagement = true;
			UseRarePrefixes = false;
			UseUnofficalPrefixes = false;
			AllowedRarePrefixCombinations = new List<KeyValuePair<Unit, Prefix>> {
				new KeyValuePair<Unit, Prefix>(
					MeasurementFactory.FindUnit("metre"), MeasurementFactory.FindPrefix("centi"))
			};
			PreferBinaryPrefixes = true;

			// Defaults - Formatting
			DefaultFormatOptions = new FormatOptions(CultureInfo.CurrentCulture);
		}

		#region Systems & Units

		// TODO - Use it!
		public List<string> AllowedUnitSystems { get; private set; }

		// TODO - Use it!
		public List<MeasurementSystem> IgnoredMeasurementSystems { get; private set; }

		// TODO - Use it!
		public bool UseEstimatedUnits { get; set; }

		// TODO - Use it!
		public bool UseRareUnits { get; set; }

		// TODO - Use it!
		public bool UseHistoricalUnits { get; set; } // TODO - Should historical be its own field on a unit?

		#endregion

		#region Prefixes

		// TODO - Use it!
		public bool UseAutomaticPrefixManagement { get; set; }

		// TODO - Use it!
		public bool UseRarePrefixes { get; set; }

		// TODO - Use it!
		public bool UseUnofficalPrefixes { get; set; }

		// TODO - Use it!
		public List<KeyValuePair<Unit, Prefix>> AllowedRarePrefixCombinations { get; private set; }

		/// <summary>
		/// An option that determines whether binary prefixes are used over standard
		/// SI prefixes for quantities involving information.
		/// </summary>
		// TODO - Use it!
		public bool PreferBinaryPrefixes { get; set; }

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
