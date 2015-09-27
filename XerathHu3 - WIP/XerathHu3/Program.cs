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

namespace XerathHu3
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Spell.Chargeable Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu Menu, SettingsMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Ezreal")
                return;

            Q = new Spell.Chargeable(SpellSlot.Q, 750, 1500, int.MaxValue, 250, int.MaxValue, 100); 
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, int.MaxValue, 200);
            E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250,int.MaxValue, 60);
            R = new Spell.Skillshot(SpellSlot.R, 3200, SkillShotType.Linear, 70, 2000, (int)(160f));
        }

    }
}
