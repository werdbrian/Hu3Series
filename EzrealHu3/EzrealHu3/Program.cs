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
using Microsoft.Win32;


namespace EzrealHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Menu EzrealMenu, SettingsMenu;
        private static Slider _Mana;


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1190, SkillShotType.Linear, (int)0.25f, Int32.MaxValue, (int)60f);
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear, (int)0.25f, Int32.MaxValue, (int)80f);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 2500, SkillShotType.Linear, (int)1f, Int32.MaxValue, (int)(160f));

            EzrealMenu = MainMenu.AddMenu("EzrealHu3", "ezrealhu3");
            EzrealMenu.AddGroupLabel("Ezreal Hu3");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            SettingsMenu = EzrealMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use E on Harass"));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("Lasthit");
            SettingsMenu.Add("lasthitQ", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("lasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddSeparator();
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killstealQ", new CheckBox("Use Q KillSteal"));

            Game.OnTick += Game_OnTick;

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
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
            {
                LastHit();
            }
            if (SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }

        }

        private static void KillSteal()
        {
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (Q.GetPrediction(target).HitChance >= HitChance.High)
                {
                    Q.Cast(target);
                }
            }
        }

        private static void Combo()
        {
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (useW && W.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (W.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        W.Cast(target);
                    }
                }
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (useW && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie
                    && o.HasBuffOfType(BuffType.Poison)))
                {
                    W.Cast(target);
                }
            }

        }
        private static void LastHit()
        {
            var useQ = SettingsMenu["lasthitQ"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["lasthitMana"].Cast<Slider>().CurrentValue;

            if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana)
            {
                    if (Q.GetPrediction(EntityManager.GetLaneMinions(radius: 1190)[0]).HitChance >= HitChance.High)
                    {
                        Q.Cast(EntityManager.GetLaneMinions(radius: 1190)[0]);
                    }                
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {

        }
    }
}