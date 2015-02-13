using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement.Comparers {

	public class DimensionDefinitionComparer : EntityComparer<DimensionDefinition> {

		public static DimensionDefinitionComparer Comparer { get; private set; }

		static DimensionDefinitionComparer() {
			Comparer = new DimensionDefinitionComparer();
		}

		internal override int CalculatePoints(DimensionDefinition dimDef) {
			int points = 0;
			if (!dimDef.IsDerived()) {
				points += 100;
			}
			if (!dimDef.IsDimensionless) {
				points += 1000;
			}
			if (!dimDef.Vector) {
				points += 10000;
			}
			return points;
		}
	}
}
