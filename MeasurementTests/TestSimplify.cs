using System.Collections.Generic;
using ForgedSoftware.Measurement;
using ForgedSoftware.Measurement.Number;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests
{
	[TestClass]
	public class TestSimplify {

		[TestMethod]
		public void TestBasicSimplify() {
			var dims = new List<Dimension> {new Dimension("metre"), new Dimension("metre")};
			var value = new DoubleWrapper(10);
			List<Dimension> newDims = dims.Simplify(ref value);
			Assert.AreEqual(2, dims.Count);
			Assert.AreEqual(1, newDims.Count);
			Assert.AreEqual(10, value.Value);
			Assert.AreEqual(2, newDims[0].Power);
			Assert.AreEqual("metre", newDims[0].Unit.Name);
		}

		[TestMethod]
		public void TestDerivedSystemSimplify() {
			var dims = new List<Dimension> {new Dimension("metre"), new Dimension("second", -1)};
			var value = new DoubleWrapper(10);
			List<Dimension> newDims = dims.Simplify(ref value);
			Assert.AreEqual(2, dims.Count);
			Assert.AreEqual(1, newDims.Count);
			Assert.AreEqual(10, value.Value);
			Assert.AreEqual(1, newDims[0].Power);
			Assert.AreEqual("metrePerSecond", newDims[0].Unit.Key);
		}

		[TestMethod]
		public void TestDerivedSystemSimplifyRecursive() {
			var dims = new List<Dimension> { new Dimension("metre"),
				new Dimension("second", -1), new Dimension("metre"), new Dimension("second", -1) };
			var value = new DoubleWrapper(10);
			List<Dimension> newDims = dims.Simplify(ref value);
			Assert.AreEqual(4, dims.Count);
			Assert.AreEqual(2, newDims.Count);
			Assert.AreEqual(10, value.Value);
			Assert.AreEqual(1, newDims[0].Power);
			Assert.AreEqual(1, newDims[1].Power);
			Assert.AreEqual("metre", newDims[0].Unit.Name);
			Assert.AreEqual("metrePerSquareSecond", newDims[1].Unit.Key);
		}
	}
}
