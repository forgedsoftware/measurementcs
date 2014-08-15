using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Extension methods for various parts of the Measurement library.
	/// </summary>
	public static class Extensions {

		public static List<T> CopyList<T>(this List<T> list) where T : ICopyable<T> {
			return list.Select(item => item.Copy()).ToList();
		}

		public static List<Dimension> Simplify(this List<Dimension> list, ref double value) {
			var newDimensions = new List<Dimension>();
			var processedDimensions = new List<int>();
			double computedValue = value;

			for (int index = 0; index < list.Count; index++)
			{
				Dimension dimension = list[index];
				if (dimension.Power != 0 && !processedDimensions.Contains(index))
				{
					for (int i = index + 1; i < list.Count; i++)
					{
						if (dimension.Unit.System.Name == list[i].Unit.System.Name)
						{
							KeyValuePair<Dimension, double> dimValuePair = dimension.Combine(computedValue, list[i]);
							dimension = dimValuePair.Key;
							computedValue = dimValuePair.Value;
							processedDimensions.Add(i);
						}
					}
					if (dimension.Power != 0)
					{
						newDimensions.Add(dimension);
					}
					processedDimensions.Add(index);
				}
			}

			value = computedValue;
			return newDimensions;
		}

		public static string ToJson(this ISerializable obj) {
			using (var stream = new MemoryStream()) {
				var a = new DataContractJsonSerializer(obj.GetType());
				a.WriteObject(stream, obj);
				stream.Position = 0;
				using (var reader = new StreamReader(stream)) {
					return reader.ReadToEnd();
				}
			}
		}

		public static string ToSuperScript(this int number) {
			return ((double) number).ToSuperScript();
		}

		public static string ToSuperScript(this double number) {
			var supers = new Dictionary<char, char> {
					{ '0', '\u2070' }, { '1', '\u00B9' }, { '2', '\u00B2' }, { '3', '\u00B3' }, { '4', '\u2074' },
					{ '5', '\u2075' }, { '6', '\u2076' }, { '7', '\u2077' }, { '8', '\u2078' }, { '9', '\u2079' }, { '-', '\u207B'}
				};

			return number.ToString().Aggregate("", (current, t) => current + supers[t]);
		}
	}
}
