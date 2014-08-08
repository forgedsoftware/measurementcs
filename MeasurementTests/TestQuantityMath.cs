using System;
using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestQuantityMath {

		#region Basic Math - Multiply

		// TODO

		#endregion

		#region Basic Math - Divide

		// TODO

		#endregion

		#region Basic Math - Add

		[TestMethod]
		public void TestAddBasic() {
			Quantity result = new Quantity(15).Add(2.4);
			Assert.AreEqual(17.4, result.Value);
			Assert.IsTrue(result.IsDimensionless());
		}

		[TestMethod]
		public void TestAddDimensionless() {
			Quantity result = new Quantity(15, new[] { "minute" }).Add(2.4);
			Assert.AreEqual(17.4, result.Value);
			Assert.IsFalse(result.IsDimensionless());
		}

		[TestMethod]
		public void TestAddQuantity() {
			Quantity result = new Quantity(2.1, new[] { "minute" }).Add(new Quantity(3.4, new[] { "minute" }));
			Assert.AreEqual(5.5, result.Value);
			Assert.IsFalse(result.IsDimensionless());
			Assert.AreEqual(1, result.Dimensions.Count);
		}

		[TestMethod]
		public void TestAddDimensionlessQuantities() {
			Quantity result = new Quantity(12.3).Add(new Quantity(13.23));
			Assert.AreEqual(25.53, result.Value);
			Assert.IsTrue(result.IsDimensionless());
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Adding a quantities without commensurable dimensions should fail")]
		public void TestAddDimensionlessNoCommensurableQuantity() {
			new Quantity(15, new[] {"minute"}).Add(new Quantity(2.4));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Adding a quantities without commensurable dimensions should fail")]
		public void TestAddNoCommensurableQuantity() {
			new Quantity(15, new[] { "minute" }).Add(new Quantity(2.4, new[] {"metre" }));
		}

		[TestMethod]
		public void TestAddCommensurableQuantities() {
			Quantity result = new Quantity(3.2, new[] { "minute" }).Add(new Quantity(30, new[] { "second" }));
			Assert.AreEqual(3.7, result.Value);
			Assert.AreEqual(1, result.Dimensions.Count);
			Assert.AreEqual("minute", result.Dimensions[0].Unit.Name);
		}

		[TestMethod]
		public void TestAddComplexCommensurableQuantities() {
			Quantity result = new Quantity(3.2, new[] { "minute", "metre", "coulomb" })
				.Add(new Quantity(3, new[] { "second", "mile", "coulomb" }));
			Assert.AreEqual(83.6672, result.Value, 0.0001);
			Assert.AreEqual(3, result.Dimensions.Count);
		}

		#endregion

		#region Basic Math - Subtract

		[TestMethod]
		public void TestSubtractBasic() {
			Quantity result = new Quantity(5.4).Subtract(2.2);
			Assert.AreEqual(3.2, result.Value);
			Assert.IsTrue(result.IsDimensionless());
		}

		[TestMethod]
		public void TestSubtractDimensionless() {
			Quantity result = new Quantity(7.82, new[] { "minute" }).Subtract(4.22);
			Assert.AreEqual(3.6, result.Value, 0.00001);
			Assert.IsFalse(result.IsDimensionless());
		}

		[TestMethod]
		public void TestSubtractQuantity() {
			Quantity result = new Quantity(4.1, new[] { "minute" }).Subtract(new Quantity(1.2, new[] { "minute" }));
			Assert.AreEqual(2.9, result.Value, 0.00001);
			Assert.IsFalse(result.IsDimensionless());
			Assert.AreEqual(1, result.Dimensions.Count);
		}

		[TestMethod]
		public void TestSubtractDimensionlessQuantities() {
			Quantity result = new Quantity(17.33).Subtract(new Quantity(13.23));
			Assert.AreEqual(4.1, result.Value, 0.00001);
			Assert.IsTrue(result.IsDimensionless());
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Subtracting a quantities without commensurable dimensions should fail")]
		public void TestSubtractDimensionlessNoCommensurableQuantity() {
			new Quantity(15, new[] { "minute" }).Subtract(new Quantity(2.4));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Subtracting a quantities without commensurable dimensions should fail")]
		public void TestSubtractNoCommensurableQuantity() {
			new Quantity(15, new[] { "minute" }).Subtract(new Quantity(2.4, new[] { "metre" }));
		}

		[TestMethod]
		public void TestSubtractCommensurableQuantities() {
			Quantity result = new Quantity(3.2, new[] { "minute" }).Subtract(new Quantity(30, new[] { "second" }));
			Assert.AreEqual(2.7, result.Value);
			Assert.AreEqual(1, result.Dimensions.Count);
			Assert.AreEqual("minute", result.Dimensions[0].Unit.Name);
		}

		[TestMethod]
		public void TestSubtractComplexCommensurableQuantities() {
			Quantity result = new Quantity(82, new[] { "minute", "metre", "coulomb" })
				.Subtract(new Quantity(3, new[] { "second", "mile", "coulomb" }));
			Assert.AreEqual(1.532799, result.Value, 0.0001);
			Assert.AreEqual(3, result.Dimensions.Count);
		}

		#endregion

		#region Math Functions

		[TestMethod]
		public void TestMathFuncAbs() {
			Assert.AreEqual(4.5, new Quantity(-4.5).Abs().Value);
			Assert.AreEqual(2.576, new Quantity(2.576).Abs().Value);
			Assert.AreEqual(0, new Quantity(0).Abs().Value);
		}

		[TestMethod]
		public void TestMathFuncAcos() {
			Assert.AreEqual(1.36944, new Quantity(0.2).Acos().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncAsin() {
			Assert.AreEqual(0.20136, new Quantity(0.2).Asin().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncAtan() {
			Assert.AreEqual(0.19740, new Quantity(0.2).Atan().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncCeiling() {
			Assert.AreEqual(-2, new Quantity(-2.5).Ceiling().Value);
			Assert.AreEqual(2, new Quantity(2).Ceiling().Value);
			Assert.AreEqual(3, new Quantity(2.1).Ceiling().Value);
		}

		[TestMethod]
		public void TestMathFuncCos() {
			Assert.AreEqual(0.98007, new Quantity(0.2).Cos().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncExp() {
			Assert.AreEqual(54.59815, new Quantity(4).Exp().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncFloor() {
			Assert.AreEqual(-3, new Quantity(-2.5).Floor().Value);
			Assert.AreEqual(2, new Quantity(2).Floor().Value);
			Assert.AreEqual(2, new Quantity(2.1).Floor().Value);
		}

		[TestMethod]
		public void TestMathFuncLog() {
			Assert.AreEqual(0.69315, new Quantity(2).Log().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncLog10() {
			Assert.AreEqual(0.30103, new Quantity(2).Log10().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncRound() {
			Assert.AreEqual(-2, new Quantity(-2.5).Round().Value);
			Assert.AreEqual(2, new Quantity(1.6).Round().Value);
			Assert.AreEqual(2, new Quantity(2.1).Round().Value);
		}

		[TestMethod]
		public void TestMathFuncSin() {
			Assert.AreEqual(0.90929, new Quantity(2).Sin().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncSqrt() {
			Assert.AreEqual(1.41421, new Quantity(2).Sqrt().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncTan() {
			Assert.AreEqual(-2.18504, new Quantity(2).Tan().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncSinh() {
			Assert.AreEqual(3.62686, new Quantity(2).Sinh().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncCosh() {
			Assert.AreEqual(3.76220, new Quantity(2).Cosh().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncTanh() {
			Assert.AreEqual(0.964027, new Quantity(2).Tanh().Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncAtan2() {
			Assert.AreEqual(0.9827937, new Quantity(2).Atan2(3).Value, 0.0001);
			Assert.AreEqual(0.9827937, new Quantity(2).Atan2(new Quantity(3)).Value, 0.0001);
		}

		[TestMethod]
		public void TestMathFuncPow() {
			Assert.AreEqual(8, new Quantity(2).Pow(3).Value);
			Assert.AreEqual(8, new Quantity(2).Pow(new Quantity(3)).Value);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Only dimensionless power values should be allowed.")]
		public void TestMathFuncPowNotDimensionless() {
			Assert.AreEqual(8, new Quantity(2).Pow(new Quantity(3, new[] { "minute" })).Value);
		}

		[TestMethod]
		public void TestMathFuncMax() {
			Assert.AreEqual(3, new Quantity(2).Max(3).Value);
			Assert.AreEqual(8.7, new Quantity(8.7).Max(new Quantity(7.2)).Value);
			Assert.AreEqual(60, new Quantity(59, new[] { "second" }).Max(new Quantity(1, new[] { "minute" })).Value);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Only commensurable quantities are allowed.")]
		public void TestMathFuncMaxMustBeCommensurable() {
			new Quantity(4, new[] { "second" }).Max(new Quantity(5, new[] { "metre" }));
		}

		[TestMethod]
		public void TestMathFuncMaxVarargs() {
			Assert.AreEqual(12.1, new Quantity(2).Max(3, 7, 8, 2.4, 12.1).Value);
			Assert.AreEqual(42.1, new Quantity(42.1).Max(new Quantity(7.2), new Quantity(1.4)).Value);
			Assert.AreEqual(7.5, new Quantity(6, "hour").Max(new Quantity(0.2, "day"), new Quantity(450, "minute")).Value);
		}

		[TestMethod]
		public void TestMathFuncMin() {
			Assert.AreEqual(2, new Quantity(2).Min(3).Value);
			Assert.AreEqual(7.2, new Quantity(8.7).Min(new Quantity(7.2)).Value);
			Assert.AreEqual(59, new Quantity(59, new[] { "second" }).Min(new Quantity(1, new[] { "minute" })).Value);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Only commensurable quantities are allowed.")]
		public void TestMathFuncMinMustBeCommensurable() {
			new Quantity(4, new[] { "second" }).Min(new Quantity(5, new[] { "metre" }));
		}

		[TestMethod]
		public void TestMathFuncMinVarargs() {
			Assert.AreEqual(2, new Quantity(2).Min(3, 7, 8, 2.4, 12.1).Value);
			Assert.AreEqual(1.4, new Quantity(42.1).Min(new Quantity(7.2), new Quantity(1.4)).Value);
			Assert.AreEqual(4.8, new Quantity(6, "hour").Min(new Quantity(0.2, "day"), new Quantity(450, "minute")).Value);
		}

		#endregion

	}
}
