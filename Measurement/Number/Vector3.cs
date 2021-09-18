using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using ForgedSoftware.Measurement.Interfaces;

namespace ForgedSoftware.Measurement.Number {

	/// <summary>
	/// A class describing a standard three dimensional vector and associate math, providing a set of
	/// vector specific functionality including dot product, cross product, angle, magnitude.
	/// TODO - Maybe create a 4D form...
	/// </summary>
	[DataContract]
	public struct Vector3 : INumber<Vector3>, IEquatable<Vector3>,
		IComparable, IComparable<Vector3>, ICloneable<Vector3>, IVector<Vector3> {

		private const int NUM_AXIS = 3;

		/// <summary>
		/// This is a reasonable epsilon for vector comparisons and equitability.
		/// </summary>
		public static readonly double EquatableEpsilon = 1E-15;

		#region Standard Vectors

		/// <summary>
		/// A static vector describing an origin vector.
		/// </summary>
		public static readonly Vector3 Origin = new Vector3(0, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the x-axis.
		/// </summary>
		public static readonly Vector3 XAxis = new Vector3(1, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the y-axis.
		/// </summary>
		public static readonly Vector3 YAxis = new Vector3(0, 1, 0);

		/// <summary>
		/// A static vector describing a unit vector along the z-axis.
		/// </summary>
		public static readonly Vector3 ZAxis = new Vector3(0, 0, 1);

		/// <summary>
		/// A static vector describing the min value of on each axis.
		/// </summary>
		public static readonly Vector3 MinValue =
			new Vector3(Double.MinValue, Double.MinValue, Double.MinValue);

		/// <summary>
		/// A static vector describing the max value of on each axis.
		/// </summary>
		public static readonly Vector3 MaxValue =
			new Vector3(Double.MaxValue, Double.MaxValue, Double.MaxValue);

		/// <summary>
		/// A static vector describing the standard double epsilon value of on each axis.
		/// </summary>
		public static readonly Vector3 Epsilon =
			new Vector3(Double.Epsilon, Double.Epsilon, Double.Epsilon);

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor
		/// </summary>
		/// <param name="x">The x axis value</param>
		/// <param name="y">The y axis value</param>
		/// <param name="z">The z axis value</param>
		public Vector3(double x, double y, double z)
				: this() {
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Array constructor
		/// </summary>
		/// <param name="values">The vector as an array of values</param>
		public Vector3(double[] values)
				: this() {
			Array = values;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Vector3(Vector3 vector)
				: this() {
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
		}

		#endregion

		#region Properties

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the x-axis.
		/// </summary>
		[DataMember(Name = "x")]
		public double X { get; private set; }

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the y-axis.
		/// </summary>
		[DataMember(Name = "y")]
		public double Y { get; private set; }

		/// <summary>
		/// A value representing the magnitude of the vector with respect to the z-axis.
		/// </summary>
		[DataMember(Name = "z")]
		public double Z { get; private set; }

		/// <summary>
		/// A array representation of the three axis of the vector.
		/// </summary>
		public double[] Array {
			get { return new[] { X, Y, Z }; }
			private set {
				if (value.Length != NUM_AXIS) {
					throw new ArgumentException("An array needs to contain 3 values to be a valid vector");
				}
				X = value[0];
				Y = value[1];
				Z = value[2];
			}
		}

		/// <summary>
		/// An approximate equivalent value of the vector as a double.
		/// In this case we use the magnitude.
		/// </summary>
		public double EquivalentValue {
			get { return Magnitude; }
		}

		#endregion

		#region Vector Functionality

		public double Magnitude {
			get { return Math.Sqrt(Pow(2).SumComponents); }
		}

		public double SumComponents {
			get { return X + Y + Z; }
		}

		public double DotProduct(Vector3 vector) {
			return (X * vector.X + Y * vector.Y + Z * vector.Z);
		}

		/// <summary>
		/// Returns the cross product of two vectors. This is a non-commutable operation.
		/// The cross product is a vector perpendicular to the other two vectors, uses the Right Hand Rule,
		/// and has a magnitude equal to the area of the parallelogram the vectors span.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The cross product of the two vectors</returns>
		public Vector3 CrossProduct(Vector3 vector) {
			// Non-commutable
			return new Vector3(
				Y * vector.Z - Z * vector.Y,
				Z * vector.X - X * vector.Z,
				X * vector.Y - Y * vector.X);
		}

		public bool IsUnitVector() {
			return (Math.Abs(Magnitude - 1) < EquatableEpsilon);
		}

		public double Angle(Vector3 vector) {
			return Math.Acos(Normalize().DotProduct(vector.Normalize()));
		}

		public Vector3 Normalize()
		{
			if (Math.Abs(Magnitude) < EquatableEpsilon)
			{
				throw new DivideByZeroException("A vector must have a magnitude of greater than 0 to normalize");
			}

			double inverse = 1 / Magnitude;
			return Multiply(inverse);
		}

		public double Distance(Vector3 vector) {
			return Math.Sqrt(
				(X - vector.X) * (X - vector.X) +
				(Y - vector.Y) * (Y - vector.Y) +
				(Z - vector.Z) * (Z - vector.Z));
		}

		public bool IsPerpendicular(Vector3 vector) {
			return Math.Abs(DotProduct(vector)) < EquatableEpsilon;
		}

		#endregion

		#region Extended Math

		public Vector3 Abs() {
			return FuncOneParam(Math.Abs);
		}

		public Vector3 Ceiling() {
			return FuncOneParam(Math.Ceiling);
		}

		public Vector3 Floor() {
			return FuncOneParam(Math.Floor);
		}

		public Vector3 Pow(double power) {
			return FuncTwoParam(Math.Pow, power);
		}

		public Vector3 Round() {
			return FuncOneParam(Math.Round);
		}

		public Vector3 Sqrt() {
			return FuncOneParam(Math.Sqrt);
		}

		/// <summary>
		/// The larger of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The larger of the two vectors</returns>
		public Vector3 Max(Vector3 vector) {
			return (this >= vector) ? this : vector;
		}

		public Vector3 Max(params Vector3[] values) {
			var vectors = new List<Vector3>(values) { this };
			return vectors.Max();
		}

		/// <summary>
		/// The smaller of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The smaller of the two vectors</returns>
		public Vector3 Min(Vector3 vector) {
			return (this <= vector) ? this : vector;
		}

		public Vector3 Min(params Vector3[] values) {
			var vectors = new List<Vector3>(values) { this };
			return vectors.Min();
		}

		/// <summary>
		/// Helper method to apply System.Math functions with one parameter.
		/// </summary>
		private Vector3 FuncOneParam(Func<double, double> func) {
			return new Vector3(func(X), func(Y), func(Z));
		}

		/// <summary>
		/// Helper method to apply System.Math functions with two parameters.
		/// </summary>
		private Vector3 FuncTwoParam(Func<double, double, double> func, double param) {
			return new Vector3(func(X, param), func(Y, param), func(Z, param));
		}

		#endregion

		#region Basic Math

		/// <summary>
		/// Adds two vectors together in a component wise fashion.
		/// </summary>
		/// <param name="add">The vector to add</param>
		/// <returns>The summed vector</returns>
		public Vector3 Add(Vector3 add) {
			return new Vector3(X + add.X, Y + add.Y, Z + add.Z);
		}

		/// <summary>
		/// Subtracts two vectors from each other in a component wise fashion
		/// </summary>
		/// <param name="subtract">The vector to subtract</param>
		/// <returns>The vector of the difference</returns>
		public Vector3 Subtract(Vector3 subtract) {
			return new Vector3(X - subtract.X, Y - subtract.Y, Z - subtract.Z);
		}

		/// <summary>
		/// Multiplies two vectors together with a cross product. This operation
		/// is non-commutable.
		/// </summary>
		/// <param name="multiply">The vector to multiply by</param>
		/// <returns>The cross product of the two vectors</returns>
		public Vector3 Multiply(Vector3 multiply) {
			return CrossProduct(multiply);
		}

		/// <summary>
		/// Division of vectors is not a well defined mathematical operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector3 Divide(Vector3 divide) {
			throw new InvalidOperationException("This operation is not implemented for vectors as it is mathematically not well defined");
		}

		/// <summary>
		/// Addition of a scalar value to a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector3 Add(double add) {
			throw new InvalidOperationException("Adding a scalar value to a vector is not valid");
		}

		/// <summary>
		/// Subtraction of a scalar value from a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector3 Subtract(double subtract) {
			throw new InvalidOperationException("Subtracting a scalar value from a vector is not valid");
		}

		/// <summary>
		/// Scalar multiplication of a vector with a value.
		/// </summary>
		/// <param name="multiply">The scalar value to multiply by</param>
		/// <returns>The multiplied vector</returns>
		public Vector3 Multiply(double multiply) {
			return new Vector3(X * multiply, Y * multiply, Z * multiply);
		}

		/// <summary>
		/// Scalar division of a vector by a value.
		/// </summary>
		/// <param name="divide">The scalar value to divide by</param>
		/// <returns>The divided vector</returns>
		public Vector3 Divide(double divide) {
			return new Vector3(X / divide, Y / divide, Z / divide);
		}
		
		/// <summary>
		/// Returns the negation of a vector which is merely the negation of
		/// each component of the vector.
		/// </summary>
		/// <returns>The negated vector</returns>
		public Vector3 Negate() {
			return new Vector3(-X, -Y, -Z);
		}

		#endregion

		#region Operator Overrides

		/// <summary>
		/// Overrides the + (addition) operator with a standard adding of vectors.
		/// </summary>
		/// <seealso cref="Add(Vector3)"/>
		public static Vector3 operator +(Vector3 vector1, Vector3 vector2) {
			return vector1.Add(vector2);
		}

		/// <summary>
		/// Overrides the - (subtraction) operator with a standard subtraction of vectors.
		/// </summary>
		/// <seealso cref="Subtract(Vector3)"/>
		public static Vector3 operator -(Vector3 vector1, Vector3 vector2) {
			return vector1.Subtract(vector2);
		}

		/// <summary>
		/// Overrides the - (negation) operator with a vector negation
		/// </summary>
		/// <seealso cref="Negate()"/>
		public static Vector3 operator -(Vector3 vector1) {
			return vector1.Negate();
		}

		/// <summary>
		/// Overrides the + (reinforcement) operator by returning a copy of the vector
		/// </summary>
		/// <seealso cref="Copy()"/>
		public static Vector3 operator +(Vector3 vector1) {
			return vector1.Clone();
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a vector and a scalar
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector3 operator *(Vector3 vector1, double d2) {
			return vector1.Multiply(d2);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a scalar and a vector
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector3 operator *(double d1, Vector3 vector2) {
			// Commutable
			return vector2.Multiply(d1);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between two vectors as the cross
		/// product of two vectors. Care should be taken here not to confuse the result
		/// with the scalar result of the dot product.
		/// </summary>
		/// <seealso cref="Multiply(Vector3)"/>
		/// <seealso cref="CrossProduct(Vector3)"/>
		public static Vector3 operator *(Vector3 vector1, Vector3 vector2) {
			return vector1.Multiply(vector2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a vector and a scalar
		/// as a scalar division.
		/// </summary>
		/// <seealso cref="Divide(double)"/>
		public static Vector3 operator /(Vector3 vector1, double d2) {
			return vector1.Divide(d2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a scalar and a vector
		/// as a scalar division.
		/// </summary>
		public static Vector3 operator /(double d1, Vector3 vector2) {
			return new Vector3(d1 / vector2.X, d1 / vector2.Y, d1 / vector2.Z);
		}

		/// <summary>
		/// Overrides the &lt; (less than) operator as the test of whether the
		/// magnitude of the first vector is less than that of the second.
		/// </summary>
		public static bool operator <(Vector3 vector1, Vector3 vector2) {
			return vector1.Magnitude < vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &lt;= (less than or equal) operator as the test of whether the
		/// magnitude of the first vector is less than or equal to that of the second.
		/// </summary>
		public static bool operator <=(Vector3 vector1, Vector3 vector2) {
			return vector1.Magnitude <= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt; (greater than) operator as the test of whether the
		/// magnitude of the first vector is greater than that of the second.
		/// </summary>
		public static bool operator >(Vector3 vector1, Vector3 vector2) {
			return vector1.Magnitude > vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt;= (greater than or equal) operator as the test of whether the
		/// magnitude of the first vector is greater than or equal to that of the second.
		/// </summary>
		public static bool operator >=(Vector3 vector1, Vector3 vector2) {
			return vector1.Magnitude >= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the == (equality) operator to test whether the vectors are equivalent
		/// based on whether the component values are the same (epsilon test).
		/// Comparing two null values also returns true.
		/// </summary>
		public static bool operator ==(Vector3 vector1, Vector3 vector2) {
			if (((object) vector1 == null) && ((object) vector2 == null)) {
				return true;
			}
			return (((object) vector1 != null) && ((object) vector2 != null) &&
				(Math.Abs(vector1.X - vector2.X) <= EquatableEpsilon) &&
				(Math.Abs(vector1.Y - vector2.Y) <= EquatableEpsilon) &&
				(Math.Abs(vector1.Z - vector2.Z) <= EquatableEpsilon));
		}

		/// <summary>
		/// Overrides the != (inequality) operator to test whether the vectors
		/// are not equivalent. It is a direct inverse of the equality operator.
		/// </summary>
		/// <seealso cref="operator ==(Vector3, Vector3)"/>
		public static bool operator !=(Vector3 vector1, Vector3 vector2) {
			return !(vector1 == vector2);
		}

		#endregion

		#region Comparing

		/// <summary>
		/// An override of the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector3, Vector3)"/>
		public override bool Equals(object obj) {
			if (obj is Vector3) {
				return this == (Vector3)obj;
			}
			return false;
		}

		/// <summary>
		/// Implements the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector3, Vector3)"/>
		public bool Equals(Vector3 vector) {
			return this == vector;
		}

		/// <summary>
		/// A standard hash code implementation, hashing the three components.
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				int hash = (int)2166136261;
				hash = hash * 16777619 ^ X.GetHashCode();
				hash = hash * 16777619 ^ Y.GetHashCode();
				hash = hash * 16777619 ^ Z.GetHashCode();
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
			if (obj is Vector3) {
				return CompareTo((Vector3)obj);
			}
			throw new ArgumentException("Cannot compare a Vector with a " + obj.GetType());
		}

		/// <summary>
		/// Direct comparison between two vectors.
		/// </summary>
		/// <param name="vector">The vector to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other vector, 0 if they
		/// are the same, and 1 if this vector is larger in terms of magnitude.</returns>
		public int CompareTo(Vector3 vector) {
			if (this < vector) {
				return -1;
			}
			if (this > vector) {
				return 1;
			}
			return 0;
		}

		#endregion

		#region Cloneable

		object ICloneable.Clone() {
			return Clone();
		}

		/// <summary>
		/// Copies the current vector using the copy constructor.
		/// </summary>
		/// <returns>The copy of the vector</returns>
		public Vector3 Clone() {
			return new Vector3(this);
		}

		#endregion

		#region ToString

		public override string ToString() {
			return ToString("G", CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Provides a formattable ToString implementation. A format string and a format
		/// provider can be defined. Format strings can be any of the following, optionally
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
					return X.ToString(valFormat, formatProvider);
				case "Y":
					return Y.ToString(valFormat, formatProvider);
				case "Z":
					return Z.ToString(valFormat, formatProvider);
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
				X.ToString(valueFormat, formatProvider),
				Y.ToString(valueFormat, formatProvider),
				Z.ToString(valueFormat, formatProvider));
		}

		#endregion

	}
}
