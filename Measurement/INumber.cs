using System;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// An interface representing a number and allowing basic math to be performed.
	/// In some cases it may not make sense to implement both the scalar and the number
	/// based maths, these cases should be avoided, but if necessary an
	/// <exception cref="InvalidOperationException" /> can be thrown.
	/// </summary>
	/// <typeparam name="TNumber">The type of the implementing concrete class</typeparam>
	public interface INumber<TNumber> where TNumber : INumber<TNumber> {

		/// <summary>
		/// Creates a new number that is the sum of this and another number.
		/// </summary>
		/// <param name="add">The number to add</param>
		/// <returns>The sum of the two numbers</returns>
		TNumber Add(TNumber add);

		/// <summary>
		/// Creates a new number that is the difference between this and another number.
		/// </summary>
		/// <param name="subtract">The number to subtract</param>
		/// <returns>The difference of the two numbers</returns>
		TNumber Subtract(TNumber subtract);

		/// <summary>
		/// Creates a new number that is the product of this and another number.
		/// </summary>
		/// <param name="multiply">The number to multiply by</param>
		/// <returns>The product of the two numbers</returns>
		TNumber Multiply(TNumber multiply);

		/// <summary>
		/// Creates a new number that is the quotient of this and another number.
		/// </summary>
		/// <param name="divide">The number to divide by</param>
		/// <returns>The quotient of the two numbers</returns>
		TNumber Divide(TNumber divide);

		/// <summary>
		/// Creates a new number that is the sum of this and a scalar value.
		/// </summary>
		/// <param name="add">The scalar value to add</param>
		/// <returns>The sum of the number and the scalar value</returns>
		TNumber Add(double add);

		/// <summary>
		/// Creates a new number that is the difference between this and a scalar value.
		/// </summary>
		/// <param name="subtract">The scalar value to subtract</param>
		/// <returns>The difference between the number and the scalar value</returns>
		TNumber Subtract(double subtract);

		/// <summary>
		/// Creates a new number that is the product of this and a scalar value.
		/// </summary>
		/// <param name="multiply">The scalar value to multiply by</param>
		/// <returns>The product of the number and the scalar value</returns>
		TNumber Multiply(double multiply);

		/// <summary>
		/// Creates a new number that is the quotient of this and a scalar value.
		/// </summary>
		/// <param name="divide">The scalar value to divide by</param>
		/// <returns>The quotient of the number and the scalar value</returns>
		TNumber Divide(double divide);

		/// <summary>
		/// Creates a new number that is the negated form of the orginal number.
		/// </summary>
		/// <returns>The negated number</returns>
		TNumber Negate();
	}
}
