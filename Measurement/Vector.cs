using System;
using System.Globalization;

namespace ForgedSoftware.Measurement {
	
	public class Vector : INumber<Vector>, IMathFunctions<Vector>, IEquatable<Vector>, IComparable, IComparable<Vector> {

		#region Standard Vectors

		public static readonly Vector Origin = new Vector(0, 0, 0);

		public static readonly Vector XAxis = new Vector(1, 0, 0);

		public static readonly Vector YAxis = new Vector(0, 1, 0);

		public static readonly Vector ZAxis = new Vector(0, 0, 1);

		public static readonly Vector MinValue =
			new Vector(Double.MinValue, Double.MinValue, Double.MinValue);

		public static readonly Vector MaxValue =
			new Vector(Double.MaxValue, Double.MaxValue, Double.MaxValue);

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

		public double XValue { get; set; }
		public double YValue { get; set; }
		public double ZValue { get; set; }

		public double[] Array {
			get { return new[] {XValue, YValue, ZValue}; }
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

		public double Magnitude {
			get { return Math.Sqrt(Pow(2).SumComponents); }
		}

		public double SumComponents {
			get { return XValue + YValue + ZValue; }
		}

		public double DotProduct(Vector vector) {
			return (XValue * vector.XValue + YValue * vector.YValue + ZValue * vector.ZValue);
		}

		public Vector CrossProduct(Vector vector) {
			// Non-commutable
			return new Vector(
				YValue * vector.ZValue - ZValue * vector.YValue,
				ZValue * vector.XValue - XValue * vector.ZValue,
				XValue * vector.YValue - YValue * vector.XValue);
		}

		public bool IsUnitVector() {
			return (Math.Abs(Magnitude - 1) < double.Epsilon);
		}

		public double Angle(Vector vector) {
			return Math.Acos(Normalize.DotProduct(vector.Normalize));
		}

		public Vector Normalize {
			get {
				if (Math.Abs(Magnitude - 0) < double.Epsilon) {
					throw new DivideByZeroException("A vector must have a magnitude of greater than 0 to normalize");
				}
				double inverse = 1 / Magnitude;
				return Multiply(inverse);
			}
		}

		public double Distance(Vector vector) {
			return Math.Sqrt(
				(XValue - vector.XValue) * (XValue - vector.XValue) +
				(YValue - vector.YValue) * (YValue - vector.YValue) +
				(ZValue - vector.ZValue) * (ZValue - vector.ZValue));
		}

		public bool IsPerpendicular(Vector vector) {
			return Math.Abs(DotProduct(vector) - 0) < double.Epsilon;
		}

		#endregion

		#region IMathFunctions

		public Vector Pow(double power) {
			return new Vector(Math.Pow(XValue, power), Math.Pow(YValue, power), Math.Pow(ZValue, power));
		}

		public Vector Sqrt() {
			return ApplyFunc(Math.Sqrt);
		}

		public Vector Max(Vector vector) {
			return (this >= vector) ? this : vector;
		}

		public Vector Min(Vector vector) {
			return (this <= vector) ? this : vector;
		}

		private Vector ApplyFunc(Func<double, double> func) {
			return new Vector(func(XValue), func(YValue), func(ZValue));
		}

		#endregion

		#region Basic Math

		public Vector Add(Vector add) {
			return new Vector(XValue + add.XValue, YValue + add.YValue, ZValue + add.ZValue);
		}

		public Vector Subtract(Vector subtract) {
			return new Vector(XValue - subtract.XValue, YValue - subtract.YValue, ZValue - subtract.ZValue);
		}

		public Vector Multiply(Vector multiply) {
			return CrossProduct(multiply);
		}

		public Vector Divide(Vector divide) {
			throw new InvalidOperationException("This operation is not implemented for vectors as it is mathematically not well defined");
		}

		public Vector Add(double add) {
			throw new InvalidOperationException("Adding a scalar value to a vector is not valid");
		}

		public Vector Subtract(double subtract) {
			throw new InvalidOperationException("Subtracting a scalar value from a vector is not valid");
		}

		public Vector Multiply(double multiply) {
			// Scalar multiplication
			return new Vector(XValue * multiply, YValue * multiply, ZValue * multiply);
		}

		public Vector Divide(double divide) {
			// Scalar division
			return new Vector(XValue / divide, YValue / divide, ZValue / divide);
		}

		public Vector Negate() {
			return new Vector(-XValue, -YValue, -ZValue);
		}

		#endregion

		#region Operator Overrides

		public static Vector operator+(Vector vector1, Vector vector2) {
			return vector1.Add(vector2);
		}

		public static Vector operator-(Vector vector1, Vector vector2) {
			return vector1.Subtract(vector2);
		}

		public static Vector operator-(Vector vector1) {
			return vector1.Negate();
		}

		public static Vector operator+(Vector vector1) {
			return new Vector(vector1);
		}

		public static Vector operator*(Vector vector1, double d2) {
			return vector1.Multiply(d2);
		}

		public static Vector operator*(double d1, Vector vector2) {
			// Commutable
			return vector2.Multiply(d1);
		}

		public static Vector operator/(Vector vector1, double d2) {
			return vector1.Divide(d2);
		}

		public static Vector operator/(double d1, Vector vector2) {
			// Commutable
			return vector2.Divide(d1);
		}

		public static bool operator<(Vector vector1, Vector vector2) {
			return vector1.Magnitude < vector2.Magnitude;
		}

		public static bool operator<=(Vector vector1, Vector vector2) {
			return vector1.Magnitude <= vector2.Magnitude;
		}

		public static bool operator>(Vector vector1, Vector vector2) {
			return vector1.Magnitude > vector2.Magnitude;
		}

		public static bool operator>=(Vector vector1, Vector vector2) {
			return vector1.Magnitude >= vector2.Magnitude;
		}

		public static bool operator==(Vector vector1, Vector vector2) {
			return ((vector2 != null) && (vector1 != null) && 
				(Math.Abs(vector1.XValue - vector2.XValue) <= double.Epsilon) &&
				(Math.Abs(vector1.YValue - vector2.YValue) <= double.Epsilon) &&
				(Math.Abs(vector1.ZValue - vector2.ZValue) <= double.Epsilon));
		}

		public static bool operator!=(Vector vector1, Vector vector2) {
			return !(vector1 == vector2);
		}

		#endregion

		#region Comparing

		public override bool Equals(object obj) {
			Vector vector = obj as Vector;
			if (vector != null) {
				return this == vector;
			}
			return false;
		}

		public bool Equals(Vector vector) {
			return this == vector;
		}

		public override int GetHashCode() {
			unchecked {
				int hash = (int)2166136261;
				hash = hash * 16777619 ^ XValue.GetHashCode();
				hash = hash * 16777619 ^ YValue.GetHashCode();
				hash = hash * 16777619 ^ ZValue.GetHashCode();
				return hash;
			}
		}

		public int CompareTo(object obj) {
			Vector vector = obj as Vector;
			if (vector != null) {
				return CompareTo(vector);
			}
			throw new ArgumentException("Cannot compare a Vector with a " + obj.GetType());
		}

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

		#region ToString

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
