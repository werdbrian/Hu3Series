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
        public static bool ulting = false;


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
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 370);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 540);

            KatarinaMenu = MainMenu.AddMenu("KatarinaHu3", "katarinahu3");
            KatarinaMenu.AddGroupLabel("Katarina Hu3 Test Version 0.2");
            KatarinaMenu.AddSeparator();
            KatarinaMenu.AddLabel("Made By MarioGK");
            SettingsMenu = KatarinaMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboE", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("qLasthit", new CheckBox("Use Q on Last Hit"));
            SettingsMenu.Add("wLasthit", new CheckBox("Use W on Last Hit"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("qLaneclear", new CheckBox("Use Q on Last Hit"));
            SettingsMenu.Add("wLaneclear", new CheckBox("Use W on Last Hit"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("ksEW", new CheckBox("Use E->W KillSteal"));
            SettingsMenu.Add("ksEWQ", new CheckBox("Use E->W->Q KillSteal"));
            SettingsMenu.Add("ksQ", new CheckBox("Use Q KillSteal"));
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
            CheckUlt();

            KillSteal();

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
        //Damages      
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 50, 75, 100, 125, 150 }[Program.Q.Level] + 0.45 * _Player.FlatMagicDamageMod));
        }
        public static float Q2Damage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 12, 25, 40, 55, 70 }[Program.Q.Level] + 0.15 * _Player.FlatMagicDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35, 70, 105, 140, 175 }[Program.W.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.6 * _Player.FlatPhysicalDamageMod));
        }
        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35, 65, 95, 125, 155 }[Program.E.Level] + 0.25 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 34 * 8, 54 * 8, 74 * 8 }[Program.R.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.37 * _Player.FlatPhysicalDamageMod));
        }
        private static void KillSteal()
        {
            var useEW = SettingsMenu["killstealEW"].Cast<CheckBox>().CurrentValue;
            var useEWQ = SettingsMenu["killstealEWQ"].Cast<CheckBox>().CurrentValue;
            var useQ = SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue;
            if (useEW && E.IsReady() && W.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead
                && e.Health < EDamage(e) + WDamage(e)))
                {
                    E.Cast(target);
                    W.Cast();
                }
            }
            if (useEWQ && Q.IsReady() && W.IsReady() && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead
                && e.Health < EDamage(e) + WDamage(e) + QDamage(e)))
                {
                    E.Cast(target);
                    W.Cast();
                    Q.Cast(target);
                }
            }
            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead
                && e.Health < QDamage(e)))
                {
                    Q.Cast(target);
                }
            }
        }
        private static void Combo()
        {
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead))
            {
                var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
                var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
                var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
                var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;
                if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && ulting == false)
                {
                    Q.Cast(target);
                }
                if (E.IsReady() && useE && target.IsValidTarget(E.Range) && ulting == false)
                {
                    E.Cast(target);
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range) && ulting == false)
                {
                    W.Cast();
                }
                if (R.IsReady() && useR && target.IsValidTarget(R.Range)
                    && !Q.IsReady()
                    && !W.IsReady()
                    && !E.IsReady()
                    && ulting == false)
                {
                    Orbwalker.DisableAttacking = true;
                    Orbwalker.DisableMovement = true;
                    ulting = true;
                    R.Cast();
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
                       Q.Cast(target);
                    }
                }

                if (useW && W.IsReady())
                {
                    foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && !o.IsDead && !o.IsZombie))
                    {
                       W.Cast();
                    }
                }
        }
        private static void CheckUlt()
        {
            if (!_Player.HasBuff("katarinarsound"))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
                ulting = false;
            }
        }
        private static void LastHit()
        {
            var useQ = SettingsMenu["lasthitQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["lasthitW"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(Q.Range) && m.Health <= QDamage(m)))
                {
                  Q.Cast(minion);
                }
            }
            if (useW && W.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(Q.Range) && m.Health <= WDamage(m)))
                {
                    W.Cast();
                }
            }
        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["laneclearQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["laneclearW"].Cast<CheckBox>().CurrentValue;

            if (useQ && Q.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(Q.Range) && m.Health <= QDamage(m)))
                {
                    Q.Cast(minion);
                }
            }
            if (useW && W.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(Q.Range) && m.Health <= WDamage(m)))
                {
                    W.Cast();
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Purple, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}