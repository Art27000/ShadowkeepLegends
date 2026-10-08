using UnityEngine;

/// <summary>
/// Один удар от начала до конца. Бонусы не трогают поля персонажей напрямую,
/// а только меняют этот объект. Порядок по ТЗ: исходный урон -> эффекты атакующего
/// (Bonus, IsSpecialAttack) -> эффекты цели (Multiplier, Reduction, WeaponDamage) -> FinalDamage.
/// </summary>
public class DamageContext
{
    public CharacterBase Attacker { get; }
    public CharacterBase Defender { get; }
    public DamageType DamageType { get; }

    /// <summary>Номер хода атакующего в этом бою, начиная с 1 (промахи тоже считаются ходами).</summary>
    public int TurnNumber { get; }

    public int WeaponDamage;   // часть урона от оружия (эффекты цели могут обнулить)
    public int Strength;       // часть урона от силы
    public int Bonus;          // плоские прибавки/вычеты от эффектов атакующего
    public int Multiplier = 1; // множитель общего урона (Скелет vs дробящее)
    public int Reduction;      // плоское снижение от эффектов цели
    public bool IsSpecialAttack;

    public int FinalDamage => Mathf.Max((WeaponDamage + Strength + Bonus) * Multiplier - Reduction, 0);

    public DamageContext(CharacterBase attacker, CharacterBase defender, int turnNumber,
                         int weaponDamage, int strength, DamageType damageType)
    {
        Attacker = attacker;
        Defender = defender;
        TurnNumber = turnNumber;
        WeaponDamage = weaponDamage;
        Strength = strength;
        DamageType = damageType;
    }

    public void Log(string message)
    {
        if (GameController.Instance != null)
            GameController.Instance.ShowLog(message);
    }
}
