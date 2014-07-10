using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ForgedSoftware.Measurement {

	[DataContract]
	public class Quantity : ISerializable {

		private Quantity() {
			Dimensions = new List<Dimension>();
		}

		public Quantity(double value)
			: this() {
			Value = value;
		}

		public Quantity(double value, IEnumerable<string> unitNames)
			: this(value) {
			foreach (string unitName in unitNames) {
				Dimensions.Add(new Dimension(unitName));
			}
		}

		public Quantity(double value, IEnumerable<Dimension> dimensions)
			: this(value) {
			Dimensions.AddRange(dimensions);
		}

		[DataMember(Name = "value")]
		public double Value { get; private set; }
		[DataMember(Name = "dimensions")]
		public List<Dimension> Dimensions { get; private set; }

		public bool IsDimensionless {
			get { return Dimensions.Count == 0; }
		}

		#region Conversion

		public Quantity Convert(string unitName) {
			return Convert(MeasurementFactory.FindUnit(unitName));
		}

		public Quantity Convert(Unit unit) {
			Quantity quantityAsBase = ConvertToBase();
			return quantityAsBase.ConvertFromBase(unit);
		}

		public Quantity ConvertToBase() {
			double convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				KeyValuePair<Dimension, double> result = dimension.ConvertToBase(convertedValue);
				convertedValue = result.Value;
				newDimensions.Add(result.Key);
			}
			return new Quantity(convertedValue, newDimensions);
		}

		protected Quantity ConvertFromBase(Unit unit) {
			double convertedValue = Value;
			var newDimensions = new List<Dimension>();

			foreach (var dimension in Dimensions) {
				if (dimension.Unit.System.Units.Contains(unit)) {
					KeyValuePair<Dimension, double> result = dimension.ConvertFromBase(convertedValue, unit);
					convertedValue = result.Value;
					newDimensions.Add(result.Key);
				}
				else {
					newDimensions.Add(dimension.Copy());
				}
			}
			return new Quantity(convertedValue, newDimensions);
		}

		#endregion

		#region Basic Math

		#region Dimensionless

		public Quantity Multiply(double value) {
			return new Quantity(Value*value, Dimensions.CopyList());
		}

		public Quantity Divide(double value) {
			return new Quantity(Value/value, Dimensions.CopyList());
		}

		public Quantity Add(double value) {
			return new Quantity(Value + value, Dimensions.CopyList());
		}

		public Quantity Subtract(double value) {
			return new Quantity(Value - value, Dimensions.CopyList());
		}

		#endregion

		public Quantity Multiply(Quantity q) {
			throw new NotImplementedException();
		}

		public Quantity Divide(Quantity q) {
			throw new NotImplementedException();
		}

		public Quantity Add(Quantity q) {
			throw new NotImplementedException();
		}

		public Quantity Subtract(Quantity q) {
			throw new NotImplementedException();
		}

		#endregion

		#region Extended Math

		public Quantity Abs() { return new Quantity(Math.Abs(Value), Dimensions.CopyList()); }
		public Quantity Acos() { return new Quantity(Math.Acos(Value), Dimensions.CopyList()); }
		public Quantity Asin() { return new Quantity(Math.Asin(Value), Dimensions.CopyList()); }
		public Quantity Atan() { return new Quantity(Math.Atan(Value), Dimensions.CopyList()); }
		public Quantity Ceiling() { return new Quantity(Math.Ceiling(Value), Dimensions.CopyList()); }
		public Quantity Cos() { return new Quantity(Math.Cos(Value), Dimensions.CopyList()); }
		public Quantity Exp() { return new Quantity(Math.Exp(Value), Dimensions.CopyList()); }
		public Quantity Floor() { return new Quantity(Math.Floor(Value), Dimensions.CopyList()); }
		public Quantity Log() { return new Quantity(Math.Log(Value), Dimensions.CopyList()); }
		public Quantity Log10() { return new Quantity(Math.Log10(Value), Dimensions.CopyList()); }
		public Quantity Round() { return new Quantity(Math.Round(Value), Dimensions.CopyList()); }
		public Quantity Sin() { return new Quantity(Math.Sin(Value), Dimensions.CopyList()); }
		public Quantity Sqrt() { return new Quantity(Math.Sqrt(Value), Dimensions.CopyList()); }
		public Quantity Tan() { return new Quantity(Math.Tan(Value), Dimensions.CopyList()); }

		// Extra functions not avaliable in JS version
		public Quantity Cosh() { return new Quantity(Math.Cosh(Value), Dimensions.CopyList()); }
		public Quantity Sinh() { return new Quantity(Math.Sinh(Value), Dimensions.CopyList()); }
		public Quantity Tanh() { return new Quantity(Math.Tanh(Value), Dimensions.CopyList()); }

		// TODO: Add Quantity based version, check dimensions are equivalent
		public Quantity Atan2(double y) { return new Quantity(Math.Atan2(y, Value), Dimensions.CopyList()); }
		// TODO: Add Quantity based version, check dimensions are equivalent
		public Quantity Pow(double y) { return new Quantity(Math.Pow(Value, y), Dimensions.CopyList()); }

		// TODO: Add Quantity based version, check dimensions are equivalent
		public Quantity Max(double y) { return new Quantity(Math.Max(Value, y), Dimensions.CopyList()); }
		// TODO: Add Quantity based version, check dimensions are equivalent
		public Quantity Min(double y) { return new Quantity(Math.Min(Value, y), Dimensions.CopyList()); }

		#endregion

		string ISerializable.ToJson() {
			return this.ToJson();
		}
	}
}
