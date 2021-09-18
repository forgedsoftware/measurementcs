using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ForgedSoftware.Measurement.Entities;

namespace ForgedSoftware.Measurement {

	/// <summary>
	/// Builds MeasurementCorpus object and all data objects using data
	/// in the measurementcommon sub repo.
	/// </summary>
	public class CorpusBuilder
	{

		/// <summary>
		/// Main builder function
		/// </summary>
		public void PrepareMeasurementCorpus(MeasurementCorpus corpus)
		{
			var fileContents = File.ReadAllText(GetPath());
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			var sourceCorpus = JsonSerializer.Deserialize<SourceCorpus>(fileContents, options);
			
			corpus.AllSystems.AddRange(PrepareSystems(sourceCorpus));
			
			corpus.RootSystems.AddRange(PrepareTree(corpus.AllSystems).ToList());
			
			corpus.Dimensions.AddRange(PrepareDimensions(sourceCorpus, corpus.AllSystems).ToList());
			
			corpus.Dimensions.ForEach(s => s.UpdateDerived(corpus));
			
			List<DimensionDefinition> inheritingDims = corpus.Dimensions.Where(d => !string.IsNullOrWhiteSpace(d.InheritedUnits)).ToList();
			inheritingDims.ForEach(a => a.Units.AddRange(corpus.Dimensions.First(d => d.Key == a.InheritedUnits).Units));
			inheritingDims.ForEach(a => a.Units.ForEach(u => u.InheritedDimensionDefinitions.Add(a)));
			inheritingDims.ForEach(a => a.BaseUnit = a.Units.First(u => u.Key == a.BaseUnitName));
			
			corpus.Prefixes.AddRange(PreparePrefixes(sourceCorpus).ToList());
		}
		
		private string GetPath()
		{
			var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent;
			if (directoryInfo?.Parent == null)
			{
				throw new Exception("Directory containing json does not exist");
			}
			return Path.Combine(directoryInfo.Parent.FullName, "../common/systems.json");
		}

		private IEnumerable<MeasurementSystem> PrepareSystems(SourceCorpus sourceCorpus)
		{
			return sourceCorpus.Systems
				.Select(system => system.Value.ToMeasurementSystem(system.Key))
				.ToList();
		}

		private IEnumerable<MeasurementSystem> PrepareTree(IList<MeasurementSystem> systems)
		{
			foreach (var system in systems)
			{
				if (string.IsNullOrWhiteSpace(system.Inherits))
				{
					yield return system;
				} else
				{
					foreach (var parentSystem in systems)
					{
						if (parentSystem.Key == system.Inherits)
						{
							parentSystem.Children.Add(system);
							system.Parent = parentSystem;
						}
					}
				}
			}
		}
		
		private IEnumerable<DimensionDefinition> PrepareDimensions(SourceCorpus corpus, List<MeasurementSystem> systems)
		{
			foreach (var (key, value) in corpus.Dimensions)
			{
				yield return value.ToDimension(key, systems);
			}
		}

		private IEnumerable<Prefix> PreparePrefixes(SourceCorpus corpus)
		{
			foreach (var (key, value) in corpus.Prefixes)
			{
				yield return value.ToPrefix(key);
			}
		}
	}
}
