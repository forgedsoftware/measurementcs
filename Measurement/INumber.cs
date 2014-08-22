
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// An extension of the IValue interface, enforcing that the number described must also be of type
	/// INumber.
	/// </summary>
	/// <typeparam name="TNumber">The type of the implementing concrete class</typeparam>
	public interface INumber<TNumber> : IValue<TNumber, double>
		where TNumber : INumber<TNumber> {
	}
}
