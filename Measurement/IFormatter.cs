
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A simple interface providing formatting to an object
	/// </summary>
	public interface IFormatter {
		string Format();
		string Format(FormatOptions options);
	}
}
