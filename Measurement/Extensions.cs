using System.Collections.Generic;
using System.Linq;

namespace ForgedSoftware.Measurement {
	public static class Extensions {
		public static List<T> CopyList<T>(this List<T> list) where T : ICopyable<T> {
			return list.Select(item => item.Copy()) as List<T>;
		}
	}
}
