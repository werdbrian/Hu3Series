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
using System.Collections.Generic;

namespace EzrealHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Menu EzrealMenu, SettingsMenu;

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
            if (_Player.ChampionName != "Ezreal")
                return;

            Q = new Spell.Skillshot(SpellSlot.Q, 1190, SkillShotType.Linear, 250, int.MaxValue, 60)
            {
                MinimumHitChance = HitChance.High
            };

            W = new Spell.Skillshot(SpellSlot.W, 990, SkillShotType.Linear, 250, int.MaxValue, 80)
            {
                MinimumHitChance = HitChance.High , AllowedCollisionCount = int.MaxValue
            };

            E = new Spell.Targeted(SpellSlot.E, 700);

            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, 1, int.MaxValue, 160)
            {
                MinimumHitChance = HitChance.High
            };


            EzrealMenu = MainMenu.AddMenu("Ezreal Hu3", "ezrealhu3");
            EzrealMenu.AddGroupLabel("Ezreal Hu3 1.0");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            SettingsMenu = EzrealMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Wharass", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("lastMana", new Slider("Mana % To Use Q LastHit", 30));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("laneMana", new Slider("Mana % To Use Q LaneClear", 30));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Wks", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rks", new CheckBox("Use R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Wdraw", new CheckBox("Draw W"));
            SettingsMenu.Add("Rdraw", new CheckBox("Draw R Combo"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

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
        private static void KillSteal()
        {
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wks"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rks"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (target == null)
                return;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && Q.GetPrediction(target).HitChance >= HitChance.Medium
                && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q)) 
            {
                Q.Cast(target);
            }
            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie && W.GetPrediction(target).HitChance >= HitChance.Medium
                && target.Health <= _Player.GetSpellDamage(target, SpellSlot.W))
            {
                W.Cast(target);
            }
            if (useR && R.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && R.GetPrediction(target).HitChance >= HitChance.High
                && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
        }

        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (target == null)
                return;         

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie)
            {
                W.Cast(target);
            }
            if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wharass"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(1300, DamageType.Physical);

            if (target == null)
                return;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie)
            {
                W.Cast(target);
            }
        }

        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["lastMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) 
                    && _Player.ManaPercent > mana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }

        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["laneMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange)
                    && _Player.ManaPercent > mana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
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