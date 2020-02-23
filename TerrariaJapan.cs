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
        private static TerrariaFontSet japaneseFontSet = null;
        private static TerrariaFontSet defaultFontSet = null;
        private static string japaneseLanguageText = null;

        public TerrariaJapan()
        {

        }

        public override void Load()
        {
            japaneseLanguageText = SynctamToTerrariaLanguageText();
            defaultFontSet = TerrariaFontSet.FromCurrentFonts();
            japaneseFontSet = new TerrariaFontSet()
            {
                ItemStack = GetFont("Fonts/Item_Stack"),
                MouseText = GetFont("Fonts/Mouse_Text"),
                DeathText = GetFont("Fonts/Death_Text"),
                CombatText = GetFont("Fonts/Combat_Text"),
                CombatCrit = GetFont("Fonts/Combat_Crit")
            };
            
            IL.Terraria.Main.DrawMenu += AddJapaneseToLanguageSelectionHook;		
        }

        private void AddJapaneseToLanguageSelectionHook(ILContext il)
        {
            var c = new ILCursor(il);

            // 
            // First, begin at the "language selection screen" in DrawMenu.
            // That is Menu Mode ID 1213 (menuMode == 1213)
            // 
            if(!c.TryGotoNext(i => i.MatchLdcI4(1213)))
                return;

            //
            // Insert Japanese as a menu option on this screen. This first addition will just update the
            // selections; a number of future modifications to show this value, make it selectable,
            // and so on are below.
            //
            // We do tis by switching the items array to make index 10 "Japanese", and 11 "Back"
            // It's convient here to use a deligate to make multiple updates.
            // 
            if(!c.TryGotoNext(i => i.MatchCallvirt(typeof(LocalizedText).GetMethod("get_Value"))))
                return;
            if(!c.TryGotoNext(i => i.MatchCallvirt(typeof(LocalizedText).GetMethod("get_Value"))))
                return;				
            c.Index++; // Move past "callvirt instance string Terraria.Localization.LocalizedText::get_Value()"
            c.Index++; // Move past "stelem.ref"
            // (0) Load address of "array9" onto the stack
            c.Emit(Ldloc, (Int16)26);
            c.EmitDelegate<Action<string[]>>((array9) => {
                // See if the current language has an entry for Japanese. 
                // If not, fall back to English.
                string japaneseKey = "Language.Japanese";
                string japaneseValue = Language.GetTextValue(japaneseKey);
                if(japaneseValue == japaneseKey)
                    japaneseValue = "日本語 (Japanese)";

                array9[10] = japaneseValue;
                array9[11] = Language.GetTextValue("UI.Back");
            });

            //
            // Even though we updated the menu items array, we need to set another local variable (decompiled as "num5") 
            // to the actual amount of  items in the array we should display. Set that to 12 instead of 11, for the new item.
            // we do that by setting num5 = 12 instead of 11
            // 
            if(!c.TryGotoNext(i => i.MatchStloc(8)))
                return;
            c.Index--; // Move to "ldc.i4.s 11"
            c.Remove();
            c.Emit(Ldc_I4, (Int16)12);			

            //
            // Update the back button be menu index 11 instead of 10. This accounts for the new menu item we inserted
            // above. We do that by setting turning (selectedMenu == 10) into (selectedMenu == 11) for the condition
            // on when to return.
            // 
            //if(!c.TryGotoNext(i => i.MatchLdfld(typeof(Main).GetField("selectedMenu", BindingFlags.Instance | BindingFlags.NonPublic))))	
            //	return;
            if(!c.TryGotoNext(i => i.MatchLdcI4(10)))
                return;
            c.Remove();
            c.Emit(Ldc_I4, (Int16)11);
        
            // 
            // Update the call to "SetLanguage" when a language is selected to do special handling for Japanese.
            // We can't use SetLanguage for this, because it requires the language resources to embedded in the Terraria executable, 
            // which we can't do, without being tModLoader or Terraria itself. We also need to load a special font, and restore
            // it when we move away from Japanese.
            //
            if(!c.TryGotoNext(i => i.MatchCallvirt(typeof(LanguageManager).GetMethod("SetLanguage", new Type[] { typeof(int) }))))
                return;
            c.Remove(); // Remove the call to SetLanguage, use our cooler callback below.
            // At this point, the selectedMenu is on the stack (as we were going to call SetLanguage). 
            // Use that here to see if we selected Japanese, and load our custom way if so, otherwise
            // just call SetLanguage.
            c.EmitDelegate<Action<int>>((selectedMenu) =>
            {
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
            c.Emit(Pop);

            //
            // At this point, Japanese is a menu selection, we've updated the variables to make it appear and be
            // selectable, but we need to adjust the spacing of the menu items so it's with the rest of the language
            // items.
            // We do this by modifying "array4": Set the index to which the extra spacing is applied to to be
            // 11 instead of 10.
            //
            if(!c.TryGotoNext(i => i.MatchLdloc(19)))
                return;
            if(!c.TryGotoNext(i => i.MatchLdloc(19)))
                return;
            c.Index++; // Move past "ldloc.s 19"
            c.Remove();
            c.Emit(Ldc_I4, 11);

            //
            // Finally, the "Back" button is larger than the others, but it now applies to the Japanese selection. 
            // Like the above mehtod, set this to now apply to menu item 11 isntead of 10. 
            // We do this by modifying array7[10] = 0.95f into -> array7[11] = 0.95f
            // 
            if(!c.TryGotoNext(i => i.MatchLdcR4(0.95f)))
                return;			
            c.Index--; // Move to "ldc.i4.s 10"
            c.Remove();
            c.Emit(Ldc_I4, 11);			
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