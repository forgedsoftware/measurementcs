using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class MeasurementSystemComparer : EntityComparer<MeasurementSystem> {

		public static MeasurementSystemComparer Comparer { get; private set; }

		static MeasurementSystemComparer() {
			Comparer = new MeasurementSystemComparer();
		}

		internal override int CalculatePoints(MeasurementSystem val) {
			int points = 0;
			if (val.IsRoot()) {
				points += 1000;
			}
			points -= (10 * val.Ancestors.Count);
			return points;
		}
	}
}
