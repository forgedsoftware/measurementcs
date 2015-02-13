using System.Collections.Generic;
using System.Globalization;
using ForgedSoftware.Measurement.Entities;

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
			AllowReorderingDimensions = true;

			// Defaults - Dimensions
			AllowDerivedDimensions = true;
			AllowVectorDimensions = false;
			IgnoredDimensions = new List<DimensionDefinition>();

			// Defaults - Units
			UseEstimatedUnits = false;
			UseRareUnits = false;

			// Defaults - Measurement Systems
			AllowedSystemsForUnits = new List<MeasurementSystem>();
			IgnoredSystemsForUnits = new List<MeasurementSystem>();

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

		/// <summary>
		/// Dimensions are usually kept in the same order where possible, however changes
		/// in powers, additions of other dimensions or changing prefixes can lead dimensions
		/// to benefit from reordering.
		/// Default: true
		/// </summary>
		public bool AllowReorderingDimensions { get; set; }

		#endregion

		#region Dimensions

		/// <summary>
		/// Whether dimensions derived from other dimensions are allowed for find and simplifications.
		/// Default: true
		/// TODO - the use in simplify should be separated out, maybe built into the simplify itself... (Remember to change documentation)
		/// </summary>
		public bool AllowDerivedDimensions { get; set; }

		/// <summary>
		/// Whether dimensions that are vectors are allowed for find and simplifications.
		/// Default: false
		/// </summary>
		public bool AllowVectorDimensions { get; set; }

		/// <summary>
		/// A list of dimension defintions that are ignored by find and for simplifications.
		/// Default: empty
		/// </summary>
		public List<DimensionDefinition> IgnoredDimensions { get; private set; }

		#endregion

		#region Units

		/// <summary>
		/// Whether units that have estimated factors to the base unit are allowed for find.
		/// Default: false
		/// </summary>
		public bool UseEstimatedUnits { get; set; }

		/// <summary>
		/// Whether units that are rarely used are allowed for find.
		/// Default: false
		/// </summary>
		public bool UseRareUnits { get; set; }

		#endregion

		#region Measurement Systems

		/// <summary>
		/// Provides a list of all available systems (and their ancestors) that can be used in find and unit find.
		/// If empty, all systems are available. By default each ancestor higher in the tree to a specified
		/// allowed system is available as well, except if restricted by IgnoredSystemsForUnits.
		/// Default: empty, all systems allowed
		/// </summary>
		public List<MeasurementSystem> AllowedSystemsForUnits { get; private set; }

		/// <summary>
		/// Provides a list of systems (and their ancestors) that may not be used even if their
		/// children are available (see AllowedSystemsForUnits). Note this also excludes ancestors
		/// that may ancestors of speicifically allowed systems. You may need to include that ancestor
		/// as a specifically allowed system.
		/// Default: empty
		/// </summary>
		/// <example>
		/// With rules:
		/// Allow A, F, E. Ignore B.
		/// 
		/// Resulting SubTree (a = allow, i = ignore):
		/// A(a)--> B(i)-->C(i)-->D(i)-->E(a)
		///         F(a)--> G(a)--/
		/// </example>
		public List<MeasurementSystem> IgnoredSystemsForUnits { get; private set; }

		#endregion

		#region Prefixes

		/// <summary>
		/// Allows prefixes that are rarely used in find results.
		/// Default: false
		/// </summary>
		public bool UseRarePrefixes { get; set; }

		/// <summary>
		/// Allows prefixes in find with SiUnofficial type.
		/// Default: false
		/// </summary>
		public bool UseUnofficalPrefixes { get; set; }

		// TODO - Documentation
		// TODO - Use it!
		public bool UseAutomaticPrefixManagement { get; set; }

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
