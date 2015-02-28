using ForgedSoftware.Measurement.Number;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestFraction {

		#region Instantiation

		[Test]
		public void TestCreatingFraction() {
			var frac1 = new Fraction(1, 4);
			Assert.AreEqual(1, frac1.Numerator);
			Assert.AreEqual(4, frac1.Denominator);
			Assert.AreEqual(0.25, frac1.EquivalentValue);
			Assert.AreEqual(0.25, frac1.ToDouble());
			Assert.AreEqual(0, frac1.ToInt32());
			Assert.AreEqual(0, frac1.ToInt64());
			Assert.AreEqual("1/4", frac1.ToString());
			Assert.AreEqual(frac1, new Fraction("1/4"));
		}

		[Test]
		public void TestCreatingFractionFromDouble() {
			var frac1 = new Fraction(0.6);
			Assert.AreEqual(3, frac1.Numerator);
			Assert.AreEqual(5, frac1.Denominator);
			Assert.AreEqual(0.6, frac1.EquivalentValue);
			Assert.AreEqual(0.6, frac1.ToDouble());
			Assert.AreEqual(0, frac1.ToInt32());
			Assert.AreEqual(0, frac1.ToInt64());
			Assert.AreEqual("3/5", frac1.ToString());
			Assert.AreEqual(frac1, Fraction.ToFraction(0.6));
			Assert.AreEqual(frac1, new Fraction("6/10"));
		}

		#endregion

		#region Fraction Specific Functionality

		[Test]
		public void TestImproperFraction() {
			var frac1 = new Fraction(33, 11);
			Assert.AreEqual(3, frac1.Numerator);
			Assert.AreEqual(1, frac1.Denominator);
		}

		[Test]
		public void TestConvertingBetweenDoubleAndFraction() {
			var double1 = new Fraction(1, 3).ToDouble();
			Assert.AreEqual("1/3", Fraction.ToFraction(double1).ToString());
		}

		[Test]
		public void TestFractionModulus() {
			var frac1 = new Fraction(-78, 24);
			Assert.AreEqual("-13/4", frac1.ToString());
			Assert.AreEqual("-1/4", frac1.Modulus(new Fraction(1)).ToString());
			Assert.AreEqual("-1/4", frac1.Modulus(new Fraction(1, 2)).ToString());
			Assert.AreEqual("-1/4", frac1.Modulus(new Fraction(3)).ToString());
			Assert.AreEqual("-13/4", frac1.Modulus(new Fraction(5)).ToString());
		}

		[Test]
		public void TestUnitFraction() {
			Assert.IsTrue(new Fraction(1, 2).IsUnitFraction());
			Assert.IsTrue(new Fraction(1, 4565).IsUnitFraction());
			Assert.IsFalse(new Fraction(-1, 2).IsUnitFraction());
			Assert.IsFalse(new Fraction(3, 2).IsUnitFraction());
			Assert.IsFalse(Fraction.NaN.IsUnitFraction());
			Assert.IsFalse(new Fraction(324325, 2343424534).IsUnitFraction());
		}

		[Test]
		public void TestProperFraction() {
			Assert.IsTrue(new Fraction(1, 2).IsProperFraction());
			Assert.IsTrue(new Fraction(1, 4565).IsProperFraction());
			Assert.IsTrue(new Fraction(-1, 2).IsProperFraction());
			Assert.IsFalse(new Fraction(3, 2).IsProperFraction());
			Assert.IsFalse(new Fraction(1).IsProperFraction());
			Assert.IsFalse(new Fraction(-1).IsProperFraction());
			Assert.IsTrue(Fraction.Zero.IsProperFraction());
			Assert.IsFalse(Fraction.NaN.IsProperFraction());
			Assert.IsTrue(new Fraction(324325, 2343424534).IsProperFraction());
		}

		#endregion

		#region Basic Math

		[Test]
		public void TestAddingFractions() {
			var frac1 = new Fraction(-1, 8);
			var result1 = frac1.Add(new Fraction(5, 8));
			Assert.AreEqual("1/2", result1.ToString());
			var result2 = frac1.Add(new Fraction(2, 8));
			Assert.AreEqual("1/8", result2.ToString());
			var result3 = frac1 + new Fraction(-13, 8);
			Assert.AreEqual("-7/4", result3.ToString());
		}

		[Test]
		public void TestSubtractingFractions() {
			var frac1 = new Fraction(-1, 8);
			var result1 = frac1.Subtract(new Fraction(5, 8));
			Assert.AreEqual("-3/4", result1.ToString());
			var result2 = frac1.Subtract(new Fraction(2, 8));
			Assert.AreEqual("-3/8", result2.ToString());
			var result3 = frac1 - new Fraction(-13, 8);
			Assert.AreEqual("3/2", result3.ToString());
		}

		[Test]
		public void TestMultiplyingFractions() {
			var frac1 = new Fraction(-1, 8);
			var result1 = frac1.Multiply(new Fraction(5, 8));
			Assert.AreEqual("-5/64", result1.ToString());
			var result2 = frac1.Multiply(new Fraction(2, 8));
			Assert.AreEqual("-1/32", result2.ToString());
			var result3 = frac1 * new Fraction(-13, 8);
			Assert.AreEqual("13/64", result3.ToString());
		}

		[Test]
		public void TestDividingFractions() {
			var frac1 = new Fraction(-1, 8);
			var result1 = frac1.Divide(new Fraction(5, 8));
			Assert.AreEqual("-1/5", result1.ToString());
			var result2 = frac1.Divide(new Fraction(2, 8));
			Assert.AreEqual("-1/2", result2.ToString());
			var result3 = frac1 / new Fraction(-13, 8);
			Assert.AreEqual("1/13", result3.ToString());
		}

		[Test]
		public void TestFractionInverse() {
			var frac1 = new Fraction(-78, 24);
			Assert.AreEqual("-13/4", frac1.ToString());
			Assert.AreEqual("-4/13", frac1.Inverse().ToString());
		}

		[Test]
		public void TestFractionNegation() {
			var frac1 = new Fraction(-78, 24);
			Assert.AreEqual("-13/4", frac1.ToString());
			Assert.AreEqual("13/4", frac1.Negate().ToString());
		}

		#endregion

		#region Extended Math

		[Test]
		public void TestAbsFraction() {
			Assert.AreEqual(new Fraction(1, 10), new Fraction(1, 10).Abs());
			Assert.AreEqual(Fraction.PositiveInfinity, Fraction.NegativeInfinity.Abs());
			Assert.AreEqual(new Fraction(2, 3), new Fraction(-2, 3).Abs());
			Assert.AreEqual(Fraction.Zero, new Fraction(0).Abs());
		}

		[Test]
		public void TestPowFraction() {
			Assert.AreEqual(new Fraction(1, 100), new Fraction(1, 10).Pow(2));
			Assert.AreEqual(new Fraction(-8, 125), new Fraction(-2, 5).Pow(3));
			Assert.AreEqual(new Fraction(4, 25), new Fraction(-2, 5).Pow(2));
		}

		[Test]
		public void TestSqrtFraction() {
			Assert.AreEqual(Fraction.NaN, Fraction.NaN.Sqrt());
			Assert.AreEqual(new Fraction(2), new Fraction(4).Sqrt());
			Assert.AreEqual(new Fraction(52, 21), new Fraction(52, 21).Pow(2).Sqrt());
		}

		#endregion

		#region Compare

		[Test]
		public void TestCompareFractions() {
			Assert.AreEqual(-1, new Fraction(2, 3).CompareTo(0.7));
			Assert.AreEqual(1, new Fraction(-1, 8).CompareTo(-2));
			Assert.AreEqual(0, new Fraction(6, 8).CompareTo("3/4"));
		}

		#endregion

		#region Indeterminates

		[Test]
		public void TestIndeterminateFraction() {
			Assert.IsTrue(Fraction.NaN.IsNaN());
			Assert.IsFalse(Fraction.NaN.IsInfinity());
			Assert.IsFalse(Fraction.NaN.IsNegativeInfinity());

			Assert.IsTrue(Fraction.PositiveInfinity.IsInfinity());
			Assert.IsTrue(Fraction.NegativeInfinity.IsInfinity());
			Assert.IsTrue(Fraction.PositiveInfinity.IsPositiveInfinity());
			Assert.IsTrue(Fraction.NegativeInfinity.IsNegativeInfinity());
			Assert.IsFalse(Fraction.PositiveInfinity.IsNaN());
		}

		[Test]
		public void TestIndeterminateFractionValues() {
			Assert.AreEqual(0, Fraction.NaN.Denominator);
			Assert.AreEqual(0, Fraction.PositiveInfinity.Denominator);
			Assert.AreEqual(0, Fraction.NegativeInfinity.Denominator);

			Assert.AreEqual(0, Fraction.NaN.Numerator);
			Assert.AreEqual(1, Fraction.PositiveInfinity.Numerator);
			Assert.AreEqual(-1, Fraction.NegativeInfinity.Numerator);

			Assert.AreEqual(Fraction.PositiveInfinity, new Fraction(1, 0));
			Assert.AreEqual(Fraction.NegativeInfinity, new Fraction(-687, 0));
			Assert.AreEqual(Fraction.NaN, new Fraction(0, 0));
		}

		[Test]
		public void TestIndeterminateFractionToString() {
			Assert.AreEqual("NaN", Fraction.NaN.ToString());
			Assert.AreEqual("Infinity", Fraction.PositiveInfinity.ToString());
			Assert.AreEqual("-Infinity", Fraction.NegativeInfinity.ToString());
		}

		#endregion

		#region ToString

		[Test]
		public void TestGeneralFractionToString() {
			Assert.AreEqual("-2/3", new Fraction(-2, 3).ToString("G", null));
			Assert.AreEqual("-2.000/3.000", new Fraction(-2, 3).ToString("GF3", null));
		}

		[Test]
		public void TestRatioFractionToString() {
			Assert.AreEqual("-2:3", new Fraction(-2, 3).ToString("R", null));
		}

		[Test]
		public void TestMixedFractionToString() {
			Assert.AreEqual("1", new Fraction(3, 3).ToString("M", null));
			Assert.AreEqual("7 1/3", new Fraction(22, 3).ToString("M", null));
			Assert.AreEqual("-2/3", new Fraction(-2, 3).ToString("M", null));
			Assert.AreEqual("-2 1/8", new Fraction(-17, 8).ToString("M", null));
			Assert.AreEqual("-2.000/3.000", new Fraction(-2, 3).ToString("MF3", null));
		}

		#endregion
	}
}
