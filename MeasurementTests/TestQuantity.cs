using System.Collections.Generic;
using System.Globalization;
using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Could do this...
using M = ForgedSoftware.Measurement.MeasurementFactory;

namespace ForgedSoftware.MeasurementTests
{
	[TestClass]
	public class TestQuantity
	{
		[TestMethod]
		public void SimpleConversion() {
			Quantity converted = MeasurementFactory.CreateQuantity(2, "hour").Convert("minute");
			Assert.AreEqual(120, converted.Value);
		}

		[TestMethod]
		public void TestSerialization() {
			string json = MeasurementFactory.CreateQuantity(2, "hour").ToJson();
			Assert.AreEqual("{\"dimensions\":[{\"power\":1,\"systemName\":\"time\",\"unitName\":\"hour\"}],\"value\":2}", json);
		}

		#region Formatting

		[TestMethod]
		public void TestFormattingWithNoCulture() {
			string result = MeasurementFactory.CreateQuantity(2, "hour").Format(new FormatOptions());
			Assert.AreEqual("2 h", result);
		}

		[TestMethod]
		public void TestFormattingWithInvariantCulture() {
			string result = MeasurementFactory.CreateQuantity(2, "hour").Format(FormatOptions.Default(CultureInfo.InvariantCulture));
			Assert.AreEqual("2.00 h", result);
		}

		[TestMethod]
		public void TestFormattingExpandingExponent() {
			string result = MeasurementFactory.CreateQuantity(2.04567e32, "hour").Format(new FormatOptions());
			Assert.AreEqual("2.04567 x 10³² h", result);
		}

		[TestMethod]
		public void TestFormattingMulitpleDimensions() {
			string result = MeasurementFactory.CreateQuantity(2, new List<string> {"hour", "metre"}).Format(new FormatOptions());
			Assert.AreEqual("2 h·m", result);
		}

		[TestMethod]
		public void TestFormattingMulitpleDimensionsWithDifferentPowers() {
			string result = MeasurementFactory.CreateQuantity(2, new List<Dimension> {
				new Dimension("hour", 2), new Dimension("metre", -3)
			}).Format(new FormatOptions());
			Assert.AreEqual("2 h²·m⁻³", result);
		}

		[TestMethod]
		public void TestFormattingValueOnly() {
			string result = MeasurementFactory.CreateQuantity(2, "hour").Format(new FormatOptions {
				Show = FormatOptions.QuantityParts.ValueOnly
			});
			Assert.AreEqual("2", result);
		}

		[TestMethod]
		public void TestFormattingDimensionsOnly() {
			string result = MeasurementFactory.CreateQuantity(2, "hour").Format(new FormatOptions {
				Show = FormatOptions.QuantityParts.DimensionsOnly
			});
			Assert.AreEqual("h", result);
		}

		[TestMethod]
		public void TestFormattingNewDefaults() {
			string result = MeasurementFactory.CreateQuantity(231223423.23, "hour").Format(new FormatOptions {
				GroupSize = 2,
				GroupSeparator = "..",
				DecimalSeparator = "*"
			});
			Assert.AreEqual("2..31..22..34..23*23 h", result);
		}

		#endregion

		#region Simplify

		[TestMethod]
		public void TestSimplify() {
			//m^2.in.ft^-1.s^-1
			var q = new Quantity(5, new List<Dimension> {
				new Dimension("metre", 2), new Dimension("inch"),
				new Dimension("foot", -1), new Dimension("second")}).Simplify();
			Assert.AreEqual(2, q.Dimensions.Count);
			Assert.AreEqual(897930.494, q.Value, 0.001);
		}

		#endregion

	}
}
