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
        public static Menu Menu, SettingsMenu;


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
            if (_Player.ChampionName != "Tristana")
                return;            
           
            Q = new Spell.Active(SpellSlot.Q, 550);
            W = new Spell.Skillshot(SpellSlot.W, 825, SkillShotType.Circular, 250, int.MaxValue, 80);
            E = new Spell.Targeted(SpellSlot.E, 550);
            R = new Spell.Targeted(SpellSlot.R, 550);

            Menu = MainMenu.AddMenu("TristanaHu3", "tristanahu3");
            Menu.AddGroupLabel("Tristana Hu3 V1.0");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));         
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Eharass", new CheckBox("Use E on Harass"));          
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Elane", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("Etower", new CheckBox("Use E on Towers"));
            SettingsMenu.Add("laneMana", new Slider("Mana % To Use Spells On LaneClear", 30));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Wks", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rks", new CheckBox("Use R KillSteal"));
            SettingsMenu.Add("ERks", new CheckBox("Use E+R KillSteal"));
            SettingsMenu.AddLabel("GapCloser/Interrupter");
            SettingsMenu.Add("Rgap", new CheckBox("Use R On GapClosers"));
            SettingsMenu.Add("Rint", new CheckBox("Use R To Interrupt"));
            SettingsMenu = Menu.AddSubMenu("Draw", "Draw");
            SettingsMenu.AddGroupLabel("Draw");
            SettingsMenu.AddLabel("Drawings");
            SettingsMenu.Add("Edraw", new CheckBox("Draw E"));
            SettingsMenu.Add("Wdraw", new CheckBox("Draw W"));
            SettingsMenu.Add("Rdraw", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnLevelUp += AIHeroClient_OnLevelUp;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,Interrupter.InterruptableSpellEventArgs e)
        {
            if (!sender.IsValidTarget(Q.Range) || e.DangerLevel != DangerLevel.High)
                return;

            if (R.IsReady() && R.IsInRange(sender) && SettingsMenu["Rint"].Cast<CheckBox>().CurrentValue)
            {
                R.Cast(sender);
            }

        }

        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!e.Sender.IsValidTarget() || !SettingsMenu["Rgap"].Cast<CheckBox>().CurrentValue)
                return;

            R.Cast(e.Sender);
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                //ForceETarget();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
                //ForceETarget();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
                //ForceETarget();
            }

            KillSteal();
        }

        private static void ForceETarget()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Physical);
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (target.HasBuff("tristaneecharge"))
            {
                Orbwalker.ForcedTarget = target;
            }

            foreach ( var minion in minions)
            {
                if (minion.HasBuff("tristaneecharge"))
                {
                    Orbwalker.ForcedTarget = minion;
                }
            }
        }

        private static void AIHeroClient_OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
    //        Q = new Spell.Active(SpellSlot.Q, (uint)_Player.GetAutoAttackRange());
        //    E = new Spell.Targeted(SpellSlot.E, (uint)_Player.GetAutoAttackRange());
      //      R = new Spell.Targeted(SpellSlot.R, (uint)_Player.GetAutoAttackRange());
        }
            
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(900, DamageType.Physical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast();
            }
            
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Physical);
            var useW = SettingsMenu["Wks"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rks"].Cast<CheckBox>().CurrentValue;
            var useER = SettingsMenu["ERks"].Cast<CheckBox>().CurrentValue;

            if (target == null)
                return;

            if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                W.Cast(target);
            }
            if (useER && W.IsReady() && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie && target.Health <= GetEdmg(target) 
                + _Player.GetSpellDamage(target, SpellSlot.R))
            {              
                R.Cast(target);
            }
        }

        private static BuffInstance GetECharge(Obj_AI_Base target)
        {
            return target.Buffs.Find(x => x.DisplayName == "tristanaecharge");
        }

        private static double GetEdmg(Obj_AI_Base target)
        {
            float ap = _Player.FlatMagicDamageMod + _Player.BaseAbilityDamage;
            float ad = _Player.FlatMagicDamageMod + _Player.BaseAttackDamage;
            if (target.GetBuffCount("TristanaECharge") != 0)
            {
                return (_Player.GetSpellDamage(target, SpellSlot.E) * ((0.3 * target.GetBuffCount("tristanaecharge") + 1))
                        + (ad) + (ap * 0.5));
            }

            return 0;
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eharass"].Cast<CheckBox>().CurrentValue;

            if (target == null)
                return;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && target.HasBuff("tristanaecharge") && !target.IsZombie)
            {
                Q.Cast();
            }

        }
        private static void LaneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(a => a.IsEnemy && a.Distance(_Player) < _Player.AttackRange);
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elane"].Cast<CheckBox>().CurrentValue;
            var useETower = SettingsMenu["Etower"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["laneMana"].Cast<Slider>().CurrentValue;

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (useE && E.IsReady() && _Player.ManaPercent > mana)
                {
                    E.Cast(minion);
                    
                }
                if (useQ && Q.IsReady() && minion.HasBuff("tristanaecharge"))
                {
                    Q.Cast();
                }
            }          
            if (useETower && E.IsReady() && minions == null && _Player.ManaPercent > mana)
            {
                E.Cast(tower);
                Q.Cast();
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawE = SettingsMenu["Edraw"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wdraw"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rdraw"].Cast<CheckBox>().CurrentValue;

            if (drawE && E.IsReady())
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW && W.IsReady())
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawR && R.IsReady())
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}
