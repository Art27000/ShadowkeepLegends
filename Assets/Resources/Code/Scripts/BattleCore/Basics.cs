using System;
using System.Collections.Generic;

namespace Shadowkeep.Battle
{
    public enum DamageType { None = 0, Slashing, Blunt, Pricking }
    public enum Team { Heroes, Enemies }
    public enum StatType { Strength, Agility, Endurance }
    public enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, Self }
    public enum BattleOutcome { Ongoing, HeroesWon, EnemiesWon, Stalemate }
    public enum BattleEventType { TurnStarted, Missed, Hit, Healed, Guarded, Died, Ended }

    /// <summary>Источник случайности. В тестах подменяется детерминированным.</summary>
    public interface IRandom
    {
        /// <summary>Целое в диапазоне [minInclusive, maxExclusive).</summary>
        int Next(int minInclusive, int maxExclusive);
    }

    public sealed class SystemRandom : IRandom
    {
        private readonly Random random;
        public SystemRandom(int seed) { random = new Random(seed); }
        public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    }

    public sealed class BattleEvent
    {
        public BattleEventType Type;
        public Combatant Actor;
        public Combatant Target;
        public int Amount;
        public string Text;
    }

    /// <summary>Один удар от начала до конца. Эффекты меняют только этот объект.</summary>
    public sealed class DamageContext
    {
        public Combatant Attacker { get; }
        public Combatant Defender { get; }
        public DamageType DamageType { get; }
        /// <summary>Номер хода атакующего в этом бою, с 1 (ход считается, даже если атак в нём не было).</summary>
        public int Turn { get; }

        public int WeaponDamage;
        public int Strength;
        public int Bonus;
        public int Multiplier = 1;
        public int Reduction;
        public bool IsSpecialAttack;

        public int FinalDamage => Math.Max((WeaponDamage + Strength + Bonus) * Multiplier - Reduction, 0);

        public DamageContext(Combatant attacker, Combatant defender)
        {
            Attacker = attacker; Defender = defender;
            Turn = attacker.TurnNumber;
            WeaponDamage = attacker.WeaponDamage;
            Strength = attacker.Strength;
            DamageType = attacker.WeaponType;
        }
    }

    /// <summary>Эффект (бывший IBonus): без состояния, номер хода берётся из контекста.</summary>
    public interface IEffect
    {
        void OnAttack(DamageContext ctx);   // эффекты атакующего, только при попадании
        void OnDefense(DamageContext ctx);  // эффекты цели на входящий урон
    }

    public abstract class EffectBase : IEffect
    {
        public virtual void OnAttack(DamageContext ctx) { }
        public virtual void OnDefense(DamageContext ctx) { }
    }
}
