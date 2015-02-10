using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class MeasurementSystemComparer : EntityComparer<MeasurementSystem> {

		public static MeasurementSystemComparer Comparer { get; private set; }

		static MeasurementSystemComparer() {
			Comparer = new MeasurementSystemComparer();
		}

		protected override int CalculatePoints(MeasurementSystem val) {
			int points = 0;
			// TODO
			return points;
		}
	}
}
