using System;
using System.Globalization;

namespace ForgedSoftware.Measurement {
	
	/// <summary>
	/// A class describing a standard three dimensional vector and associate math, providing a set of
	/// vector specific functionality including dot product, cross product, angle, magnitude.
	/// </summary>
	public class Vector : INumber<Vector>, IMathFunctions<Vector>, IFormattable,
			IEquatable<Vector>, IComparable, IComparable<Vector>, ICopyable<Vector> {

		/// <summary>
		/// This is a reasonable epsilon for vector comparisons and equitability.
		/// </summary>
		public static readonly double EquatableEpsilon = 1E-15;

		#region Standard Vectors

		/// <summary>
		/// A static vector describing an origin vector.
		/// </summary>
		public static readonly Vector Origin = new Vector(0, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the x-axis.
		/// </summary>
		public static readonly Vector XAxis = new Vector(1, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the y-axis.
		/// </summary>
		public static readonly Vector YAxis = new Vector(0, 1, 0);

		/// <summary>
		/// A static vector describing a unit vector along the z-axis.
		/// </summary>
		public static readonly Vector ZAxis = new Vector(0, 0, 1);

		/// <summary>
		/// A static vector describing the min value of on each axis.
		/// </summary>
		public static readonly Vector MinValue =
			new Vector(Double.MinValue, Double.MinValue, Double.MinValue);

		/// <summary>
		/// A static vector describing the max value of on each axis.
		/// </summary>
		public static readonly Vector MaxValue =
			new Vector(Double.MaxValue, Double.MaxValue, Double.MaxValue);

		/// <summary>
		/// A static vector describing the standard double epsilon value of on each axis.
		/// </summary>
		public static readonly Vector Epsilon =
			new Vector(Double.Epsilon, Double.Epsilon, Double.Epsilon);

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor
		/// </summary>
		/// <param name="x">The x axis value</param>
		/// <param name="y">The y axis value</param>
		/// <param name="z">The z axis value</param>
		public Vector(double x, double y, double z) {
			XValue = x;
			YValue = y;
			ZValue = z;
		}

		/// <summary>
		/// Array constructor
		/// </summary>
		/// <param name="values">The vector as an array of values</param>
		public Vector(double[] values) {
			Array = values;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Vector(Vector vector) {
			XValue = vector.XValue;
			YValue = vector.YValue;
			ZValue = vector.ZValue;
		}

		#endregion

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the x-axis.
		/// </summary>
		public double XValue { get; set; }

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the y-axis.
		/// </summary>
		public double YValue { get; set; }

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the z-axis.
		/// </summary>
		public double ZValue { get; set; }

		/// <summary>
		/// A array representation of the three axis of the vector.
		/// </summary>
		public double[] Array {
			get { return new[] { XValue, YValue, ZValue }; }
			set {
				if (value.Length != 3) {
					throw new ArgumentException("An array needs to contain 3 values to be a valid vector");
				}
				XValue = value[0];
				YValue = value[1];
				ZValue = value[2];
			}
		}

		#region Vector Functionality

		/// <summary>
		/// Calculates the magnitude or size of the vector
		/// </summary>
		public double Magnitude {
			get { return Math.Sqrt(Pow(2).SumComponents); }
		}

		/// <summary>
		/// Returns the sum of each of the components of the vector
		/// </summary>
		public double SumComponents {
			get { return XValue + YValue + ZValue; }
		}

		/// <summary>
		/// Returns the dot product or scalar product of two vectors. This is equivalent to the
		/// multiplication of each vector component and then summing them together.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The dot product of the two vectors</returns>
		public double DotProduct(Vector vector) {
			return (XValue * vector.XValue + YValue * vector.YValue + ZValue * vector.ZValue);
		}

		/// <summary>
		/// Returns the cross product of two vectors. This is a non-commutable operation.
		/// The cross product is a vector perpendicular to the other two vectors, uses the Right Hand Rule,
		/// and has a magnitude equal to the area of the parallelogram the vectors span.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The cross product of the two vectors</returns>
		public Vector CrossProduct(Vector vector) {
			// Non-commutable
			return new Vector(
				YValue * vector.ZValue - ZValue * vector.YValue,
				ZValue * vector.XValue - XValue * vector.ZValue,
				XValue * vector.YValue - YValue * vector.XValue);
		}

		/// <summary>
		/// Checks if the vector is a unit vector. Unit vectors are vectors that have
		/// a magnitude of 1.
		/// </summary>
		/// <returns>True if it is a unit vector, else false</returns>
		public bool IsUnitVector() {
			return (Math.Abs(Magnitude - 1) < EquatableEpsilon);
		}

		/// <summary>
		/// Returns the angle between the two vectors in radians.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The angle between the two vectors</returns>
		public double Angle(Vector vector) {
			return Math.Acos(Normalize.DotProduct(vector.Normalize));
		}

		/// <summary>
		/// Returns the normalized form of the vector. A normalized vector is a vector
		/// scaled to the size of a unit vector.
		/// </summary>
		public Vector Normalize {
			get {
				if (Math.Abs(Magnitude) < EquatableEpsilon) {
					throw new DivideByZeroException("A vector must have a magnitude of greater than 0 to normalize");
				}
				double inverse = 1 / Magnitude;
				return Multiply(inverse);
			}
		}

		/// <summary>
		/// Finds the distance between two vectors using the Pythagoras theorem.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The distance between the two vectors</returns>
		public double Distance(Vector vector) {
			return Math.Sqrt(
				(XValue - vector.XValue) * (XValue - vector.XValue) +
				(YValue - vector.YValue) * (YValue - vector.YValue) +
				(ZValue - vector.ZValue) * (ZValue - vector.ZValue));
		}

		/// <summary>
		/// Determines if two vectors are perpendicular, that is at right angles to each other.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>True if perpendicular, else false</returns>
		public bool IsPerpendicular(Vector vector) {
			return Math.Abs(DotProduct(vector)) < EquatableEpsilon;
		}

		#endregion

		#region IMathFunctions

		/// <summary>
		/// The power function raises each component of the vector to a specified power.
		/// </summary>
		/// <param name="power">The power to raise by</param>
		/// <returns>A vector raised to the provided power</returns>
		public Vector Pow(double power) {
			return FuncTwoParam(Math.Pow, power);
		}

		/// <summary>
		/// The squareroot function applies a squareroot to each component of the vector.
		/// </summary>
		/// <returns>The squareroot of the vector</returns>
		public Vector Sqrt() {
			return FuncOneParam(Math.Sqrt);
		}

		/// <summary>
		/// The larger of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The larger of the two vectors</returns>
		public Vector Max(Vector vector) {
			return (this >= vector) ? this : vector;
		}

		/// <summary>
		/// The smaller of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The smaller of the two vectors</returns>
		public Vector Min(Vector vector) {
			return (this <= vector) ? this : vector;
		}

		/// <summary>
		/// Helper method to apply System.Math functions with one parameter.
		/// </summary>
		private Vector FuncOneParam(Func<double, double> func) {
			return new Vector(func(XValue), func(YValue), func(ZValue));
		}

		/// <summary>
		/// Helper method to apply System.Math functions with two parameters.
		/// </summary>
		private Vector FuncTwoParam(Func<double, double, double> func, double param) {
			return new Vector(func(XValue, param), func(YValue, param), func(ZValue, param));
		}

		#endregion

		#region Basic Math

		/// <summary>
		/// Adds two vectors together in a component wise fashion.
		/// </summary>
		/// <param name="add">The vector to add</param>
		/// <returns>The summed vector</returns>
		public Vector Add(Vector add) {
			return new Vector(XValue + add.XValue, YValue + add.YValue, ZValue + add.ZValue);
		}

		/// <summary>
		/// Subtracts two vectors from each other in a component wise fashion
		/// </summary>
		/// <param name="subtract">The vector to subtract</param>
		/// <returns>The vector of the difference</returns>
		public Vector Subtract(Vector subtract) {
			return new Vector(XValue - subtract.XValue, YValue - subtract.YValue, ZValue - subtract.ZValue);
		}

		/// <summary>
		/// Multiplies two vectors together with a cross product. This operation
		/// is non-commutable.
		/// </summary>
		/// <param name="multiply">The vector to multiply by</param>
		/// <returns>The cross product of the two vectors</returns>
		public Vector Multiply(Vector multiply) {
			return CrossProduct(multiply);
		}

		/// <summary>
		/// Division of vectors is not a well defined mathematical operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector Divide(Vector divide) {
			throw new InvalidOperationException("This operation is not implemented for vectors as it is mathematically not well defined");
		}

		/// <summary>
		/// Addition of a scalar value to a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector Add(double add) {
			throw new InvalidOperationException("Adding a scalar value to a vector is not valid");
		}

		/// <summary>
		/// Subtraction of a scalar value from a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector Subtract(double subtract) {
			throw new InvalidOperationException("Subtracting a scalar value from a vector is not valid");
		}

		/// <summary>
		/// Scalar multiplication of a vector with a value.
		/// </summary>
		/// <param name="multiply">The scalar value to multiply by</param>
		/// <returns>The multiplied vector</returns>
		public Vector Multiply(double multiply) {
			return new Vector(XValue * multiply, YValue * multiply, ZValue * multiply);
		}

		/// <summary>
		/// Scalar division of a vector by a value.
		/// </summary>
		/// <param name="divide">The scalar value to divide by</param>
		/// <returns>The divided vector</returns>
		public Vector Divide(double divide) {
			return new Vector(XValue / divide, YValue / divide, ZValue / divide);
		}
		
		/// <summary>
		/// Returns the negation of a vector which is merely the negation of
		/// each component of the vector.
		/// </summary>
		/// <returns>The negated vector</returns>
		public Vector Negate() {
			return new Vector(-XValue, -YValue, -ZValue);
		}

		#endregion

		#region Operator Overrides

		/// <summary>
		/// Overrides the + (addition) operator with a standard adding of vectors.
		/// </summary>
		/// <seealso cref="Add(Vector)"/>
		public static Vector operator +(Vector vector1, Vector vector2) {
			return vector1.Add(vector2);
		}

		/// <summary>
		/// Overrides the - (subtraction) operator with a standard subtraction of vectors.
		/// </summary>
		/// <seealso cref="Subtract(Vector)"/>
		public static Vector operator -(Vector vector1, Vector vector2) {
			return vector1.Subtract(vector2);
		}

		/// <summary>
		/// Overrides the - (negation) operator with a vector negation
		/// </summary>
		/// <seealso cref="Negate()"/>
		public static Vector operator -(Vector vector1) {
			return vector1.Negate();
		}

		/// <summary>
		/// Overrides the + (reinforcement) operator by returning a copy of the vector
		/// </summary>
		/// <seealso cref="Copy()"/>
		public static Vector operator +(Vector vector1) {
			return vector1.Copy();
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a vector and a scalar
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector operator *(Vector vector1, double d2) {
			return vector1.Multiply(d2);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a scalar and a vector
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector operator *(double d1, Vector vector2) {
			// Commutable
			return vector2.Multiply(d1);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between two vectors as the cross
		/// product of two vectors. Care should be taken here not to confuse the result
		/// with the scalar result of the dot product.
		/// </summary>
		/// <seealso cref="Multiply(Vector)"/>
		/// <seealso cref="CrossProduct(Vector)"/>
		public static Vector operator *(Vector vector1, Vector vector2) {
			return vector1.Multiply(vector2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a vector and a scalar
		/// as a scalar division.
		/// </summary>
		/// <seealso cref="Divide(double)"/>
		public static Vector operator /(Vector vector1, double d2) {
			return vector1.Divide(d2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a scalar and a vector
		/// as a scalar division.
		/// </summary>
		public static Vector operator /(double d1, Vector vector2) {
			return new Vector(d1 / vector2.XValue, d1 / vector2.YValue, d1 / vector2.ZValue);
		}

		/// <summary>
		/// Overrides the &lt; (less than) operator as the test of whether the
		/// magnitude of the first vector is less than that of the second.
		/// </summary>
		public static bool operator <(Vector vector1, Vector vector2) {
			return vector1.Magnitude < vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &lt;= (less than or equal) operator as the test of whether the
		/// magnitude of the first vector is less than or equal to that of the second.
		/// </summary>
		public static bool operator <=(Vector vector1, Vector vector2) {
			return vector1.Magnitude <= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt; (greater than) operator as the test of whether the
		/// magnitude of the first vector is greater than that of the second.
		/// </summary>
		public static bool operator >(Vector vector1, Vector vector2) {
			return vector1.Magnitude > vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt;= (greater than or equal) operator as the test of whether the
		/// magnitude of the first vector is greater than or equal to that of the second.
		/// </summary>
		public static bool operator >=(Vector vector1, Vector vector2) {
			return vector1.Magnitude >= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the == (equality) operator to test whether the vectors are equivalent
		/// based on whether the component values are the same (epsilon test).
		/// Comparing two null values also returns true.
		/// </summary>
		public static bool operator ==(Vector vector1, Vector vector2) {
			if (((object) vector1 == null) && ((object) vector2 == null)) {
				return true;
			}
			return (((object) vector1 != null) && ((object) vector2 != null) &&
				(Math.Abs(vector1.XValue - vector2.XValue) <= EquatableEpsilon) &&
				(Math.Abs(vector1.YValue - vector2.YValue) <= EquatableEpsilon) &&
				(Math.Abs(vector1.ZValue - vector2.ZValue) <= EquatableEpsilon));
		}

		/// <summary>
		/// Overrides the != (inequality) operator to test whether the vectors
		/// are not equivalent. It is a direct inverse of the equality operator.
		/// </summary>
		/// <seealso cref="operator ==(Vector, Vector)"/>
		public static bool operator !=(Vector vector1, Vector vector2) {
			return !(vector1 == vector2);
		}

		#endregion

		#region Comparing

		/// <summary>
		/// An override of the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector, Vector)"/>
		public override bool Equals(object obj) {
			Vector vector = obj as Vector;
			if (vector != null) {
				return this == vector;
			}
			return false;
		}

		/// <summary>
		/// Implements the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector, Vector)"/>
		public bool Equals(Vector vector) {
			return this == vector;
		}

		/// <summary>
		/// A standard hash code implementation, hashing the three components.
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				int hash = (int)2166136261;
				hash = hash * 16777619 ^ XValue.GetHashCode();
				hash = hash * 16777619 ^ YValue.GetHashCode();
				hash = hash * 16777619 ^ ZValue.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Compares an object with the vector. If the object is not a vector,
		/// throws <exception cref="ArgumentException" />. Otherwise returns
		/// an appropriate comparison value.
		/// </summary>
		/// <param name="obj">The object to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other vector, 0 if they
		/// are the same, and 1 if this vector is larger in terms of magnitude.</returns>
		public int CompareTo(object obj) {
			Vector vector = obj as Vector;
			if (vector != null) {
				return CompareTo(vector);
			}
			throw new ArgumentException("Cannot compare a Vector with a " + obj.GetType());
		}

		/// <summary>
		/// Direct comparison between two vectors.
		/// </summary>
		/// <param name="vector">The vector to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other vector, 0 if they
		/// are the same, and 1 if this vector is larger in terms of magnitude.</returns>
		public int CompareTo(Vector vector) {
			if (this < vector) {
				return -1;
			}
			if (this > vector) {
				return 1;
			}
			return 0;
		}

		#endregion

		#region Copyable

		/// <summary>
		/// Copies the current vector using the copy constructor.
		/// </summary>
		/// <returns>The copy of the vector</returns>
		public Vector Copy() {
			return new Vector(this);
		}

		#endregion

		#region ToString

		/// <summary>
		/// Provides a formattable ToString implementation. A format string and a format
		/// provide can be defined. Format strings can be any of the following, optionally
		/// appended by a number format string to be used:
		/// <list type="bullet">
		/// <item><term>G</term><description>A General format string, e.g. (1.2, 3.4, 5.6)</description></item>
		/// <item><term>X</term><description>An X component string, e.g. 1.2</description></item>
		/// <item><term>Y</term><description>A Y component string, e.g. 3.4</description></item>
		/// <item><term>Z</term><description>A Z component string, e.g. 5.6</description></item>
		/// <item><term>L</term><description>A Long format string, e.g. (x=1.2, y=3.4, z=5.6)</description></item>
		/// <item><term>A</term><description>A Array format string, e.g. [1.2, 3.4, 5.6]</description></item>
		/// </list>
		/// </summary>
		/// <param name="format">The format string to use</param>
		/// <param name="formatProvider">The format provider to use</param>
		/// <returns>The formatted string</returns>
		public string ToString(string format, IFormatProvider formatProvider) {
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}
			if (string.IsNullOrEmpty(format)) {
				format = "G";
			}
			string valFormat = "G";
			if (format.Length > 1) {
				valFormat = format.Substring(1);
				format = format.Substring(0, 1);
			}
			switch (format) {
				case "X":
					return XValue.ToString(valFormat, formatProvider);
				case "Y":
					return YValue.ToString(valFormat, formatProvider);
				case "Z":
					return ZValue.ToString(valFormat, formatProvider);
				case "L":
					return Format("(x={0}, y={1}, z={2})", valFormat, formatProvider);
				case "A":
					return Format("[{0}, {1}, {2}]", valFormat, formatProvider);
				default: // "G"
					return Format("({0}, {1}, {2})", valFormat, formatProvider);
			}
		}
		
		private string Format(string formatStr, string valueFormat, IFormatProvider formatProvider) {
			return String.Format(formatStr,
				XValue.ToString(valueFormat, formatProvider),
				YValue.ToString(valueFormat, formatProvider),
				ZValue.ToString(valueFormat, formatProvider));
		}

		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		#endregion
	}
}
