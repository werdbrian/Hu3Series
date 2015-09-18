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


namespace EzrealHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Menu EzrealMenu, SettingsMenu;


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

            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1190, SkillShotType.Linear, (int)0.25f, Int32.MaxValue, (int)60f);
            W = new Spell.Skillshot(SpellSlot.W, 790, SkillShotType.Linear, (int)0.25f, Int32.MaxValue, (int)80f);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 2500, SkillShotType.Linear, (int)1f, Int32.MaxValue, (int)(160f));

            EzrealMenu = MainMenu.AddMenu("Ezreal Hu3", "ezrealhu3");
            EzrealMenu.AddGroupLabel("Ezreal Hu3");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            SettingsMenu = EzrealMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("lasthitQ", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("lasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killstealQ", new CheckBox("Use Q KillSteal Not Working"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));

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
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
            {
                LastHit();
            }
            if (SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }

        }
        //Damages
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35, 55, 75, 95, 110 }[Program.Q.Level] + 0.4 * _Player.FlatMagicDamageMod + 1.1 * _Player.FlatPhysicalDamageMod));
        }
        private static void KillSteal()
        {
            foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie && hero.Health > QDamage(hero)))
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
            var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    W.Cast(target);
                }
                if (useW && W.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High)
                {
                    R.Cast(target);
                }
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
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
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsEnemy && a.Health <= QDamage(a));
                if (minion == null) return;
                Q.Cast(minion);
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