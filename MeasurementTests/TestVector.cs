using System;
using System.Globalization;
using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestVector {

		#region General

		[TestMethod]
		public void TestVectorCreateWithArray() {
			var v1 = new Vector3(new[] {2.3, 4.5, 2.1});
			Assert.AreEqual(2.3, v1.X);
			Assert.AreEqual(4.5, v1.Y);
			Assert.AreEqual(2.1, v1.Z);
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "Should throw error if less than 3 elements are supplied in the array")]
		public void TestVectorCreateWithArrayNotEnoughValues() {
			var v1 = new Vector3(new[] {2.1});
		}

		#endregion

		#region Vector Specific Functions

		[TestMethod]
		public void TestVectorMagnitude() {
			Assert.AreEqual(0, Vector3.Origin.Magnitude);
			Assert.AreEqual(1, Vector3.XAxis.Magnitude);
			Assert.AreEqual(3.7417, new Vector3(1, 2, 3).Magnitude, 0.0001);
		}

		[TestMethod]
		public void TestVectorSumComponents() {
			Assert.AreEqual(32.5, new Vector3(-5, 36, 1.5).SumComponents);
		}

		[TestMethod]
		public void TestVectorDotProduct() {
			var v1 = new Vector3(1, 2, 3);
			var v2 = new Vector3(3, 2, 1);
			Assert.AreEqual(0, Vector3.XAxis.DotProduct(Vector3.YAxis));
			Assert.AreEqual(3, Vector3.ZAxis.DotProduct(v1));
			Assert.AreEqual(0, v2.DotProduct(Vector3.Origin));
			Assert.AreEqual(10, v1.DotProduct(v2));
		}

		[TestMethod]
		public void TestVectorCrossProduct() {
			var v1 = new Vector3(2, 3, 4);
			var v2 = new Vector3(3, 1, 1);
			Assert.AreEqual(Vector3.ZAxis, Vector3.XAxis.CrossProduct(Vector3.YAxis));
			Assert.AreEqual(new Vector3(-1, 10, -7), v1.CrossProduct(v2));
			Assert.AreEqual(new Vector3(1, -10, 7), v2.CrossProduct(v1));
		}

		[TestMethod]
		public void TestVectorAngle() {
			Assert.AreEqual(Math.PI/2, Vector3.XAxis.Angle(Vector3.YAxis));
			Assert.AreEqual(0, Vector3.ZAxis.Angle(Vector3.ZAxis));
		}

		[TestMethod]
		public void TestVectorUnitVectors() {
			Assert.IsFalse(Vector3.Origin.IsUnitVector());
			Assert.IsTrue(Vector3.XAxis.IsUnitVector());
			Assert.IsTrue(Vector3.YAxis.IsUnitVector());
			Assert.IsTrue(Vector3.ZAxis.IsUnitVector());
			Assert.IsFalse(new Vector3(0, 1, 1).IsUnitVector());
			Assert.IsTrue(new Vector3(Math.Sqrt(0.5), 0, Math.Sqrt(0.5)).IsUnitVector());
		}

		[TestMethod]
		public void TestVectorNormalize() {
			Assert.IsTrue(Vector3.XAxis.Normalize.IsUnitVector());
			Assert.IsTrue(new Vector3(1, 45345, -23.656).Normalize.IsUnitVector());
			Assert.IsTrue(new Vector3(1454534.234234, 453452.123, -2233.656123).Normalize.IsUnitVector());
		}

		[TestMethod]
		[ExpectedException(typeof(DivideByZeroException), "If the magnitude of the vector is 0, it should throw an exception")]
		public void TestVectorNormalizeDivideByZero() {
			var v1 = Vector3.Origin.Normalize;
		}

		[TestMethod]
		public void TestVectorDistance() {
			Assert.AreEqual(Math.Sqrt(2), Vector3.XAxis.Distance(Vector3.YAxis));
			Assert.AreEqual(1, Vector3.XAxis.Distance(Vector3.Origin));
		}

		[TestMethod]
		public void TestVectorIsPerpendicular() {
			Assert.IsTrue(Vector3.XAxis.IsPerpendicular(Vector3.YAxis));
			Assert.IsTrue(Vector3.Origin.IsPerpendicular(new Vector3(233, 23, 2)));
			Assert.IsFalse(Vector3.XAxis.IsPerpendicular(new Vector3(2, 3, 4)));
		}

		#endregion

		#region Math

		[TestMethod]
		public void TestVectorAdd() {
			Assert.AreEqual(Vector3.XAxis, Vector3.Origin.Add(Vector3.XAxis));
			Assert.AreEqual(new Vector3(2, 3.5, 29.1), new Vector3(2, 4.5, 7).Add(new Vector3(0, -1, 22.1)));
		}

		[TestMethod]
		public void TestVectorSubtract() {
			Assert.AreEqual(new Vector3(-1, 0, 1), Vector3.ZAxis.Subtract(Vector3.XAxis));
			Assert.AreEqual(new Vector3(2, 1, -4.5), new Vector3(5, 2, 3.5).Subtract(new Vector3(3, 1, 8)));
		}

		[TestMethod]
		public void TestVectorMultiply() {
			var v1 = new Vector3(5, 1, 2);
			var v2 = new Vector3(1, 7, -1);
			Assert.AreEqual(Vector3.YAxis, Vector3.ZAxis.Multiply(Vector3.XAxis));
			Assert.AreEqual(new Vector3(-15, 7, 34), v1.Multiply(v2));
			Assert.AreEqual(new Vector3(15, -7, -34), v2.Multiply(v1));
		}

		[TestMethod]
		[ExpectedException(typeof (InvalidOperationException), "Should have exception as invalid operation")]
		public void TestVectorDivide() {
			var v1 = new Vector3(1, 1, 1).Divide(new Vector3(2, 3, 4));
		}

		[TestMethod]
		[ExpectedException(typeof (InvalidOperationException), "Should have exception as invalid operation")]
		public void TestVectorAddScalar() {
			var v1 = new Vector3(1, 1, 1).Add(2);
		}

		[TestMethod]
		[ExpectedException(typeof (InvalidOperationException), "Should have exception as invalid operation")]
		public void TestVectorSubtractScalar() {
			var v1 = new Vector3(1, 1, 1).Subtract(2);
		}

		[TestMethod]
		public void TestVectorMultiplyScalar() {
			var v1 = new Vector3(5, 1, 2);
			var v2 = new Vector3(1, 7, -1);
			Assert.AreEqual(new Vector3(0, 0, 13), Vector3.ZAxis.Multiply(13));
			Assert.AreEqual(new Vector3(15, 3, 6), v1.Multiply(3));
			Assert.AreEqual(new Vector3(-2, -14, 2), v2.Multiply(-2));
		}

		[TestMethod]
		public void TestVectorDivideScalar() {
			var v1 = new Vector3(2, 82, 6);
			var v2 = new Vector3(25, 7.5, -2.5);
			Assert.AreEqual(new Vector3(0, 0, -2), Vector3.ZAxis.Divide(-0.5));
			Assert.AreEqual(new Vector3(1, 41, 3), v1.Divide(2));
			Assert.AreEqual(new Vector3(-5, -1.5, 0.5), v2.Divide(-5));
		}

		[TestMethod]
		public void TestVectorNegate() {
			Assert.AreEqual(new Vector3(0, -1, 0), Vector3.YAxis.Negate());
			Assert.AreEqual(new Vector3(343, -23.56, -8), new Vector3(-343, 23.56, 8).Negate());
		}

		#endregion

		#region Math Functions

		[TestMethod]
		public void TestVectorPow() {
			Assert.AreEqual(Vector3.Origin, Vector3.Origin.Pow(3));
			Assert.AreEqual(Vector3.XAxis, Vector3.XAxis.Pow(5));
			Assert.AreEqual(new Vector3(16, 144, 9), new Vector3(4, 12, 3).Pow(2));
		}

		[TestMethod]
		public void TestVectorSqrt() {
			Assert.AreEqual(Vector3.Origin, Vector3.Origin.Sqrt());
			Assert.AreEqual(Vector3.XAxis, Vector3.XAxis.Sqrt());
			Assert.AreEqual(new Vector3(4, 1, 2), new Vector3(16, 1, 4).Sqrt());
		}

		[TestMethod]
		public void TestVectorMaxMin() {
			var v1 = new Vector3(1, 2, 3);
			var v2 = new Vector3(2, 3, 4);
			Assert.AreEqual(Vector3.XAxis, Vector3.XAxis.Max(Vector3.YAxis));
			Assert.AreEqual(Vector3.ZAxis, Vector3.ZAxis.Max(Vector3.Origin));
			Assert.AreEqual(Vector3.XAxis, Vector3.XAxis.Min(Vector3.YAxis));
			Assert.AreEqual(Vector3.Origin, Vector3.ZAxis.Min(Vector3.Origin));
			Assert.AreEqual(v2, v1.Max(v2));
			Assert.AreEqual(v1, v1.Min(v2));
		}

		#endregion

		#region Equals, Compare, Equality

		[TestMethod]
		public void TestVectorEquals() {
			var v1 = new Vector3(1, 2, 3);
			var v2 = new Vector3(1, 3, 2);
			var v3 = new Vector3(1, 2, 3);
			Assert.AreEqual(v1, v3);
			Assert.AreNotEqual(v1, v2);
		}

		[TestMethod]
		public void TestVectorEqualsStandardVectors() {
			Assert.AreEqual(Vector3.XAxis, Vector3.XAxis);
			Assert.AreNotEqual(Vector3.Origin, Vector3.XAxis);
			Assert.AreNotEqual(Vector3.XAxis, Vector3.YAxis);
			Assert.AreNotEqual(Vector3.YAxis, Vector3.ZAxis);
		}

		[TestMethod]
		public void TestVectorCompare() {
			var v1 = new Vector3(7, 8, 10);
			var v2 = new Vector3(4.2, 7.777, 6.54);
			Assert.AreEqual(-1, Vector3.Origin.CompareTo(Vector3.XAxis));
			Assert.AreEqual(0, Vector3.YAxis.CompareTo(Vector3.ZAxis));
			Assert.AreEqual(1, Vector3.XAxis.CompareTo(Vector3.Origin));
			Assert.AreEqual(1, v1.CompareTo(v2));
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException),
			"An exception should be thrown if the provided object is of the wrong type")]
		public void TestVectorCompareWithWrongType() {
			var a = new Vector3(8, 7, 6).CompareTo("8.76");
		}

		[TestMethod]
		public void TestVectorLessThanGreaterThan() {
			Assert.IsTrue(Vector3.XAxis > Vector3.Origin);
			Assert.IsFalse(Vector3.XAxis > Vector3.YAxis);
			Assert.IsTrue(Vector3.YAxis >= Vector3.ZAxis);
			Assert.IsTrue(new Vector3(2, 3, 4) < new Vector3(3, 3, 4));
			Assert.IsTrue(Vector3.XAxis <= Vector3.ZAxis);
		}

		[TestMethod]
		public void TestVectorCopy() {
			var v1 = new Vector3(23, 122, -2323.2);
			Assert.AreEqual(v1, v1.Copy());
		}

		#endregion

		#region ToString

		[TestMethod]
		public void TestVectorToString() {
			Assert.AreEqual("(9, 3, 4)", new Vector3(9, 3, 4).ToString());
			Assert.AreEqual("(0, 0, -2)", new Vector3(0, 0, -2).ToString());
			Assert.AreEqual("(3.42, 8.9543, 7)", new Vector3(3.42, 8.9543, 7).ToString());
		}

		[TestMethod]
		public void TestVectorCustomToString() {
			var v1 = new Vector3(8324.32, 2342.34, -23.5456534);
			Assert.AreEqual("8324.32", v1.ToString("X", CultureInfo.InvariantCulture));
			Assert.AreEqual("2342", v1.ToString("YF0", null));
			Assert.AreEqual("-2.355E+001", v1.ToString("ZE3", null));
			Assert.AreEqual("(x=8324, y=2342, z=-24)", v1.ToString("LF0", null));
			Assert.AreEqual("[8.3E+003, 2.3E+003, -2.4E+001]", v1.ToString("AE1", null));
			Assert.AreEqual("(8324, 2342, -24)", v1.ToString("GF0", null));
		}

		#endregion

	}
}
