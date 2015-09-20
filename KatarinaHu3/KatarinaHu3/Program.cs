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
using SharpDX;


namespace KatarinaHu3
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Menu KatarinaMenu, SettingsMenu;
        public static bool inult = false;


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
            if (Player.Instance.ChampionName != "Katarina")
                return;

            TargetSelector.Init();
            Bootstrap.Init(null);
            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 370);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 540);

            KatarinaMenu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            KatarinaMenu.AddGroupLabel("Tristana Hu3 1.8");
            KatarinaMenu.AddSeparator();
            KatarinaMenu.AddLabel("Made By MarioGK");
            SettingsMenu = KatarinaMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboE", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassE", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killsteal", new CheckBox("KillSteal"));
            SettingsMenu.Add("killstealEW", new CheckBox("Use E->W KillSteal"));
            SettingsMenu.Add("killstealEWQ", new CheckBox("Use E->W->Q KillSteal"));
            SettingsMenu.Add("killstealQ", new CheckBox("Use Q KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                LaneClear();
            }
            if (SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }
        }
        //Damages      
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 60, 85, 110, 135, 160 }[Program.Q.Level] + 0.45 * _Player.FlatMagicDamageMod));
        }
        public static float Q2Damage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 15, 30, 45, 60, 75 }[Program.Q.Level] + 0.15 * _Player.FlatMagicDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 40, 75, 110, 145, 180 }[Program.W.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.6 * _Player.FlatPhysicalDamageMod));
        }
        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 40, 70, 100, 130, 160 }[Program.E.Level] + 0.25 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35 * 8, 55 * 8, 75 * 8 }[Program.R.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.37 * _Player.FlatPhysicalDamageMod));
        }
        private static void Killsteal()
        {
            var useKS = SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue;
            var useEW = SettingsMenu["killstealEW"].Cast<CheckBox>().CurrentValue;
            var useEWQ = SettingsMenu["killstealEWQ"].Cast<CheckBox>().CurrentValue;
            var useQ = SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead))
            {
                if (target == null) return;
                if (useKS && useEW && target.Health < EDamage(target) + WDamage(target))
                {
                    E.Cast(target);
                    W.Cast();
                }
                if (useKS && useEWQ && target.Health < EDamage(target) + WDamage(target) + QDamage(target))
                {
                    E.Cast(target);
                    W.Cast();
                    Q.Cast(target);
                }
                if (useKS && useQ && target.Health < QDamage(target) + WDamage(target))
                {
                    Q.Cast(target);
                }
            }
        }
        private static void Combo()
        {
            foreach (var target in HeroManager.Enemies.Where(o => !o.IsDead))
            {
                var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
                var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
                var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
                var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;
                if (target == null || inult == true) return;
                if (useQ && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useE && target.IsValidTarget(Q.Range))
                {
                    E.Cast(target);
                }
                if (useW && target.IsValidTarget(Q.Range))
                {
                    W.Cast();
                }
                if (useR && target.IsValidTarget(Q.Range))
                {
                    R.Cast();
                    inult = true;
                }
            }

        }
        private static void Harass()
        {
            foreach (var target in HeroManager.Enemies.Where(o => !o.IsDead))
            {
                var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
                var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;
                var useE = SettingsMenu["harassE"].Cast<CheckBox>().CurrentValue;
                if (target == null || inult == true) return;
                if (useQ && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useE && target.IsValidTarget(Q.Range))
                {
                    E.Cast(target);
                }
                if (useW && target.IsValidTarget(Q.Range))
                {
                    W.Cast();
                }
            }

        }
    }
}