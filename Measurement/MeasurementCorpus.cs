using System;
using System.Collections.Generic;
using System.Linq;
using ForgedSoftware.Measurement.Comparers;
using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement
{
    /// <summary>
    /// A corpus of data that provides accessors to the measurement systems, dimension definitions, and prefixes.
    /// It provides accessor functions to simply usage as well as collections of data.
    /// It is loaded using the CorpusBuilder.
    /// </summary>
    public class MeasurementCorpus
    {
        #region Helper Delegates

        public delegate List<T> Matcher<T>(IEnumerable<T> items, string value, Func<T, string> accessor,
            bool ignoreCase);

        public delegate List<T> ListMatcher<T>(IEnumerable<T> items, string value, Func<T, List<string>> accessor,
            bool ignoreCase);

        #endregion

        #region Properties

        /// <summary>
        /// A set of all measurement systems.
        /// </summary>
        public List<MeasurementSystem> AllSystems { get; }

        /// <summary>
        /// A tree of measurement systems, this list contains all of the root nodes.
        /// </summary>
        public List<MeasurementSystem> RootSystems { get; }

        /// <summary>
        /// A set of dimension definitions.
        /// </summary>
        public List<DimensionDefinition> Dimensions { get; }

        /// <summary>
        /// A set of prefixes.
        /// </summary>
        public List<Prefix> Prefixes { get; }

        /// <summary>
        /// The options to use within the library.
        /// </summary>
        public MeasurementOptions Options { get; private set; }

        /// <summary>
        /// A set of all of the units across all dimension definitions.
        /// </summary>
        public List<Unit> AllUnits
        {
            get { return Dimensions.SelectMany(s => s.Units).ToList(); }
        }

        #endregion

        #region Setup

        public static MeasurementCorpus Corpus { get; } = new();

        private MeasurementCorpus()
        {
            // Initialize Lists
            AllSystems = new List<MeasurementSystem>();
            RootSystems = new List<MeasurementSystem>();
            Dimensions = new List<DimensionDefinition>();
            Prefixes = new List<Prefix>();

            // Create Default Options
            ResetToDefaultOptions();

            // Load Data
            new CorpusBuilder().PrepareMeasurementCorpus(this);

            // Reload Default Options After Loading Data
            ResetToDefaultOptions();
        }

        #endregion

        #region Options

        /// <summary>
        /// Resets the Options property to the default options.
        /// </summary>
        public void ResetToDefaultOptions()
        {
            Options = new MeasurementOptions(this);
        }

        #endregion

        #region Quantity Factory Methods

        /// <summary>
        /// A factory method that creates a new dimensionless quantity with a given value.
        /// </summary>
        /// <param name="val">The value of the quantity</param>
        /// <returns>The new quantity</returns>
        public static Quantity CreateQuantity(double val)
        {
            return new Quantity(val);
        }

        /// <summary>
        /// A factory method that creates a new quantity with a single dimension and a given value.
        /// </summary>
        /// <param name="val">The value of the quantity</param>
        /// <param name="unitName">The name of the unit to be used by the dimension</param>
        /// <returns>The new quantity</returns>
        public static Quantity CreateQuantity(double val, string unitName)
        {
            return new Quantity(val, new List<string> {unitName});
        }

        /// <summary>
        /// A factory method that creates a new quantity with multiple single power dimensions and a given value.
        /// </summary>
        /// <param name="val">The value of the quantity</param>
        /// <param name="unitNames">The names of the units to be used in respective dimensions</param>
        /// <returns>The new quantity</returns>
        public static Quantity CreateQuantity(double val, IEnumerable<string> unitNames)
        {
            return new Quantity(val, unitNames);
        }

        /// <summary>
        /// A factory method that creates a new quantity with multiple provided dimensions and a given value.
        /// </summary>
        /// <param name="val">The value of the quantity</param>
        /// <param name="dimensions">The dimensions to be used by the quantity</param>
        /// <returns>The new quantity</returns>
        public static Quantity CreateQuantity(double val, IEnumerable<Dimension> dimensions)
        {
            return new Quantity(val, dimensions);
        }

        #endregion

        #region Find

        #region Dimensions

        public DimensionDefinition FindDimension(string dimensionName, bool ignoreCase = true)
        {
            return FindDimensions(dimensionName, ignoreCase).FirstOrDefault();
        }

        public DimensionDefinition FindDimensionPartial(string dimensionName, bool ignoreCase = true)
        {
            return FindDimensionsPartial(dimensionName, ignoreCase).FirstOrDefault();
        }

        public List<DimensionDefinition> FindDimensions(string dimensionName, bool ignoreCase = true)
        {
            return FindDimensionsImpl(new List<DimensionDefinition>(), dimensionName, ignoreCase, Matches, ListMatches);
        }

        public List<DimensionDefinition> FindDimensionsPartial(string dimensionName, bool ignoreCase = true)
        {
            return FindDimensionsImpl(FindDimensions(dimensionName, ignoreCase), dimensionName, ignoreCase,
                PartialMatches, PartialListMatches);
        }

        private List<DimensionDefinition> FindDimensionsImpl(List<DimensionDefinition> existingMatches,
            string dimensionName, bool ignoreCase,
            Matcher<DimensionDefinition> matcher, ListMatcher<DimensionDefinition> listMatcher)
        {
            if (string.IsNullOrWhiteSpace(dimensionName))
            {
                throw new ArgumentException("dimensionName must be a string at least one character of non-whitespace");
            }

            return existingMatches
                .Union(matcher(Dimensions, dimensionName, u => u.Key, ignoreCase))
                .Union(matcher(Dimensions, dimensionName, u => u.Name, ignoreCase))
                .Union(matcher(Dimensions, dimensionName, u => u.Symbol, ignoreCase))
                .Union(listMatcher(Dimensions, dimensionName, u => u.OtherNames, ignoreCase))
                .Union(listMatcher(Dimensions, dimensionName, u => u.OtherSymbols, ignoreCase))
                .Where(DimensionFilter)
                .OrderByDescending(x => x, DimensionDefinitionComparer.Comparer).ToList();
        }

        public bool DimensionFilter(DimensionDefinition arg)
        {
            if (!Options.AllowVectorDimensions && arg.Vector)
            {
                return false;
            }

            if (!Options.AllowDerivedDimensions && arg.IsDerived())
            {
                return false;
            }

            if (Options.IgnoredDimensions.Contains(arg))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Units

        public Unit FindBaseUnit(string dimensionName, bool ignoreCase = true)
        {
            var dimension = FindDimension(dimensionName, ignoreCase);
            return dimension?.BaseUnit;
        }

        public Unit FindBaseUnitPartial(string dimensionName, bool ignoreCase = true)
        {
            var dimension = FindDimensionPartial(dimensionName, ignoreCase);
            return dimension?.BaseUnit;
        }

        public Unit FindUnit(string unitName, string dimensionName, bool ignoreCase = true)
        {
            return FindUnits(unitName, FindDimension(dimensionName, ignoreCase), ignoreCase).FirstOrDefault();
        }

        public Unit FindUnitPartial(string unitName, string dimensionName, bool ignoreCase = true)
        {
            return FindUnitsPartial(unitName, FindDimensionPartial(dimensionName, ignoreCase), ignoreCase)
                .FirstOrDefault();
        }

        public Unit FindUnit(string unitName, DimensionDefinition dimension = null, bool ignoreCase = true)
        {
            return FindUnits(unitName, dimension, ignoreCase).FirstOrDefault();
        }

        public Unit FindUnitPartial(string unitName, DimensionDefinition dimension = null, bool ignoreCase = true)
        {
            return FindUnitsPartial(unitName, dimension, ignoreCase).FirstOrDefault();
        }

        public List<Unit> FindUnits(string unitName, DimensionDefinition dimension = null, bool ignoreCase = true)
        {
            return FindUnitsImpl(new List<Unit>(), unitName, dimension, ignoreCase, Matches, ListMatches);
        }

        public List<Unit> FindUnitsPartial(string unitName, DimensionDefinition dimension = null,
            bool ignoreCase = true)
        {
            return FindUnitsImpl(FindUnits(unitName, dimension, ignoreCase), unitName, dimension, ignoreCase,
                PartialMatches, PartialListMatches);
        }

        private List<Unit> FindUnitsImpl(List<Unit> existingMatches, string unitName, DimensionDefinition dimension,
            bool ignoreCase,
            Matcher<Unit> matcher, ListMatcher<Unit> listMatcher)
        {
            if (string.IsNullOrWhiteSpace(unitName))
            {
                throw new ArgumentException("unitName must be a string at least one character of non-whitespace");
            }

            var allUnits = (dimension == null) ? AllUnits : dimension.Units;
            var foundMatches = existingMatches
                .Union(matcher(allUnits, unitName, u => u.Key, ignoreCase))
                .Union(matcher(allUnits, unitName, u => u.Name, ignoreCase))
                .Union(matcher(allUnits, unitName, u => u.Plural, ignoreCase))
                .Union(matcher(allUnits, unitName, u => u.Symbol, ignoreCase))
                .Union(listMatcher(allUnits, unitName, u => u.OtherNames, ignoreCase))
                .Union(listMatcher(allUnits, unitName, u => u.OtherSymbols, ignoreCase))
                .Distinct()
                .Where(UnitFilter)
                .OrderByDescending(x => x, UnitComparer.Comparer).ToList();

            return foundMatches;
        }

        public bool UnitFilter(Unit arg)
        {
            if (!Options.UseRareUnits && arg.IsRare)
            {
                return false;
            }

            if (!Options.UseEstimatedUnits && arg.IsEstimation)
            {
                return false;
            }

            if (!arg.MeasurementSystems.Any(SystemFilter))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Prefixes

        public Prefix FindPrefix(string prefixName, bool ignoreCase = true)
        {
            return FindPrefixes(prefixName, ignoreCase).FirstOrDefault();
        }

        public Prefix FindPrefixPartial(string prefixName, bool ignoreCase = true)
        {
            return FindPrefixesPartial(prefixName, ignoreCase).FirstOrDefault();
        }

        public List<Prefix> FindPrefixes(string prefixName, bool ignoreCase = true)
        {
            return FindPrefixImpl(new List<Prefix>(), prefixName, ignoreCase, Matches);
        }

        public List<Prefix> FindPrefixesPartial(string prefixName, bool ignoreCase = true)
        {
            return FindPrefixImpl(FindPrefixes(prefixName, ignoreCase), prefixName, ignoreCase, PartialMatches);
        }

        private List<Prefix> FindPrefixImpl(List<Prefix> existingMatches, string prefixName, bool ignoreCase,
            Matcher<Prefix> matcher)
        {
            if (string.IsNullOrWhiteSpace(prefixName))
            {
                throw new ArgumentException("prefixName must be a string at least one character of non-whitespace");
            }

            return existingMatches
                .Union(matcher(Prefixes, prefixName, p => p.Key, ignoreCase))
                .Union(matcher(Prefixes, prefixName, p => p.Symbol, ignoreCase))
                .Where(PrefixFilter)
                .OrderByDescending(x => x, PrefixComparer.Comparer).ToList();
        }

        public bool PrefixFilter(Prefix arg)
        {
            if (!Options.UseRarePrefixes && arg.IsRare)
            {
                return false;
            }

            if (!Options.UseUnofficalPrefixes && arg.Type == PrefixType.SiUnofficial)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Systems

        public MeasurementSystem FindSystem(string systemName, bool ignoreCase = true)
        {
            return FindSystems(systemName, ignoreCase).FirstOrDefault();
        }

        public MeasurementSystem FindSystemPartial(string systemName, bool ignoreCase = true)
        {
            return FindSystemsPartial(systemName, ignoreCase).FirstOrDefault();
        }

        public List<MeasurementSystem> FindSystems(string systemName, bool ignoreCase = true)
        {
            return FindSystemsImpl(new List<MeasurementSystem>(), systemName, ignoreCase, Matches);
        }

        public List<MeasurementSystem> FindSystemsPartial(string systemName, bool ignoreCase = true)
        {
            return FindSystemsImpl(FindSystems(systemName, ignoreCase), systemName, ignoreCase, PartialMatches);
        }

        private List<MeasurementSystem> FindSystemsImpl(List<MeasurementSystem> existingMatches, string systemName,
            bool ignoreCase,
            Matcher<MeasurementSystem> matcher)
        {
            if (string.IsNullOrWhiteSpace(systemName))
            {
                throw new ArgumentException("systemName must be a string at least one character of non-whitespace");
            }

            return existingMatches
                .Union(matcher(AllSystems, systemName, p => p.Key, ignoreCase))
                .Union(matcher(AllSystems, systemName, p => p.Name, ignoreCase))
                .Where(SystemFilter)
                .OrderByDescending(x => x, MeasurementSystemComparer.Comparer).ToList();
        }

        public bool SystemFilter(MeasurementSystem arg)
        {
            return FindAllowedSystems().Contains(arg);
        }

        private List<MeasurementSystem> FindAllowedSystems()
        {
            var allowedSystems = new List<MeasurementSystem>(
                Options.AllowedSystemsForUnits.IsEmpty()
                    ? AllSystems
                    : Options.AllowedSystemsForUnits.SelectMany(s => s.Ancestors).Distinct());
            var toRemove = new List<MeasurementSystem>();
            foreach (MeasurementSystem ignore in Options.IgnoredSystemsForUnits)
            {
                if (allowedSystems.Contains(ignore))
                {
                    toRemove.Add(ignore);
                    CheckRemoveParent(ref toRemove, ignore.Parent);
                }
            }

            toRemove.ForEach(r => allowedSystems.Remove(r));
            return allowedSystems;
        }

        private void CheckRemoveParent(ref List<MeasurementSystem> toRemove, MeasurementSystem parent)
        {
            if (parent != null)
            {
                if (!Options.AllowedSystemsForUnits.Contains(parent))
                {
                    toRemove.Add(parent);
                    CheckRemoveParent(ref toRemove, parent.Parent);
                }
            }
        }

        #endregion

        #endregion

        #region Helper Functions

		private static List<T> Matches<T>(IEnumerable<T> items, string value, Func<T, string> accessor, bool ignoreCase)
        {
			return items.Where(u => (!string.IsNullOrWhiteSpace(accessor(u)))
				&& accessor(u).Equals(value, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)).ToList();
		}

		private static List<T> PartialMatches<T>(IEnumerable<T> items, string value, Func<T, string> accessor, bool ignoreCase = true)
        {
			return items.Where(u => (!string.IsNullOrWhiteSpace(accessor(u)))
				&& accessor(u).IndexOf(value, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0).ToList();
		}

		private static List<T> ListMatches<T>(IEnumerable<T> items, string value, Func<T, List<string>> accessor, bool ignoreCase = true)
        {
			return items.Where(u => accessor(u).Any(y => y.Equals(value, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))).ToList();
		}

		private static List<T> PartialListMatches<T>(IEnumerable<T> items, string value, Func<T, List<string>> accessor, bool ignoreCase = true)
        {
			return items.Where(u => accessor(u).Any(y => y.IndexOf(value, (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0)).ToList();
		}

        #endregion
    }
}