using System;
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
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace TerrariaJapan
{
	public class TerrariaJapan : Mod
	{
		private static GameCulture japanese = new GameCulture("ja-JP", 2434);
		private static TerrariaFontSet japaneseFontSet = null;
		private static TerrariaFontSet defaultFontSet = null;
		private static string japaneseLanguageText = null;

		public TerrariaJapan()
		{

		}

		public override void Load()
		{
			LoadJapaneseFonts();
			// AddJapaneseSelectionToLanguageMenu();

			japaneseLanguageText = SynctamToTerrariaLanguageText();

			japaneseFontSet.LoadIntoTerraria();			
			LanguageManager.Instance.LoadLanguageFromFileText(japaneseLanguageText);
		}

		private void AddJapaneseSelectionToLanguageMenu()
		{
            IL.Terraria.Main.DrawMenu += HookLanguageMenu;
		}

		private void HookLanguageMenu(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(i => i.MatchLdarg(10));
			c.Emit(Ldarg_0);
			c.EmitDelegate<Action>(() =>
			{
				// Regular c# code
				var selectedMenu = (int)typeof(Main).GetProperty("selectedMenu", BindingFlags.NonPublic).GetValue(Main.instance);
				if(selectedMenu == japanese.LegacyId)
				{
					japaneseFontSet.LoadIntoTerraria();
					LanguageManager.Instance.LoadLanguageFromFileText(japaneseLanguageText);
				}
				else
				{
					defaultFontSet.LoadIntoTerraria();
					LanguageManager.Instance.SetLanguage(selectedMenu);
				}
			});
		}

		private void OnLanguageChanged(LanguageManager manager)
		{
			if(manager.ActiveCulture != japanese)
			{
				defaultFontSet.LoadIntoTerraria();
			}
		}

		private void LoadJapaneseFonts()
		{
			defaultFontSet = TerrariaFontSet.FromCurrentFonts();

			japaneseFontSet = new TerrariaFontSet()
			{
				ItemStack = GetFont("Fonts/Item_Stack"),
				MouseText = GetFont("Fonts/Mouse_Text"),
				DeathText = GetFont("Fonts/Death_Text"),
				CombatText = GetFont("Fonts/Combat_Text"),
				CombatCrit = GetFont("Fonts/Combat_Crit")
			};
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

				return JsonConvert.SerializeObject(result);
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