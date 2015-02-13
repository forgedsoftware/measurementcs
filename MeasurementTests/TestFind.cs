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
			Assert.AreEqual(-1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
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
			MeasurementCorpus.FindDimensionPartial("");
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
			Assert.AreEqual("electricCurrent", results[0].Key);
			Assert.AreEqual("electricResistance", results[3].Key);
		}

		#endregion

		#region Units

		[TestMethod]
		public void TestUnitFilter() {
			var unit = new Unit { Key = "test" };
			unit.MeasurementSystems.Add(MeasurementCorpus.FindSystem("si"));
			Assert.IsTrue(MeasurementCorpus.UnitFilter(unit));
			unit.IsRare = true;
			Assert.IsFalse(MeasurementCorpus.UnitFilter(unit));
			MeasurementCorpus.Options.UseRareUnits = true;
			Assert.IsTrue(MeasurementCorpus.UnitFilter(unit));
			unit.IsEstimation = true;
			Assert.IsFalse(MeasurementCorpus.UnitFilter(unit));
			MeasurementCorpus.Options.UseEstimatedUnits = true;
			Assert.IsTrue(MeasurementCorpus.UnitFilter(unit));
			MeasurementCorpus.Options.IgnoredSystemsForUnits.Add(unit.MeasurementSystems[0]);
			Assert.IsFalse(MeasurementCorpus.UnitFilter(unit));
		}

		[TestMethod]
		public void TestUnitComparer() {
			var unit1 = new Unit { Key = "test1", DimensionDefinition = MeasurementCorpus.FindDimension("time") };
			unit1.MeasurementSystems.Add(MeasurementCorpus.FindSystem("si"));
			var unit2 = new Unit { Key = "test1", DimensionDefinition = MeasurementCorpus.FindDimension("time") };
			unit2.MeasurementSystems.Add(MeasurementCorpus.FindSystem("si"));

			Assert.AreEqual(0, UnitComparer.Comparer.Compare(null, null));
			Assert.AreEqual(1, UnitComparer.Comparer.Compare(unit1, null));
			Assert.AreEqual(-1, UnitComparer.Comparer.Compare(null, unit2));
			Assert.AreEqual(0, UnitComparer.Comparer.Compare(unit1, unit2));

			unit2.Key = "test0";
			Assert.AreEqual(-1, UnitComparer.Comparer.Compare(unit1, unit2));
			unit2.Key = "test1";

			unit1.IsRare = true;
			Assert.AreEqual(-1, UnitComparer.Comparer.Compare(unit1, unit2));
			unit1.IsRare = false;

			unit2.IsEstimation = true;
			Assert.AreEqual(1, UnitComparer.Comparer.Compare(unit1, unit2));
			unit2.IsEstimation = false;
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestUnitFindNoInput() {
			MeasurementCorpus.FindUnit("");
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestUnitFindPartialNoInput() {
			MeasurementCorpus.FindUnitPartial("");
		}

		[TestMethod]
		public void TestUnitFind() {
			Assert.IsNull(MeasurementCorpus.FindUnit("xyz"));
			Assert.IsNull(MeasurementCorpus.FindUnit("seco"));
			Assert.AreEqual("second", MeasurementCorpus.FindUnit("seconds").Key);
			Assert.AreEqual("time", MeasurementCorpus.FindUnit("seconds").DimensionDefinition.Key);
			Assert.AreEqual("second", MeasurementCorpus.FindUnit("s").Key);
			Assert.AreEqual("metre", MeasurementCorpus.FindUnit("meter").Key);
			Assert.AreEqual("acreFoot", MeasurementCorpus.FindUnit("acre feet").Key);
			Assert.AreEqual("squareYard", MeasurementCorpus.FindUnit("sq yd").Key);
		}

		[TestMethod]
		public void TestUnitFindPartial() {
			Assert.IsNull(MeasurementCorpus.FindUnitPartial("xyz"));
			Assert.AreEqual("second", MeasurementCorpus.FindUnitPartial("seco").Key);
			Assert.AreEqual("\"", MeasurementCorpus.FindUnitPartial("seco", "plane").Symbol);
		}

		[TestMethod]
		public void TestUnitFindMultiple() {
			List<Unit> results = MeasurementCorpus.FindUnitsPartial("sq");
			Assert.AreEqual(28, results.Count);
			Assert.AreEqual("squareMetre", results[0].Key);
		}

		#endregion

		#region Measurement Systems

		[TestMethod]
		public void TestSystemFilterAsRoot() {
			// Setup
			var sys = new MeasurementSystem { Key = "test" };
			MeasurementCorpus.AllSystems.Add(sys);
			MeasurementCorpus.RootSystems.Add(sys);

			// Test
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys));
			MeasurementCorpus.Options.IgnoredSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.SystemFilter(sys));
			MeasurementCorpus.Options.AllowedSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.SystemFilter(sys)); // Ignored Overrides Allowed
			MeasurementCorpus.Options.IgnoredSystemsForUnits.Clear();
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys));

			// Cleanup
			MeasurementCorpus.AllSystems.Remove(sys);
			MeasurementCorpus.RootSystems.Remove(sys);
		}

		[TestMethod]
		public void TestSystemFilterAsLeaf() {
			// Setup
			var sys = new MeasurementSystem {
				Key = "test",
				Parent = MeasurementCorpus.FindSystem("siCommon")
			};
			MeasurementCorpus.AllSystems.Add(sys);

			// Test
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys));
			MeasurementCorpus.Options.IgnoredSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.SystemFilter(sys));
			MeasurementCorpus.Options.IgnoredSystemsForUnits.Clear();
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys));

			MeasurementCorpus.Options.IgnoredSystemsForUnits.Add(sys.Parent);
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys));
			Assert.IsFalse(MeasurementCorpus.SystemFilter(sys.Parent));
			Assert.IsFalse(MeasurementCorpus.SystemFilter(sys.Parent.Parent));
			MeasurementCorpus.Options.AllowedSystemsForUnits.Add(sys.Parent.Parent);
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys.Parent.Parent));
			Assert.IsTrue(MeasurementCorpus.SystemFilter(sys.Parent.Parent.Parent));

			// Cleanup
			MeasurementCorpus.AllSystems.Remove(sys);
		}

		[TestMethod]
		public void TestSystemComparer() {
			var sys1 = new MeasurementSystem { Key = "test1" };
			var sys2 = new MeasurementSystem { Key = "test1" };

			Assert.AreEqual(0, MeasurementSystemComparer.Comparer.Compare(null, null));
			Assert.AreEqual(1, MeasurementSystemComparer.Comparer.Compare(sys1, null));
			Assert.AreEqual(-1, MeasurementSystemComparer.Comparer.Compare(null, sys2));
			Assert.AreEqual(0, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));

			sys1.Key = "test0";
			Assert.AreEqual(1, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
			sys1.Key = "test1";

			sys1.Parent = MeasurementCorpus.FindSystem("siCommon");
			Assert.AreEqual(-1, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
			sys2.Parent = MeasurementCorpus.FindSystem("siCommon");
			Assert.AreEqual(0, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
			sys1.Parent = MeasurementCorpus.FindSystem("si");
			Assert.AreEqual(1, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestSystemFindNoInput() {
			MeasurementCorpus.FindSystem("");
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestSystemFindPartialNoInput() {
			MeasurementCorpus.FindSystemPartial("");
		}

		[TestMethod]
		public void TestSystemFind() {
			Assert.IsNull(MeasurementCorpus.FindSystem("xyz"));
			Assert.IsNull(MeasurementCorpus.FindSystem("siC"));
			Assert.AreEqual("si", MeasurementCorpus.FindSystem("si").Key);
			Assert.AreEqual("australia", MeasurementCorpus.FindSystem("Common Australian Metric (SI)").Key);
		}

		[TestMethod]
		public void TestSystemFindPartial() {
			Assert.IsNull(MeasurementCorpus.FindSystemPartial("xyz"));
			Assert.AreEqual("si", MeasurementCorpus.FindSystemPartial("si").Key);
			Assert.AreEqual("siCommon", MeasurementCorpus.FindSystemPartial("siC").Key);
			Assert.AreEqual("natural", MeasurementCorpus.FindSystemPartial("Natural Units").Key);
			Assert.AreEqual("qcd", MeasurementCorpus.FindSystemPartial("Chromo").Key);
		}

		[TestMethod]
		public void TestSystemFindMultiple() {
			List<MeasurementSystem> results = MeasurementCorpus.FindSystemsPartial("Metric");
			Assert.AreEqual(6, results.Count);
			Assert.AreEqual("metric", results[0].Key);
			Assert.AreEqual("canada", results[5].Key);
		}

		#endregion

		#region Prefixes

		[TestMethod]
		public void TestPrefixFilter() {
			var prefix = new Prefix { Key = "test", Type = PrefixType.Si};

			Assert.IsTrue(MeasurementCorpus.PrefixFilter(prefix));
			prefix.Type = PrefixType.SiUnofficial;
			Assert.IsFalse(MeasurementCorpus.PrefixFilter(prefix));
			MeasurementCorpus.Options.UseUnofficalPrefixes = true;
			Assert.IsTrue(MeasurementCorpus.PrefixFilter(prefix));
			prefix.IsRare = true;
			Assert.IsFalse(MeasurementCorpus.PrefixFilter(prefix));
			MeasurementCorpus.Options.UseRarePrefixes = true;
			Assert.IsTrue(MeasurementCorpus.PrefixFilter(prefix));
		}

		[TestMethod]
		public void TestPrefixComparer() {
			var prefix1 = new Prefix { Key = "test1" };
			var prefix2 = new Prefix { Key = "test1" };

			Assert.AreEqual(0, PrefixComparer.Comparer.Compare(null, null));
			Assert.AreEqual(1, PrefixComparer.Comparer.Compare(prefix1, null));
			Assert.AreEqual(-1, PrefixComparer.Comparer.Compare(null, prefix2));
			Assert.AreEqual(0, PrefixComparer.Comparer.Compare(prefix1, prefix2));

			prefix1.Key = "test0";
			Assert.AreEqual(1, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix1.Key = "test1";

			Assert.AreEqual(0, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix1.IsRare = true;
			Assert.AreEqual(-1, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix2.IsRare = true;
			Assert.AreEqual(0, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix1.Type = PrefixType.SiBinary;
			Assert.AreEqual(-1, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix2.Type = PrefixType.SiUnofficial;
			Assert.AreEqual(1, PrefixComparer.Comparer.Compare(prefix1, prefix2));
			prefix1.Type = PrefixType.SiUnofficial;
			Assert.AreEqual(0, PrefixComparer.Comparer.Compare(prefix1, prefix2));
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestPrefixFindNoInput() {
			MeasurementCorpus.FindPrefix("");
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentException), "An exception should be thrown if no input provided")]
		public void TestPrefixFindPartialNoInput() {
			MeasurementCorpus.FindPrefixPartial("");
		}

		[TestMethod]
		public void TestPrefixFind() {
			MeasurementCorpus.Options.UseRarePrefixes = true;
			Assert.IsNull(MeasurementCorpus.FindPrefix("xyz"));
			Assert.IsNull(MeasurementCorpus.FindPrefix("cent"));
			Assert.AreEqual("kilo", MeasurementCorpus.FindPrefix("kilo").Key);
			Assert.AreEqual("mega", MeasurementCorpus.FindPrefix("M").Key);
		}

		[TestMethod]
		public void TestPrefixFindPartial() {
			MeasurementCorpus.Options.UseRarePrefixes = true;
			Assert.IsNull(MeasurementCorpus.FindPrefixPartial("xyz"));
			Assert.AreEqual("centi", MeasurementCorpus.FindPrefixPartial("cent").Key);
			Assert.AreEqual("kilo", MeasurementCorpus.FindPrefixPartial("kilo").Key);
			Assert.AreEqual("kibi", MeasurementCorpus.FindPrefixPartial("kib").Key);
			Assert.AreEqual("micro", MeasurementCorpus.FindPrefixPartial("Mi").Key);
			Assert.AreEqual("mebi", MeasurementCorpus.FindPrefixPartial("Mi", false).Key);
		}

		[TestMethod]
		public void TestPrefixFindMultiple() {
			List<Prefix> results = MeasurementCorpus.FindPrefixesPartial("to");
			Assert.AreEqual(4, results.Count);
			Assert.AreEqual("atto", results[0].Key);
			Assert.AreEqual("zepto", results[3].Key);
		}

		#endregion

	}
}
