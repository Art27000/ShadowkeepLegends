using System.Collections.Generic;
using Shadowkeep.Battle;
using UnityEngine;

/// <summary>
/// Переходник между данными проекта (ScriptableObject) и бойцами BattleCore.
/// Позже способности врагов переедут в данные, и switch по имени исчезнет.
/// </summary>
public static class CombatantFactory
{
    public static Combatant CreateEnemy(EnemyData d)
    {
        var c = new Combatant
        {
            Name = d.enemyName,
            Team = Team.Enemies,
            FlatMaxHp = d.maxHp,
            BaseStrength = d.strength,
            BaseAgility = d.agility,
            BaseEndurance = d.endurance,
            WeaponDamage = d.weaponDamage,
            WeaponType = Shadowkeep.Battle.DamageType.None
        };
        foreach (var e in EffectsFor(d.enemyName)) c.Effects.Add(e);
        c.Actions.Add(AttackAction.Instance);
        c.RestoreFull();
        return c;
    }

    public static WeaponDef ToWeaponDef(Weapon w)
    {
        // Shadowkeep.Battle.DamageType указан полностью: в проекте есть и старый глобальный DamageType.
        System.Enum.TryParse(w.damageType, true, out Shadowkeep.Battle.DamageType type);
        return new WeaponDef(w.weaponName, w.damage, type);
    }

    public static Combatant CreateHero(string name, Dictionary<HeroClass, int> levels, Weapon weapon)
    {
        int str = Random.Range(1, 4), agi = Random.Range(1, 4), end = Random.Range(1, 4); // 1..3 по ТЗ
        return SpecContent.CreateHero(name, str, agi, end, levels, ToWeaponDef(weapon));
    }

    private static List<IEffect> EffectsFor(string enemyName)
    {
        var list = new List<IEffect>();
        switch (enemyName)
        {
            case "Goblin": break;
            case "Skeleton": list.Add(new DoubleDamageFromBlunt()); break;
            case "Slime": list.Add(new ImmuneToSlashing()); break;
            case "Ghost": list.Add(new SneakAttack()); break;
            case "Golem": list.Add(new StoneSkin()); break;
            case "Dragon": list.Add(new FireBreath()); break;
            default: Debug.LogWarning($"CombatantFactory: для врага '{enemyName}' не заданы способности."); break;
        }
        return list;
    }
}
