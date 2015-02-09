using System;

namespace ForgedSoftware.Measurement.Interfaces {

	/// <summary>
	/// An interface representing a value and allowing basic math to be performed.
	/// In some cases it may not make sense to implement both the scalar and the value
	/// based maths, these cases should be avoided, but if necessary an
	/// <exception cref="InvalidOperationException" /> can be thrown.
	/// TODO - Consider adding 'Modulus' operation
	/// </summary>
	/// <typeparam name="TValue">The type of the value</typeparam>
	/// <typeparam name="TScalar">The type of the scalar to use</typeparam>
	public interface IValue<TValue, TScalar> {

		/// <summary>
		/// Creates a new value that is the sum of this and another value.
		/// </summary>
		/// <param name="add">The value to add</param>
		/// <returns>The sum of the two values</returns>
		TValue Add(TValue add);

		/// <summary>
		/// Creates a new value that is the difference between this and another value.
		/// </summary>
		/// <param name="subtract">The value to subtract</param>
		/// <returns>The difference of the two values</returns>
		TValue Subtract(TValue subtract);

		/// <summary>
		/// Creates a new value that is the product of this and another value.
		/// </summary>
		/// <param name="multiply">The value to multiply by</param>
		/// <returns>The product of the two values</returns>
		TValue Multiply(TValue multiply);

		/// <summary>
		/// Creates a new value that is the quotient of this and another value.
		/// </summary>
		/// <param name="divide">The value to divide by</param>
		/// <returns>The quotient of the two values</returns>
		TValue Divide(TValue divide);

		/// <summary>
		/// Creates a new value that is the sum of this and a scalar value.
		/// </summary>
		/// <param name="add">The scalar value to add</param>
		/// <returns>The sum of the value and the scalar value</returns>
		TValue Add(TScalar add);

		/// <summary>
		/// Creates a new value that is the difference between this and a scalar value.
		/// </summary>
		/// <param name="subtract">The scalar value to subtract</param>
		/// <returns>The difference between the value and the scalar value</returns>
		TValue Subtract(TScalar subtract);

		/// <summary>
		/// Creates a new value that is the product of this and a scalar value.
		/// </summary>
		/// <param name="multiply">The scalar value to multiply by</param>
		/// <returns>The product of the value and the scalar value</returns>
		TValue Multiply(TScalar multiply);

		/// <summary>
		/// Creates a new value that is the quotient of this and a scalar value.
		/// </summary>
		/// <param name="divide">The scalar value to divide by</param>
		/// <returns>The quotient of the value and the scalar value</returns>
		TValue Divide(TScalar divide);

		/// <summary>
		/// Creates a new value that is the negated form of the orginal value.
		/// </summary>
		/// <returns>The negated value</returns>
		TValue Negate();
	}
}
