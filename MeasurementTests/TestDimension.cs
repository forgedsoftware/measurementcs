using ForgedSoftware.Measurement;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestDimension {

		[Test]
		public void TestCreatingDimension() {
			var dim = new Dimension("hour", 2);
			Assert.AreEqual(2, dim.Power);
			Assert.AreEqual("hour", dim.Unit.Name);
		}

		#region Formatting

		[Test]
		public void TestFormattingDimension() {
			var dim = new Dimension("hour", -32);
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("h⁻³²", result);
		}

		[Test]
		public void TestFormattingDimensionWithPowerOfOne() {
			var dim = new Dimension("hour", 1);
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("h", result);
		}

		[Test]
		public void TestFormattingDimensionWithPowerOfOneShowAllPowers() {
			var dim = new Dimension("hour", 1);
			string result = dim.Format(new QuantityFormatInfo {
				ShowAllPowers = true
			});
			Assert.AreEqual("h¹", result);
		}

		[Test]
		public void TestFormattingDimensionSimpleFullName() {
			var dim = new Dimension("second", 2);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("second squared", result);
		}

		[Test]
		public void TestFormattingDimensionWithPowerOfZeroFullName() {
			var dim = new Dimension("hour", 0);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("hour to the power of 0", result);
		}

		[Test]
		public void TestFormattingDimensionComplexFullName() {
			var dim = new Dimension("minute", -4);
			string result = dim.Format(new QuantityFormatInfo {
				TextualDescription = true
			});
			Assert.AreEqual("per minute to the power of 4", result);
		}

		[Test]
		public void TestFormattingDimensionInAscii() {
			var dim = new Dimension("minute", 5);
			string result = dim.Format(new QuantityFormatInfo {
				AsciiOnly = true
			});
			Assert.AreEqual("min^5", result);
		}

		[Test]
		public void TestFormattingWithPrefix() {
			var dim = new Dimension("second", "mega");
			string result = dim.Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("Ms", result);
		}

		[Test]
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
