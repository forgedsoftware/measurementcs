using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using ForgedSoftware.Measurement.Interfaces;

namespace ForgedSoftware.Measurement.Number {

	/// <summary>
	/// A class describing a standard two dimensional vector and associate math, providing a set of
	/// vector specific functionality including dot product, angle, and magnitude.
	/// TODO - Test me!
	/// </summary>
	[DataContract]
	public struct Vector2 : INumber<Vector2>, IFormattable, IEquatable<Vector2>,
		IComparable, IComparable<Vector2>, ICloneable<Vector2>, IVector<Vector2> {

		private const int NUM_AXIS = 2;

		/// <summary>
		/// This is a reasonable epsilon for vector comparisons and equitability.
		/// </summary>
		public static readonly double EquatableEpsilon = 1E-15;

		#region Standard Vectors

		/// <summary>
		/// A static vector describing an origin vector.
		/// </summary>
		public static readonly Vector2 Origin = new Vector2(0, 0);

		/// <summary>
		/// A static vector describing a unit vector along the x-axis.
		/// </summary>
		public static readonly Vector2 XAxis = new Vector2(1, 0);

		/// <summary>
		/// A static vector describing a unit vector along the y-axis.
		/// </summary>
		public static readonly Vector2 YAxis = new Vector2(0, 1);

		/// <summary>
		/// A static vector describing the min value of on each axis.
		/// </summary>
		public static readonly Vector2 MinValue =
			new Vector2(Double.MinValue, Double.MinValue);

		/// <summary>
		/// A static vector describing the max value of on each axis.
		/// </summary>
		public static readonly Vector2 MaxValue =
			new Vector2(Double.MaxValue, Double.MaxValue);

		/// <summary>
		/// A static vector describing the standard double epsilon value of on each axis.
		/// </summary>
		public static readonly Vector2 Epsilon =
			new Vector2(Double.Epsilon, Double.Epsilon);

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor
		/// </summary>
		/// <param name="x">The x axis value</param>
		/// <param name="y">The y axis value</param>
		public Vector2(double x, double y)
				: this() {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Array constructor
		/// </summary>
		/// <param name="values">The vector as an array of values</param>
		public Vector2(double[] values)
				: this() {
			Array = values;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Vector2(Vector2 vector)
				: this() {
			X = vector.X;
			Y = vector.Y;
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
		/// A array representation of the two axis of the vector.
		/// </summary>
		public double[] Array {
			get { return new[] { X, Y }; }
			private set {
				if (value.Length != NUM_AXIS) {
					throw new ArgumentException("An array needs to contain 2 values to be a valid vector");
				}
				X = value[0];
				Y = value[1];
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
			get { return X + Y; }
		}

		public double DotProduct(Vector2 vector) {
			return (X * vector.X + Y * vector.Y);
		}

		public bool IsUnitVector() {
			return (Math.Abs(Magnitude - 1) < EquatableEpsilon);
		}

		public double Angle(Vector2 vector) {
			return Math.Acos(Normalize.DotProduct(vector.Normalize));
		}

		public Vector2 Normalize {
			get {
				if (Math.Abs(Magnitude) < EquatableEpsilon) {
					throw new DivideByZeroException("A vector must have a magnitude of greater than 0 to normalize");
				}
				double inverse = 1 / Magnitude;
				return Multiply(inverse);
			}
		}

		public double Distance(Vector2 vector) {
			return Math.Sqrt(
				(X - vector.X) * (X - vector.X) +
				(Y - vector.Y) * (Y - vector.Y));
		}

		public bool IsPerpendicular(Vector2 vector) {
			return Math.Abs(DotProduct(vector)) < EquatableEpsilon;
		}

		#endregion

		#region Extended Math

		public Vector2 Abs() {
			return FuncOneParam(Math.Abs);
		}

		public Vector2 Ceiling() {
			return FuncOneParam(Math.Ceiling);
		}

		public Vector2 Floor() {
			return FuncOneParam(Math.Floor);
		}

		public Vector2 Pow(double power) {
			return FuncTwoParam(Math.Pow, power);
		}

		public Vector2 Round() {
			return FuncOneParam(Math.Round);
		}

		public Vector2 Sqrt() {
			return FuncOneParam(Math.Sqrt);
		}

		public Vector2 Max(Vector2 vector) {
			return (this >= vector) ? this : vector;
		}

		public Vector2 Max(params Vector2[] values) {
			var vectors = new List<Vector2>(values) { this };
			return vectors.Max();
		}

		public Vector2 Min(Vector2 vector) {
			return (this <= vector) ? this : vector;
		}

		public Vector2 Min(params Vector2[] values) {
			var vectors = new List<Vector2>(values) { this };
			return vectors.Min();
		}

		/// <summary>
		/// Helper method to apply System.Math functions with one parameter.
		/// </summary>
		private Vector2 FuncOneParam(Func<double, double> func) {
			return new Vector2(func(X), func(Y));
		}

		/// <summary>
		/// Helper method to apply System.Math functions with two parameters.
		/// </summary>
		private Vector2 FuncTwoParam(Func<double, double, double> func, double param) {
			return new Vector2(func(X, param), func(Y, param));
		}

		#endregion

		#region Basic Math

		public Vector2 Add(Vector2 add) {
			return new Vector2(X + add.X, Y + add.Y);
		}

		public Vector2 Subtract(Vector2 subtract) {
			return new Vector2(X - subtract.X, Y - subtract.Y);
		}

		/// <summary>
		/// Multiplying two 2D vectors to return another is not a defined operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector2 Multiply(Vector2 multiply) {
			throw new InvalidOperationException("This operation is not implemented as it does not make sense for 2D vectors");
		}

		/// <summary>
		/// Division of vectors is not a well defined mathematical operation.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector2 Divide(Vector2 divide) {
			throw new InvalidOperationException("This operation is not implemented for vectors as it is mathematically not well defined");
		}

		/// <summary>
		/// Addition of a scalar value to a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector2 Add(double add) {
			throw new InvalidOperationException("Adding a scalar value to a vector is not valid");
		}

		/// <summary>
		/// Subtraction of a scalar value from a vector is not mathematically defined.
		/// This method is provided for completeness but will throw an <exception cref="InvalidOperationException" />
		/// </summary>
		public Vector2 Subtract(double subtract) {
			throw new InvalidOperationException("Subtracting a scalar value from a vector is not valid");
		}

		public Vector2 Multiply(double multiply) {
			return new Vector2(X * multiply, Y * multiply);
		}

		public Vector2 Divide(double divide) {
			return new Vector2(X / divide, Y / divide);
		}
		
		public Vector2 Negate() {
			return new Vector2(-X, -Y);
		}

		#endregion

		#region Operator Overrides

		/// <summary>
		/// Overrides the + (addition) operator with a standard adding of vectors.
		/// </summary>
		/// <seealso cref="Add(Vector2)"/>
		public static Vector2 operator +(Vector2 vector1, Vector2 vector2) {
			return vector1.Add(vector2);
		}

		/// <summary>
		/// Overrides the - (subtraction) operator with a standard subtraction of vectors.
		/// </summary>
		/// <seealso cref="Subtract(Vector2)"/>
		public static Vector2 operator -(Vector2 vector1, Vector2 vector2) {
			return vector1.Subtract(vector2);
		}

		/// <summary>
		/// Overrides the - (negation) operator with a vector negation
		/// </summary>
		/// <seealso cref="Negate()"/>
		public static Vector2 operator -(Vector2 vector1) {
			return vector1.Negate();
		}

		/// <summary>
		/// Overrides the + (reinforcement) operator by returning a copy of the vector
		/// </summary>
		/// <seealso cref="Copy()"/>
		public static Vector2 operator +(Vector2 vector1) {
			return vector1.Clone();
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a vector and a scalar
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector2 operator *(Vector2 vector1, double d2) {
			return vector1.Multiply(d2);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator between a scalar and a vector
		/// as a scalar multiply.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Vector2 operator *(double d1, Vector2 vector2) {
			// Commutable
			return vector2.Multiply(d1);
		}

		/// <summary>
		/// Overrides the / (division) operator between a vector and a scalar
		/// as a scalar division.
		/// </summary>
		/// <seealso cref="Divide(double)"/>
		public static Vector2 operator /(Vector2 vector1, double d2) {
			return vector1.Divide(d2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a scalar and a vector
		/// as a scalar division.
		/// </summary>
		public static Vector2 operator /(double d1, Vector2 vector2) {
			return new Vector2(d1 / vector2.X, d1 / vector2.Y);
		}

		/// <summary>
		/// Overrides the &lt; (less than) operator as the test of whether the
		/// magnitude of the first vector is less than that of the second.
		/// </summary>
		public static bool operator <(Vector2 vector1, Vector2 vector2) {
			return vector1.Magnitude < vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &lt;= (less than or equal) operator as the test of whether the
		/// magnitude of the first vector is less than or equal to that of the second.
		/// </summary>
		public static bool operator <=(Vector2 vector1, Vector2 vector2) {
			return vector1.Magnitude <= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt; (greater than) operator as the test of whether the
		/// magnitude of the first vector is greater than that of the second.
		/// </summary>
		public static bool operator >(Vector2 vector1, Vector2 vector2) {
			return vector1.Magnitude > vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the &gt;= (greater than or equal) operator as the test of whether the
		/// magnitude of the first vector is greater than or equal to that of the second.
		/// </summary>
		public static bool operator >=(Vector2 vector1, Vector2 vector2) {
			return vector1.Magnitude >= vector2.Magnitude;
		}

		/// <summary>
		/// Overrides the == (equality) operator to test whether the vectors are equivalent
		/// based on whether the component values are the same (epsilon test).
		/// Comparing two null values also returns true.
		/// </summary>
		public static bool operator ==(Vector2 vector1, Vector2 vector2) {
			if (((object) vector1 == null) && ((object) vector2 == null)) {
				return true;
			}
			return (((object) vector1 != null) && ((object) vector2 != null) &&
				(Math.Abs(vector1.X - vector2.X) <= EquatableEpsilon) &&
				(Math.Abs(vector1.Y - vector2.Y) <= EquatableEpsilon));
		}

		/// <summary>
		/// Overrides the != (inequality) operator to test whether the vectors
		/// are not equivalent. It is a direct inverse of the equality operator.
		/// </summary>
		/// <seealso cref="operator ==(Vector2, Vector2)"/>
		public static bool operator !=(Vector2 vector1, Vector2 vector2) {
			return !(vector1 == vector2);
		}

		#endregion

		#region Comparing

		/// <summary>
		/// An override of the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector2, Vector2)"/>
		public override bool Equals(object obj) {
			if (obj is Vector2) {
				return this == (Vector2)obj;
			}
			return false;
		}

		/// <summary>
		/// Implements the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Vector2, Vector2)"/>
		public bool Equals(Vector2 vector) {
			return this == vector;
		}

		/// <summary>
		/// A standard hash code implementation, hashing the two components.
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				int hash = (int)2166136261;
				hash = hash * 16777619 ^ X.GetHashCode();
				hash = hash * 16777619 ^ Y.GetHashCode();
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
			if (obj is Vector2) {
				return CompareTo((Vector2)obj);
			}
			throw new ArgumentException("Cannot compare a Vector with a " + obj.GetType());
		}

		/// <summary>
		/// Direct comparison between two vectors.
		/// </summary>
		/// <param name="vector">The vector to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other vector, 0 if they
		/// are the same, and 1 if this vector is larger in terms of magnitude.</returns>
		public int CompareTo(Vector2 vector) {
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
		public Vector2 Clone() {
			return new Vector2(this);
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
		/// <item><term>G</term><description>A General format string, e.g. (1.2, 3.4)</description></item>
		/// <item><term>X</term><description>An X component string, e.g. 1.2</description></item>
		/// <item><term>Y</term><description>A Y component string, e.g. 3.4</description></item>
		/// <item><term>L</term><description>A Long format string, e.g. (x=1.2, y=3.4)</description></item>
		/// <item><term>A</term><description>A Array format string, e.g. [1.2, 3.4]</description></item>
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
				case "L":
					return Format("(x={0}, y={1})", valFormat, formatProvider);
				case "A":
					return Format("[{0}, {1}]", valFormat, formatProvider);
				default: // "G"
					return Format("({0}, {1})", valFormat, formatProvider);
			}
		}
		
		private string Format(string formatStr, string valueFormat, IFormatProvider formatProvider) {
			return String.Format(formatStr,
				X.ToString(valueFormat, formatProvider),
				Y.ToString(valueFormat, formatProvider));
		}

		#endregion

	}
}
