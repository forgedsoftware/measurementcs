
namespace ForgedSoftware.Measurement {
	public interface ICopyable<T> where T: ICopyable<T> {
		T Copy();
	}
}
