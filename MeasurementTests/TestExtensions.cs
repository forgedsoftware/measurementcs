using System.Globalization;
using ForgedSoftware.Measurement;
using ForgedSoftware.Measurement.Number;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestExtensions {

		#region Superscipt

		[Test]
		public void TestToSuperScript() {
			const int number = 54;
			Assert.AreEqual("⁵⁴", number.ToSuperScript());
		}

		[Test]
		public void TestToSuperScriptZero() {
			const int number = 0;
			Assert.AreEqual("⁰", number.ToSuperScript());
		}

		[Test]
		public void TestToSuperScriptNegative() {
			const int number = int.MinValue;
			Assert.AreEqual("⁻²¹⁴⁷⁴⁸³⁶⁴⁸", number.ToSuperScript());
		}

		#endregion

		[Test]
		public void TestOperationsAllowed() {
			var v3 = new Vector3(3,2,1);
			Assert.IsFalse(v3.OperationsAllowed(v => v.Add(1), v => v.Subtract(1), v => v.Multiply(1), v => v.Divide(1)));
			Assert.IsTrue(v3.OperationsAllowed(v => v.Multiply(1), v => v.Divide(1)));
		}

		#region Double ToString

		[Test]
		public void TestDoubleExtendedToString() {
			var d1 = 434324235.32243E15;
			Assert.AreEqual("4.34 x 10^23", d1.ExtendedToString("TG3", CultureInfo.CurrentCulture));
			Assert.AreEqual("4.34 x 10^23 {0}", d1.ExtendedToString("QTG3", CultureInfo.CurrentCulture));
		}

		#endregion

	}
}
