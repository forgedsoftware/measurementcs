using System;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// A representation of a double value with a positive and negative uncertainty
	/// </summary>
	public struct Uncertainty : INumberMath<Uncertainty>, IFormattable, IEquatable<Uncertainty>,
		IComparable, IComparable<Uncertainty>, ICopyable<Uncertainty> {

		private const double SQRT_POWER = 0.5;

		/// <summary>
		/// This is a reasonable epsilon for vector comparisons and equitability.
		/// </summary>
		public static readonly double EquatableEpsilon = 1E-15;

		#region Static Factory Methods

		/// <summary>
		/// Creates an uncertainty from a value and a range from the minimum to the maximum.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="minimum">The minimum value of the uncertainty</param>
		/// <param name="maximum">The maximum value of the uncertainty</param>
		/// <returns>The uncertainty double from the range</returns>
		public static Uncertainty FromRange(double value, double minimum, double maximum) {
			return new Uncertainty(value, value - minimum, maximum - value);
		}

		/// <summary>
		/// Creates an uncertainty from a value and a percentage of error below and above the value.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="lowerPercentage">The percentage of error below the value (0-1)</param>
		/// <param name="upperPercentage">The percentage of error above the value (0-1)</param>
		/// <returns>The uncertainty double given these percentages</returns>
		public static Uncertainty FromPercentage(double value, double lowerPercentage, double upperPercentage) {
			return new Uncertainty(value, value * lowerPercentage, value * upperPercentage, true);
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Basic constructor with no uncertainty.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="isRelative">Determines if the uncertainty should be treated as relative</param>
		public Uncertainty(double value, bool isRelative = false)
				: this() {
			Value = value;
			IsRelative = isRelative;
		}

		/// <summary>
		/// A constructor that defines the value and a higher and lower uncertainty.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="lowerUncertainty">The lower uncertainty value</param>
		/// <param name="upperUncertainty">The upper uncertainty value</param>
		/// <param name="isRelative">Determines if the uncertainty should be treated as relative</param>
		public Uncertainty(double value, double lowerUncertainty, double upperUncertainty, bool isRelative = false) 
				: this(value, isRelative) {
			if (lowerUncertainty < 0) {
				throw new ArgumentException("The LowerUncertainty must be greater than or equal to zero");
			}
			if (upperUncertainty < 0) {
				throw new ArgumentException("The UpperUncertainty must be greater than or equal to zero");
			}
			LowerUncertainty = lowerUncertainty;
			UpperUncertainty = upperUncertainty;
		}

		/// <summary>
		/// A constructor that defines the value and an uncertainty.
		/// The uncertainty is equal for both lower and upper uncertainties.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="uncertainty">The uncertainty to use</param>
		/// <param name="isRelative">Determines if the uncertainty should be treated as relative</param>
		public Uncertainty(double value, double uncertainty, bool isRelative = false)
				: this (value, uncertainty, uncertainty, isRelative) {
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="uDouble">The object to be copied</param>
		public Uncertainty(Uncertainty uDouble)
				: this() {
			Value = uDouble.Value;
			LowerUncertainty = uDouble.LowerUncertainty;
			UpperUncertainty = uDouble.UpperUncertainty;
			IsRelative = uDouble.IsRelative;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The central value.
		/// </summary>
		public double Value { get; private set; }

		/// <summary>
		/// The amount of uncertainty below the value.
		/// </summary>
		public double LowerUncertainty { get; private set; }

		/// <summary>
		/// The amount of uncertainty above the value.
		/// </summary>
		public double UpperUncertainty { get; private set; }

		/// <summary>
		/// Determines if this uncertainty should be treated as relative or
		/// absolute. A relative uncertainty is defined based on a percentage,
		/// whereas an absolute is defined by an actual set of values.
		/// </summary>
		public bool IsRelative { get; private set; }

		/// <summary>
		/// The minimum value that fits within the known uncertainty.
		/// </summary>
		public double Minimum {
			get { return Value - LowerUncertainty; }
		}

		/// <summary>
		/// The maximum value that fits within the know uncertainty.
		/// </summary>
		public double Maximum {
			get { return Value + UpperUncertainty; }
		}

		/// <summary>
		/// The percentage (between 0-1) that the upper uncertainty is of the value.
		/// </summary>
		public double UpperPercentage {
			get { return UpperUncertainty / Value; }
		}

		/// <summary>
		/// The percentage (between 0-1) that the lower uncertainty is of the value.
		/// </summary>
		public double LowerPercentage {
			get { return LowerUncertainty / Value; }
		}

		/// <summary>
		/// The total amount of uncertainty.
		/// </summary>
		public double TotalUncertainty {
			get { return UpperUncertainty + LowerUncertainty; }
		}

		#endregion

		#region Uncertainty Functions

		/// <summary>
		/// Determines if two uncertainty values are consistent. This is is a loose form
		/// of equivalency. Values are consistent if they are within the experimental uncertainty
		/// of each other.
		/// </summary>
		/// <param name="u">The other uncertainty value</param>
		/// <returns>True if consistent, else false.</returns>
		public bool IsConsistent(Uncertainty u) {
			return (Math.Abs(Value - u.Value) <= Math.Abs((TotalUncertainty + u.TotalUncertainty)/2));
		}

		#endregion

		#region IMathFunction

		/// <summary>
		/// Calculates an uncertainty raised to a power. The uncertainty is calculated using the relative
		/// uncertainty multiplied by the power.
		/// </summary>
		/// <param name="power">The power to raise the value by</param>
		/// <returns>The uncertainty value raised to a power</returns>
		public Uncertainty Pow(double power) {
			return FromPercentage(Math.Pow(Value, power), LowerPercentage * power, UpperPercentage * power);
		}

		/// <summary>
		/// Calculates the square root of the uncertainty value. The uncertainty is calculated by multiplying
		/// the relative uncertainty by the equivalent power of a square root, namely 0.5.
		/// </summary>
		/// <returns>The sqare root of the uncertainty</returns>
		public Uncertainty Sqrt() {
			return FromPercentage(Math.Sqrt(Value), LowerPercentage * SQRT_POWER, UpperPercentage * SQRT_POWER);
		}

		/// <summary>
		/// Finds the largest uncertainty out of two. Uses purely the central value and ignores the uncertainties.
		/// </summary>
		/// <param name="other">The other uncertainty</param>
		/// <returns>The larger of the two uncertainties</returns>
		public Uncertainty Max(Uncertainty other) {
			return (Value >= other.Value) ? this : other;
		}

		/// <summary>
		/// Finds the smallest uncertainty out of two. Uses purely the central value and ignores the uncertainties.
		/// </summary>
		/// <param name="other">The other uncertainty</param>
		/// <returns>The smaller of the two uncertainties</returns>
		public Uncertainty Min(Uncertainty other) {
			return (Value <= other.Value) ? this : other;
		}

		#endregion

		#region Basic Math

		/// <summary>
		/// Adds the values and adds the absolute uncertainty of two uncertainty values.
		/// </summary>
		/// <param name="add">The uncertainty value to add</param>
		/// <returns>The sum of the uncertainty values</returns>
		public Uncertainty Add(Uncertainty add) {
			return new Uncertainty(Value + add.Value, LowerUncertainty + add.LowerUncertainty, UpperUncertainty + add.UpperUncertainty);
		}

		/// <summary>
		/// Subtracts the values and adds the absolutes uncertainty of two uncertainty values.
		/// </summary>
		/// <param name="subtract">The uncertainty value to subtract</param>
		/// <returns>The difference of the uncertainty values</returns>
		public Uncertainty Subtract(Uncertainty subtract) {
			return new Uncertainty(Value - subtract.Value, LowerUncertainty + subtract.LowerUncertainty, UpperUncertainty + subtract.UpperUncertainty);
		}

		/// <summary>
		/// Multiplies the values and adds the relative percentages of the two uncertainty values.
		/// </summary>
		/// <param name="multiply">The uncertainty value to multiply by</param>
		/// <returns>The product of the uncertainty values</returns>
		public Uncertainty Multiply(Uncertainty multiply) {
			return FromPercentage(Value * multiply.Value, LowerPercentage + multiply.LowerPercentage, UpperPercentage + multiply.UpperPercentage);
		}

		/// <summary>
		/// Divides the values and adds the relative percentages of the two uncertainty values.
		/// </summary>
		/// <param name="divide">The uncertainty value to divide by</param>
		/// <returns>The quotient of the uncertainty values</returns>
		public Uncertainty Divide(Uncertainty divide) {
			return FromPercentage(Value / divide.Value, LowerPercentage + divide.LowerPercentage, UpperPercentage + divide.UpperPercentage);
		}

		/// <summary>
		/// Adds a constant to this uncertainty value, maintaining the uncertainties of this uncertainty.
		/// </summary>
		/// <param name="add">The constant to add</param>
		/// <returns>The sum of this uncertainty and the constant</returns>
		public Uncertainty Add(double add) {
			return new Uncertainty(Value + add, LowerUncertainty, UpperUncertainty);
		}

		/// <summary>
		/// Subtracts a constant from this uncertainty value, maintaining the uncertainties of this uncertainty.
		/// </summary>
		/// <param name="subtract">The constant to subtract</param>
		/// <returns>The difference of this uncertainty and the constant</returns>
		public Uncertainty Subtract(double subtract) {
			return new Uncertainty(Value - subtract, LowerUncertainty, UpperUncertainty);
		}

		/// <summary>
		/// Multiplies the uncertainty with a constant, if the uncertainty is relative,
		/// maintain the relative percentage uncertainity. If it is absolute, 
		/// multiply the uncertainty by the constant.
		/// </summary>
		/// <param name="multiply">The constant to multiply by</param>
		/// <returns>The product of the uncertainty and the constant</returns>
		public Uncertainty Multiply(double multiply) {
			if (IsRelative) {
				return FromPercentage(Value * multiply, LowerPercentage, UpperPercentage);
			}
			return new Uncertainty(Value * multiply, Math.Abs(LowerUncertainty * multiply), Math.Abs(UpperUncertainty * multiply));
		}

		/// <summary>
		/// Divides the uncertainty with a constant, if the uncertainty is relative,
		/// maintain the relative percentage uncertainity. If it is absolute, 
		/// divide the uncertainty by the constant.
		/// </summary>
		/// <param name="divide">The constant to divide by</param>
		/// <returns>The quotient of the uncertainty and the constant</returns>
		public Uncertainty Divide(double divide) {
			if (IsRelative) {
				return FromPercentage(Value / divide, LowerPercentage, UpperPercentage);
			}
			return new Uncertainty(Value / divide, Math.Abs(LowerUncertainty / divide), Math.Abs(UpperUncertainty / divide));
		}

		/// <summary>
		/// Negates the uncertainty, keeping the existing uncertainty values.
		/// </summary>
		/// <returns>The negated uncertainty</returns>
		public Uncertainty Negate() {
			return new Uncertainty(-Value, LowerUncertainty, UpperUncertainty);
		}

		#endregion

		#region Operator Overrides

		/// <summary>
		/// Overrides the + (addition) operator with an adding of uncertainties.
		/// </summary>
		/// <seealso cref="Add(Uncertainty)"/>
		public static Uncertainty operator +(Uncertainty u1, Uncertainty u2) {
			return u1.Add(u2);
		}

		/// <summary>
		/// Overrides the - (subtraction) operator with a subtraction of uncertainties.
		/// </summary>
		/// <seealso cref="Subtract(Uncertainty)"/>
		public static Uncertainty operator -(Uncertainty u1, Uncertainty u2) {
			return u1.Subtract(u2);
		}

		/// <summary>
		/// Overrides the - (negation) operator with a negation
		/// </summary>
		/// <seealso cref="Negate()"/>
		public static Uncertainty operator -(Uncertainty u1) {
			return u1.Negate();
		}

		/// <summary>
		/// Overrides the + (reinforcement) operator by returning a copy of the uncertainty
		/// </summary>
		/// <seealso cref="Copy()"/>
		public static Uncertainty operator +(Uncertainty u1) {
			return u1.Copy();
		}

		/// <summary>
		/// Overrides the * (multiplication) operator as a multiplication between
		/// an uncertainty and a constant.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Uncertainty operator *(Uncertainty u1, double d2) {
			return u1.Multiply(d2);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator as a multiplication between
		/// an constant and an uncertainty.
		/// </summary>
		/// <seealso cref="Multiply(double)"/>
		public static Uncertainty operator *(double d1, Uncertainty u2) {
			// Commutable
			return u2.Multiply(d1);
		}

		/// <summary>
		/// Overrides the * (multiplication) operator as a multiplication between
		/// two uncertanties.
		/// </summary>
		/// <seealso cref="Multiply(Uncertainty)"/>
		public static Uncertainty operator *(Uncertainty u1, Uncertainty u2) {
			return u1.Multiply(u2);
		}

		/// <summary>
		/// Overrides the / (division) operator between an uncertainty and a
		/// constant as a division.
		/// </summary>
		/// <seealso cref="Divide(double)"/>
		public static Uncertainty operator /(Uncertainty u1, double d2) {
			return u1.Divide(d2);
		}

		/// <summary>
		/// Overrides the / (division) operator between a constant and a
		/// uncertainty as a division.
		/// </summary>
		public static Uncertainty operator /(double d1, Uncertainty u2) {
			if (u2.IsRelative) {
				return FromPercentage(d1 / u2.Value, u2.LowerPercentage, u2.UpperPercentage);
			}
			return new Uncertainty(d1 / u2.Value, Math.Abs(d1 / u2.LowerUncertainty), Math.Abs(d1 / u2.UpperUncertainty));
		}

		/// <summary>
		/// Overrides the &lt; (less than) operator as the test of whether the
		/// value of the first uncertainty is less than that of the second.
		/// </summary>
		public static bool operator <(Uncertainty u1, Uncertainty u2) {
			return u1.Value < u2.Value;
		}

		/// <summary>
		/// Overrides the &lt;= (less than or equal) operator as the test of whether the
		/// value of the first uncertainty is less than or equal to that of the second.
		/// </summary>
		public static bool operator <=(Uncertainty u1, Uncertainty u2) {
			return u1.Value <= u2.Value;
		}

		/// <summary>
		/// Overrides the &gt; (greater than) operator as the test of whether the
		/// value of the first uncertainty is greater than that of the second.
		/// </summary>
		public static bool operator >(Uncertainty u1, Uncertainty u2) {
			return u1.Value > u2.Value;
		}

		/// <summary>
		/// Overrides the &gt;= (greater than or equal) operator as the test of whether the
		/// value of the first uncertainty is greater than or equal to that of the second.
		/// </summary>
		public static bool operator >=(Uncertainty u1, Uncertainty u2) {
			return u1.Value >= u2.Value;
		}

		/// <summary>
		/// Overrides the == (equality) operator to test whether the uncertainties are equivalent
		/// based on whether the value and uncertainties are the same (epsilon test).
		/// Comparing two null values also returns true.
		/// </summary>
		public static bool operator ==(Uncertainty u1, Uncertainty u2) {
			if (((object)u1 == null) && ((object)u2 == null)) {
				return true;
			}
			return (((object)u1 != null) && ((object)u2 != null) &&
				(Math.Abs(u1.Value - u2.Value) <= EquatableEpsilon) &&
				(Math.Abs(u1.LowerUncertainty - u2.LowerUncertainty) <= EquatableEpsilon) &&
				(Math.Abs(u1.UpperUncertainty - u2.UpperUncertainty) <= EquatableEpsilon));
		}

		/// <summary>
		/// Overrides the != (inequality) operator to test whether the vectors
		/// are not equivalent. It is a direct inverse of the equality operator.
		/// </summary>
		/// <seealso cref="operator ==(Uncertainty, Uncertainty)"/>
		public static bool operator !=(Uncertainty u1, Uncertainty u2) {
			return !(u1 == u2);
		}

		#endregion

		#region Comparing & Equality

		/// <summary>
		/// An override of the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Uncertainty, Uncertainty)"/>
		public override bool Equals(object obj) {
			if (obj is Uncertainty) {
				return this == (Uncertainty)obj;
			}
			return false;
		}

		/// <summary>
		/// Implements the equals method. It checks equality not reference equals.
		/// </summary>
		/// <seealso cref="operator ==(Uncertainty, Uncertainty)"/>
		public bool Equals(Uncertainty other) {
			return this == other;
		}

		/// <summary>
		/// A standard hash code implementation, hashing the value and uncertainties.
		/// </summary>
		public override int GetHashCode() {
			unchecked {
				int hash = (int)2166136261;
				hash = hash * 16777619 ^ Value.GetHashCode();
				hash = hash * 16777619 ^ LowerUncertainty.GetHashCode();
				hash = hash * 16777619 ^ UpperUncertainty.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Compares an object with the uncertainty. If the object is not an uncertainty,
		/// throws <exception cref="ArgumentException" />. Otherwise returns
		/// an appropriate comparison value.
		/// </summary>
		/// <param name="obj">The object to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other uncertainty value, 0 if they
		/// are the same, and 1 if this uncertainty value is larger.</returns>
		public int CompareTo(object obj) {
			if (obj is Uncertainty) {
				return CompareTo((Uncertainty)obj);
			}
			throw new ArgumentException("Cannot compare a Uncertainty with a " + obj.GetType());
		}

		/// <summary>
		/// Direct comparison between two uncertainties.
		/// </summary>
		/// <param name="other">The uncertainty to compare with</param>
		/// <returns>Returns -1 if this is smaller than the other uncertainty's value, 0 if they
		/// are the same, and 1 if this uncertainty's value is larger</returns>
		public int CompareTo(Uncertainty other) {
			if (this < other) {
				return -1;
			}
			if (this > other) {
				return 1;
			}
			return 0;
		}

		#endregion

		#region Copyable

		/// <summary>
		/// Copies the uncertainty using the copy constructor
		/// </summary>
		/// <returns>The copied uncertainty</returns>
		public Uncertainty Copy() {
			return new Uncertainty(this);
		}

		#endregion

		#region ToString

		public override string ToString() {
			throw new NotImplementedException();
		}

		public string ToString(string format, IFormatProvider formatProvider) {
			throw new NotImplementedException();
		}

		#endregion

	}
}
