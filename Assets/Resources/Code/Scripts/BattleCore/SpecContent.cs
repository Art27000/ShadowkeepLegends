using System.Collections.Generic;
using System.Linq;

namespace Shadowkeep.Battle
{
    public enum HeroClass { Bandit, Knight, Barbarian }

    public struct WeaponDef
    {
        public string Name; public int Damage; public DamageType Type;
        public WeaponDef(string name, int damage, DamageType type) { Name = name; Damage = damage; Type = type; }
    }

    /// <summary>Контент из ТЗ. Позже его заменят ScriptableObject-данные; тесты и симуляции останутся прежними.</summary>
    public static class SpecContent
    {
        public static readonly WeaponDef Sword = new WeaponDef("Меч", 3, DamageType.Slashing);
        public static readonly WeaponDef Club = new WeaponDef("Дубина", 3, DamageType.Blunt);
        public static readonly WeaponDef Dagger = new WeaponDef("Кинжал", 2, DamageType.Pricking);
        public static readonly WeaponDef Axe = new WeaponDef("Топор", 4, DamageType.Slashing);
        public static readonly WeaponDef Spear = new WeaponDef("Копьё", 3, DamageType.Pricking);
        public static readonly WeaponDef LegendarySword = new WeaponDef("Легендарный меч", 10, DamageType.Slashing);

        public static int HpPerLevel(HeroClass c) => c == HeroClass.Bandit ? 4 : c == HeroClass.Knight ? 5 : 6;
        public static WeaponDef StartWeapon(HeroClass c) => c == HeroClass.Bandit ? Dagger : c == HeroClass.Knight ? Sword : Club;

        /// <summary>Герой с уровнями по классам (мультикласс) и базовыми характеристиками.</summary>
        public static Combatant CreateHero(string name, int str, int agi, int end,
                                           IDictionary<HeroClass, int> levels, WeaponDef weapon)
        {
            int L(HeroClass c) => levels.TryGetValue(c, out var v) ? v : 0;
            var h = new Combatant
            {
                Name = name, Team = Team.Heroes,
                BaseStrength = str, BaseAgility = agi, BaseEndurance = end,
                FlatMaxHp = levels.Sum(kv => kv.Value * HpPerLevel(kv.Key)),
                EnduranceLevels = levels.Values.Sum(),
                WeaponDamage = weapon.Damage, WeaponType = weapon.Type
            };
            if (L(HeroClass.Bandit) >= 1) h.Effects.Add(new SneakAttack());
            if (L(HeroClass.Bandit) >= 2) h.AddModifier(StatType.Agility, 1);
            if (L(HeroClass.Bandit) >= 3) h.Effects.Add(new Poison());
            if (L(HeroClass.Knight) >= 1) h.Effects.Add(new Impulse());
            if (L(HeroClass.Knight) >= 2) h.Effects.Add(new Shield());
            if (L(HeroClass.Knight) >= 3) h.AddModifier(StatType.Strength, 1);
            if (L(HeroClass.Barbarian) >= 1) h.Effects.Add(new Rage());
            if (L(HeroClass.Barbarian) >= 2) h.Effects.Add(new StoneSkin());
            if (L(HeroClass.Barbarian) >= 3) h.AddModifier(StatType.Endurance, 1);
            h.Actions.Add(AttackAction.Instance);
            h.Actions.Add(DefendAction.Instance);
            h.RestoreFull();
            return h;
        }

        public static readonly string[] EnemyNames = { "Гоблин", "Скелет", "Слайм", "Призрак", "Голем", "Дракон" };
        public static WeaponDef EnemyReward(string name) =>
            name == "Гоблин" ? Dagger : name == "Скелет" ? Club : name == "Слайм" ? Spear :
            name == "Призрак" ? Sword : name == "Голем" ? Axe : LegendarySword;

        public static Combatant CreateEnemy(string name)
        {
            // имя: здоровье, сила, ловкость, выносливость, урон оружия
            var d = new Dictionary<string, int[]>
            {
                ["Гоблин"] = new[] { 5, 1, 1, 1, 2 }, ["Скелет"] = new[] { 10, 2, 2, 1, 2 },
                ["Слайм"] = new[] { 8, 3, 1, 2, 1 },  ["Призрак"] = new[] { 6, 1, 3, 1, 3 },
                ["Голем"] = new[] { 10, 3, 1, 3, 1 }, ["Дракон"] = new[] { 20, 3, 3, 3, 4 },
            }[name];
            var e = new Combatant
            {
                Name = name, Team = Team.Enemies, FlatMaxHp = d[0],
                BaseStrength = d[1], BaseAgility = d[2], BaseEndurance = d[3],
                WeaponDamage = d[4], WeaponType = DamageType.None
            };
            if (name == "Скелет") e.Effects.Add(new DoubleDamageFromBlunt());
            if (name == "Слайм") e.Effects.Add(new ImmuneToSlashing());
            if (name == "Призрак") e.Effects.Add(new SneakAttack());
            if (name == "Голем") e.Effects.Add(new StoneSkin());
            if (name == "Дракон") e.Effects.Add(new FireBreath());
            e.Actions.Add(AttackAction.Instance);
            e.RestoreFull();
            return e;
        }
    }
}
