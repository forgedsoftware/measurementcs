using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace ForgedSoftware.Measurement {
	public static class Extensions {
		public static List<T> CopyList<T>(this List<T> list) where T : ICopyable<T> {
			return list.Select(item => item.Copy()) as List<T>;
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
	}
}
