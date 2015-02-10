using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using ForgedSoftware.Measurement.Entities;
using ForgedSoftware.Measurement.Interfaces;

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
		public static List<Dimension> SimpleSimplify<TNumber>(this List<Dimension> list, ref TNumber value)
				where TNumber : INumber<TNumber> {
			var newDimensions = new List<Dimension>();
			var processedDimensions = new List<int>();
			TNumber computedValue = value;

			for (int index = 0; index < list.Count; index++) {
				Dimension dimension = list[index];
				if (dimension.Power != 0 && !processedDimensions.Contains(index)) {
					for (int i = index + 1; i < list.Count; i++) {
						if (dimension.Unit.DimensionDefinition.Name == list[i].Unit.DimensionDefinition.Name) {
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

		public static List<Dimension> Simplify<TNumber>(this List<Dimension> list, ref TNumber value)
				where TNumber : INumber<TNumber> {
			// TODO somewhere some option control is needed
				// want to deep simplify?
				// prefer base systems?
			var dimensionLists = new List<SimplifiedDimensions<TNumber>>();
			// simple simplify
			TNumber copy = value.Copy();
			List<Dimension> basicSimplifiedDimensions = list.SimpleSimplify(ref copy);
			// keep copy of simplified dimensions
			dimensionLists.Add(new SimplifiedDimensions<TNumber>(basicSimplifiedDimensions, copy));
			if (!MeasurementCorpus.Options.IgnoreDerivedSystems) {
				// convert dimensions to base systems
				TNumber copy2 = value.Copy();
				List<Dimension> baseSystemDimensions = basicSimplifiedDimensions
					.SelectMany(d => d.ToBaseSystems(ref copy2)).ToList();
				// simple simplify
				baseSystemDimensions = baseSystemDimensions.SimpleSimplify(ref copy2);
				// keep copy of dimensions in base systems
				dimensionLists.Add(new SimplifiedDimensions<TNumber>(baseSystemDimensions, copy2));
				// find matching systems based on base dimensions
				dimensionLists.AddRange(FindDerivedDimensions(baseSystemDimensions, copy2));
				// keep only the results with a max of X dimensions (determine X somehow!)
				// score dimension sets and choose heuristically the fittest result
			}
			dimensionLists.ForEach(x => x.ScoreList());
			SimplifiedDimensions<TNumber> bestDimensions = dimensionLists.OrderByDescending(x => x.Score).First();
			value = bestDimensions.Value;
			return bestDimensions.Dimensions;
		}

		private static List<SimplifiedDimensions<TNumber>> FindDerivedDimensions<TNumber>
				(List<Dimension> dimensions, TNumber value)
				where TNumber : INumber<TNumber> {
			var simplifiedDimensions = new List<SimplifiedDimensions<TNumber>>();
			// for each derived system
			foreach (DimensionDefinition dimDef in MeasurementCorpus.Dimensions.Where(s => s.IsDerived())) {
				List<Dimension> neededDimensions = dimDef.Derived.CopyList();
				// for each existing dimension
				List<Dimension> currentDimensions = dimensions.CopyList();
				var currentDimensionsToRemove = new List<Dimension>();
				foreach (Dimension dimension in currentDimensions) {
					if (dimension.Unit.DimensionDefinition.IsDerived()) {
						break;
					}
					bool exactMatchFound = dimension.MatchDimensions(neededDimensions);
					if (exactMatchFound) {
						currentDimensionsToRemove.Add(dimension);
					}
				}
				currentDimensionsToRemove.ForEach(d => currentDimensions.Remove(d));

				// if neededDimensions is empty
				if (neededDimensions.Count == 0) {
					// add derived system to dimensions
					currentDimensions.Add(new Dimension(dimDef.BaseUnit, 1));
					// save dimensions as a new set
					simplifiedDimensions.Add(new SimplifiedDimensions<TNumber>(currentDimensions, value.Copy())); // Do wee need to simple simplify again here??
					// repeat for each derived system until no more matches are found - recursive?
					simplifiedDimensions.AddRange(FindDerivedDimensions(currentDimensions, value.Copy()));
				}
			}
			return simplifiedDimensions;
		}

		/// <summary>
		/// A helper class to provide handling and scoring of dimensions while simplifying.
		/// </summary>
		private class SimplifiedDimensions<TNumber>
				where TNumber : INumber<TNumber> {

			public SimplifiedDimensions(List<Dimension> dimensions, TNumber value) {
				Dimensions = dimensions;
				Value = value;
			}

			public List<Dimension> Dimensions { get; private set; }
			public TNumber Value { get; private set; }
			public double Score { get; private set; }

			/// <summary>
			/// Heuristically score the list of dimensions
			/// </summary>
			public void ScoreList() {
				double score = 1000;
				// minimise number of dimensions
				score -= Dimensions.Count * 10;
				// minimise total powers
				score -= Dimensions.Select(x => Math.Abs(x.Power)).Sum() * 5;
				// penalize derived dimensions
				score -= Dimensions.Count(d => d.Unit.DimensionDefinition.IsDerived()) * 5;
				// favour positive dimensions over negative dimensions
				score -= Dimensions.Count(x => x.Power < 0) * 3;
				// favour units in the original dimension list ??????
				// TODO
				// TODO - can we determine anything heuristically about the value? maybe penalize really small/large numbers/NaN?
				Score = score;
			}

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
