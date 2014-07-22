using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests 
{
	[TestClass]
	public class TestExtensions {

		[TestMethod]
		public void TestToSuperScript() {
			const double number = 54;
			Assert.AreEqual("⁵⁴", number.ToSuperScript());
		}
	}
}
