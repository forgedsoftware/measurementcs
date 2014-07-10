
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A simple interface to provide copy functionality. This should be implemented as a deep copy.
	/// </summary>
	/// <typeparam name="T">The type or supertype of the implementing class</typeparam>
	public interface ICopyable<T> where T: ICopyable<T> {
		T Copy();
	}
}
