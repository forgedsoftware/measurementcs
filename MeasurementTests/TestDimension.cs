using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestDimension {

		[TestMethod]
		public void TestCreatingDimension() {
			var dim = new Dimension("hour", 2);
			Assert.AreEqual(2, dim.Power);
			Assert.AreEqual("hour", dim.Unit.Name);
		}

		#region Formatting

		[TestMethod]
		public void TestFormattingDimension() {
			var dim = new Dimension("hour", -32);
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("h⁻³²", result);
		}

		[TestMethod]
		public void TestFormattingDimensionWithPowerOfOne() {
			var dim = new Dimension("hour", 1);
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("h", result);
		}

		[TestMethod]
		public void TestFormattingDimensionWithPowerOfOneShowAllPowers() {
			var dim = new Dimension("hour", 1);
			string result = dim.Format(new QuantityFormatInfo {
				ShowAllPowers = true
			});
			Assert.AreEqual("h¹", result);
		}

		[TestMethod]
		public void TestFormattingDimensionSimpleFullName() {
			var dim = new Dimension("second", 2);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("second squared", result);
		}

		[TestMethod]
		public void TestFormattingDimensionWithPowerOfZeroFullName() {
			var dim = new Dimension("hour", 0);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("hour to the power of 0", result);
		}

		[TestMethod]
		public void TestFormattingDimensionComplexFullName() {
			var dim = new Dimension("minute", -4);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("per minute to the power of 4", result);
		}

		[TestMethod]
		public void TestFormattingDimensionInAscii() {
			var dim = new Dimension("minute", 5);
			string result = dim.Format(new QuantityFormatInfo {
				AsciiOnly = true
			});
			Assert.AreEqual("min^5", result);
		}

		[TestMethod]
		public void TestFormattingWithPrefix() {
			var dim = new Dimension("second", "mega");
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("Ms", result);
		}

		[TestMethod]
		public void TestFormattingWithPrefixFullName() {
			var dim = new Dimension("metre", "pico");
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("picometre", result);
		}

		#endregion
	}
}
