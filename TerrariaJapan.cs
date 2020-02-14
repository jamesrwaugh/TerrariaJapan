using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Localization;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace TerrariaJapan
{
	using TerrariaLocalizaionStructure = Dictionary<string, Dictionary<string, string>>;

	public class TerrariaJapan : Mod
	{
		// public static GameCulture Japanse = new GameCulture("ja-JP", 10);

		public TerrariaJapan()
		{

		}

		public override void Load()
		{
			var results = SynctamToTerraria();
			File.AppendAllText("out3.txt", JsonConvert.SerializeObject(results));
		}

		private TerrariaLocalizaionStructure SynctamToTerraria()
		{
			using (var reader = new StreamReader(GetFileStream("Localization/TerrariaSynctam.csv")))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var result = new TerrariaLocalizaionStructure();
				
				csv.GetRecords<SynctamCsvRow>().ToList().ForEach(row => 
				{
					if(!result.ContainsKey(row.Group))
						result[row.Group] = new Dictionary<string, string>();
					result[row.Group].Add(row.Key, row.BestTranslation);
				});

				return result;				
			}
		}

		private class SynctamCsvRow
		{
			[Name("[[FileID]]")]
			public string FileID { get; set; }

			[Name("[[Group]]")]
			public string Group { get; set; }

			[Name("[[Key]]")]
			public string Key { get; set; }

			[Name("[[Japanese]]")]
			public string Japanese { get; set; }

			[Name("[[MTrans]]")]
			public string MTrans { get; set; }	

			public string BestTranslation { get => !string.IsNullOrWhiteSpace(Japanese) ? Japanese : MTrans; }
		}
	}
}