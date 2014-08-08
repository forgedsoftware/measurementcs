
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A prefix is a scale modifier used in certain systems of measurement,
	/// particularly the metric system.
	/// </summary>
	/// <example>
	/// kilo-, centi-, kili-, tera-, pico- are all examples of prefixes.
	/// </example>
	public class Prefix {
		public string Name { get; set; }
		public string Symbol { get; set; }
		public double Multiplier { get; set; }
		public PrefixType Type { get; set; }
	}

	/// <summary>
	/// A PrefixType is an enum that represents the type of a particular Prefix.
	/// </summary>
	public enum PrefixType {
		MetricPrefix, BinaryMetricPrefix, UnofficialMetricPrefix
	}
}
