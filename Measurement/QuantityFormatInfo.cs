using System;
using System.Globalization;
using ForgedSoftware.Measurement.Interfaces;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A set of options for formatting quantities and dimensions.
	/// </summary>
	public class QuantityFormatInfo : IFormatProvider, ICloneable<QuantityFormatInfo> {

		public QuantityFormatInfo(NumberFormatInfo numberFormat = null) {
			if (numberFormat == null) {
				numberFormat = CultureInfo.CurrentCulture.NumberFormat;
			}
			NumberFormat = numberFormat;
			FormatParts = QuantityParts.All;
			Precision = -1;
			DefaultDoubleFormat = "G";
			ScientificExponent = true;
			SortDimensions = true;
			UnitSeparator = "·";
		}

		#region Format Provider Handling

		public object GetFormat(Type formatType) {
			return (formatType == typeof (QuantityFormatInfo)) ? this : null;
		}

		public static QuantityFormatInfo GetInstance(IFormatProvider formatProvider) {
			var cultureProvider = formatProvider as CultureInfo;
			if (cultureProvider != null) {
				return new QuantityFormatInfo(cultureProvider.NumberFormat);
			}
			var numberFormatProvider = formatProvider as NumberFormatInfo;
			if (numberFormatProvider != null) {
				return new QuantityFormatInfo(numberFormatProvider);
			}
			var info = formatProvider as QuantityFormatInfo;
			if (info != null) {
				return info;
			}
			if (formatProvider != null) {
				info = formatProvider.GetFormat(typeof (QuantityFormatInfo)) as QuantityFormatInfo;
				if (info != null) {
					return info;
				}
			}
			return CurrentInfo;
		}

		public static QuantityFormatInfo CurrentInfo {
			get {
				return MeasurementCorpus.Corpus.Options.QuantityFormat;
			}
		}

		#endregion

		#region Properties

		// General
		public QuantityParts FormatParts { get; set; }
		public bool AsciiOnly { get; set; }

		// Value
		public NumberFormatInfo NumberFormat { get; set; }
		public int Precision { get; set; }
		public bool ScientificExponent { get; set; }
		public string DefaultDoubleFormat { get; set; }

		// Dimensions
		public bool SortDimensions { get; set; }
		public string UnitSeparator { get; set; }
		public bool TextualDescription { get; set; }
		public bool ShowAllPowers { get; set; }

		[Flags]
		public enum QuantityParts {
			None = 0,
			Value = 1,
			Dimensions = 2,
			All = Value|Dimensions
		}

		#endregion

		#region Cloneable

		object ICloneable.Clone() {
			return Clone();
		}

		public QuantityFormatInfo Clone() {
			return (QuantityFormatInfo) MemberwiseClone();
		}

		#endregion
	}
}
