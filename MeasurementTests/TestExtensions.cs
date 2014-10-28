using System.Globalization;
using ForgedSoftware.Measurement;
using ForgedSoftware.Measurement.Number;
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

		[TestMethod]
		public void TestOperationsAllowed() {
			var v3 = new Vector3(3,2,1);
			Assert.IsFalse(v3.OperationsAllowed(v => v.Add(1), v => v.Subtract(1), v => v.Multiply(1), v => v.Divide(1)));
			Assert.IsTrue(v3.OperationsAllowed(v => v.Multiply(1), v => v.Divide(1)));
		}

		#region Double ToString

		[TestMethod]
		public void TestDoubleExtendedToString() {
			var d1 = 434324235.32243E15;
			Assert.AreEqual("4.34 x 10^23", d1.ExtendedToString("T3", CultureInfo.CurrentCulture));
			Assert.AreEqual("4.34 x 10^23 {0}", d1.ExtendedToString("QT3", CultureInfo.CurrentCulture));
		}

		#endregion

	}
}
