using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;

namespace ThreshHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active Q2;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Menu ThreshMenu, SettingsMenu;


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
            if (Player.Instance.ChampionName != "Thresh")
                return;

            Q = new Spell.Skillshot(SpellSlot.Q, 1080, SkillShotType.Linear, 350, Int32.MaxValue, 60);
            Q2 = new Spell.Active(SpellSlot.Q, 1300);
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, Int32.MaxValue, 300);
            E = new Spell.Skillshot(SpellSlot.E, 500, SkillShotType.Linear, 1, Int32.MaxValue, 110);
            R = new Spell.Active(SpellSlot.R, 350);

            ThreshMenu = MainMenu.AddMenu("Thresh Hu3", "threshhu3");
            ThreshMenu.AddGroupLabel("Thresh Hu3 0.4");
            ThreshMenu.AddSeparator();
            ThreshMenu.AddLabel("Made By MarioGK");
            SettingsMenu = ThreshMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qc", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Q2c", new CheckBox("Follow with Q on Combo"));
            SettingsMenu.Add("Ew", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("Rc", new CheckBox("Use R on Combo"));
            SettingsMenu.Add("Rmin", new Slider("Min Enemies to use R", 2, 1, 5));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qh", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Q2h", new CheckBox("Use Q Follow on Harass"));
            SettingsMenu.Add("Eh", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R Combo"));
            SettingsMenu.AddLabel("Misc");
            SettingsMenu.Add("gapE", new CheckBox("Anti GapCloser E"));


            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;

        }
        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!e.Sender.IsValidTarget() || !SettingsMenu["gapE"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            E.Cast(e.Sender);
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

        }
        private static void Combo()
        {

            var useQ = SettingsMenu["Qc"].Cast<CheckBox>().CurrentValue;
            var useQ2 = SettingsMenu["Q2c"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ec"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rc"].Cast<CheckBox>().CurrentValue;
            var minR = SettingsMenu["Rmin"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(1300, DamageType.Mixed);

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
               Q.Cast(target);
            }

            if (useQ2 && target.HasBuff("ThreshQ") && !target.IsValidTarget(E.Range) && target.IsValidTarget(Q2.Range) && !target.IsDead && !target.IsZombie)
            {
                Q2.Cast();
            }

            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie && E.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                var getOutRange = Player.Instance.Distance(target) < target.Distance(Game.CursorPos);
                var predictionE = E.GetPrediction(target);
                var x = Player.Instance.ServerPosition.X - target.ServerPosition.X;
                var y = Player.Instance.ServerPosition.Y - target.ServerPosition.Y;
                var v3 = new Vector3(
                Player.Instance.ServerPosition.X + x,
                Player.Instance.ServerPosition.Y + y,
                Player.Instance.ServerPosition.Z);

                       E.Cast(getOutRange ? predictionE.CastPosition : v3);
            }

            if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && Player.Instance.CountEnemiesInRange(R.Range) >= minR)
            {
                 R.Cast();
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var useQ2 = SettingsMenu["Q2h"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(1300, DamageType.Mixed);

            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }

            if (useQ2 && target.HasBuff("ThreshQ") && !target.IsValidTarget(E.Range) && target.IsValidTarget(Q2.Range) && !target.IsDead && !target.IsZombie)
            {
                Q2.Cast();
            }

            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie && E.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                var getOutRange = Player.Instance.Distance(target) < target.Distance(Game.CursorPos);
                var predictionE = E.GetPrediction(target);
                var x = Player.Instance.ServerPosition.X - target.ServerPosition.X;
                var y = Player.Instance.ServerPosition.Y - target.ServerPosition.Y;
                var v3 = new Vector3(
                Player.Instance.ServerPosition.X + x,
                Player.Instance.ServerPosition.Y + y,
                Player.Instance.ServerPosition.Z);

                E.Cast(getOutRange ? predictionE.CastPosition : v3);
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
                new Circle() { Color = Color.BlueViolet, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawE)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (drawR)
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}