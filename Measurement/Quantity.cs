﻿using System;
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

		#endregion

		string ISerializable.ToJson() {
			return this.ToJson();
		}
	}
}
