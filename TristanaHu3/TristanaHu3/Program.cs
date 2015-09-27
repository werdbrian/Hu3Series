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


namespace TristanaHu3
{
    class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Targeted R;
        public static Menu Menu, SettingsMenu, KeysMenu;


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
            if (Player.Instance.ChampionName != "Tristana")
                return;
            Bootstrap.Init(null);
            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q, 543 + level * 7);
            W = new Spell.Skillshot(SpellSlot.W, 825, SkillShotType.Circular, (int)0.25f, Int32.MaxValue, (int)80f);
            E = new Spell.Targeted(SpellSlot.E, 543 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 543 + level * 7);

            Menu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            Menu.AddGroupLabel("Tristana Hu3 0.1");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qc", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Ec", new CheckBox("Use E on Combo"));         
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qh", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Eh", new CheckBox("Use E on Harass"));          
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlc", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Elc", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("Etower", new CheckBox("Use E on Towers"));         
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Wkill", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rkill", new CheckBox("Use R KillSteal"));
            SettingsMenu.Add("ERkill", new CheckBox("Use E+R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            
            KillSteal();
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useQ = SettingsMenu["Qc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ec"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast();
            }
            
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 100, 130, 155, 180 }[Program.W.Level] + 0.5 * _Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 280, 380, 480 }[Program.R.Level] + 0.9 * _Player.FlatMagicDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 100 + 0.75 * _Player.FlatMagicDamageMod + 0.50 * _Player.FlatPhysicalDamageMod,
                                140 + 0.75 * _Player.FlatMagicDamageMod + 0.50 * _Player.FlatPhysicalDamageMod,
                                170 + 0.75 * _Player.FlatMagicDamageMod + 0.50 * _Player.FlatPhysicalDamageMod,
                                210 + 0.75 * _Player.FlatMagicDamageMod + 0.50 * _Player.FlatPhysicalDamageMod,
                                240 + 0.75 * _Player.FlatMagicDamageMod + 0.50 * _Player.FlatPhysicalDamageMod }[Program.E.Level]));
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useW = SettingsMenu["Wkill"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rkill"].Cast<CheckBox>().CurrentValue;
            var useER = SettingsMenu["ERkill"].Cast<CheckBox>().CurrentValue;

            if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= RDamage(target))
            {
                R.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && target.Health <= WDamage(target))
            {
                W.Cast(target);
            }
            var eStacks = target.Buffs.Find(b => b.Name == "tristanaecharge").Count;
            var ERdamage = (EDamage(target) * ((0.30 * eStacks) + 1) + RDamage(target));
            if (useER && W.IsReady() && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= ERdamage)
            {              
                R.Cast(target);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && target.HasBuff("tristanaecharge") && !target.IsDead && !target.IsZombie)
            {
                Q.Cast();
            }

        }
        private static void LaneClear()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var useETower = SettingsMenu["Etower"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady())
            {
                E.Cast(minion);
            }
            if (useQ && Q.IsReady())
            {
                Q.Cast();
            }
            if (useETower && E.IsReady() && minion == null)
            {
                E.Cast(tower);
                Q.Cast();
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Purple, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}