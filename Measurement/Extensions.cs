﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Extension methods for various parts of the Measurement library.
	/// </summary>
	public static class Extensions {

		/// <summary>
		/// Deep copies a list of where the members extend ICopyable
		/// </summary>
		/// <typeparam name="T">Type of the list members</typeparam>
		/// <param name="list">The list to copy</param>
		/// <returns>The copied list</returns>
		public static List<T> CopyList<T>(this List<T> list) where T : ICopyable<T> {
			return list.Select(item => item.Copy()).ToList();
		}

		/// <summary>
		/// A generic extension method for simplifying a list of dimensions, maintaining
		/// the order of the dimensions where possible.
		/// Simplification is done by combining dimensions that have units of the same system
		/// and converting the associated value as needed.
		/// </summary>
		/// <param name="list">The list of dimensions to simplify</param>
		/// <param name="value">The value to be converted as the dimensions are simplified</param>
		/// <returns>A new list of simplified dimensions</returns>
		public static List<Dimension> Simplify<TNumber>(this List<Dimension> list, ref TNumber value) 
				where TNumber : INumber<TNumber> {
			var newDimensions = new List<Dimension>();
			var processedDimensions = new List<int>();
			TNumber computedValue = value;

			for (int index = 0; index < list.Count; index++) {
				Dimension dimension = list[index];
				if (dimension.Power != 0 && !processedDimensions.Contains(index)) {
					for (int i = index + 1; i < list.Count; i++) {
						if (dimension.Unit.System.Name == list[i].Unit.System.Name) {
							dimension = dimension.Combine(ref computedValue, list[i]);
							processedDimensions.Add(i);
						}
					}
					if (dimension.Power != 0) {
						newDimensions.Add(dimension);
					}
					processedDimensions.Add(index);
				}
			}

			value = computedValue;
			return newDimensions;
		}

		/// <summary>
		/// Serializes an object that implements ISerializable into a json string.
		/// </summary>
		/// <param name="obj">The object to be serialized</param>
		/// <returns>A json string of the serialized object</returns>
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

		/// <summary>
		/// Deserializes a json string into an object.
		/// </summary>
		/// <typeparam name="T">The type of the object</typeparam>
		/// <param name="json">The json string representing the object</param>
		/// <returns>The deserialized object</returns>
		public static T FromJson<T>(this string json) where T : ISerializable {
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
				var dataContract = new DataContractJsonSerializer(typeof (T));
				return (T)dataContract.ReadObject(stream);
			}
		}

		/// <summary>
		/// Converts an integer value into a string of superscript unicode.
		/// </summary>
		/// <param name="number">The number to be converted</param>
		/// <returns>The value as superscript unicode</returns>
		public static string ToSuperScript(this int number) {
			var supers = new Dictionary<char, char> {
					{ '0', '\u2070' }, { '1', '\u00B9' }, { '2', '\u00B2' }, { '3', '\u00B3' }, { '4', '\u2074' },
					{ '5', '\u2075' }, { '6', '\u2076' }, { '7', '\u2077' }, { '8', '\u2078' }, { '9', '\u2079' }, { '-', '\u207B'}
				};

			return number.ToString(CultureInfo.InvariantCulture).Aggregate("", (current, t) => current + supers[t]);
		}

		public static bool OperationsAllowed<TNumber>(this TNumber number, params Func<TNumber, TNumber>[] neededOperations)
				where TNumber : INumber<TNumber> {
			bool allowed = true;
			try {
				neededOperations.ToList().ForEach(f => f(number));
			} catch (Exception ex) {
				allowed = false;
			}
			return allowed;
		}

		public static string ExtendedToString(this double value, string format, IFormatProvider formatProvider) {
			string startLetter = format.Substring(0, 1).ToUpper();
			if (new Regex(@"[C-G]|N|P|R|X|[^A-Z]").IsMatch(startLetter)) {
				return value.ToString(format, formatProvider);
			}
			switch (startLetter) {
				case "Q": // Quantity format string
					return value.ExtendedToString(format.Substring(1), formatProvider) + " {0}";
				case "S": // Straight scientific format
					return ScientificFormat(value, format.Substring(1), formatProvider, false);
				case "T": // Text version of scientific format
					return ScientificFormat(value, format.Substring(1), formatProvider, true);
				default:
					throw new FormatException("Provided format was not valid.");
			}
		}

		private static string ScientificFormat(double value, string format, IFormatProvider formatProvider, bool asciiOnly) {
			//int sigFig = int.Parse(format);

			string valueStr = value.ToString("G" + format, formatProvider);

			// Exponents
			int eIndex = valueStr.IndexOf("E", StringComparison.InvariantCulture);
			if (eIndex >= 0) {
				double exponent = Math.Floor(Math.Log(value) / Math.Log(10));
				valueStr = valueStr.Substring(0, eIndex);
				string exponentStr = (asciiOnly) ? "^" + exponent : ((int)exponent).ToSuperScript();
				valueStr += " x 10" + exponentStr;
			}

			return valueStr;
		}
	}
}
