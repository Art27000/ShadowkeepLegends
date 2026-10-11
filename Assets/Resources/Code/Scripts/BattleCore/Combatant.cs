using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowkeep.Battle
{
    public sealed class Combatant
    {
        public string Name;
        public Team Team;

        public int BaseStrength, BaseAgility, BaseEndurance;
        /// <summary>Здоровье без выносливости: сумма «HP за уровень» героя или здоровье монстра из таблицы.</summary>
        public int FlatMaxHp;
        /// <summary>Сколько раз выносливость прибавляется к здоровью (уровень героя; у монстров 0).</summary>
        public int EnduranceLevels;

        public int WeaponDamage;
        public DamageType WeaponType;

        public int ActionPointsPerTurn = 3;
        public int ActionPoints;
        public int TurnNumber;
        public bool Guarding;
        public int Hp;

        public readonly List<IEffect> Effects = new List<IEffect>();
        public readonly List<IBattleAction> Actions = new List<IBattleAction>();
        public readonly Dictionary<IBattleAction, int> Cooldowns = new Dictionary<IBattleAction, int>();
        private readonly List<KeyValuePair<StatType, int>> modifiers = new List<KeyValuePair<StatType, int>>();

        // Итоговые характеристики = база + модификаторы (их можно снять вместе с артефактом).
        public int Strength => BaseStrength + Mod(StatType.Strength);
        public int Agility => BaseAgility + Mod(StatType.Agility);
        public int Endurance => BaseEndurance + Mod(StatType.Endurance);
        public int MaxHp => FlatMaxHp + Endurance * EnduranceLevels;
        public bool IsAlive => Hp > 0;

        public void AddModifier(StatType stat, int value) => modifiers.Add(new KeyValuePair<StatType, int>(stat, value));
        private int Mod(StatType s) { int sum = 0; foreach (var m in modifiers) if (m.Key == s) sum += m.Value; return sum; }

        public void RestoreFull() => Hp = MaxHp;
        public void Heal(int amount) => Hp = Math.Min(MaxHp, Hp + Math.Max(amount, 0));

        public int CooldownOf(IBattleAction a) => Cooldowns.TryGetValue(a, out var v) ? v : 0;
        public override string ToString() => Name;
    }
}
