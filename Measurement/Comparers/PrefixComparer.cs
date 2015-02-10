using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class PrefixComparer : EntityComparer<Prefix> {

		public static PrefixComparer Comparer { get; private set; }

		static PrefixComparer() {
			Comparer = new PrefixComparer();
		}

		protected override int CalculatePoints(Prefix val) {
			int points = 0;
			// TODO
			return points;
		}
	}
}
