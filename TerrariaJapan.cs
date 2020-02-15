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
	public class TerrariaJapan : Mod
	{
		// public static GameCulture Japanse = new GameCulture("ja-JP", 10);

		public TerrariaJapan()
		{

		}

		public override void Load()
		{
			LoadJapaneseFonts();
			var results = SynctamToTerrariaLanguageText();
			LanguageManager.Instance.LoadLanguageFromFileText(results);
		}

		private void LoadJapaneseFonts()
		{
			Main.fontItemStack = GetFont("Fonts/Item_Stack");
			Main.fontMouseText = GetFont("Fonts/Mouse_Text");
			Main.fontDeathText = GetFont("Fonts/Death_Text");			
			Main.fontCombatText[0] = GetFont("Fonts/Combat_Text");
			Main.fontCombatText[1] = GetFont("Fonts/Combat_Crit");
		}

		private string SynctamToTerrariaLanguageText()
		{
			using (var reader = new StreamReader(GetFileStream("Localization/TerrariaSynctam.csv")))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var result = new Dictionary<string, Dictionary<string, string>>();
				
				csv.GetRecords<SynctamCsvRow>().ToList().ForEach(row => 
				{
					if(!result.ContainsKey(row.Group))
						result[row.Group] = new Dictionary<string, string>();
					result[row.Group].Add(row.Key, row.BestTranslation);
				});

				var text = JsonConvert.SerializeObject(result);
				File.WriteAllText("language_debug.txt", text);

				return text;
			}
		}

		private class SynctamCsvRow
		{
			[Name("[[Group]]")]
			public string Group { get; set; }

			[Name("[[Key]]")]
			public string Key { get; set; }

			[Name("[[Japanese]]")]
			public string Japanese { get; set; }

			[Name("[[MTrans]]")]
			public string MachineTranslation { get; set; }	

			public string BestTranslation { get => !string.IsNullOrWhiteSpace(Japanese) ? Japanese : MachineTranslation; }
		}
	}
}