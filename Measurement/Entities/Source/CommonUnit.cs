using System;
using System.Collections.Generic;

namespace ForgedSoftware.Measurement.Entities
{
    public class CommonUnit
    {
        public string Name { get; set; }
        public string Plural { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
        public List<string> OtherNames { get; set; }
        public List<string> OtherSymbols { get; set; }
        public List<string> Systems { get; set; }
        public double Multiplier { get; set; }
        public double Offset { get; set; }
        public bool Rare { get; set; }
        public bool Estimation { get; set; }
        public string PrefixName { get; set; }
        public string PrefixFreeName { get; set; }

        public Unit ToUnit(string key, DimensionDefinition dimension, List<MeasurementSystem> systems)
        {
            var unit = new Unit {
                Key = key,
                Name = Name,
                Plural = Plural,
                DimensionDefinition = dimension,
                Type = (UnitType) Enum.Parse(typeof(UnitType), Type, true), // TODO use json enum parsing
                OtherNames = OtherNames ?? new(),
                OtherSymbols = OtherSymbols ?? new(),
                Symbol = Symbol,
                Multiplier = Multiplier,
                Offset = Offset,
                IsRare = Rare,
                IsEstimation = Estimation,
                PrefixName = PrefixName,
                PrefixFreeName = PrefixFreeName,
                MeasurementSystemNames = Systems
            };
            dimension.Units.Add(unit);
            if (dimension.BaseUnitName == unit.Key) {
                dimension.BaseUnit = unit;
            }
            unit.UpdateMeasurementSystems(systems);

            return unit;
        }
    }
}