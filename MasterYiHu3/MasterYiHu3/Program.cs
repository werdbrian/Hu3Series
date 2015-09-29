using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;
using Color = System.Drawing.Color;
using SharpDX;

namespace MasterYiHu3
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
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
            if (_Player.ChampionName != "MasterYi")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 500);

            Menu = MainMenu.AddMenu("MasterYiHu3", "masteryihu3");
            Menu.AddGroupLabel("MasterYi Hu3 V1.0");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.AddLabel("Evade With Q/W (WIP)");
            SettingsMenu.Add("evade", new CheckBox("Use Evade Q/W (WIP)"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Game_OnTick(EventArgs args)
        {

            if (_Player.IsDead || MenuGUI.IsChatOpen || _Player.IsRecalling) return;

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

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;

            if (target == null)
                return;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

            if (target == null)
                return;

            if (useQ && Q.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && !target.IsZombie && target.IsValidTarget(150))
            {
                E.Cast();
            }
            if (useR && R.IsReady() && !target.IsZombie && target.IsValidTarget(R.Range))
            {
                R.Cast();
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }

        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
            {
                Q.Cast(minion);
            }
        }

        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);

            if (minions == null)
                return;

            foreach (var minion in minions)
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead)
                return;

            if (SettingsMenu["Qdraw"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
        }
    }
}
