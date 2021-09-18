using System.Collections.Generic;

namespace ForgedSoftware.Measurement.Entities
{
    public class SourceCorpus
    {
        public Dictionary<string, SourceMeasurementSystem> Systems { get; set; }
        public Dictionary<string, CommonDimension> Dimensions { get; set; }
        public Dictionary<string, CommonPrefix> Prefixes { get; set; }
    }
}