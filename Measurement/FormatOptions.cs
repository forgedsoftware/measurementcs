using System;
using System.Globalization;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A set of options for formatting quantities and dimensions.
	/// </summary>
	public class FormatOptions {

		#region Constructors

		/// <summary>
		/// Parameterless constructor, does not use a format provider. Sets all options to defaults.
		/// </summary>
		public FormatOptions() {
			Show = QuantityParts.All;
			Precision = -1;
			ExpandExponent = true;
			DecimalSeparator = ".";
			GroupSeparator = "";
			GroupSize = 3;
			Sort = true;
			UnitSeparator = "·";
		}

		/// <summary>
		/// Main constructor. Utilises an IFormatProvider to provide number formatting.
		/// </summary>
		/// <param name="provider">provides culutral details</param>
		public FormatOptions(IFormatProvider provider) : this() {
			var culture = provider as CultureInfo;
			if (culture != null) {
				provider = culture.NumberFormat;
			}
			var numProvider = provider as NumberFormatInfo;
			if (numProvider != null) {
				Precision = numProvider.NumberDecimalDigits;
				GroupSeparator = numProvider.NumberGroupSeparator;
				if (numProvider.NumberGroupSizes.Length > 0) {
					GroupSize = numProvider.NumberGroupSizes[numProvider.NumberGroupSizes.Length - 1];
				}
				DecimalSeparator = numProvider.NumberDecimalSeparator;
			}
		}

		#endregion

		#region Options

		// General
		public QuantityParts Show { get; set; }
		public bool Ascii { get; set; }
		
		// Value
		public int Precision { get; set; }
		public bool ExpandExponent { get; set; }
		public string DecimalSeparator { get; set; }
		public string GroupSeparator { get; set; }
		public int GroupSize { get; set; }

		// Dimensions
		public bool Sort { get; set; }
		public string UnitSeparator { get; set; }
		public bool FullName { get; set; }
		public bool ShowAllPowers { get; set; }

		public enum QuantityParts {
			ValueOnly, DimensionsOnly, All
		}

		#endregion

		#region Static Instances

		public static FormatOptions Default(IFormatProvider provider) {
			return new FormatOptions(provider);
		}

		public static FormatOptions Raw(IFormatProvider provider) {
			return new FormatOptions(provider) {
				Ascii = true,
				ExpandExponent = false
			};
		}

		public static FormatOptions ValueOnly(IFormatProvider provider) {
			return new FormatOptions(provider) {
				Show = QuantityParts.ValueOnly
			};
		}

		public static FormatOptions DimensionsOnly(IFormatProvider provider) {
			return new FormatOptions(provider) {
				Show = QuantityParts.DimensionsOnly
			};
		}

		#endregion

	}
}
