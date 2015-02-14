
namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// A simple interface providing formatting to an object
	/// </summary>
	public interface IFormatter {

		/// <summary>
		/// Formats the object with a default set of format options.
		/// </summary>
		/// <returns>The formatted string</returns>
		string Format();

		/// <summary>
		/// Formats the object with a provided set of format options.
		/// </summary>
		/// <param name="info">The format options to use</param>
		/// <returns>The formatted string</returns>
		string Format(QuantityFormatInfo info);
	}
}
