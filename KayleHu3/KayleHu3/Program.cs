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

namespace KayleHu3
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Targeted W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        public static Menu Menu, SettingsMenu;


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
            if (Player.Instance.ChampionName != "Kayle")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Targeted(SpellSlot.W, 900);
            E = new Spell.Active(SpellSlot.E, 525);
            R = new Spell.Targeted(SpellSlot.R, 900);

            Menu = MainMenu.AddMenu("Kayle Hu3", "kaylehu3");
            Menu.AddGroupLabel("Kayle Hu3 0.1");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qc", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wc", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ec", new CheckBox("Use E on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qh", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Eh", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlh", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("Elh", new CheckBox("Use E on LastHit"));
            SettingsMenu.Add("manaLH", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Elc", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("manaLC", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.AddLabel("Ult Manager");
            SettingsMenu.Add("Rme", new CheckBox("Use R in Yourself"));
            SettingsMenu.Add("HPme", new Slider("Health % To Use R", 20, 0, 100));
            SettingsMenu.Add("Rally", new CheckBox("Use R in Ally"));          
            SettingsMenu.Add("HPally", new Slider("Health % To Use R on Ally", 20, 0, 100));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qd", new CheckBox("Draw Q"));
            SettingsMenu.Add("Wd", new CheckBox("Draw Q"));
            SettingsMenu.Add("Ed", new CheckBox("Draw E"));
            SettingsMenu.Add("Rd", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
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
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie && target.Health <= Player.Instance.GetSpellDamage(target ,SpellSlot.Q))
            {
                Q.Cast(target);
            }

        }

        private static void Combo()
        {
            var useQ = SettingsMenu["Qc"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ec"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useW && W.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie && Player.Instance.HealthPercent < 95)
            {
                W.Cast(_Player);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elh"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(650) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast();
                }
            }
        }    

        private static void AutoUlt()
        {
            var Rme = SettingsMenu["Rme"].Cast<CheckBox>().CurrentValue;
            var Rally = SettingsMenu["Rally"].Cast<CheckBox>().CurrentValue;
            var HPme = SettingsMenu["HPme"].Cast<Slider>().CurrentValue;
            var HPally = SettingsMenu["HPally"].Cast<Slider>().CurrentValue;
            var allies = HeroManager.Allies.OrderBy(a => a.Health).Where(a => !a.IsDead && !a.IsZombie);

            if (Rme && R.IsReady() && Player.Instance.HealthPercent < HPme && R.IsReady() && Player.Instance.CountEnemiesInRange(R.Range) >= 1)
            {
                R.Cast(_Player);
            }
            foreach (var ally in allies)
            {
                if (Rally && R.IsReady() && ally.Health < HPally && R.IsReady() && ally.CountEnemiesInRange(R.Range) >= 1)
                {
                    R.Cast(ally);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawQ = SettingsMenu["Qd"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wd"].Cast<CheckBox>().CurrentValue;
            var drawE = SettingsMenu["Ed"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rd"].Cast<CheckBox>().CurrentValue;

            if (drawQ)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawE)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawR)
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}
