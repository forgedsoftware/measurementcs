using ForgedSoftware.Measurement;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestPrefixes {

		[Test]
		public void TestCreatingQuantityWithPrefix() {
			var q = new Quantity(5, new Dimension("metre", "kilo"));
			Assert.AreEqual("kilo", q.Dimensions[0].Prefix.Key);
		}

		#region Tidy Prefixes

		[Test]
		public void TestAddingPrefix() {
			var q = new Quantity(5123, "metre");
			Assert.IsNull(q.Dimensions[0].Prefix);
			q = q.TidyPrefixes();
			Assert.IsNotNull(q.Dimensions[0].Prefix);
		}

		#endregion
	}
}
