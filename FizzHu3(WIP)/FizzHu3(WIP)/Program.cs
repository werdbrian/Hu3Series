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

namespace FizzHu3_WIP_
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
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
            if (_Player.ChampionName != "Fizz")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 550);
            W = new Spell.Active(SpellSlot.W, (uint) _Player.GetAutoAttackRange());
            // Fix E
            E = new Spell.Skillshot(SpellSlot.E, 400, SkillShotType.Circular, 250 , Int32.MaxValue, 300);
            R = new Spell.Skillshot(SpellSlot.R, 550, SkillShotType.Linear, 250 ,Int32.MaxValue, 250);

            Menu = MainMenu.AddMenu("FizzHu3", "fizzhu3");
            Menu.AddGroupLabel("Fizz Hu3 V0.1");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("Rcombo", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Wharass", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("laneMana", new Slider("Mana % To Use Spells On LaneClear", 30, 0, 100));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("Elane", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("laneMana", new Slider("Mana % To Use Spells On LaneClear", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Rks", new CheckBox("Use R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Edraw", new CheckBox("Draw E"));
            SettingsMenu.Add("Rdraw", new CheckBox("Draw R"));

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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }

            KillSteal();
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rcombo"].Cast<CheckBox>().CurrentValue;

            if (target == null)
                return;

            if (useR && R.IsReady() && !target.IsZombie && R.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(R.Range))
            {
                R.Cast(target);
            }
            if (useW && W.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                W.Cast();
            }
            if (useQ && Q.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            // Fix E
            if (useE && E.IsReady() && !target.IsZombie && E.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }


        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wharass"].Cast<CheckBox>().CurrentValue;
            if (target == null)
                return;

            if (useW && W.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                W.Cast();
            }
            if (useQ && Q.IsReady() && !target.IsZombie && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["Rks"].Cast<CheckBox>().CurrentValue;
            var lich = new Item((int)ItemId.Lich_Bane);

            if (useR && R.IsReady() && !target.IsZombie && R.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(R.Range) && target.Health
                <= _Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
            if (useQ && Q.IsReady() && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (useQ && Q.IsReady() && lich.IsOwned() && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q) + _Player.GetItemDamage(target, ItemId.Lich_Bane))
            {
                Q.Cast(target);
            }

        }
        private static void LaneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elane"].Cast<CheckBox>().CurrentValue;
            var lich = new Item((int)ItemId.Lich_Bane);

            if (minions == null)
                return;

            foreach (var minion in minions)
            {
                if (Q.IsReady() && lich.IsOwned() && useQ && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q) + _Player.GetItemDamage(minion, ItemId.Lich_Bane))
                {
                    Q.Cast(minion);
                }
                if (Q.IsReady() && useQ && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (E.IsReady() && useE && minion.IsValidTarget(E.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }

            }

        }
        private static void LastHit()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            var useQ = SettingsMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var lich = new Item((int)ItemId.Lich_Bane);

            if (minions == null)
                return;
            foreach(var minion in minions)
            {
                if (Q.IsReady()&& lich.IsOwned() && useQ && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q) + _Player.GetItemDamage(minion, ItemId.Lich_Bane))
                {
                    Q.Cast(minion);
                }
                if (Q.IsReady() && useQ && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawQ = SettingsMenu["Qdraw"].Cast<CheckBox>().CurrentValue;
            var drawE = SettingsMenu["Edraw"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rdraw"].Cast<CheckBox>().CurrentValue;

            if (drawQ && Q.IsReady())
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawE && E.IsReady())
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
