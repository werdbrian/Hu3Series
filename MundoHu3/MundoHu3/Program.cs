using System;
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
        public static Menu EzrealMenu, SettingsMenu;
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
            if (Player.Instance.ChampionName != "DrMundo")
                return;


            Q = new Spell.Skillshot(SpellSlot.Q, 970, SkillShotType.Linear, 250, 2000, 60);
            W = new Spell.Active(SpellSlot.W, 160);
            E = new Spell.Active(SpellSlot.E, 125);
            R = new Spell.Active(SpellSlot.R);

            EzrealMenu = MainMenu.AddMenu("Mundo Hu3", "mundohu3");
            EzrealMenu.AddGroupLabel("Mundo Hu3 V0.4");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            SettingsMenu = EzrealMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboE", new CheckBox("Use E on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use W on Harass"));
            SettingsMenu.Add("harassE", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlh", new CheckBox("Use Q on Last Hit"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q on Lane Clear"));
            SettingsMenu.AddLabel("Auto Ult");
            SettingsMenu.Add("autoR", new CheckBox("Use R"));
            SettingsMenu.Add("healthAutoR", new Slider("Min Health To Ult", 10, 0, 100));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));


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
            if (autoR && Player.Instance.HealthPercent < healthAutoR)
            {
                R.Cast();
            }

        }

        private static void CheckW()
        {
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var usingW = Player.HasBuff("BurningAgony");
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            if (useW && usingW)
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
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && Wing == false)
            {
                W.Cast();
                Wing = true;
            }

            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["harassE"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && Wing == false)
            {
                W.Cast();
                Wing = true;
            }

            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast();
            }
        }
        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlh"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }

        }

        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
        }
    }
}