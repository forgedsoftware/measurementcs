
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// An interface that provides a series of math functions
	/// for a specific type of number.
	/// </summary>
	/// <seealso cref="System.Math"/>
	/// <typeparam name="TNumber">The type of the number being used</typeparam>
	public interface INumberMath<TNumber> : INumber<TNumber>
		where TNumber : INumber<TNumber> {

		/// <summary>
		/// Raises the number to a power.
		/// </summary>
		/// <param name="power">The power to raise to</param>
		/// <returns>A new number raised to the power</returns>
		TNumber Pow(double power);
		
		/// <summary>
		/// Returns the square root of the number.
		/// </summary>
		/// <returns>A new number that is the square root of this</returns>
		TNumber Sqrt();

		/// <summary>
		/// Returns the max of this and another number.
		/// </summary>
		/// <param name="other">The other number</param>
		/// <returns>The larger of the two numbers</returns>
		TNumber Max(TNumber other);

		/// <summary>
		/// Returns the min of this and another number.
		/// </summary>
		/// <param name="other">The other number</param>
		/// <returns>The smaller of the two numbers</returns>
		TNumber Min(TNumber other);
	}
}
