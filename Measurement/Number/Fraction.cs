using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement.Number {

	/// <summary>
	/// Provides a representation of a Fractional number to provide the functionality of representing the full set of Rational numbers.
	/// It automatically reduces and normalizes fractions after each operation.
	/// </summary>
	/// <remarks>
	/// This class is based on the excellent Fraction v2.2 class by Syed Mehroz Alam, Marc C. Brooks & Jeffery Sax.
	/// <see cref="http://www.codeproject.com/Articles/9078/Fraction-class-in-C"/>
	/// </remarks>
	[DataContract]
	public struct Fraction : INumber<Fraction>, IEquatable<Fraction>,
		IComparable, IComparable<Fraction>, ICopyable<Fraction> {

		#region Constants

		private const double EPSILON_DOUBLE = 1.0 / Int64.MaxValue;

		public static readonly Fraction Zero = new Fraction(0, 1);
		public static readonly Fraction NaN = new Fraction(Indeterminates.NaN);
		public static readonly Fraction PositiveInfinity = new Fraction(Indeterminates.PositiveInfinity);
		public static readonly Fraction NegativeInfinity = new Fraction(Indeterminates.NegativeInfinity);
		public static readonly Fraction Epsilon = new Fraction(1, Int64.MaxValue);
		public static readonly Fraction MaxValue = new Fraction(Int64.MaxValue);
		public static readonly Fraction MinValue = new Fraction(Int64.MinValue);

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor providing a Fraction from a numerator and a denominator.
		/// </summary>
		/// <param name="numerator">The numerator (top number)</param>
		/// <param name="denominator">The denominator (bottom number)</param>
		public Fraction(long numerator, long denominator = 1)
				: this() {

			// Prevent MinValue bugs
			if (numerator == long.MinValue) {
				numerator++;
			}
			if (denominator == long.MinValue) {
				denominator++;
			}

			Numerator = numerator;
			Denominator = denominator;
			ReduceImplementation(ref this);
		}

		/// <summary>
		/// Constructor for Fraction for any given double value.
		/// </summary>
		/// <seealso cref="ToFraction(double)"/>
		/// <example>
		/// 0.4 => 2/5
		/// 0.25 => 1/4
		/// </example>
		/// <param name="value">The double to create a Fraction from</param>
		public Fraction(double value)
				: this() {
			this = ToFraction(value);
		}

		/// <summary>
		/// Constructor for a Fraction from a string value.
		/// </summary>
		/// <seealso cref="ToFraction(string)"/>
		/// <param name="value">The string to create a Fraction from</param>
		public Fraction(string value)
				: this() {
			this = ToFraction(value);
		}

		// Indeterminates constructor
		private Fraction(Indeterminates type)
				: this() {
			Numerator = (long) type;
			Denominator = 0;
		}
		
		// Copy constructor
		private Fraction(Fraction f)
			: this(f.Numerator, f.Denominator) {
		}

		#endregion

		#region Properties

		/// <summary>
		/// The Numerator is the top integer value of the Fraction.
		/// </summary>
		[DataMember(Name = "numerator")]
		public long Numerator { get; private set; }

		/// <summary>
		/// The Denominator is the bottom integer value of the Fraction
		/// </summary>
		[DataMember(Name = "denominator")]
		public long Denominator { get; private set; }

		public double EquivalentValue {
			get { return ToDouble(); }
		}

		#endregion

		#region ToFraction

		/// <summary>
		/// A static method to create a fraction from a numerator and a denominator.
		/// </summary>
		public static Fraction ToFraction(long numerator, long denominator = 1) {
			return new Fraction(numerator, denominator);
		}

		/// <summary>
		/// Creates a Fraction from a double value by attempting to create a reasonable
		/// parsing of the double and reducing it. It also handles indeterminate double values
		/// such as NaN, NegativeInfinity, & PositiveInfinity.
		/// </summary>
		/// <exception cref="OverflowException">If the value is too large to be stored in an Int64</exception>
		/// <exception cref="ArithmeticException">If the value is smaller than the available Epsilon</exception>
		/// <param name="value">The double value to convert into a Fraction</param>
		/// <returns>The Fraction closest to the double provided</returns>
		public static Fraction ToFraction(double value) {
			// Indeterminates
			if (double.IsNaN(value)) {
				return NaN;
			}
			if (double.IsNegativeInfinity(value)) {
				return NegativeInfinity;
			}
			if (double.IsPositiveInfinity(value)) {
				return PositiveInfinity;
			}
			if (Math.Abs(value) < EPSILON_DOUBLE) {
				return Zero;
			}

			// Overflows and Epsilons
			if (value > Int64.MaxValue) {
				throw new OverflowException(string.Format("Double {0} too large", value));
			}
			if (value < -Int64.MaxValue) {
				throw new OverflowException(string.Format("Double {0} too small", value));
			}
			if (-EPSILON_DOUBLE < value && value < EPSILON_DOUBLE) {
				throw new ArithmeticException(string.Format("Double {0} cannot be represented", value));
			}

			// Conversion
			int sign = Math.Sign(value);
			value = Math.Abs(value);

			return ConvertDoubleToFraction(sign, value);
		}

		/// <summary>
		/// Converts a string to a Fraction. If in the form 'X/Y', each part will be parsed as an integer.
		/// Otherwise the entire string is parsed as a double or an integer value.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the parameter is null or empty</exception>
		/// <param name="value">The string value to convert into a Fraction</param>
		/// <returns>The Fraction represented by the provided string</returns>
		public static Fraction ToFraction(string value) {
			if (string.IsNullOrEmpty(value)) {
				throw new ArgumentNullException("value");
			}

			NumberFormatInfo info = NumberFormatInfo.CurrentInfo;

			// Indeterminates
			string trimmedValue = value.Trim();

			if (trimmedValue == info.NaNSymbol) {
				return NaN;
			}
			if (trimmedValue == info.PositiveInfinitySymbol) {
				return PositiveInfinity;
			}
			if (trimmedValue == info.NegativeInfinitySymbol) {
				return NegativeInfinity;
			}

			// Test if string is fraction in the form Numerator/Denominator
			int slashPos = value.IndexOf('/');
			if (slashPos > -1) {
				long numerator = Convert.ToInt64(value.Substring(0, slashPos));
				long denominator = Convert.ToInt64(value.Substring(slashPos + 1));

				return new Fraction(numerator, denominator);
			}

			// Try to parse as double or long
			int decimalPos = value.IndexOf(info.CurrencyDecimalSeparator);
			if (decimalPos > -1) {
				return new Fraction(Convert.ToDouble(value));
			}
			return new Fraction(Convert.ToInt64(value));
		}

		#endregion

		#region To Primitives

		/// <summary>
		/// Attempts to convert the Fraction into an Int32 value.
		/// </summary>
		/// <exception cref="NotFiniteNumberException">If the value is indeterminate</exception>
		/// <exception cref="OverflowException">If the value is larger than Int32.MaxValue or smaller than Int32.MinValue</exception>
		/// <returns>The best guess (lossful) value of the Fraction</returns>
		public Int32 ToInt32() {
			if (Denominator == 0) {
				throw new NotFiniteNumberException(string.Format("Cannot convert {0} to Int32", IndeterminateTypeName(Numerator)));
			}

			long bestGuess = Numerator / Denominator;

			if (bestGuess > Int32.MaxValue || bestGuess < Int32.MinValue) {
				throw new OverflowException("Cannot convert to Int32");
			}

			return (Int32)bestGuess;
		}

		/// <summary>
		/// Attempts to convert the Fraction into an Int64 value.
		/// </summary>
		/// <exception cref="NotFiniteNumberException">If the value is indeterminate</exception>
		/// <returns>The best guess (lossful) value of the Fraction</returns>
		public Int64 ToInt64() {
			if (Denominator == 0) {
				throw new NotFiniteNumberException(string.Format("Cannot convert {0} to Int64", IndeterminateTypeName(Numerator)));
			}

			return Numerator / Denominator;
		}

		/// <summary>
		/// Converts the Fraction into a double value. If the fraction is indeterminate (NaN,
		/// PositiveInfinity, or NegativeInfinity), this is reflected in the double value.
		/// </summary>
		/// <returns>The converted Fraction as a double.</returns>
		public double ToDouble() {
			if (Denominator == 1) {
				return Numerator;
			}
			if (Denominator == 0) {
				switch (NormalizeIndeterminate(Numerator)) {
					case Indeterminates.NegativeInfinity:
						return double.NegativeInfinity;
					case Indeterminates.PositiveInfinity:
						return double.PositiveInfinity;
					default: // Indeterminates.NaN
						return double.NaN;
				}
			}
			return (double)Numerator / Denominator;
		}

		#endregion

		#region Fraction Functionality

		/// <summary>
		/// A proper fraction (as opposed to improper) is a fraction whose absolute value is less than 1.
		/// </summary>
		/// <returns>True is fraction is proper, else false.</returns>
		[Pure]
		public bool IsProperFraction() {
			Fraction absFraction = Abs();
			return absFraction.Numerator < absFraction.Denominator;
		}

		/// <summary>
		/// A unit fraction is a fraction in the form '1/X' where its inverse is a positive integer.
		/// </summary>
		/// <returns>True if a unit fraction, else false.</returns>
		[Pure]
		public bool IsUnitFraction() {
			return Numerator == 1 && Denominator > 0;
		}

		/// <summary>
		/// Returns the inverse (or reciprocal) of this fraction by swapping the
		/// numerator and denominator. Indeterminate values remain unchanged.
		/// </summary>
		/// <returns>The inverted fraction</returns>
		public Fraction Inverse() {
			if (Denominator == 0) {
				return this;
			}
			return new Fraction(Denominator, Numerator);
		}

		/// <summary>
		/// Returns the modulus of the fraction based on another.
		/// </summary>
		/// <param name="modulo">The fraction to use as the modulo</param>
		/// <returns>The result of the modulus operation</returns>
		public Fraction Modulus(Fraction modulo) {
			return ModulusImplementation(this, modulo);
		}

		/// <summary>
		/// Cross reduces two fractions, ignoring indeterminate values.
		/// Warning: Care should be taken with this operation as it modifies both
		/// existing fractions!
		/// </summary>
		/// <param name="left">The left fraction to reduce</param>
		/// <param name="right">The right fraction to reduce</param>
		public static void CrossReducePair(ref Fraction left, ref Fraction right) {
			// Ignore Indeterminates
			if (left.Denominator == 0 || right.Denominator == 0) {
				return;
			}

			long gcdTop = GCD(left.Numerator, right.Denominator);
			left.Numerator = left.Numerator / gcdTop;
			right.Denominator = right.Denominator / gcdTop;

			long gcdBottom = GCD(left.Denominator, right.Numerator);
			right.Numerator = right.Numerator / gcdBottom;
			left.Denominator = left.Denominator / gcdBottom;
		}

		#region Indeterminates

		/// <summary>
		/// Returns true if the fraction is NaN, else false.
		/// </summary>
		public bool IsNaN() {
			return Denominator == 0 && NormalizeIndeterminate(Numerator) == Indeterminates.NaN;
		}

		/// <summary>
		/// Returns true if the fraction is positive or negative infinity, else false.
		/// </summary>
		public bool IsInfinity() {
			return Denominator == 0 && NormalizeIndeterminate(Numerator) != Indeterminates.NaN;
		}

		/// <summary>
		/// Returns true if the fraction is PositiveInfinity, else false.
		/// </summary>
		public bool IsPositiveInfinity() {
			return Denominator == 0 && NormalizeIndeterminate(Numerator) == Indeterminates.PositiveInfinity;
		}

		/// <summary>
		/// Returns true if the fraction is NegativeInfinity, else false.
		/// </summary>
		public bool IsNegativeInfinity() {
			return Denominator == 0 && NormalizeIndeterminate(Numerator) == Indeterminates.NegativeInfinity;
		}

		private static Indeterminates NormalizeIndeterminate(long numerator) {
			switch (Math.Sign(numerator)) {
				case 1:
					return Indeterminates.PositiveInfinity;
				case -1:
					return Indeterminates.NegativeInfinity;
				default:
					return Indeterminates.NaN;
			}
		}

		private static int IndeterminantCompare(Indeterminates leftType, Fraction right) {
			switch (leftType) {
				case Indeterminates.PositiveInfinity:
					return (right.IsPositiveInfinity()) ? 0 : 1;
				case Indeterminates.NegativeInfinity:
					return (right.IsNegativeInfinity()) ? 0 : -1;
				default: //Indeterminates.NaN
					return (right.IsNaN()) ? 0 : ((right.IsNegativeInfinity()) ? 1 : -1);
			}
		}

		/// <summary>
		/// Enum used to determine the indeterminates represented by a fraction.
		/// Fractions where the Denominator is 0 are considered indeterminate.
		/// </summary>
		private enum Indeterminates {
			NaN = 0,
			PositiveInfinity = 1,
			NegativeInfinity = -1
		}

		#endregion

		#region Implementation

		private static Fraction ConvertDoubleToFraction(int sign, double value) {
			// Based on http://homepage.smc.edu/kennedy_john/CONFRAC.PDF
			long fractionNumerator = (long)value;
			double fractionDenominator = 1;
			double previousDenominator = 0;
			double remainingDigits = value;
			int maxIterations = 594;	// found at http://www.ozgrid.com/forum/archive/index.php/t-22530.html

			while (remainingDigits != Math.Floor(remainingDigits)
					&& Math.Abs(value - (fractionNumerator / fractionDenominator)) > double.Epsilon) {

				remainingDigits = 1.0 / (remainingDigits - Math.Floor(remainingDigits));

				double scratch = fractionDenominator;

				fractionDenominator = (Math.Floor(remainingDigits) * fractionDenominator) + previousDenominator;
				fractionNumerator = (long)(value * fractionDenominator + 0.5);

				previousDenominator = scratch;

				if (maxIterations-- < 0) {
					break;
				}
			}

			return new Fraction(fractionNumerator * sign, (long)fractionDenominator);
		}

		private static void ReduceImplementation(ref Fraction frac) {
			// Clean up Indeterminates
			if (frac.Denominator == 0) {
				frac.Numerator = (long)NormalizeIndeterminate(frac.Numerator);
				return;
			}

			// All forms of zero should have the same form
			if (frac.Numerator == 0) {
				frac.Denominator = 1;
				return;
			}

			long iGCD = GCD(frac.Numerator, frac.Denominator);
			frac.Numerator /= iGCD;
			frac.Denominator /= iGCD;

			// If negative sign in denominator
			if (frac.Denominator < 0) {
				// Move negative sign to numerator
				frac.Numerator = -frac.Numerator;
				frac.Denominator = -frac.Denominator;
			}
		}

		/// <summary>
		/// Calculates the GCD (Greatest Common Denominator) of two values.
		/// </summary>
		private static long GCD(long left, long right) {
			// Quick Abs
			if (left < 0) {
				left = -left;
			}
			if (right < 0) {
				right = -right;
			}

			// If either zero or one, the GCD is 1
			if (left <= 1 || right <= 1) {
				return 1;
			}

			do {
				if (left < right) {
					long temp = left; // swap the two operands
					left = right;
					right = temp;
				}
				left %= right;
			} while (left != 0);

			return right;
		}

		private static Fraction AddImplementation(Fraction left, Fraction right) {
			if (left.IsNaN() || right.IsNaN()) {
				return NaN;
			}

			long gcd = GCD(left.Denominator, right.Denominator); // Must return >= 1
			long leftDenominator = left.Denominator / gcd;
			long rightDenominator = right.Denominator / gcd;

			try {
				checked {
					long numerator = left.Numerator * rightDenominator + right.Numerator * leftDenominator;
					long denominator = leftDenominator * rightDenominator * gcd;

					return new Fraction(numerator, denominator);
				}
			} catch (Exception e) {
				throw new Exception("Addition exception", e);
			}
		}

		private static Fraction MultiplyImplementation(Fraction left, Fraction right) {
			if (left.IsNaN() || right.IsNaN()) {
				return NaN;
			}

			CrossReducePair(ref left, ref right);

			try {
				checked {
					long numerator = left.Numerator * right.Numerator;
					long denominator = left.Denominator * right.Denominator;
					return new Fraction(numerator, denominator);
				}
			}
			catch (Exception e) {
				throw new Exception("Multiply exception", e);
			}
		}

		private static Fraction ModulusImplementation(Fraction left, Fraction right) {
			if (left.IsNaN() || right.IsNaN()) {
				return NaN;
			}

			try {
				checked {
					// this will discard any fractional places...
					Int64 quotient = (Int64)(left / right);
					var whole = new Fraction(quotient * right.Numerator, right.Denominator);
					return left - whole;
				}
			}
			catch (Exception e) {
				throw new Exception("Modulus exception", e);
			}
		}

		#endregion

		#endregion

		#region Basic Math

		public Fraction Add(Fraction add) {
			return AddImplementation(this, add);
		}

		public Fraction Subtract(Fraction subtract) {
			return AddImplementation(this, subtract.Negate());
		}

		public Fraction Multiply(Fraction multiply) {
			return MultiplyImplementation(this, multiply);
		}

		public Fraction Divide(Fraction divide) {
			return MultiplyImplementation(this, divide.Inverse());
		}

		public Fraction Add(double add) {
			return AddImplementation(this, ToFraction(add));
		}

		public Fraction Subtract(double subtract) {
			return AddImplementation(this, ToFraction(subtract).Negate());
		}

		public Fraction Multiply(double multiply) {
			return MultiplyImplementation(this, ToFraction(multiply));
		}

		public Fraction Divide(double divide) {
			return MultiplyImplementation(this, ToFraction(divide).Inverse());
		}

		public Fraction Negate() {
			return new Fraction(-Numerator, Denominator);
		}

		#endregion

		#region Operator Overrides

		#region Unary Negation

		public static Fraction operator -(Fraction left) {
			return left.Negate();
		}

		#endregion

		#region Addition

		public static Fraction operator +(Fraction left, Fraction right) {
			return left.Add(right);
		}

		public static Fraction operator +(Fraction left, double right) {
			return left.Add(right);
		}

		public static Fraction operator +(double left, Fraction right) {
			return ToFraction(left).Add(right);
		}

		public static Fraction operator +(Fraction left, long right) {
			return left.Add(ToFraction(right));
		}

		public static Fraction operator +(long left, Fraction right) {
			return ToFraction(left).Add(right);
		}

		#endregion

		#region Subtraction

		public static Fraction operator -(Fraction left, Fraction right) {
			return left.Subtract(right);
		}

		public static Fraction operator -(Fraction left, double right) {
			return left.Subtract(right);
		}

		public static Fraction operator -(double left, Fraction right) {
			return ToFraction(left).Subtract(right);
		}

		public static Fraction operator -(Fraction left, long right) {
			return left.Subtract(ToFraction(right));
		}

		public static Fraction operator -(long left, Fraction right) {
			return ToFraction(left).Subtract(right);
		}

		#endregion

		#region Multiplication

		public static Fraction operator *(Fraction left, Fraction right) {
			return left.Multiply(right);
		}

		public static Fraction operator *(long left, Fraction right) {
			return ToFraction(left).Multiply(right);
		}

		public static Fraction operator *(Fraction left, long right) {
			return left.Multiply(ToFraction(right));
		}

		public static Fraction operator *(double left, Fraction right) {
			return ToFraction(left).Multiply(right);
		}

		public static Fraction operator *(Fraction left, double right) {
			return left.Multiply(ToFraction(right));
		}

		#endregion

		#region Division

		public static Fraction operator /(Fraction left, Fraction right) {
			return left.Divide(right);
		}

		public static Fraction operator /(long left, Fraction right) {
			return ToFraction(left).Divide(right);
		}

		public static Fraction operator /(Fraction left, long right) {
			return left.Divide(ToFraction(right));
		}

		public static Fraction operator /(double left, Fraction right) {
			return ToFraction(left).Divide(right);
		}

		public static Fraction operator /(Fraction left, double right) {
			return left.Divide(right);
		}

		#endregion

		#region Modulus

		public static Fraction operator %(Fraction left, Fraction right) {
			return left.Modulus(right);
		}

		public static Fraction operator %(long left, Fraction right) {
			return ToFraction(left).Modulus(right);
		}

		public static Fraction operator %(Fraction left, long right) {
			return left.Modulus(right);
		}

		public static Fraction operator %(double left, Fraction right) {
			return ToFraction(left).Modulus(right);
		}

		public static Fraction operator %(Fraction left, double right) {
			return left.Modulus(right);
		}

		#endregion

		#region Equal

		public static bool operator ==(Fraction left, Fraction right) {
			return left.CompareEquality(right, false);
		}

		public static bool operator ==(Fraction left, long right) {
			return left.CompareEquality(new Fraction(right), false);
		}

		public static bool operator ==(Fraction left, double right) {
			return left.CompareEquality(new Fraction(right), false);
		}

		#endregion

		#region Not-Equal

		public static bool operator !=(Fraction left, Fraction right) {
			return left.CompareEquality(right, true);
		}

		public static bool operator !=(Fraction left, long right) {
			return left.CompareEquality(new Fraction(right), true);
		}

		public static bool operator !=(Fraction left, double right) {
			return left.CompareEquality(new Fraction(right), true);
		}

		#endregion

		#region Inequality

		public static bool operator <(Fraction left, Fraction right) {
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(Fraction left, Fraction right) {
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(Fraction left, Fraction right) {
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(Fraction left, Fraction right) {
			return left.CompareTo(right) >= 0;
		}

		#endregion

		#region Implict Conversion From Primitives

		public static implicit operator Fraction(long value) {
			return new Fraction(value);
		}

		public static implicit operator Fraction(double value) {
			return new Fraction(value);
		}

		public static implicit operator Fraction(string value) {
			return new Fraction(value);
		}

		#endregion

		#region Explicit Conversion To Primitives

		public static explicit operator int(Fraction fraction) {
			return fraction.ToInt32();
		}

		public static explicit operator long(Fraction fraction) {
			return fraction.ToInt64();
		}

		public static explicit operator double(Fraction fraction) {
			return fraction.ToDouble();
		}

		public static implicit operator string(Fraction fraction) {
			return fraction.ToString();
		}

		#endregion

		#endregion

		#region Extended Math

		public Fraction Abs() {
			return new Fraction(Math.Abs(Numerator), Math.Abs(Denominator));
		}

		public Fraction Pow(double power) {
			// Not exact - converts to double
			return new Fraction(Math.Pow(Numerator, power) / Math.Pow(Denominator, power));
		}

		public Fraction Sqrt() {
			// Not exact - converts to double
			return new Fraction(Math.Sqrt(Numerator) / Math.Sqrt(Denominator));
		}

		public Fraction Ceiling() {
			// Not exact - converts to double
			return ToFraction(Math.Ceiling(ToDouble()));
		}

		public Fraction Floor() {
			// Not exact - converts to double
			return ToFraction(Math.Floor(ToDouble()));
		}

		public Fraction Round() {
			// Not exact - converts to double
			return ToFraction(Math.Round(ToDouble()));
		}

		public Fraction Max(Fraction other) {
			return (this >= other) ? this : other;
		}

		public Fraction Max(params Fraction[] values) {
			var vectors = new List<Fraction>(values) { this };
			return vectors.Max();
		}

		public Fraction Min(Fraction other) {
			return (this <= other) ? this : other;
		}

		public Fraction Min(params Fraction[] values) {
			var vectors = new List<Fraction>(values) { this };
			return vectors.Min();
		}

		#endregion

		#region Copyable

		public Fraction Copy() {
			return new Fraction(this);
		}

		#endregion

		#region Equality

		public override bool Equals(object obj) {
			if (obj == null || !(obj is Fraction)) {
				return false;
			}

			try {
				var right = (Fraction)obj;
				return CompareEquality(right, false);
			} catch {
				// can't throw in an Equals!
				return false;
			}
		}

		public override int GetHashCode() {
			ReduceImplementation(ref this);

			int numeratorHash = Numerator.GetHashCode();
			int denominatorHash = Denominator.GetHashCode();

			return (numeratorHash ^ denominatorHash);
		}

		public bool Equals(Fraction right) {
			return CompareEquality(right, false);
		}

		private bool CompareEquality(Fraction right, bool notEqualCheck) {

			ReduceImplementation(ref this);
			ReduceImplementation(ref right);

			if (Numerator == right.Numerator && Denominator == right.Denominator) {
				// Special-case rule, two NaNs are always both equal
				return (notEqualCheck && IsNaN()) || !notEqualCheck;
			}
			return notEqualCheck;
		}

		#endregion

		#region Comparison

		public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}

			Fraction right;

			if (obj is Fraction) {
				right = (Fraction) obj;
			} else if (obj is long) {
				right = (long) obj;
			} else if (obj is double) {
				right = (double) obj;
			} else if (obj is string) {
				right = (string) obj;
			} else {
				throw new ArgumentException("Must be convertible to Fraction", "obj");
			}

			return CompareTo(right);
		}

		public int CompareTo(Fraction right) {
			if (Denominator == 0) {
				return IndeterminantCompare(NormalizeIndeterminate(Numerator), right);
			}
			if (right.Denominator == 0) {
				return -IndeterminantCompare(NormalizeIndeterminate(right.Numerator), this);
			}

			// They're both normal Fractions
			CrossReducePair(ref this, ref right);

			try {
				checked {
					long leftScale = Numerator * right.Denominator;
					long rightScale = Denominator * right.Numerator;

					if (leftScale < rightScale) {
						return -1;
					}
					if (leftScale > rightScale) {
						return 1;
					}
					return 0;
				}
			}
			catch (Exception e) {
				throw new Exception(string.Format("CompareTo({0}, {1}) exception", this, right), e);
			}
		}

		#endregion

		#region ToString

		public override string ToString() {
			return ToString("G", null);
		}

		/// <summary>
		/// Provides formatted ToString functionality. Format strings can be any of the following, optionally
		/// appended by a number format string to be used:
		/// <list type="bullet">
		/// <item><term>G</term><description>A General format string, e.g. 1/3, 17/4, -8/3, 5</description></item>
		/// <item><term>M</term><description>A Mixed Fraction format string, e.g. 1/3, 4 1/4, -2 2/3, 5</description></item>
		/// <item><term>R</term><description>A Ratio format string, e.g. 1:3, 17:4, -8:3, 5:1</description></item>
		/// </list>
		/// </summary>
		/// <param name="format">The format string to be used, defaults to "G"</param>
		/// <param name="formatProvider">The formatProvider to use, defaults to the current culture</param>
		/// <returns>The formatted string</returns>
		public string ToString(string format, IFormatProvider formatProvider) {
			if (string.IsNullOrEmpty(format)) {
				format = "G";
			}
			if (format.Length == 1) {
				format += "G";
			}
			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}

			// Indeterminates
			if (Denominator == 0) {
				return IndeterminateTypeName(Numerator);
			}

			switch (format.Substring(0, 1).ToUpper()) {
				case "M": // Mixed Fraction
					return MixedFractionToString(format.Substring(1), formatProvider);
				case "R": // Ratio
					return Numerator.ToString(format.Substring(1), formatProvider) + ":"
						+ Denominator.ToString(format.Substring(1), formatProvider);
				default: // General 'G'
					return GeneralToString(format.Substring(1), formatProvider);
			}
		}

		private string MixedFractionToString(string format, IFormatProvider formatProvider) {
			// Whole Part
			string mixedFraction = "";
			long wholePart = Numerator/Denominator;
			if (wholePart != 0) {
				mixedFraction += wholePart.ToString(format, formatProvider) + " ";
			}

			// Proper Remainder
			long remainder = Numerator - (wholePart*Denominator);
			if (remainder != 0) {
				if (wholePart != 0) {
					remainder = Math.Abs(remainder);
				}
				mixedFraction += remainder.ToString(format, formatProvider) + "/"
					+ Denominator.ToString(format, formatProvider);
			}
			return mixedFraction.Trim();
		}

		private string GeneralToString(string format, IFormatProvider formatProvider) {
			if (Denominator == 1) {
				return Numerator.ToString(format, formatProvider);
			}
			return Numerator.ToString(format, formatProvider) + "/"
				+ Denominator.ToString(format, formatProvider);
		}

		private string IndeterminateTypeName(long numerator) {
			NumberFormatInfo info = NumberFormatInfo.CurrentInfo;

			switch (NormalizeIndeterminate(numerator)) {
				case Indeterminates.PositiveInfinity:
					return info.PositiveInfinitySymbol;
				case Indeterminates.NegativeInfinity:
					return info.NegativeInfinitySymbol;
				default: // Indetermindates.NaN
					return info.NaNSymbol;
			}
		}

		#endregion
	}
}
