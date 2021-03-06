﻿using System;
using System.Collections.Generic;
using System.Globalization;
using ForgedSoftware.Measurement;
using NUnit.Framework;

// Could do this...
using M = ForgedSoftware.Measurement.MeasurementCorpus;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestQuantity {

		[Test]
		public void SimpleConversion() {
			Quantity converted = MeasurementCorpus.CreateQuantity(2, "hour").Convert("minute");
			Assert.AreEqual(120, converted.Value);
		}

		#region Serialization

		private void TestIfOnMono() {
			if (Extensions.IsRunningOnMono()) {
				Assert.Inconclusive("Test is running on mono and DataContracts on mono are not fully implemented.");
			}
		}

		[Test]
		public void TestSerialization() {
			TestIfOnMono();
			string json = MeasurementCorpus.CreateQuantity(2, "hour").ToJson();
			Assert.AreEqual("{\"dimensions\":[{\"dimensionName\":\"time\",\"unitName\":\"hour\"}],\"value\":2}", json);
		}

		[Test]
		public void TestSerializationWithPrefixAndPower() {
			TestIfOnMono();
			string json = new Quantity(42.42, new[] { new Dimension("metre", -2, "kilo"), new Dimension("second") }).ToJson();
			Assert.AreEqual("{\"dimensions\":[{\"dimensionName\":\"length\",\"power\":-2,\"prefix\":\"kilo\",\"unitName\":\"metre\"}," +
				"{\"dimensionName\":\"time\",\"unitName\":\"second\"}],\"value\":42.42}", json);
		}

		[Test]
		public void TestDeserialization() {
			TestIfOnMono();
			Quantity q = Quantity.FromJson("{\"dimensions\":[{\"dimensionName\":\"time\",\"unitName\":\"hour\"}],\"value\":8.594}");
			Assert.IsNotNull(q);
			Assert.AreEqual(8.594, q.Value);
			Assert.IsFalse(q.IsDimensionless());
			Assert.AreEqual(1, q.Dimensions.Count);
			Assert.AreEqual(1, q.Dimensions[0].Power);
			Assert.AreEqual("hour", q.Dimensions[0].Unit.Name);
		}

		[Test]
		public void TestDimensionlessDeserialization() {
			TestIfOnMono();
			Quantity q = Quantity.FromJson("{\"value\":3.2}");
			Assert.IsNotNull(q);
			Assert.AreEqual(3.2, q.Value);
			Assert.IsTrue(q.IsDimensionless());
		}

		[Test]
		public void TestSerializationAndDeserialization() {
			TestIfOnMono();
			var q1 = new Quantity(123.945345, new[] { "metre", "hour", "newton" });
			Quantity q2 = Quantity.FromJson(q1.ToJson());
			Assert.AreEqual(q1.Value, q2.Value);
			Assert.AreEqual(q1.Dimensions.Count, q2.Dimensions.Count);
			Assert.AreEqual(q1.Dimensions[2].Unit.Name, q2.Dimensions[2].Unit.Name);
		}

		#endregion

		#region Formatting

		[Test]
		public void TestFormattingWithNoCulture() {
			string result = MeasurementCorpus.CreateQuantity(2, "hour").Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("2 h", result);
		}

		[Test]
		public void TestFormattingWithInvariantCulture() {
			string result = MeasurementCorpus.CreateQuantity(2, "hour").Format(new QuantityFormatInfo(CultureInfo.InvariantCulture.NumberFormat));
			Assert.AreEqual("2 h", result);
		}

		[Test]
		public void TestFormattingExpandingExponent() {
			string result = MeasurementCorpus.CreateQuantity(2.04567e32, "hour").Format(QuantityFormatInfo.CurrentInfo);
			Assert.AreEqual("2.04567 x 10³² h", result);
		}

		[Test]
		public void TestFormattingMulitpleDimensions() {
			string result = MeasurementCorpus.CreateQuantity(2, new List<string> { "hour", "metre" }).Format(new QuantityFormatInfo());
			Assert.AreEqual("2 h·m", result);
		}

		[Test]
		public void TestFormattingMulitpleDimensionsWithDifferentPowers() {
			string result = MeasurementCorpus.CreateQuantity(2, new List<Dimension> {
				new Dimension("hour", 2), new Dimension("metre", -3)
			}).Format(new QuantityFormatInfo());
			Assert.AreEqual("2 h²·m⁻³", result);
		}

		[Test]
		public void TestFormattingValueOnly() {
			string result = MeasurementCorpus.CreateQuantity(2, "hour").Format(new QuantityFormatInfo {
				FormatParts = QuantityFormatInfo.QuantityParts.Value
			});
			Assert.AreEqual("2", result);
		}

		[Test]
		public void TestFormattingDimensionsOnly() {
			string result = MeasurementCorpus.CreateQuantity(2, "hour").Format(new QuantityFormatInfo {
				FormatParts = QuantityFormatInfo.QuantityParts.Dimensions
			});
			Assert.AreEqual("h", result);
		}

		[Test]
		public void TestFormattingNewDefaults() {
			var info = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			info.NumberDecimalSeparator = "*";
			info.NumberGroupSeparator = "..";
			info.NumberGroupSizes = new[] { 2 };
			string result = MeasurementCorpus.CreateQuantity(231223423.23, "hour").Format(new QuantityFormatInfo(info) {
				DefaultDoubleFormat = "N"
			});
			Assert.AreEqual("2..31..22..34..23*23 h", result);
		}

		[Test]
		public void TestFormattingInfinity() {
			string result = MeasurementCorpus.CreateQuantity(double.NegativeInfinity, "hour").Format(new QuantityFormatInfo());
			Assert.AreEqual("-Infinity h", result);
		}

		[Test]
		public void TestFormattingNaN() {
			string result = MeasurementCorpus.CreateQuantity(double.NaN, "hour").Format(new QuantityFormatInfo());
			Assert.AreEqual("NaN h", result);
		}

		#endregion

		#region Simplify

		[Test]
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
