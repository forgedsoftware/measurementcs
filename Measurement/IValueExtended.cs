
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// An interface that provides a series of math functions
	/// for a specific type of value.
	/// </summary>
	/// <seealso cref="System.Math"/>
	/// <typeparam name="TValue">The type of the value being used</typeparam>
	/// <typeparam name="TScalar">The type of the scalar being used</typeparam>
	public interface IValueExtended<TValue, TScalar> : IValue<TValue, TScalar>
		where TValue : IValue<TValue, TScalar> {

		#region General Functions

		/// <summary>
		/// Returns the absolute value of the value.
		/// </summary>
		/// <seealso cref="System.Math.Abs(double)"/>
		/// <returns>The absolute value</returns>
		TValue Abs();

		/// <summary>
		/// Returns the ceiling of the value.
		/// </summary>
		/// <seealso cref="System.Math.Ceiling(double)"/>
		/// <returns>The ceiling of the value</returns>
		TValue Ceiling();

		/// <summary>
		/// Returns the floor of the value.
		/// </summary>
		/// <seealso cref="System.Math.Floor(double)"/>
		/// <returns>The floor of the value</returns>
		TValue Floor();

		/// <summary>
		/// Raises the value to a power.
		/// </summary>
		/// <param name="power">A scalar of the power to raise to</param>
		/// <returns>A new value raised to the power</returns>
		TValue Pow(double power);

		/// <summary>
		/// Rounds the value
		/// </summary>
		/// <seealso cref="System.Math.Round(double)"/>
		/// <returns>The rounded value</returns>
		TValue Round();

		/// <summary>
		/// Returns the square root of the value.
		/// </summary>
		/// <seealso cref="System.Math.Sqrt(double)"/>
		/// <returns>A new value that is the square root of this</returns>
		TValue Sqrt();

		#endregion

		#region Max

		/// <summary>
		/// Returns the max of this and another value.
		/// If equal, this value should be preferred.
		/// </summary>
		/// <param name="other">The other value</param>
		/// <seealso cref="System.Math.Max(double, double)"/>
		/// <returns>The larger of the two values</returns>
		TValue Max(TValue other);

		/// <summary>
		/// Returns the max of this and any other value in the varargs.
		/// </summary>
		/// <param name="values">The other values</param>
		/// <returns>The largest value</returns>
		TValue Max(params TValue[] values);

		#endregion

		#region Min

		/// <summary>
		/// Returns the min of this and another value.
		/// If equal, this value should be preferred.
		/// </summary>
		/// <param name="other">The other value</param>
		/// <seealso cref="System.Math.Max(double, double)"/>
		/// <returns>The smaller of the two values</returns>
		TValue Min(TValue other);

		/// <summary>
		/// Returns the min of this and any other value in the varargs.
		/// </summary>
		/// <param name="values">The other values</param>
		/// <returns>The smallest value</returns>
		TValue Min(params TValue[] values);

		#endregion

	}
}
