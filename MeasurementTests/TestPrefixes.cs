using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestPrefixes {

		[TestMethod]
		public void TestCreatingQuantityWithPrefix() {
			var q = new Quantity(5, new Dimension("metre", "kilo"));
			Assert.AreEqual("kilo", q.Dimensions[0].Prefix.Key);
		}

		#region Tidy Prefixes

		[TestMethod]
		public void TestAddingPrefix() {
			var q = new Quantity(5123, "metre");
			Assert.IsNull(q.Dimensions[0].Prefix);
			q = q.TidyPrefixes();
			Assert.IsNotNull(q.Dimensions[0].Prefix);
		}

		#endregion
	}
}
