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
        //
        private static float ZedProc = 0.0f;
        private static AIHeroClient ZedTarget = null;


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
            if (_Player.ChampionName != "Kayle")
                return;

            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Targeted(SpellSlot.W, 900);
            E = new Spell.Active(SpellSlot.E, 525);
            R = new Spell.Targeted(SpellSlot.R, 900);



            Menu = MainMenu.AddMenu("Kayle Hu3", "kaylehu3");
            Menu.AddGroupLabel("Kayle Hu3 V1.0");
            Menu.AddSeparator();
            Menu.AddLabel("Made By MarioGK");

            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("Qcombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("Wcombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("Ecombo", new CheckBox("Use E on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("Qharass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("Eharass", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlast", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("Elast", new CheckBox("Use E on LastHit"));
            SettingsMenu.Add("manaLast", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("Qlane", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("Elane", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("manaLane", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qks", new CheckBox("Use Q KillSteal"));
            SettingsMenu.AddLabel("Ult Manager");
            SettingsMenu.Add("Rme", new CheckBox("Use R in Yourself"));
            SettingsMenu.Add("HPme", new Slider("Health % To Use R", 20, 0, 100));
            SettingsMenu.Add("Rally", new CheckBox("Use R in Ally"));          
            SettingsMenu.Add("HPally", new Slider("Health % To Use R on Ally", 20, 0, 100));
            SettingsMenu.Add("Rdangerous", new CheckBox("Use R on dangerous spells ? (Only R Zed)"));     
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("Qdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Wdraw", new CheckBox("Draw Q"));
            SettingsMenu.Add("Edraw", new CheckBox("Draw E"));
            SettingsMenu.Add("Rdraw", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }

        private static void Game_OnTick(EventArgs args)
        {
            KillSteal();

            AutoUlt();

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
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.BaseSkinName == "Zed" && args.Slot == SpellSlot.R)
            {
                ZedTarget = new AIHeroClient((short)args.Target.Index, (uint)args.Target.NetworkId);
                ZedProc = Game.Time + 2.9f;
            }
        }

        private static void KillSteal()
        {
            var useQ = SettingsMenu["Qks"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target ,SpellSlot.Q))
            {
                Q.Cast(target);
            }

        }

        private static void Combo()
        {
            var useQ = SettingsMenu["Qcombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wcombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ecombo"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }   
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast();
            }
            if (useW && W.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie && _Player.HealthPercent < 95)
            {
                W.Cast(_Player);
            }
        }

        private static void Harass()
        {
            var useQ = SettingsMenu["Qharass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eharass"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast();
            }
        }

        private static void LaneClear()
        {
            var useQ = SettingsMenu["Qlane"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elane"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlast"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elast"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.Health).Where(m => m.IsEnemy);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(650) && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast();
                }
            }
        }    

        private static void AutoUlt()
        {
            var Rme = SettingsMenu["Rme"].Cast<CheckBox>().CurrentValue;
            var Rally = SettingsMenu["Rally"].Cast<CheckBox>().CurrentValue;
            var HPme = SettingsMenu["HPme"].Cast<Slider>().CurrentValue;
            var HPally = SettingsMenu["HPally"].Cast<Slider>().CurrentValue;
            var useDangerous = SettingsMenu["Rdangerous"].Cast<CheckBox>().CurrentValue;
            var allies = HeroManager.Allies.OrderBy(a => a.Health).Where(a => !a.IsZombie);

            if (useDangerous && Game.Time == ZedProc)
            {
                R.Cast(ZedTarget);
            }

            if (Rme && R.IsReady() && _Player.HealthPercent < HPme && R.IsReady() && _Player.CountEnemiesInRange(R.Range) >= 1)
            {
                R.Cast(_Player);
            }
            foreach (var ally in allies)
            {
                if (Rally && R.IsReady() && ally.Health < HPally && R.IsReady() && ally.CountEnemiesInRange(R.Range) >= 1)
                {
                    R.Cast(ally);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            var drawQ = SettingsMenu["Qdraw"].Cast<CheckBox>().CurrentValue;
            var drawW = SettingsMenu["Wdraw"].Cast<CheckBox>().CurrentValue;
            var drawE = SettingsMenu["Edraw"].Cast<CheckBox>().CurrentValue;
            var drawR = SettingsMenu["Rdraw"].Cast<CheckBox>().CurrentValue;

            if (drawQ && Q.IsReady())
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawW && W.IsReady())
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (drawE && E.IsReady())
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (drawR && R.IsReady())
            {
                new Circle() { Color = Color.Pink, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}
