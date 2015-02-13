using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class PrefixComparer : EntityComparer<Prefix> {

		public static PrefixComparer Comparer { get; private set; }

		static PrefixComparer() {
			Comparer = new PrefixComparer();
		}

		internal override int CalculatePoints(Prefix val) {
			int points = 0;
			if (val.Type != PrefixType.SiUnofficial) {
				points += 1000;
			}
			if (!val.IsRare) {
				points += 100;
			}
			if (val.Type != PrefixType.SiBinary) {
				points += 10;
			}
			return points;
		}
	}
}
