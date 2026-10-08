/// <summary>
/// Эффект персонажа. Бонусы не хранят состояния боя: номер хода приходит в DamageContext,
/// поэтому ничего не нужно сбрасывать между боями.
/// </summary>
public interface IBonus
{
    /// <summary>Один раз при получении (например, +1 к характеристике).</summary>
    void OnAcquired(CharacterBase self);

    /// <summary>Эффекты атакующего (шаг 3 боя по ТЗ). Вызывается только при попадании.</summary>
    void OnAttack(DamageContext ctx);

    /// <summary>Эффекты цели на входящий урон (шаг 4 боя по ТЗ).</summary>
    void OnDefense(DamageContext ctx);
}

/// <summary>Базовый класс с пустыми реализациями: бонус переопределяет только нужное.</summary>
public abstract class BonusBase : IBonus
{
    public virtual void OnAcquired(CharacterBase self) { }
    public virtual void OnAttack(DamageContext ctx) { }
    public virtual void OnDefense(DamageContext ctx) { }
}
