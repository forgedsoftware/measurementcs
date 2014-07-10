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
	}
}
