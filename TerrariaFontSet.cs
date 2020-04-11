using Terraria;
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
                ItemStack = Main.instance.OurLoad<DynamicSpriteFont>("Fonts/Item_Stack"),
                MouseText = Main.instance.OurLoad<DynamicSpriteFont>("Fonts/Mouse_Text"),
                DeathText = Main.instance.OurLoad<DynamicSpriteFont>("Fonts/Death_Text"),
                CombatText = Main.instance.OurLoad<DynamicSpriteFont>("Fonts/Combat_Text"),
                CombatCrit = Main.instance.OurLoad<DynamicSpriteFont>("Fonts/Combat_Crit")
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