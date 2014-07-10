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
	}
}
