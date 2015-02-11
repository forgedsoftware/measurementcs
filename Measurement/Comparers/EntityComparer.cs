using System;
using System.Collections.Generic;
using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public abstract class EntityComparer<T> : IComparer<T>
			where T : Entity {

		public int Compare(T x, T y) {
			// Check If Null
			if (x == null) {
				return (y == null) ? 0 : -1;
			}
			if (y == null) {
				return 1;
			}

			// Roughly sort based on points
			int xPoints = CalculatePoints(x);
			int yPoints = CalculatePoints(y);

			// Fine sort based on key
			if (xPoints == yPoints) {
				return string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
			}
			return (xPoints > yPoints) ? 1 : -1;
		}

		/// <summary>
		/// Rates an entity by assigning points to it. Higher points will
		/// appear earlier in sorted results.
		/// </summary>
		/// <param name="val">The entity.</param>
		/// <returns>An integer amount of points</returns>
		protected abstract int CalculatePoints(T val);
	}
}
