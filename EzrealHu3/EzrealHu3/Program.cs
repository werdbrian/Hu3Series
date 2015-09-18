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
        public static Menu EzrealMenu, ComboMenu, HarassMenu, LastHitMenu;
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

            LastHitMenu = EzrealMenu.AddSubMenu("LastHit", "LastHit");
            LastHitMenu.AddGroupLabel("LastHit");
            LastHitMenu.AddSeparator();
            LastHitMenu.Add("lasthitQ", new CheckBox("Use Q"));
            LastHitMenu.Add("lasthitMana", new Slider("ManaQ", 30, 0, 100));
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
        }

        private static void Combo()
        {
            var useQ = ComboMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["comboW"].Cast<CheckBox>().CurrentValue;

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
        private static void LastHit()
        {
            var useQ = LastHitMenu["lasthitQ"].Cast<CheckBox>().CurrentValue;
            var mana = LastHitMenu["lasthitMana"].Cast<Slider>().CurrentValue;

            if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana)
            {
                    if (Q.GetPrediction(EntityManager.GetLaneMinions(radius: 1190)[0]).HitChance >= HitChance.High)
                    {
                        Q.Cast(EntityManager.GetLaneMinions(radius: 1190)[0]);
                    }                
            }
        }
     }
}