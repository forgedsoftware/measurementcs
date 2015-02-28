using System;
using ForgedSoftware.Measurement.Number;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestUncertainty {

		#region Constructing Object

		[Test]
		public void TestUncertaintyFromRange() {
			Uncertainty u = Uncertainty.FromRange(2, 1, 4);
			Assert.AreEqual(2, u.Value);
			Assert.AreEqual(1, u.LowerUncertainty);
			Assert.AreEqual(2, u.UpperUncertainty);
			Assert.IsFalse(u.IsRelative);
		}

		[Test]
		public void TestUncertaintyFromPercentage() {
			Uncertainty u = Uncertainty.FromPercentage(2, 0, 1.25);
			Assert.AreEqual(2, u.Value);
			Assert.AreEqual(0, u.LowerUncertainty);
			Assert.AreEqual(2.5, u.UpperUncertainty);
			Assert.IsTrue(u.IsRelative);
		}

		[Test]
		public void TestUncertaintyFromEvenValues() {
			var u = new Uncertainty(4.5, 1.2);
			Assert.AreEqual(1.2, u.LowerUncertainty);
			Assert.AreEqual(1.2, u.UpperUncertainty);
			Assert.IsFalse(u.IsRelative);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void TestUncertaintyLowerUncertaintyPositive() {
			var u = new Uncertainty(1, -0.1, 0.1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestUncertaintyUpperUncertaintyPositive() {
			var u = new Uncertainty(1, 0.1, -0.1);
		}

		#endregion

		#region Properties & Functions

		[Test]
		public void TestUncertaintyProperties() {
			var u = new Uncertainty(7.6, 1.1, 0.4);
			Assert.AreEqual(1.5, u.TotalUncertainty);
			Assert.AreEqual(6.5, u.Minimum);
			Assert.AreEqual(8.0, u.Maximum);
			Assert.AreEqual(0.1447368, u.LowerPercentage, 0.00001);
			Assert.AreEqual(0.0526316, u.UpperPercentage, 0.00001);
			Assert.IsFalse(u.IsSymmetric());
		}

		[Test]
		public void TestUncertaintySymmetric() {
			var u1 = new Uncertainty(6, 0.5);
			var u2 = Uncertainty.FromPercentage(7, 0.10, 0.10);
			Assert.IsTrue(u1.IsSymmetric());
			Assert.AreEqual(u1.LowerUncertainty, u1.UpperUncertainty);
			Assert.AreEqual(u1.LowerPercentage, u1.UpperPercentage);
			Assert.IsTrue(u2.IsSymmetric());
			Assert.AreEqual(u2.LowerUncertainty, u2.UpperUncertainty);
			Assert.AreEqual(u2.LowerPercentage, u2.UpperPercentage);
		}

		[Test]
		public void TestUncertaintyIsConsistent() {
			var u1 = new Uncertainty(7, 0.6);
			var u2 = new Uncertainty(6, 0.5);
			var u3 = new Uncertainty(5, 0.5);
			Assert.IsTrue(u1.IsConsistent(u2));
			Assert.IsFalse(u1.IsConsistent(u3));
			Assert.IsTrue(u2.IsConsistent(u3));
		}

		#endregion

		#region Math Functions

		[Test]
		public void TestUncertaintyPow() {
			var u1 = new Uncertainty(2, 0.2);
			Assert.AreEqual(0.1, u1.UpperPercentage);
			Assert.IsFalse(u1.IsRelative);
			var u2 = u1.Pow(2);
			Assert.AreEqual(4, u2.Value);
			Assert.AreEqual(0.2, u2.UpperPercentage);
			Assert.AreEqual(0.8, u2.UpperUncertainty);
			Assert.IsTrue(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintySqrt() {
			var u1 = new Uncertainty(9, 0.4);
			var u2 = u1.Sqrt();
			Assert.AreEqual(3, u2.Value);
			Assert.AreEqual(0.0666667, u2.UpperUncertainty, 0.000001);
			Assert.IsTrue(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyMax() {
			var u1 = new Uncertainty(2, 1, 0.5);
			var u2 = new Uncertainty(3, 0.5, 1);
			var u3 = u1.Max(u2);
			Assert.AreEqual(u2, u3);
		}

		[Test]
		public void TestUncertaintyMin() {
			var u1 = new Uncertainty(4, 1, 0.5);
			var u2 = new Uncertainty(4, 0.5, 1);
			Assert.AreEqual(u1, u1.Min(u2));
			var u3 = new Uncertainty(2, 3, 3);
			Assert.AreEqual(u3, u2.Min(u3));
		}

		#endregion

		#region Basic Math

		[Test]
		public void TestUncertaintyAdd() {
			var u1 = new Uncertainty(6, 0.5, 0.2);
			var u2 = new Uncertainty(2.1, 0.1, 0.3);
			var u3 = u1.Add(u2);
			Assert.AreEqual(u3, u1 + u2);
			Assert.AreEqual(8.1, u3.Value);
			Assert.AreEqual(0.6, u3.LowerUncertainty);
			Assert.AreEqual(0.5, u3.UpperUncertainty);
			Assert.IsFalse(u3.IsRelative);
		}

		[Test]
		public void TestUncertaintySubtract() {
			var u1 = new Uncertainty(12, 1.1, 0.8);
			var u2 = new Uncertainty(4.3, 1, 0.2);
			var u3 = u1.Subtract(u2);
			Assert.AreEqual(u3, u1 - u2);
			Assert.AreEqual(7.7, u3.Value);
			Assert.AreEqual(2.1, u3.LowerUncertainty);
			Assert.AreEqual(1.0, u3.UpperUncertainty);
			Assert.IsFalse(u3.IsRelative);
		}

		[Test]
		public void TestUncertaintyMultiply() {
			var u1 = new Uncertainty(5, 0.5, 1.0);
			var u2 = new Uncertainty(2.2, 0.1, 0.1);
			var u3 = u1.Multiply(u2);
			Assert.AreEqual(u3, u1 * u2);
			Assert.AreEqual(11, u3.Value);
			Assert.AreEqual(1.6, u3.LowerUncertainty, 0.00001);
			Assert.AreEqual(2.7, u3.UpperUncertainty, 0.00001);
			Assert.IsTrue(u3.IsRelative);
		}

		[Test]
		public void TestUncertaintyDivide() {
			var u1 = new Uncertainty(12, 0.2, 1.0);
			var u2 = new Uncertainty(3, 0.1, 0.1);
			var u3 = u1.Divide(u2);
			Assert.AreEqual(u3, u1 / u2);
			Assert.AreEqual(4, u3.Value);
			Assert.AreEqual(0.2, u3.LowerUncertainty, 0.00001);
			Assert.AreEqual(0.466667, u3.UpperUncertainty, 0.00001);
			Assert.IsTrue(u3.IsRelative);
		}

		[Test]
		public void TestUncertaintyAddConstant() {
			var u1 = new Uncertainty(7.6, 0.2, 0.3);
			var u2 = u1.Add(7);
			Assert.AreEqual(u2, u1 + 7.0);
			Assert.AreEqual(u2, 7.0 + u1);
			Assert.AreEqual(14.6, u2.Value);
			Assert.AreEqual(0.2, u2.LowerUncertainty);
			Assert.AreEqual(0.3, u2.UpperUncertainty);
			Assert.IsFalse(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintySubtractConstant() {
			var u1 = new Uncertainty(7.6, 0.2, 0.3);
			var u2 = u1.Subtract(7);
			Assert.AreEqual(u2, u1 - 7.0);
			Assert.AreEqual(-0.6, (7.0 - u1).Value, 0.00001);
			Assert.AreEqual(0.6, u2.Value, 0.00001);
			Assert.AreEqual(0.2, u2.LowerUncertainty);
			Assert.AreEqual(0.3, u2.UpperUncertainty);
			Assert.IsFalse(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyMultiplyConstant() {
			var u1 = new Uncertainty(7, 0.2, 0.7);
			var u2 = u1.Multiply(3);
			Assert.AreEqual(u2, u1 * 3);
			Assert.AreEqual(u2, 3 * u1);
			Assert.AreEqual(21, u2.Value);
			Assert.AreEqual(0.6, u2.LowerUncertainty, 0.00001);
			Assert.AreEqual(2.1, u2.UpperUncertainty, 0.00001);
			Assert.IsFalse(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyMultiplyConstantRelative() {
			var u1 = Uncertainty.FromPercentage(8, 0.2, 0.3);
			var u2 = u1.Multiply(2);
			Assert.AreEqual(u2, u1 * 2);
			Assert.AreEqual(u2, 2 * u1);
			Assert.AreEqual(16, u2.Value);
			Assert.AreEqual(0.2, u2.LowerPercentage, 0.00001);
			Assert.AreEqual(0.3, u2.UpperPercentage, 0.00001);
			Assert.AreEqual(3.2, u2.LowerUncertainty, 0.00001);
			Assert.AreEqual(4.8, u2.UpperUncertainty, 0.00001);
			Assert.IsTrue(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyDivideConstant() {
			var u1 = new Uncertainty(6, 0.3, 0.2);
			var u2 = u1.Divide(3);
			Assert.AreEqual(u2, u1 / 3);
			Assert.AreEqual(0.5, (3 / u1).Value, 0.00001);
			Assert.AreEqual(2, u2.Value);
			Assert.AreEqual(0.1, u2.LowerUncertainty, 0.00001);
			Assert.AreEqual(0.0666667, u2.UpperUncertainty, 0.00001);
			Assert.IsFalse(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyDivideConstantRelative() {
			var u1 = Uncertainty.FromPercentage(8, 0.1, 0.2);
			var u2 = u1.Divide(2);
			Assert.AreEqual(u2, u1 / 2);
			Assert.AreEqual(0.25, (2 / u1).Value, 0.00001);
			Assert.AreEqual(4, u2.Value);
			Assert.AreEqual(0.1, u2.LowerPercentage, 0.00001);
			Assert.AreEqual(0.2, u2.UpperPercentage, 0.00001);
			Assert.AreEqual(0.4, u2.LowerUncertainty, 0.00001);
			Assert.AreEqual(0.8, u2.UpperUncertainty, 0.00001);
			Assert.IsTrue(u2.IsRelative);
		}

		[Test]
		public void TestUncertaintyNegate() {
			var u1 = new Uncertainty(6.2, 0.9, 0.1);
			var u2 = u1.Negate();
			Assert.AreEqual(u2, -u1);
			Assert.AreEqual(u2, u1 * -1);
			Assert.AreEqual(-6.2, u2.Value);
			Assert.AreEqual(0.9, u2.LowerUncertainty);
			Assert.AreEqual(0.1, u2.UpperUncertainty);
			Assert.IsFalse(u2.IsRelative);
		}

		#endregion

		#region Equality, Comparisons

		[Test]
		public void TestUncertaintyEquality() {
			var u1 = new Uncertainty(3, 0.1);
			var u2 = new Uncertainty(4, 0.1);
			var u3 = new Uncertainty(5, 0.3);
			var u4 = new Uncertainty(3, 0.2);

			Assert.IsTrue(u2 > u1);
			Assert.IsTrue(u3 >= u2);
			Assert.IsFalse(u1 > u4);

			Assert.IsTrue(u2 < u3);
			Assert.IsTrue(u4 <= u3);

			Assert.IsFalse(u4 == u1);
			Assert.IsFalse(u3 == u2);
			Assert.IsTrue(u3 == u3.Clone());
		}

		[Test]
		public void TestUncertaintyEquals() {
			var u1 = new Uncertainty(0.6, 0.1, 0.2);
			var u2 = new Uncertainty(0.3, 0.1, 0.2);
			Assert.IsTrue(u1.Equals(u1));
			Assert.IsTrue(u1.Equals(u1.Clone()));
			Assert.IsFalse(u1.Equals(u2));
		}

		[Test]
		public void TestUncertaintyHashCode() {
			var u1 = new Uncertainty(0.8, 0.1, 0.2);
			var u2 = new Uncertainty(0.8, 0.2, 0.1);
			Assert.AreNotEqual(u1.GetHashCode(), u2.GetHashCode());
			Assert.AreEqual(u1.GetHashCode(), u1.Clone().GetHashCode());
		}

		[Test]
		public void TestUncertaintyCompare() {
			var u1 = new Uncertainty(0.8, 0.2, 0.1);
			var u2 = new Uncertainty(0.9, 0.2, 0.1);
			Assert.AreEqual(0, u1.CompareTo(u1.Clone()));
			Assert.AreEqual(-1, u1.CompareTo(u2));
			Assert.AreEqual(1, u2.CompareTo(u1));
		}

		#endregion

		#region ToString

		[Test]
		public void TestUncertaintyToString() {
			var u1 = new Uncertainty(3.2, 0.5);
			Assert.AreEqual("3.2 (±0.5)", u1.ToString());
			Assert.AreEqual("3.2 (±0.5)", u1.ToString("G", null));
			Assert.AreEqual("3.2 (±0.5)", u1.ToString("A", null));
			Assert.AreEqual("3.2 (±15.625%)", u1.ToString("R", null));
			Assert.AreEqual("3.2 (±15.6%)", u1.ToString("RF1", null));
			Assert.AreEqual("3.2 (+-0.5)", u1.ToString("T", null));
			Assert.AreEqual("3.2", u1.ToString("V", null));
			Assert.AreEqual("(3.2 (±0.5))", u1.ToString("B", null));
			Assert.AreEqual("3.2 ±0.5", u1.ToString("N", null));
			Assert.AreEqual("3.2 (±0.5) {0}", u1.ToString("F", null));
		}

		[Test]
		public void TestUncertaintyToStringRelative() {
			var u1 = Uncertainty.FromPercentage(2, 0.05, 0.1);
			Assert.AreEqual("2 (+5%, -10%)", u1.ToString());
			Assert.AreEqual("2 (+5%, -10%)", u1.ToString("G", null));
			Assert.AreEqual("2 (+0.1, -0.2)", u1.ToString("A", null));
			Assert.AreEqual("2 (+5%, -10%)", u1.ToString("R", null));
			Assert.AreEqual("2.0 (+5.0%, -10.0%)", u1.ToString("RF1", null));
			Assert.AreEqual("2 (+5%, -10%)", u1.ToString("T", null));
			Assert.AreEqual("2", u1.ToString("V", null));
			Assert.AreEqual("(2 (+5%, -10%))", u1.ToString("B", null));
			Assert.AreEqual("2 +5%, -10%", u1.ToString("N", null));
			Assert.AreEqual("2 {0} (+5%, -10%)", u1.ToString("F", null));
		}

		#endregion

	}
}
