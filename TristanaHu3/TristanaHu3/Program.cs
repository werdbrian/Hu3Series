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
        public static Menu Menu, SettingsMenu, ActivatorMenu;


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

            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q, 543 + level * 7);
            W = new Spell.Skillshot(SpellSlot.W, 880, SkillShotType.Circular, (int)0.50f, Int32.MaxValue, (int)250f);
            E = new Spell.Targeted(SpellSlot.E, 543 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 543 + level * 7);

            Menu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            Menu.AddGroupLabel("Tristana Hu3 2.2");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboE", new CheckBox("Use E on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassE", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("laneclearQ", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("laneclearE", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("laneclearQtower", new CheckBox("Use Q on Towers"));
            SettingsMenu.Add("laneclearEtower", new CheckBox("Use E on Towers"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killsteal", new CheckBox("KillSteal"));
            SettingsMenu.Add("killstealW", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("killstealR", new CheckBox("Use R KillSteal"));
            SettingsMenu.Add("killstealER", new CheckBox("Use E+R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));


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
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                LaneClear();
            }
            if (SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }
        }
          
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 80, 105, 130, 155, 180 }[Program.W.Level] + 0.5 * _Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 300, 400, 500 }[Program.R.Level] + 1.0 * _Player.FlatMagicDamageMod));
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
            var useW = SettingsMenu["killstealW"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["killstealR"].Cast<CheckBox>().CurrentValue;
            var useER = SettingsMenu["killstealER"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);
            var estacks = target.Buffs.Find(buff => buff.Name == "tristanaecharge").Count;
            var erdamage = (EDamage(target) * ((0.30 * estacks) + 1) + RDamage(target));

            if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= RDamage(target))
            {
                R.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && target.Health <= WDamage(target))
            {
                R.Cast(target);
            }
            if (useER && W.IsReady() && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= erdamage)
            {
                R.Cast(target);
            }

        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target == null) return;
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target == null) return;
            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["harassE"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie && target.HasBuff("tristanaecharge"))
            {
                Q.Cast();
            }
        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["laneclearQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["laneclearE"].Cast<CheckBox>().CurrentValue;
            var useQtower = SettingsMenu["laneclearQtower"].Cast<CheckBox>().CurrentValue;
            var useEtower = SettingsMenu["laneclearEtower"].Cast<CheckBox>().CurrentValue;

            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            if (minion == null && tower == null) return;

            if (useE && E.IsReady() && (tower == null))
            {
                E.Cast(minion);
            }
            if (useQ && Q.IsReady())
            {
                Q.Cast();
            }
            if (useEtower && E.IsReady())
            {
                E.Cast(tower);
            }
            if (tower.HasBuff("tristanaecharge"))
            {
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