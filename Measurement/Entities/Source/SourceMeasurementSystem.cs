namespace ForgedSoftware.Measurement.Entities
{
    public class SourceMeasurementSystem
    {
        public string Name { get; set; }
        public bool Historical { get; set; }
        public string Inherits { get; set; }
        
        public MeasurementSystem ToMeasurementSystem(string key)
        {
            return new()
            {
                Key = key,
                Name = Name,
                IsHistorical = Historical,
                Inherits = Inherits
            };
        }
    }
}