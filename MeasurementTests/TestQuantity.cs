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

		#region Serialization

		[TestMethod]
		public void TestSerialization() {
			string json = MeasurementFactory.CreateQuantity(2, "hour").ToJson();
			Assert.AreEqual("{\"dimensions\":[{\"systemName\":\"time\",\"unitName\":\"hour\"}],\"value\":2}", json);
		}

		[TestMethod]
		public void TestSerializationWithPrefixAndPower()
		{
			string json = new Quantity(42.42, new[] { new Dimension("metre", -2, "kilo"), new Dimension("second") }).ToJson();
			Assert.AreEqual("{\"dimensions\":[{\"power\":-2,\"prefix\":\"kilo\",\"systemName\":\"length\",\"unitName\":\"metre\"}," +
				"{\"systemName\":\"time\",\"unitName\":\"second\"}],\"value\":42.42}", json);
		}

		[TestMethod]
		public void TestDeserialization() {
			Quantity q = Quantity.FromJson("{\"dimensions\":[{\"systemName\":\"time\",\"unitName\":\"hour\"}],\"value\":8.594}");
			Assert.IsNotNull(q);
			Assert.AreEqual(8.594, q.Value);
			Assert.IsFalse(q.IsDimensionless());
			Assert.AreEqual(1, q.Dimensions.Count);
			Assert.AreEqual(1, q.Dimensions[0].Power);
			Assert.AreEqual("hour", q.Dimensions[0].Unit.Name);
		}

		[TestMethod]
		public void TestDimensionlessDeserialization() {
			Quantity q = Quantity.FromJson("{\"value\":3.2}");
			Assert.IsNotNull(q);
			Assert.AreEqual(3.2, q.Value);
			Assert.IsTrue(q.IsDimensionless());
		}

		[TestMethod]
		public void TestSerializationAndDeserialization() {
			var q1 = new Quantity(123.945345, new[] { "metre", "hour", "newton" });
			Quantity q2 = Quantity.FromJson(q1.ToJson());
			Assert.AreEqual(q1.Value, q2.Value);
			Assert.AreEqual(q1.Dimensions.Count, q2.Dimensions.Count);
			Assert.AreEqual(q1.Dimensions[2].Unit.Name, q2.Dimensions[2].Unit.Name);
		}

		#endregion

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

		[TestMethod]
		public void TestFormattingInfinity() {
			string result = MeasurementFactory.CreateQuantity(double.NegativeInfinity, "hour").Format(new FormatOptions());
			Assert.AreEqual("-Infinity h", result);
		}

		[TestMethod]
		public void TestFormattingNaN() {
			string result = MeasurementFactory.CreateQuantity(double.NaN, "hour").Format(new FormatOptions());
			Assert.AreEqual("NaN h", result);
		}

		#endregion

		#region Simplify

		[TestMethod]
		public void TestSimplify() {
			//m^2.in.ft^-1.s^-1
			var q = new Quantity(30, new List<Dimension> {
				new Dimension("metre", 2), new Dimension("inch"),
				new Dimension("foot", -1), new Dimension("second")}).Simplify();
			Assert.AreEqual(2, q.Dimensions.Count);
			Assert.AreEqual(2.5, q.Value, 0.001);
		}

		#endregion

	}
}
