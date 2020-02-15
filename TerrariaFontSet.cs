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
using ReLogic.Graphics;

namespace TerrariaJapan
{
    class TerrariaFontSet
    {
        public DynamicSpriteFont ItemStack { get; set; }
        public DynamicSpriteFont MouseText { get; set; }
        public DynamicSpriteFont DeathText { get; set; }
        public DynamicSpriteFont CombatText { get; set; }
        public DynamicSpriteFont CombatCrit { get; set; }

        public static TerrariaFontSet FromCurrentFonts()
        {
            return new TerrariaFontSet()
            {
                ItemStack = Main.fontItemStack,
                MouseText = Main.fontMouseText,
                DeathText = Main.fontDeathText,	
                CombatText = Main.fontCombatText[0],
                CombatCrit = Main.fontCombatText[1]
            };
        }

        public void LoadIntoTerraria()
        {
			Main.fontItemStack = ItemStack;
			Main.fontMouseText = MouseText;
			Main.fontDeathText = DeathText;			
			Main.fontCombatText[0] = CombatText;
			Main.fontCombatText[1] = CombatCrit;
        }
    }
}