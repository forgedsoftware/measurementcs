using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class UnitComparer : EntityComparer<Unit> {

		public static UnitComparer Comparer { get; private set; }

		static UnitComparer() {
			Comparer = new UnitComparer();
		}

		internal override int CalculatePoints(Unit val) {
			int points = 0;
			points += DimensionDefinitionComparer.Comparer.CalculatePoints(val.DimensionDefinition);
			if (val.IsBaseUnit()) {
				points += 10000;
			}
			if (!val.IsRare) {
				points += 1000;
			}
			if (!val.IsEstimation) {
				points += 100;
			}
			return points;
		}
	}
}
