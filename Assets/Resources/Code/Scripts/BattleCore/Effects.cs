namespace Shadowkeep.Battle
{
    /// <summary>Скрытая атака: +1, если ловкость атакующего выше ловкости цели.</summary>
    public sealed class SneakAttack : EffectBase
    {
        public override void OnAttack(DamageContext c) { if (c.Attacker.Agility > c.Defender.Agility) c.Bonus += 1; }
    }
    /// <summary>Яд: +1 на втором ходу, +2 на третьем и далее.</summary>
    public sealed class Poison : EffectBase
    {
        public override void OnAttack(DamageContext c) { if (c.Turn >= 2) c.Bonus += c.Turn - 1; }
    }
    /// <summary>Порыв к действию: в первый ход ещё раз урон оружия.</summary>
    public sealed class Impulse : EffectBase
    {
        public override void OnAttack(DamageContext c) { if (c.Turn == 1) c.Bonus += c.WeaponDamage; }
    }
    /// <summary>Щит: -3 к получаемому урону, если сила цели выше силы атакующего.</summary>
    public sealed class Shield : EffectBase
    {
        public override void OnDefense(DamageContext c) { if (c.Defender.Strength > c.Attacker.Strength) c.Reduction += 3; }
    }
    /// <summary>Ярость: +2 в первые 3 хода, потом -1.</summary>
    public sealed class Rage : EffectBase
    {
        public override void OnAttack(DamageContext c) { c.Bonus += c.Turn <= 3 ? 2 : -1; }
    }
    /// <summary>Каменная кожа: получаемый урон снижается на выносливость.</summary>
    public sealed class StoneSkin : EffectBase
    {
        public override void OnDefense(DamageContext c) { c.Reduction += c.Defender.Endurance; }
    }
    /// <summary>Дыхание огнём: каждый 3-й ход +3 урона.</summary>
    public sealed class FireBreath : EffectBase
    {
        public override void OnAttack(DamageContext c) { if (c.Turn % 3 == 0) { c.Bonus += 3; c.IsSpecialAttack = true; } }
    }
    /// <summary>Слайм: рубящее оружие не наносит урона (сила и эффекты работают).</summary>
    public sealed class ImmuneToSlashing : EffectBase
    {
        public override void OnDefense(DamageContext c) { if (c.DamageType == DamageType.Slashing) c.WeaponDamage = 0; }
    }
    /// <summary>Скелет: вдвое больше урона от дробящего оружия.</summary>
    public sealed class DoubleDamageFromBlunt : EffectBase
    {
        public override void OnDefense(DamageContext c) { if (c.DamageType == DamageType.Blunt) c.Multiplier *= 2; }
    }
}
