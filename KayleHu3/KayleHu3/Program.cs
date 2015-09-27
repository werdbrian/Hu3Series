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
            if (Player.Instance.ChampionName != "Ezreal")
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
            SettingsMenu.Add("Ec", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Rc", new CheckBox("Use R on Combo"));
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
            SettingsMenu.Add("hpMe", new Slider("Health % To Use R", 20, 0, 100));
            SettingsMenu.Add("Rally", new CheckBox("Use R in Ally"));          
            SettingsMenu.Add("hpAlly", new Slider("Health % To Use R on Ally", 20, 0, 100));
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
            KillSteal();
        }

        //Damages
        public static float GetDamage(SpellSlot spell, Obj_AI_Base target)
        {
            float ap = _Player.FlatMagicDamageMod + _Player.BaseAbilityDamage;
            float ad = _Player.FlatMagicDamageMod + _Player.BaseAttackDamage;
            if (spell == SpellSlot.Q)
            {
                if (!Q.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 60f + 50f * (Q.Level - 1) + 60 / 100 * ap + 100 / 100 * ad);
            }
            else if (spell == SpellSlot.E)
            {
                if (!E.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Mixed, ad + 10f + 5f * (E.Level - 1) + 15 / 100 * ap);
            }

            return 0;
        }
        private static void KillSteal()
        {
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie && target.Health <= GetDamage(SpellSlot.Q, target))
            {
                Q.Cast(target);
            }

        }
        private static void Combo()
        {
        }
        private static void LaneClear()
        {
        }
        private static void LastHit()
        {
        }
        private static void Harass()
        {
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
