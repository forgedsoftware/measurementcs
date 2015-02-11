using System;
using System.Collections.Generic;
using ForgedSoftware.Measurement;
using ForgedSoftware.Measurement.Comparers;
using ForgedSoftware.Measurement.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForgedSoftware.MeasurementTests {

	[TestClass]
	public class TestFind {

		[TestCleanup]
		public void AfterTest() {
			MeasurementCorpus.ResetToDefaultOptions();
		}

		#region Dimension Definitions

		[TestMethod]
		public void TestDimensionFilter() {
			var def = new DimensionDefinition { Key = "test" };
			Assert.IsTrue(MeasurementCorpus.DimensionFilter(def));
			def.Vector = true;
			Assert.IsFalse(MeasurementCorpus.DimensionFilter(def));
			MeasurementCorpus.Options.AllowVectorDimensions = true;
			Assert.IsTrue(MeasurementCorpus.DimensionFilter(def));
			def.DerivedString = "temperature/time";
			def.UpdateDerived();
			Assert.IsTrue(MeasurementCorpus.DimensionFilter(def));
			MeasurementCorpus.Options.AllowDerivedDimensions = false;
			Assert.IsFalse(MeasurementCorpus.DimensionFilter(def));
			MeasurementCorpus.Options.AllowDerivedDimensions = true;
			Assert.IsTrue(MeasurementCorpus.DimensionFilter(def));
			MeasurementCorpus.Options.IgnoredDimensions.Add(def);
			Assert.IsFalse(MeasurementCorpus.DimensionFilter(def));
		}

		[TestMethod]
		public void TestDimensionComparer() {
			var def1 = new DimensionDefinition { Key = "test1" };
			var def2 = new DimensionDefinition { Key = "test1" };

			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(null, null));
			Assert.AreEqual(1, DimensionDefinitionComparer.Comparer.Compare(def1, null));
			Assert.AreEqual(-1, DimensionDefinitionComparer.Comparer.Compare(null, def2));
			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(def1, def2));

			def2.Key = "test0";
			Assert.AreEqual(1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));

			def2.Key = "test1";
			def1.IsDimensionless = true;
			Assert.AreEqual(-1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def2.IsDimensionless = true;
			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def1.Vector = true;
			Assert.AreEqual(-1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def2.Vector = true;
			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def2.DerivedString = "temperature";
			def2.UpdateDerived();
			Assert.AreEqual(1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def1.DerivedString = "time*time";
			def1.UpdateDerived();
			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestDimensionFindNoInput() {
			MeasurementCorpus.FindDimension("");
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestDimensionFindPartialNoInput() {
			MeasurementCorpus.FindDimension("");
		}

		[TestMethod]
		public void TestDimensionFind() {
			Assert.IsNull(MeasurementCorpus.FindDimension("foo123"));
			Assert.IsNull(MeasurementCorpus.FindDimension("temp"));
			Assert.AreEqual("time", MeasurementCorpus.FindDimension("TIME").Key);
			Assert.AreEqual("electricCurrent", MeasurementCorpus.FindDimension("electric current").Key);
			Assert.AreEqual("electricResistance", MeasurementCorpus.FindDimension("R").Key);
			Assert.AreEqual("area", MeasurementCorpus.FindDimension("a").Key);
			Assert.AreEqual("length", MeasurementCorpus.FindDimension("radius").Key);
			Assert.AreEqual("planeAngle", MeasurementCorpus.FindDimension("β").Key);
		}

		[TestMethod]
		public void TestDimensionFindPartial() {
			Assert.IsNull(MeasurementCorpus.FindDimensionPartial("foo123"));
			Assert.AreEqual("time", MeasurementCorpus.FindDimensionPartial("IME").Key);
			Assert.AreEqual("time", MeasurementCorpus.FindDimensionPartial("dur").Key);
			Assert.AreEqual("electricCurrent", MeasurementCorpus.FindDimensionPartial("current").Key);
			Assert.AreEqual("electricPotential", MeasurementCorpus.FindDimensionPartial("electricPot").Key);
			Assert.AreEqual("planeAngle", MeasurementCorpus.FindDimensionPartial("β").Key);
		}

		[TestMethod]
		public void TestDimensionFindMultiple() {
			List<DimensionDefinition> results = MeasurementCorpus.FindDimensionsPartial("ELECTRIC");
			Assert.AreEqual(4, results.Count);
			Assert.AreEqual("electricCharge", results[0].Key);
			Assert.AreEqual("electricCurrent", results[3].Key);
		}

		#endregion

		#region Units

		// TODO

		#endregion

		#region Measurement Systems

		// TODO

		#endregion

		#region Prefixes

		// TODO

		#endregion

	}
}
