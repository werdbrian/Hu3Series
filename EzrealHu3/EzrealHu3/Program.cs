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


namespace EzrealHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Menu EzrealMenu, SettingsMenu, ActivatorMenu;


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

            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1190, SkillShotType.Linear, (int)0.25f, 2000, (int)60f);
            W = new Spell.Skillshot(SpellSlot.W, 990, SkillShotType.Linear, (int)0.25f, 1600, (int)80f);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, (int)1f, 2000, (int)(160f));

            EzrealMenu = MainMenu.AddMenu("Ezreal Hu3", "ezrealhu3");
            EzrealMenu.AddGroupLabel("Ezreal Hu3 1.9");
            EzrealMenu.AddSeparator();
            EzrealMenu.AddLabel("Made By MarioGK");

            SettingsMenu = EzrealMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use W on Harass"));
            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("lasthitQ", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("lasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("laneclearQ", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("laneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killsteal", new CheckBox("KillSteal"));
            SettingsMenu.Add("killstealQ", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("killstealW", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("killstealR", new CheckBox("Use R KillSteal"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R Combo"));
            ActivatorMenu = EzrealMenu.AddSubMenu("Activator", "Activator");
            ActivatorMenu.Add("useitems", new CheckBox("Use Activator"));
            ActivatorMenu.AddGroupLabel("Potions Health");
            ActivatorMenu.Add("healthP", new CheckBox("Health Potion", true));
            ActivatorMenu.Add("healthS", new Slider("Min HP %", 20));
            ActivatorMenu.AddGroupLabel("Potions Mana");
            ActivatorMenu.Add("manaP", new CheckBox("Mana Potion", true));
            ActivatorMenu.Add("manaS", new Slider("Min Mana %", 20));
            ActivatorMenu.AddGroupLabel("Items");
            ActivatorMenu.Add("blade", new CheckBox("Blade Of Knight Ruined"));
            ActivatorMenu.Add("bladeS", new Slider("▲ Enemie Helth % To Use ▲", 80, 1, 100));
            ActivatorMenu.Add("youmu", new CheckBox("Youmuu's Ghostblade"));
            ActivatorMenu.Add("youmuS", new Slider("▲ Enemie Helth % To Use ▲", 80, 1, 100));
            ActivatorMenu.Add("changeT", new CheckBox("Change Trinket To Blue"));
            ActivatorMenu.Add("changeS", new Slider("▲ Level To Change Trinket ▲", 6, 1, 18));
            ActivatorMenu.Add("cleanser", new CheckBox("Use Cleanses Items On CC(WIP)"));


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
            if (SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }
            if (ActivatorMenu["useitems"].Cast<CheckBox>().CurrentValue)
            {
                hpPOT();
            }
        }
        //Damages
        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 35, 55, 75, 95, 115 }[Program.Q.Level] + 0.4 * _Player.FlatMagicDamageMod + 1.1 * _Player.FlatPhysicalDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 70, 115, 160, 205, 250 }[Program.W.Level] + 0.8 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 320, 470, 600 }[Program.R.Level] + 0.9 * _Player.FlatMagicDamageMod + 1.0 * _Player.FlatPhysicalDamageMod));
        }
        private static void KillSteal()
        {
            var useQ = SettingsMenu["killstealQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["killstealW"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["killstealR"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie && hero.Health <= QDamage(hero)))
            {
                if (Q.IsReady() && useQ && Q.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && useW && W.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (R.IsReady() && useR && R.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(2000))
                {
                    R.Cast(target);
                }
            }
        }

        private static void Combo()
        {
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (useR && R.IsReady() && R.GetPrediction(target).HitChance >= HitChance.High && target.Health <= RDamage(target) && target.IsValidTarget(R.Range))
                {
                    R.Cast(target);
                }
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (target == null) return;
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High)
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
            }
        }

        private static void LastHit()
        {
            var useQ = SettingsMenu["lasthitQ"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["lasthitMana"].Cast<Slider>().CurrentValue;
            foreach (var target in ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy && !a.IsValidTarget(550)))
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana && target.Health <= QDamage(target))
                {
                    if (target == null) return;
                    Q.Cast(target);
                }

            }
        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["laneclearQ"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["laneclearMana"].Cast<Slider>().CurrentValue;

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy))
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana && minion.Health <= QDamage(minion))
                {
                    if (minion == null) return;
                    Q.Cast(minion);
                }

            }
        }
        private static void Items()
        {


            var useManaP = ActivatorMenu["manaP"].Cast<CheckBox>().CurrentValue;
            var manaS = ActivatorMenu["manaS"].Cast<Slider>().CurrentValue;
            var changeT = ActivatorMenu["changeT"].Cast<Slider>().CurrentValue;
            var changeS = ActivatorMenu["changeS"].Cast<Slider>().CurrentValue;
            var useBlade = ActivatorMenu["blade"].Cast<CheckBox>().CurrentValue;
            var bladeS = ActivatorMenu["bladeS"].Cast<Slider>().CurrentValue;
            var useYoumou = ActivatorMenu["youmou"].Cast<CheckBox>().CurrentValue;
            var youmouS = ActivatorMenu["youmouS"].Cast<Slider>().CurrentValue;
            var useCleanser = ActivatorMenu["cleanser"].Cast<CheckBox>().CurrentValue;
            var blade1 = new Item((int)ItemId.Blade_of_the_Ruined_King);
            var blade2 = new Item((int)ItemId.Bilgewater_Cutlass);
            var youmu = new Item((int)ItemId.Youmuus_Ghostblade);

            var manaP = new Item((int)ItemId.Mana_Potion);
            var trinketG = new Item((int)ItemId.Warding_Totem_Trinket);
            var trinketB = new Item((int)ItemId.Scrying_Orb_Trinket);


            if (useManaP && Player.Instance.ManaPercent < manaS)
            {
                manaP.Cast();
            }
            if (trinketG.IsOwned() && !trinketB.IsOwned() && Player.Instance.Level == changeS && Shop.IsOpen)
            {
                Shop.SellItem(ItemId.Warding_Totem_Trinket);
                Shop.BuyItem(ItemId.Scrying_Orb_Trinket);
            }
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead))
            {
                if (target.HealthPercent <= bladeS && blade1.IsOwned() && blade1.IsReady())
                {
                    blade1.Cast(target);
                }
                if (target.HealthPercent <= bladeS && blade2.IsOwned() && blade2.IsReady())
                {
                    blade2.Cast(target);
                }
                if (target.HealthPercent <= youmouS && youmu.IsOwned() && youmu.IsReady())
                {
                    youmu.Cast();
                }
            }
        }
        private static void hpPOT()
        {
            var useHealthP = ActivatorMenu["healthP"].Cast<CheckBox>().CurrentValue;
            var healthS = ActivatorMenu["healthS"].Cast<Slider>().CurrentValue;
            var healthP = new Item((int)ItemId.Health_Potion);
            if (useHealthP && Player.Instance.HealthPercent < healthS && healthP.IsOwned())
            {
                healthP.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
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