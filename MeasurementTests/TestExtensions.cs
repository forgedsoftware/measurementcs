using ForgedSoftware.Measurement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestExtensions {

		#region Superscipt

		[TestMethod]
		public void TestToSuperScript() {
			const int number = 54;
			Assert.AreEqual("⁵⁴", number.ToSuperScript());
		}

		[TestMethod]
		public void TestToSuperScriptZero() {
			const int number = 0;
			Assert.AreEqual("⁰", number.ToSuperScript());
		}

		[TestMethod]
		public void TestToSuperScriptNegative() {
			const int number = int.MinValue;
			Assert.AreEqual("⁻²¹⁴⁷⁴⁸³⁶⁴⁸", number.ToSuperScript());
		}

		#endregion

	}
}
