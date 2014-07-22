using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace ForgedSoftware.Measurement {
	public static class Extensions {
		public static List<T> CopyList<T>(this List<T> list) where T : ICopyable<T> {
			return list.Select(item => item.Copy()).ToList();
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
