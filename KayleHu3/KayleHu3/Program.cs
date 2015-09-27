using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace KayleHu3
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Targeted W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        public static Menu Menu, SettingsMenu;


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Ezreal")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Targeted(SpellSlot.W, 900);
            E = new Spell.Active(SpellSlot.E, 525);
            R = new Spell.Targeted(SpellSlot.R, 900);

            Menu = MainMenu.AddMenu("Kayle Hu3", "kaylehu3");
            Menu.AddGroupLabel("Kayle Hu3 0.1");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qc", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wc", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ec", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Rc", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qh", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Eh", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlh", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("Elh", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Qlc", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Wks", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rks", new CheckBox("Use R KillSteal"));
            SettingsMenu.AddLabel("Ult Manager");
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R Combo"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
    }
}
