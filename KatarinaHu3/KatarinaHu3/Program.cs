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
        public static bool inult = false;


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
            KatarinaMenu.AddGroupLabel("Katarina Hu3 1.7");
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
            SettingsMenu.Add("LaneHit", new CheckBox("Use Smart LastHitting"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("LaneClear", new CheckBox("Use Smart LaneClearing"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killsteal", new CheckBox("KillSteal"));
            SettingsMenu.Add("killstealEW", new CheckBox("Use E->W KillSteal"));
            SettingsMenu.Add("killstealEWQ", new CheckBox("Use E->W->Q KillSteal"));
            SettingsMenu.Add("killstealQ", new CheckBox("Use Q KillSteal"));
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

            if (SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }
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
                (float)(new[] { 60, 85, 110, 135, 160 }[Program.Q.Level] + 0.45 * _Player.FlatMagicDamageMod));
        }
        public static float Q2Damage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 15, 30, 45, 60, 75 }[Program.Q.Level] + 0.15 * _Player.FlatMagicDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 40, 75, 110, 145, 180 }[Program.W.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.6 * _Player.FlatPhysicalDamageMod));
        }
        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 40, 70, 100, 130, 160 }[Program.E.Level] + 0.25 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35 * 8, 55 * 8, 75 * 8 }[Program.R.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.37 * _Player.FlatPhysicalDamageMod));
        }
        //Ranges
        public static float rangeQ()
        {
            if (Q.IsReady())
            {
                return Q.Range;
            }
            return _Player.AttackRange;
        }
        private static void KillSteal()
        {
            var useKS = SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue;
            var useEW = SettingsMenu["killstealEW"].Cast<CheckBox>().CurrentValue;
            var useEWQ = SettingsMenu["killstealEWQ"].Cast<CheckBox>().CurrentValue;
            var useQ = SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(rangeQ(), DamageType.Magical);
                if (target == null) return;
                if (useKS && useEW && target.Health < EDamage(target) + WDamage(target))
                {
                    E.Cast(target);
                    W.Cast();
                    Chat.Print("KS EW");
                }
                if (useKS && useEWQ && target.Health < EDamage(target) + WDamage(target) + QDamage(target))
                {
                    E.Cast(target);
                    W.Cast();
                    Q.Cast(target);
                    Chat.Print("KS EWQ");
                }
                if (useKS && useQ && target.Health < QDamage(target) + WDamage(target))
                {
                    Q.Cast(target);
                    Chat.Print("KS Q");
                }
            }
        private static void Combo()
        {
                var target = TargetSelector.GetTarget(rangeQ(), DamageType.Magical);
                var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
                var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
                var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
                var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;
                if (target == null || inult == true) return;
                if (useQ && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useE && target.IsValidTarget(W.Range))
                {
                    E.Cast(target);
                }
                if (useW && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
                if (useR && target.IsValidTarget(R.Range) 
                    && !Q.IsReady()
                    && !W.IsReady()
                    && !E.IsReady()
                    && inult == false)
                {
                    R.Cast();
                    inult = true;
                }
            }
        private static void Harass()
        {
                var target = TargetSelector.GetTarget(rangeQ(), DamageType.Magical);
                var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
                var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;
                if (target == null || inult == true) return;
                if (useQ && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useW && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        private static void CheckUlt()
        {
            Chat.Print("Check Ult");
            if (_Player.IsDead) return;
            if (!_Player.HasBuff("katarinarsound"))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
                inult = false;
            }
            else
            {
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
                inult = true;
            }
        }
        private static void LastHit()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy && a.IsValidTarget(Q.Range)).OrderBy(a => a.Health).FirstOrDefault();
            var LH = SettingsMenu["LastHit"].Cast<CheckBox>().CurrentValue;
                var hasBuff = minion.HasBuff("katarinaqmark");
                if (minion == null || inult == true) return;
                if (LH && Q.IsReady() && W.IsReady() && minion.IsValidTarget(W.Range)
                    && minion.Health < QDamage(minion) + Q2Damage(minion) + WDamage(minion))
                {
                    Q.Cast(minion);
                    if (hasBuff)
                    {
                        W.Cast();
                    }                
                }
                if(LH && Q.IsReady() && minion.IsValidTarget(Q.Range)
                    && minion.Health < QDamage(minion))
                    {
                        Q.Cast(minion);
                }
                if (LH && W.IsReady() && minion.IsValidTarget(W.Range)
                    && minion.Health < WDamage(minion))
                {
                    W.Cast();
                }
            }
        private static void LaneClear()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy && a.IsValidTarget(Q.Range)).OrderBy(a => a.Health).FirstOrDefault();
            var LH = SettingsMenu["LastHit"].Cast<CheckBox>().CurrentValue;
            var hasBuff = minion.HasBuff("katarinaqmark");
            if (minion == null || inult == true) return;
            if (LH && Q.IsReady() && W.IsReady() && minion.IsValidTarget(W.Range)
                && minion.Health < QDamage(minion) + Q2Damage(minion) + WDamage(minion))
            {
                Q.Cast(minion);
                if (hasBuff)
                {
                    W.Cast();
                }
            }
            if (LH && Q.IsReady() && minion.IsValidTarget(Q.Range)
                && minion.Health < QDamage(minion))
            {
                Q.Cast(minion);
            }
            if (LH && W.IsReady() && minion.IsValidTarget(W.Range)
                && minion.Health < WDamage(minion))
            {
                W.Cast();
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