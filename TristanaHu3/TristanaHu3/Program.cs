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
        public static Menu TristanaMenu, SettingsMenu, ActivatorMenu;


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

            TargetSelector.Init();
            Bootstrap.Init(null);
            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q, 543 + level * 7);
            W = new Spell.Skillshot(SpellSlot.W, 880, SkillShotType.Circular, (int)0.50f, Int32.MaxValue, (int)250f);
            E = new Spell.Targeted(SpellSlot.E, 543 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 543 + level * 7);

            TristanaMenu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            TristanaMenu.AddGroupLabel("Tristana Hu3 1.3");
            TristanaMenu.AddSeparator();
            TristanaMenu.AddLabel("Made By MarioGK");
            SettingsMenu = TristanaMenu.AddSubMenu("Settings", "Settings");
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
        //Damages      
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
        private static void KillSteal()
        {
            var useW = SettingsMenu["killstealW"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["killstealR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.IsDead && !hero.IsZombie && hero.Health <= RDamage(hero)))
            {
                if (R.IsReady() && useR && R.Cast(target))
                {
                    R.Cast(target);
                }
            }
            foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(W.Range) && !hero.IsDead && !hero.IsZombie && hero.Health <= WDamage(hero)))
            {
                if (W.IsReady() && useR && W.Cast(target))
                {
                    W.Cast(target);
                }
            }
        }

        private static void Combo()
        {
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                        E.Cast(target);
                }
            }
            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    Q.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["harassE"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                    E.Cast(target);
                }
            }
            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (target.HasBuff("tristanecharge"))
                    {
                        Q.Cast();
                    }                  
                }
            }

        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["laneclearQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["laneclearE"].Cast<CheckBox>().CurrentValue;
            var useQtower = SettingsMenu["laneclearQtower"].Cast<CheckBox>().CurrentValue;
            var useEtower = SettingsMenu["laneclearEtower"].Cast<CheckBox>().CurrentValue;
            
            if (useE && E.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(E.Range)))
                {
                    E.Cast(minion);
                }
            }

            if (useQ && Q.IsReady())
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && m.IsValidTarget(Q.Range)))
                {
                    if(minion.HasBuff("tristanaecharge"))
                    {
                        Q.Cast();
                    }
                }
            }
            if (useEtower && E.IsReady())
            {
                foreach (Obj_AI_Turret tower in ObjectManager.Get<Obj_AI_Turret>())
                {
                    if (!tower.IsDead && tower.Health > 200 && tower.IsEnemy && tower.IsValidTarget())
                    {
                        E.Cast(tower);
                    }
                    if (useQtower && tower.HasBuff("tristanaecharge"))
                    {
                        Q.Cast();
                    }
                }
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