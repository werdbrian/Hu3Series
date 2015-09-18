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
        public static Menu EzrealMenu, ComboMenu, HarassMenu, LaneMenu;


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, (int)0.75f, Int32.MaxValue, (int)40f);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, (int)0.5f, Int32.MaxValue, (int)90f);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, (int)0.6f, Int32.MaxValue, (int)(80 * Math.PI / 180));

            EzrealMenu = MainMenu.AddMenu("EzrealHu3", "ezrealhu3");
            EzrealMenu.AddGroupLabel("EzrealHu3");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            ComboMenu = EzrealMenu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo");
            ComboMenu.AddSeparator();
            ComboMenu.Add("comboQ", new CheckBox("Use Q"));
            ComboMenu.Add("comboW", new CheckBox("Use W"));

            HarassMenu = EzrealMenu.AddSubMenu("Harass", "Harass");
            HarassMenu.AddGroupLabel("Harass");
            HarassMenu.AddSeparator();
            HarassMenu.Add("harassQ", new CheckBox("Use Q"));
            HarassMenu.Add("harassW", new CheckBox("Use E"));
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
        }

        private static void Combo()
        {
            var useQ = ComboMenu["combo.q"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["combo.w"].Cast<CheckBox>().CurrentValue;

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

            var useQ = HarassMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["harassW"].Cast<CheckBox>().CurrentValue;

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
    }
}