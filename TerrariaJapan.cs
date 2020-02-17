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
		//private static GameCulture japanese = new GameCulture("ja-JP", 2434);
		private static TerrariaFontSet japaneseFontSet = null;
		private static TerrariaFontSet defaultFontSet = null;
		private static string japaneseLanguageText = null;

		public TerrariaJapan()
		{

		}

		public override void Load()
		{
			LoadJapaneseFonts();
			AddJapaneseSelectionToLanguageMenu();

			japaneseLanguageText = SynctamToTerrariaLanguageText();

			//japaneseFontSet.LoadIntoTerraria();			
			// LanguageManager.Instance.LoadLanguageFromFileText(japaneseLanguageText);
		}

		private void AddJapaneseSelectionToLanguageMenu()
		{
            IL.Terraria.Main.DrawMenu += HookLanguageMenu;
		}

		private void HookLanguageMenu(ILContext il)
		{
			var c = new ILCursor(il);

			// Jump to the "Language selection" menu, menuMode ID 1212
			if(!c.TryGotoNext(i => i.MatchLdcI4(1213)))
				return;

			// See where Polish is inserted. As of writing, this was the last language inserted, so we will
			// insert after it.
			if(!c.TryGotoNext(i => i.MatchLdstr("Language.Polish")))
				return;

			c.Index++; // Move past "ldstr "Language.Polish""
			c.Index++; // Move past "call string Terraria.Localization.Language::GetTextValue(string)"
			c.Index++; // Move past "stelem.ref"

			// Here, we will now insert Japanese an a language selection option.

			// (0) Load address of "array9" onto the stack
			c.Emit(Ldloc, (Int16)26);

			c.EmitDelegate<Action<string[]>>((array9) => {
				// Regular C# code
				File.WriteAllText("output.txt", array9.Length.ToString());
				array9[1] = "Japanese"; // Language.GetTextValue("Language.Japanese");
				array9[10] = "Japanese"; // Language.GetTextValue("Language.Japanese");
			});

			// Here, we will divert the call to "SetLanguage" on the selected option to load Japanese
			// if it was selected, or just load the selected one normally if it was not.

			if(!c.TryGotoNext(i => i.MatchCallvirt(typeof(LanguageManager).GetMethod("SetLanguage", new Type[] { typeof(int) }))))
				return;

			// Don't call SetLanguage, but instead call our hook below.
			/*c.Remove();

			// At this point, the selectedMenu is on the stack (we were going to call SetLanguage). Use that here to see if 
			// we selected Japanese, and load it if so. We can't use SetLanguage for this, because it requires the language
			// resources to embedded in the Terraria executable, which we can't do, without being tModLoader or Terraria itself.
			c.EmitDelegate<Action<int>>((selectedMenu) =>
			{
				// Regular C# code
				if(selectedMenu == 10)
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

			c.Emit(Pop);*/
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