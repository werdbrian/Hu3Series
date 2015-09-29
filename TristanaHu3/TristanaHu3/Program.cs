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
            Bootstrap.Init(null);
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Tristana")
                return;
            
            W = new Spell.Skillshot(SpellSlot.W, 825, SkillShotType.Circular, 250, Int32.MaxValue, 80);

            Menu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            Menu.AddGroupLabel("Tristana Hu3 V0.4");
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
            SettingsMenu.Add("laneclearMana", new Slider("Mana % To Use Spells On LaneClear", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Wkill", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rkill", new CheckBox("Use R KillSteal"));
            SettingsMenu.Add("ERkill", new CheckBox("Use E+R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Ed", new CheckBox("Draw E"));
            SettingsMenu.Add("Wd", new CheckBox("Draw W"));
            SettingsMenu.Add("Rd", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            GetRange();

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
        private static void GetRange()
        {
            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q, 543 + level * 7);
            E = new Spell.Targeted(SpellSlot.E, 543 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 543 + level * 7);
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

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useW = SettingsMenu["Wkill"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rkill"].Cast<CheckBox>().CurrentValue;
            var useER = SettingsMenu["ERkill"].Cast<CheckBox>().CurrentValue;

            if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
            {
                W.Cast(target);
            }
            if (useER && W.IsReady() && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= GetEdmg(target) 
                + Player.Instance.GetSpellDamage(target, SpellSlot.R))
            {              
                R.Cast(target);
            }
        }

        private static BuffInstance GetECharge(Obj_AI_Base target)
        {
            return target.Buffs.Find(x => x.DisplayName == "TristanaECharge");
        }

        private static double GetEdmg(Obj_AI_Base target)
        {
            float ap = _Player.FlatMagicDamageMod + _Player.BaseAbilityDamage;
            float ad = _Player.FlatMagicDamageMod + _Player.BaseAttackDamage;
            if (target.GetBuffCount("TristanaECharge") != 0)
            {
                return (Player.Instance.GetSpellDamage(target, SpellSlot.E) * ((0.3 * target.GetBuffCount("TristanaECharge") + 1))
                        + (ad) + (ap * 0.5));
            }

            return 0;
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
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            var eminion = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && !m.IsDead && m.HasBuff("tristanaecharge"));
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var useETower = SettingsMenu["Etower"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["laneclearMana"].Cast<Slider>().CurrentValue;
            foreach (var minion in minions)
            {
                if (useE && E.IsReady() && Player.Instance.ManaPercent > mana)
                {
                    E.Cast(minion);
                    
                }
                if (useQ && Q.IsReady() && minion.HasBuff("tristanaecharge"))
                {
                    Q.Cast();
                }
            }          
            if (useETower && E.IsReady() && minions == null && Player.Instance.ManaPercent > mana)
            {
                E.Cast(tower);
                Q.Cast();
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawE = SettingsMenu["Ed"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wd"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rd"].Cast<CheckBox>().CurrentValue;

            if (drawE)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawR)
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}