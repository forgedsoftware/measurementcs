using System.Collections.Generic;
using System.Linq;

namespace ForgedSoftware.Measurement.Entities
{
    public class CommonDimension
    {
        public string Name { get; set; }
        public List<string> OtherNames { get; set; }
        public List<string> OtherSymbols { get; set; }
        public string Symbol { get; set; }
        public string Derived { get; set; }
        public string BaseUnit { get; set; }
        public bool Vector { get; set; }
        public bool Dimensionless { get; set; }
        public string InheritedUnits { get; set; }
        
        public Dictionary<string, CommonUnit> Units { get; set; }

        public DimensionDefinition ToDimension(string key, List<MeasurementSystem> systems)
        {
            var dimension = new DimensionDefinition {
                Key = key,
                Name = Name,
                Symbol = Symbol,
                DerivedString = Derived,
                BaseUnitName = BaseUnit,
                Vector = Vector,
                IsDimensionless = Dimensionless,
                InheritedUnits = InheritedUnits,
                OtherNames = OtherNames ?? new (),
                OtherSymbols = OtherSymbols ?? new ()
            };
            dimension.Units.AddRange(Units.ToList().Select(unit => unit.Value.ToUnit(unit.Key, dimension, systems)));
            return dimension;
        }
    }
}