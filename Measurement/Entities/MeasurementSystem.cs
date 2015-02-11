using System.Collections.Generic;
using ForgedSoftware.Measurement.Interfaces;

namespace ForgedSoftware.Measurement.Entities {
	/// <summary>
	/// A measurement system is a man-made construction that provides a series
	/// of units for providing a quantified view of the world.
	/// </summary>
	/// <example>
	/// Examples of measurement systems are: metric system, imperial system
	/// </example>
	public class MeasurementSystem : Entity, ITreeNode<MeasurementSystem> {

		public MeasurementSystem() {
			Children = new List<MeasurementSystem>();
			Units = new List<Unit>();
		}

		#region Properties

		// Basic Properties
		public string Name { get; set; }
		public bool IsHistorical { get; set; }
		public string Inherits { get; set; }

		// Calculated Properties
		public List<Unit> Units { get; private set; }

		#endregion

		#region Measurement System Tree

		public MeasurementSystem Parent { get; set; }
		public List<MeasurementSystem> Children { get; private set; }

		public bool IsRoot() {
			return (Parent == null);
		}

		public List<MeasurementSystem> Ancestors {
			get {
				List<MeasurementSystem> list = (Parent == null) ? new List<MeasurementSystem>() : Parent.Ancestors;
				list.Add(this);
				return list;
			}
		}

		#endregion
	}
}
