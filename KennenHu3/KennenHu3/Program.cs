using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;


namespace KennenHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Menu Menu, SettingsMenu;

        private static List<HitChance> HitChances { get; set; }

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
            if (_Player.ChampionName != "Kennen")
                return;

            Q = new Spell.Skillshot(SpellSlot.Q, 1040, SkillShotType.Linear, 250, int.MaxValue, 60);
            W = new Spell.Active(SpellSlot.W, 790);
            E = new Spell.Active(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 540);

            for (int i = (int) HitChance.Medium; i <= Enum.GetValues(typeof(HitChance)).Cast<HitChance>().Count() - 1; i++)
            {
                HitChances.Add((HitChance)i);
            }

            Menu = MainMenu.AddMenu("Kennen Hu3", "kennenhu3");
            Menu.AddGroupLabel("Kennen Hu3 1.0");           
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R on Combo"));
            SettingsMenu.Add("Rmin", new Slider("Min, Enemies To Use R", 3, 1, 5));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Wharass", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("Wlast", new CheckBox("Use W on LastHit"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Wlane", new CheckBox("Use W on LaneClear"));
            SettingsMenu.Add("Elane", new CheckBox("Use E on LaneClear"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Wks", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rks", new CheckBox("Use R KillSteal"));
            SettingsMenu.AddLabel("Item(Beta)");
            SettingsMenu.Add("Qdraw", new CheckBox("Use Zhonyas On Dangerous"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Wdraw", new CheckBox("Draw W"));
            SettingsMenu.Add("Rdraw", new CheckBox("Draw R"));
            SettingsMenu = Menu.AddSubMenu("HitChance", "HitChance");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("General settings");
            var hitchanceSlider = new Slider(string.Format("Hitchance: {0}", HitChances[0]), 0, 0, HitChances.Count - 1);
            hitchanceSlider.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                hitchanceSlider.DisplayName = string.Format("Hitchance: {0}", HitChances[changeArgs.NewValue]);
                Q.MinimumHitChance = HitChances[changeArgs.NewValue]; // You coudl set them all here if you want to, would probably be the best idea, except for killsteals, ok
            };
            Menu.Add("Hitchance", hitchanceSlider);

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            KillSteal();

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                LaneClear();
            }
        }
        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wharass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;
        }

        private static void KillSteal()
        {
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wks"].Cast<CheckBox>().CurrentValue;
            var Qpred = Q.MinimumHitChance = HitChance.High;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && target.Health <= _Player.GetSpellDamage(target, SpellSlot.W))
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && useW && W.IsReady() && target.IsValidTarget(W.Range) && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q) 
                + _Player.GetSpellDamage(target, SpellSlot.W))
            {
                Q.Cast(target);
                W.Cast();
            }

        }

        private static void LaneClear()
        {

        }

        private static void LastHit()
        {

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawQ = SettingsMenu["Qdraw"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wdraw"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rdraw"].Cast<CheckBox>().CurrentValue;

            if (drawQ && Q.IsReady())
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW && W.IsReady())
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawR && R.IsReady())
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}