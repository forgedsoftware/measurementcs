using System;

namespace ForgedSoftware.Measurement.Entities
{
    public class CommonPrefix
    {
        public string Symbol { get; set; }
        public string Type { get; set; }
        public double Multiplier { get; set; }
        public double Power { get; set; }
        public double Base { get; set; }
        public bool Rare { get; set; }

        public Prefix ToPrefix(string key)
        {
            return new()
            {
                Key = key,
                Symbol = Symbol,
                Type = (PrefixType) Enum.Parse(typeof(PrefixType), Type, true), // TODO use json enum parsing,
                IsRare = Rare,
                Multiplier = Multiplier,
                Power = Power,
                Base = Base
            };
        }
    }
}