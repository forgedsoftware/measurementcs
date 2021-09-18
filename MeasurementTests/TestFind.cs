using System;
using System.Collections.Generic;
using System.Linq;
using ForgedSoftware.Measurement;
using ForgedSoftware.Measurement.Comparers;
using ForgedSoftware.Measurement.Entities;
using NUnit.Framework;

namespace ForgedSoftware.MeasurementTests {

	[TestFixture]
	public class TestFind {

		[TearDown]
		public void AfterTest() {
			MeasurementCorpus.Corpus.ResetToDefaultOptions();
		}

		#region Dimension Definitions

		[Test]
		public void TestDimensionFilter() {
			var def = new DimensionDefinition { Key = "test" };
			Assert.IsTrue(MeasurementCorpus.Corpus.DimensionFilter(def));
			def.Vector = true;
			Assert.IsFalse(MeasurementCorpus.Corpus.DimensionFilter(def));
			MeasurementCorpus.Corpus.Options.AllowVectorDimensions = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.DimensionFilter(def));
			def.DerivedString = "temperature/time";
			def.UpdateDerived(MeasurementCorpus.Corpus);
			Assert.IsTrue(MeasurementCorpus.Corpus.DimensionFilter(def));
			MeasurementCorpus.Corpus.Options.AllowDerivedDimensions = false;
			Assert.IsFalse(MeasurementCorpus.Corpus.DimensionFilter(def));
			MeasurementCorpus.Corpus.Options.AllowDerivedDimensions = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.DimensionFilter(def));
			MeasurementCorpus.Corpus.Options.IgnoredDimensions.Add(def);
			Assert.IsFalse(MeasurementCorpus.Corpus.DimensionFilter(def));
		}

		[Test]
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
			def2.UpdateDerived(MeasurementCorpus.Corpus);
			Assert.AreEqual(1, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
			def1.DerivedString = "time*time";
			def1.UpdateDerived(MeasurementCorpus.Corpus);
			Assert.AreEqual(0, DimensionDefinitionComparer.Comparer.Compare(def1, def2));
		}

		[Test]
		public void TestDimensionFindNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindDimension(""));
		}

		[Test]
		public void TestDimensionFindPartialNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindDimensionPartial(""));
		}

		[Test]
		public void TestDimensionFind() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindDimension("foo123"));
			Assert.IsNull(MeasurementCorpus.Corpus.FindDimension("temp"));
			Assert.AreEqual("time", MeasurementCorpus.Corpus.FindDimension("TIME").Key);
			Assert.AreEqual("electricCurrent", MeasurementCorpus.Corpus.FindDimension("electric current").Key);
			Assert.AreEqual("electricResistance", MeasurementCorpus.Corpus.FindDimension("R").Key);
			Assert.AreEqual("area", MeasurementCorpus.Corpus.FindDimension("a").Key);
			Assert.AreEqual("length", MeasurementCorpus.Corpus.FindDimension("radius").Key);
			Assert.AreEqual("planeAngle", MeasurementCorpus.Corpus.FindDimension("β").Key);
		}

		[Test]
		public void TestDimensionFindPartial() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindDimensionPartial("foo123"));
			Assert.AreEqual("time", MeasurementCorpus.Corpus.FindDimensionPartial("IME").Key);
			Assert.AreEqual("time", MeasurementCorpus.Corpus.FindDimensionPartial("dur").Key);
			Assert.AreEqual("electricCurrent", MeasurementCorpus.Corpus.FindDimensionPartial("current").Key);
			Assert.AreEqual("electricPotential", MeasurementCorpus.Corpus.FindDimensionPartial("electricPot").Key);
			Assert.AreEqual("planeAngle", MeasurementCorpus.Corpus.FindDimensionPartial("β").Key);
		}

		[Test]
		public void TestDimensionFindMultiple() {
			var results = MeasurementCorpus.Corpus.FindDimensionsPartial("ELECTRIC");
			Assert.AreEqual(4, results.Count);
			Assert.Contains("electricCurrent", results.Select(result => result.Key).ToList());
			Assert.Contains("electricResistance", results.Select(result => result.Key).ToList());
		}

		#endregion

		#region Units

		[Test]
		public void TestUnitFilter() {
			var unit = new Unit { Key = "test" };
			unit.MeasurementSystems.Add(MeasurementCorpus.Corpus.FindSystem("si"));
			Assert.IsTrue(MeasurementCorpus.Corpus.UnitFilter(unit));
			unit.IsRare = true;
			Assert.IsFalse(MeasurementCorpus.Corpus.UnitFilter(unit));
			MeasurementCorpus.Corpus.Options.UseRareUnits = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.UnitFilter(unit));
			unit.IsEstimation = true;
			Assert.IsFalse(MeasurementCorpus.Corpus.UnitFilter(unit));
			MeasurementCorpus.Corpus.Options.UseEstimatedUnits = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.UnitFilter(unit));
			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Add(unit.MeasurementSystems[0]);
			Assert.IsFalse(MeasurementCorpus.Corpus.UnitFilter(unit));
		}

		[Test]
		public void TestUnitComparer() {
			var unit1 = new Unit { Key = "test1", DimensionDefinition = MeasurementCorpus.Corpus.FindDimension("time") };
			unit1.MeasurementSystems.Add(MeasurementCorpus.Corpus.FindSystem("si"));
			var unit2 = new Unit { Key = "test1", DimensionDefinition = MeasurementCorpus.Corpus.FindDimension("time") };
			unit2.MeasurementSystems.Add(MeasurementCorpus.Corpus.FindSystem("si"));

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

		[Test]
		public void TestUnitFindNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindUnit(""));
		}

		[Test]
		public void TestUnitFindPartialNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindUnitPartial(""));
		}

		[Test]
		public void TestUnitFind() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindUnit("xyz"));
			Assert.IsNull(MeasurementCorpus.Corpus.FindUnit("seco"));
			Assert.AreEqual("second", MeasurementCorpus.Corpus.FindUnit("seconds").Key);
			Assert.AreEqual("time", MeasurementCorpus.Corpus.FindUnit("seconds").DimensionDefinition.Key);
			Assert.AreEqual("second", MeasurementCorpus.Corpus.FindUnit("s").Key);
			Assert.AreEqual("metre", MeasurementCorpus.Corpus.FindUnit("meter").Key);
			Assert.AreEqual("acreFoot", MeasurementCorpus.Corpus.FindUnit("acre feet").Key);
			Assert.AreEqual("squareYard", MeasurementCorpus.Corpus.FindUnit("sq yd").Key);
		}

		[Test]
		public void TestUnitFindPartial() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindUnitPartial("xyz"));
			Assert.AreEqual("second", MeasurementCorpus.Corpus.FindUnitPartial("seco").Key);
			Assert.AreEqual("\"", MeasurementCorpus.Corpus.FindUnitPartial("seco", "plane").Symbol);
		}

		[Test]
		public void TestUnitFindMultiple() {
			List<Unit> results = MeasurementCorpus.Corpus.FindUnitsPartial("sq");
			Assert.AreEqual(28, results.Count);
			Assert.AreEqual("squareMetre", results[0].Key);
		}

		#endregion

		#region Measurement Systems

		[Test]
		public void TestSystemFilterAsRoot() {
			// Setup
			var sys = new MeasurementSystem { Key = "test" };
			MeasurementCorpus.Corpus.AllSystems.Add(sys);
			MeasurementCorpus.Corpus.RootSystems.Add(sys);

			// Test
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys));
			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.Corpus.SystemFilter(sys));
			MeasurementCorpus.Corpus.Options.AllowedSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.Corpus.SystemFilter(sys)); // Ignored Overrides Allowed
			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Clear();
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys));

			// Cleanup
			MeasurementCorpus.Corpus.AllSystems.Remove(sys);
			MeasurementCorpus.Corpus.RootSystems.Remove(sys);
		}

		[Test]
		public void TestSystemFilterAsLeaf() {
			// Setup
			var sys = new MeasurementSystem {
				Key = "test",
				Parent = MeasurementCorpus.Corpus.FindSystem("siCommon")
			};
			MeasurementCorpus.Corpus.AllSystems.Add(sys);

			// Test
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys));
			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Add(sys);
			Assert.IsFalse(MeasurementCorpus.Corpus.SystemFilter(sys));
			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Clear();
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys));

			MeasurementCorpus.Corpus.Options.IgnoredSystemsForUnits.Add(sys.Parent);
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys));
			Assert.IsFalse(MeasurementCorpus.Corpus.SystemFilter(sys.Parent));
			Assert.IsFalse(MeasurementCorpus.Corpus.SystemFilter(sys.Parent.Parent));
			MeasurementCorpus.Corpus.Options.AllowedSystemsForUnits.Add(sys.Parent.Parent);
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys.Parent.Parent));
			Assert.IsTrue(MeasurementCorpus.Corpus.SystemFilter(sys.Parent.Parent.Parent));

			// Cleanup
			MeasurementCorpus.Corpus.AllSystems.Remove(sys);
		}

		[Test]
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

			sys1.Parent = MeasurementCorpus.Corpus.FindSystem("siCommon");
			Assert.AreEqual(-1, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
			sys2.Parent = MeasurementCorpus.Corpus.FindSystem("siCommon");
			Assert.AreEqual(0, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
			sys1.Parent = MeasurementCorpus.Corpus.FindSystem("si");
			Assert.AreEqual(1, MeasurementSystemComparer.Comparer.Compare(sys1, sys2));
		}

		[Test]
		public void TestSystemFindNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindSystem(""));
		}

		[Test]
		public void TestSystemFindPartialNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindSystemPartial(""));
		}

		[Test]
		public void TestSystemFind() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindSystem("xyz"));
			Assert.IsNull(MeasurementCorpus.Corpus.FindSystem("siC"));
			Assert.AreEqual("si", MeasurementCorpus.Corpus.FindSystem("si").Key);
			Assert.AreEqual("australia", MeasurementCorpus.Corpus.FindSystem("Common Australian Metric (SI)").Key);
		}

		[Test]
		public void TestSystemFindPartial() {
			Assert.IsNull(MeasurementCorpus.Corpus.FindSystemPartial("xyz"));
			Assert.AreEqual("si", MeasurementCorpus.Corpus.FindSystemPartial("si").Key);
			Assert.AreEqual("siCommon", MeasurementCorpus.Corpus.FindSystemPartial("siC").Key);
			Assert.AreEqual("natural", MeasurementCorpus.Corpus.FindSystemPartial("Natural Units").Key);
			Assert.AreEqual("qcd", MeasurementCorpus.Corpus.FindSystemPartial("Chromo").Key);
		}

		[Test]
		public void TestSystemFindMultiple() {
			List<MeasurementSystem> results = MeasurementCorpus.Corpus.FindSystemsPartial("Metric");
			Assert.AreEqual(6, results.Count);
			Assert.AreEqual("metric", results[0].Key);
			Assert.AreEqual("canada", results[5].Key);
		}

		#endregion

		#region Prefixes

		[Test]
		public void TestPrefixFilter() {
			var prefix = new Prefix { Key = "test", Type = PrefixType.Si};

			Assert.IsTrue(MeasurementCorpus.Corpus.PrefixFilter(prefix));
			prefix.Type = PrefixType.SiUnofficial;
			Assert.IsFalse(MeasurementCorpus.Corpus.PrefixFilter(prefix));
			MeasurementCorpus.Corpus.Options.UseUnofficalPrefixes = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.PrefixFilter(prefix));
			prefix.IsRare = true;
			Assert.IsFalse(MeasurementCorpus.Corpus.PrefixFilter(prefix));
			MeasurementCorpus.Corpus.Options.UseRarePrefixes = true;
			Assert.IsTrue(MeasurementCorpus.Corpus.PrefixFilter(prefix));
		}

		[Test]
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

		[Test]
		public void TestPrefixFindNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindPrefix(""));
		}

		[Test]
		public void TestPrefixFindPartialNoInput() {
			Assert.Throws<ArgumentException>(() => MeasurementCorpus.Corpus.FindPrefixPartial(""));
		}

		[Test]
		public void TestPrefixFind() {
			MeasurementCorpus.Corpus.Options.UseRarePrefixes = true;
			Assert.IsNull(MeasurementCorpus.Corpus.FindPrefix("xyz"));
			Assert.IsNull(MeasurementCorpus.Corpus.FindPrefix("cent"));
			Assert.AreEqual("kilo", MeasurementCorpus.Corpus.FindPrefix("kilo").Key);
			Assert.AreEqual("mega", MeasurementCorpus.Corpus.FindPrefix("M").Key);
		}

		[Test]
		public void TestPrefixFindPartial() {
			MeasurementCorpus.Corpus.Options.UseRarePrefixes = true;
			Assert.IsNull(MeasurementCorpus.Corpus.FindPrefixPartial("xyz"));
			Assert.AreEqual("centi", MeasurementCorpus.Corpus.FindPrefixPartial("cent").Key);
			Assert.AreEqual("kilo", MeasurementCorpus.Corpus.FindPrefixPartial("kilo").Key);
			Assert.AreEqual("kibi", MeasurementCorpus.Corpus.FindPrefixPartial("kib").Key);
			Assert.AreEqual("micro", MeasurementCorpus.Corpus.FindPrefixPartial("Mi").Key);
			Assert.AreEqual("mebi", MeasurementCorpus.Corpus.FindPrefixPartial("Mi", false).Key);
		}

		[Test]
		public void TestPrefixFindMultiple() {
			List<Prefix> results = MeasurementCorpus.Corpus.FindPrefixesPartial("to");
			Assert.AreEqual(4, results.Count);
			Assert.AreEqual("atto", results[0].Key);
			Assert.AreEqual("zepto", results[3].Key);
		}

		#endregion

	}
}
