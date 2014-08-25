
namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A interface describing the general functionality of vectors
	/// </summary>
	/// <typeparam name="TVector">The type of the concrete vector</typeparam>
	public interface IVector<TVector> where TVector : IVector<TVector> {

		/// <summary>
		/// A array representation of the axis of the vector.
		/// </summary>
		double[] Array { get; }

		/// <summary>
		/// Calculates the magnitude or size of the vector
		/// </summary>
		double Magnitude { get; }

		/// <summary>
		/// Returns the sum of each of the components of the vector
		/// </summary>
		double SumComponents { get; }

		/// <summary>
		/// Returns the normalized form of the vector. A normalized vector is a vector
		/// scaled to the size of a unit vector.
		/// </summary>
		TVector Normalize { get; }

		/// <summary>
		/// Returns the dot product or scalar product of two vectors. This is equivalent to the
		/// multiplication of each vector component and then summing them together.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The dot product of the two vectors</returns>
		double DotProduct(TVector vector);

		/// <summary>
		/// Returns the angle between the two vectors in radians.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The angle between the two vectors</returns>
		double Angle(TVector vector);

		/// <summary>
		/// Checks if the vector is a unit vector. Unit vectors are vectors that have
		/// a magnitude of 1.
		/// </summary>
		/// <returns>True if it is a unit vector, else false</returns>
		bool IsUnitVector();

		/// <summary>
		/// Finds the distance between two vectors using the Pythagoras theorem.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The distance between the two vectors</returns>
		double Distance(TVector vector);

		/// <summary>
		/// Determines if two vectors are perpendicular, that is at right angles to each other.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>True if perpendicular, else false</returns>
		bool IsPerpendicular(TVector vector);
	}
}
