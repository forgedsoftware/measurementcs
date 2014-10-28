using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement.Number {
	
	/// <summary>
	/// A class describing a standard four dimensional vector and associate math, providing a set of
	/// vector specific functionality including dot product, angle, magnitude.
	/// TODO - Test me!
	/// </summary>
	[DataContract]
	public struct Vector4 : INumber<Vector4>, IFormattable, IEquatable<Vector4>,
		IComparable, IComparable<Vector4>, ICopyable<Vector4>, IVector<Vector4> {

		private const int NUM_AXIS = 4;

		/// <summary>
		/// This is a reasonable epsilon for vector comparisons and equitability.
		/// </summary>
		public static readonly double EquatableEpsilon = 1E-15;

		#region Standard Vectors

		/// <summary>
		/// A static vector describing an origin vector.
		/// </summary>
		public static readonly Vector4 Origin = new Vector4(0, 0, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the x-axis.
		/// </summary>
		public static readonly Vector4 XAxis = new Vector4(1, 0, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the y-axis.
		/// </summary>
		public static readonly Vector4 YAxis = new Vector4(0, 1, 0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the z-axis.
		/// </summary>
		public static readonly Vector4 ZAxis = new Vector4(0, 0, 1, 0);

		/// <summary>
		/// A static vector describing a unit vector along the t-axis.
		/// </summary>
		public static readonly Vector4 TAxis = new Vector4(0, 0, 0, 1);

		/// <summary>
		/// A static vector describing the min value of on each axis.
		/// </summary>
		public static readonly Vector4 MinValue =
			new Vector4(Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue);

		/// <summary>
		/// A static vector describing the max value of on each axis.
		/// </summary>
		public static readonly Vector4 MaxValue =
			new Vector4(Double.MaxValue, Double.MaxValue, Double.MaxValue, Double.MaxValue);

		/// <summary>
		/// A static vector describing the standard double epsilon value of on each axis.
		/// </summary>
		public static readonly Vector4 Epsilon =
			new Vector4(Double.Epsilon, Double.Epsilon, Double.Epsilon, Double.Epsilon);

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor
		/// </summary>
		/// <param name="x">The x axis value</param>
		/// <param name="y">The y axis value</param>
		/// <param name="z">The z axis value</param>
		/// <param name="t">The t axis value</param>
		public Vector4(double x, double y, double z, double t)
				: this() {
			X = x;
			Y = y;
			Z = z;
			T = t;
		}

		/// <summary>
		/// Array constructor
		/// </summary>
		/// <param name="values">The vector as an array of values</param>
		public Vector4(double[] values)
				: this() {
			Array = values;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Vector4(Vector4 vector)
				: this() {
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			T = vector.T;
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
		/// A value representing the magnitude of the vector with respect to the t-axis.
		/// </summary>
		[DataMember(Name = "t")]
		public double T { get; private set; }

		/// <summary>
		/// A array representation of the three axis of the vector.
		/// </summary>
		public double[] Array {
			get { return new[] { X, Y, Z, T }; }
			private set {
				if (value.Length != NUM_AXIS) {
					throw new ArgumentException("An array needs to contain 4 values to be a valid vector");
				}
				X = value[0];
				Y = value[1];
				Z = value[2];
				T = value[3];
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
			get { return X + Y + Z + T; }
		}

		public double DotProduct(Vector4 vector) {
			return (X * vector.X + Y * vector.Y + Z * vector.Z + T * vector.T);
		}

		public bool IsUnitVector() {
			return (Math.Abs(Magnitude - 1) < EquatableEpsilon);
		}

		public double Angle(Vector4 vector) {
			return Math.Acos(Normalize.DotProduct(vector.Normalize));
		}

		public Vector4 Normalize {
			get {
				if (Math.Abs(Magnitude) < EquatableEpsilon) {
					throw new DivideByZeroException("A vector must have a magnitude of greater than 0 to normalize");
				}
				double inverse = 1 / Magnitude;
				return Multiply(inverse);
			}
		}

		public double Distance(Vector4 vector) {
			return Math.Sqrt(
				(X - vector.X) * (X - vector.X) +
				(Y - vector.Y) * (Y - vector.Y) +
				(Z - vector.Z) * (Z - vector.Z) +
				(T - vector.T) * (T - vector.T));
		}

		public bool IsPerpendicular(Vector4 vector) {
			return Math.Abs(DotProduct(vector)) < EquatableEpsilon;
		}

		#endregion

		#region Extended Math

		public Vector4 Abs() {
			return FuncOneParam(Math.Abs);
		}

		public Vector4 Ceiling() {
			return FuncOneParam(Math.Ceiling);
		}

		public Vector4 Floor() {
			return FuncOneParam(Math.Floor);
		}

		public Vector4 Pow(double power) {
			return FuncTwoParam(Math.Pow, power);
		}

		public Vector4 Round() {
			return FuncOneParam(Math.Round);
		}

		public Vector4 Sqrt() {
			return FuncOneParam(Math.Sqrt);
		}

		/// <summary>
		/// The larger of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The larger of the two vectors</returns>
		public Vector4 Max(Vector4 vector) {
			return (this >= vector) ? this : vector;
		}

		public Vector4 Max(params Vector4[] values) {
			var vectors = new List<Vector4>(values) { this };
			return vectors.Max();
		}

		/// <summary>
		/// The smaller of two vectors in terms of magnitude. The first vector is preferred
		/// if they are even.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The smaller of the two vectors</returns>
		public Vector4 Min(Vector4 vector) {
			return (this <= vector) ? this : vector;
		}

		public Vector4 Min(params Vector4[] values) {
			var vectors = new List<Vector4>(values) { this };
			return vectors.Min();
		}

		/// <summary>
		/// Helper method to apply System.Math functions with one parameter.
		/// </summary>
		private Vector4 FuncOneParam(Func<double, double> func) {
			return new Vector4(func(X), func(Y), func(Z), func(T));
		}

		/// <summary>
		/// Helper method to apply System.Math functions with two parameters.
		/// </summary>
		private Vector4 FuncTwoParam(Func<double, double, double> func, double param) {
			return new Vector4(func(X, param), func(Y, param), func(Z, param), func(T, param));
		}

		#endregion

		#region Basic Math

		/// <summary>
		/// Adds two vectors together in a component wise fashion.
		/// </summary>
		/// <param name="add">The vector to add</param>
		/// <returns>The summed vector</returns>
		public Vector4 Add(Vector4 add) {
			return new Vector4(X + add.X, Y + add.Y, Z + add.Z, T + add.T);
		}

		/// <summary>
		/// Subtracts two vectors from each other in a component wise fashion
		/// </summary>
		/// <param name="subtract">The vector to subtract</param>
		/// <returns>The vector of the difference</returns>
		public Vector4 Subtract(Vector4 subtract) {
			return new Vector4(X - subtract.X, Y - subtract.Y, Z - subtract.Z, T - subtract.T);
		}

		/// <summary>
		/// Multiplying two 4D vectors to return another is not a defined operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector4 Multiply(Vector4 multiply) {
			throw new InvalidOperationException("This operation is not implemented as it does not make sense for 4D vectors");
		}

		/// <summary>
		/// Division of vectors is not a well defined mathematical operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector4 Divide(Vector4 divide) {
			throw new InvalidOperationException("This operation is not implemented for vectors as it is mathematically not well defined");
		}

		/// <summary>
		/// Addition of a scalar value to a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector4 Add(double add) {
			throw new InvalidOperationException("Adding a scalar value to a vector is not valid");
		}

		/// <summary>
		/// Subtraction of a scalar value from a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector4 Subtract(double subtract) {
			throw new InvalidOperationException("Subtracting a scalar value from a vector is not valid");
		}

		/// <summary>
		/// Scalar multiplication of a vector with a value.
		/// </summary>
		/// <param name="multiply">The scalar value to multiply by</param>
		/// <returns>The multiplied vector</returns>
		public Vector4 Multiply(double multiply) {
			return new Vector4(X * multiply, Y * multiply, Z * multiply, T * multiply);
		}

		/// <summary>
		/// Scalar division of a vector by a value.
		/// </summary>
		/// <param name="divide">The scalar value to divide by</param>
		/// <returns>The divided vector</returns>
		public Vector4 Divide(double divide) {
			return new Vector4(X / divide, Y / divide, Z / divide, T / divide);
		}
		
		/// <summary>
		/// Returns the negation of a vector which is merely the negation of
		/// each component of the vector.
		/// </summary>
		/// <returns>The negated vector</returns>
		public Vector4 Negate() {
			return new Vector4(-X, -Y, -Z, -T);
		}

		#endregion

		#region Operator Overrides

		/// <summary>
		/// Overrides the + (addition) operator with a standard adding of vectors.
		/// </summary>
		/// <seealso cref="Add(Vector4)"/>
		public static Vector4 operator +(Vector4 vector1, Vector4 vector2) {
			return vector1.Add(vector2);
		}

		/// <summary>
		/// Overrides the - (subtraction) operator with a standard subtraction of vectors.
		/// </summary>
		/// <seealso cref="Subtract(Vector4)"/>
		public static Vector4 operator -(Vector4 vector1, Vector4 vector2) {
			return vector1.Subtract(vector2);
		}

		/// <summary>
		/// Overrides the - (negation) operator with a vector negation
		/// </summary>
		/// <seealso cref="Negate()"/>
		public static Vector4 operator -(Vector4 vector1) {
			return vector1.Negate();
		}

		/// <summary>
		/// Overrides the + (reinforcement) operator by returning a copy of the vector
		/// </summary>
		/// <seealso cref="Copy()"/>
		public static Vector4 operator +(Vector4 vector1) {
			return vector1.Copy();
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a vector and a scalar
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector4 operator *(Vector4 vector1, double d2) {
			return vector1.Multiply(d2);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a scalar and a vector
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector4 operator *(double d1, Vector4 vector2) {
			// Commutable
			return vector2.Multiply(d1);
		}

		/// <summary>
		/// Overrides the / (division) operator between a vector and a scalar
		/// as a scalar division.
		/// </summary>
		/// <seealso cref="Divide(double)"/>
		public static Vector4 operator /(Vector4 vector1, double d2) {
			return vector1.Divide(d2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a scalar and a vector
		/// as a scalar division.
		/// </summary>
		public static Vector4 operator /(double d1, Vector4 vector2) {
			return new Vector4(d1 / vector2.X, d1 / vector2.Y, d1 / vector2.Z, d1 / vector2.T);
		}

		/// <summary>
		/// Overrides the &lt; (less than) operator as the test of whether the
		/// magnitude of the first vector is less than that of the second.
		/// </summary>
		public static bool operator <(Vector4 vector1, Vector4 vector2) {
			return vector1.Magnitude < vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &lt;= (less than or equal) operator as the test of whether the
		/// magnitude of the first vector is less than or equal to that of the second.
		/// </summary>
		public static bool operator <=(Vector4 vector1, Vector4 vector2) {
			return vector1.Magnitude <= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt; (greater than) operator as the test of whether the
		/// magnitude of the first vector is greater than that of the second.
		/// </summary>
		public static bool operator >(Vector4 vector1, Vector4 vector2) {
			return vector1.Magnitude > vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt;= (greater than or equal) operator as the test of whether the
		/// magnitude of the first vector is greater than or equal to that of the second.
		/// </summary>
		public static bool operator >=(Vector4 vector1, Vector4 vector2) {
			return vector1.Magnitude >= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the == (equality) operator to test whether the vectors are equivalent
		/// based on whether the component values are the same (epsilon test).
		/// Comparing two null values also returns true.
		/// </summary>
		public static bool operator ==(Vector4 vector1, Vector4 vector2) {
			if (((object) vector1 == null) && ((object) vector2 == null)) {
				return true;
			}
			return (((object) vector1 != null) && ((object) vector2 != null) &&
				(Math.Abs(vector1.X - vector2.X) <= EquatableEpsilon) &&
				(Math.Abs(vector1.Y - vector2.Y) <= EquatableEpsilon) &&
				(Math.Abs(vector1.Z - vector2.Z) <= EquatableEpsilon) &&
				(Math.Abs(vector1.T - vector2.T) <= EquatableEpsilon));
		}

		/// <summary>
		/// Overrides the != (inequality) operator to test whether the vectors
		/// are not equivalent. It is a direct inverse of the equality operator.
		/// </summary>
		/// <seealso cref="operator ==(Vector4, Vector4)"/>
		public static bool operator !=(Vector4 vector1, Vector4 vector2) {
			return !(vector1 == vector2);
		}

		#endregion

		#region Comparing

		/// <summary>
		/// An override of the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector4, Vector4)"/>
		public override bool Equals(object obj) {
			if (obj is Vector4) {
				return this == (Vector4)obj;
			}
			return false;
		}

		/// <summary>
		/// Implements the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector4, Vector4)"/>
		public bool Equals(Vector4 vector) {
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
				hash = hash * 16777619 ^ T.GetHashCode();
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
			if (obj is Vector4) {
				return CompareTo((Vector4)obj);
			}
			throw new ArgumentException("Cannot compare a Vector with a " + obj.GetType());
		}

		/// <summary>
		/// Direct comparison between two vectors.
		/// </summary>
		/// <param name="vector">The vector to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other vector, 0 if they
		/// are the same, and 1 if this vector is larger in terms of magnitude.</returns>
		public int CompareTo(Vector4 vector) {
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
		public Vector4 Copy() {
			return new Vector4(this);
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
		/// <item><term>G</term><description>A General format string, e.g. (1.2, 3.4, 5.6, 0.3)</description></item>
		/// <item><term>X</term><description>An X component string, e.g. 1.2</description></item>
		/// <item><term>Y</term><description>A Y component string, e.g. 3.4</description></item>
		/// <item><term>Z</term><description>A Z component string, e.g. 5.6</description></item>
		/// <item><term>T</term><description>A Z component string, e.g. 0.3</description></item>
		/// <item><term>L</term><description>A Long format string, e.g. (x=1.2, y=3.4, z=5.6, t=0.3)</description></item>
		/// <item><term>A</term><description>A Array format string, e.g. [1.2, 3.4, 5.6, 0.3]</description></item>
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
				case "T":
					return T.ToString(valFormat, formatProvider);
				case "L":
					return Format("(x={0}, y={1}, z={2}, t={3})", valFormat, formatProvider);
				case "A":
					return Format("[{0}, {1}, {2}, {3}]", valFormat, formatProvider);
				default: // "G"
					return Format("({0}, {1}, {2}, {3})", valFormat, formatProvider);
			}
		}
		
		private string Format(string formatStr, string valueFormat, IFormatProvider formatProvider) {
			return String.Format(formatStr,
				X.ToString(valueFormat, formatProvider),
				Y.ToString(valueFormat, formatProvider),
				Z.ToString(valueFormat, formatProvider),
				T.ToString(valueFormat, formatProvider));
		}

		#endregion

	}
}
