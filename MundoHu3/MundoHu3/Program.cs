﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;


namespace MundoHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Menu Menu, SettingsMenu;
        public static bool Wing = false;


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.ChampionName != "DrMundo")
                return;


            Q = new Spell.Skillshot(SpellSlot.Q, 960, SkillShotType.Linear, 250, 2000, 60);
            W = new Spell.Active(SpellSlot.W, 295);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R);

            Menu = MainMenu.AddMenu("Mundo Hu3", "mundohu3");
            Menu.AddGroupLabel("Mundo Hu3 V1.0");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Wharass", new CheckBox("Use W on Harass"));
            SettingsMenu.Add("Eharass", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on Last Hit"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on Lane Clear"));
            SettingsMenu.AddLabel("Auto Ult");
            SettingsMenu.Add("autoR", new CheckBox("Use R"));
            SettingsMenu.Add("healthAutoR", new Slider("Min Health To Ult", 10, 0, 100));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Wdraw", new CheckBox("Draw W"));


            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            AutoUlt();

            CheckW();
            
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

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
            {
                LastHit();
            }
        }

        private static void AutoUlt()
        {
            var autoR = SettingsMenu["autoR"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = SettingsMenu["healthAutoR"].Cast<Slider>().CurrentValue;
            if (autoR && _Player.HealthPercent < healthAutoR)
            {
                R.Cast();
            }

        }
        public static float GetDamage(SpellSlot spell, Obj_AI_Base target)
        {
            if (spell == SpellSlot.Q)
            {
                if (!Q.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, (80f + 50f * (Q.Level - 1)) + target.Health * (15 + 3 * (Q.Level - 1)) / 100);
            }
            return 0;
        }

        private static void CheckW()
        {
            var usingW = Player.HasBuff("burningagony");
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Wing && usingW)
            {
                    if (!target.IsValidTarget(W.Range) && Wing == true)
                    {
                        W.Cast();
                        Wing = false;
                    }
                }
            }

        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie && Wing == false)
            {
                W.Cast();
                Wing = true;
            }

            if (useE && E.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wharass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eharass"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie && Wing == false)
            {
                W.Cast();
                Wing = true;
            }

            if (useE && E.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsZombie)
            {
                E.Cast();
            }
        }
        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && minion.Health <= GetDamage(SpellSlot.Q, minion))
                {
                    Q.Cast(minion);
                }
            }

        }

        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && minion.Health <= GetDamage(SpellSlot.Q, minion))
                {
                    Q.Cast(minion);
                }
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawQ = SettingsMenu["Qdraw"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wdraw"].Cast<CheckBox>().CurrentValue;

            if (drawQ && Q.IsReady())
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW && W.IsReady())
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
        }
    }
}