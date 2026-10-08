using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterBase : MonoBehaviour
{
    public List<IBonus> activeBonuses = new List<IBonus>();
    public int strength { get; set; }
    public int agility { get; set; }
    public int endurance { get; set; }
    public int maxHp { get; set; }
    public int currentHp { get; set; }
    public Weapon weapon { get; set; }

    /// <summary>Выставляется перед Attack(), чтобы монстр показал особую анимацию (дыхание дракона).</summary>
    public bool tempBool = false;

    /// <summary>Сколько атак персонаж совершил в текущем бою.</summary>
    public int TurnNumber { get; private set; }

    public abstract void GenerateStats();
    public abstract int GetMaxHp();
    public abstract void Attack();

    public string getWeaponName() => weapon.weaponName;
    public Button GetWeapon() => weapon.weaponObj;

    public virtual int GetWeaponDamage() => weapon != null ? weapon.getDamage() : 0;
    public virtual DamageType GetDamageType() => weapon != null ? weapon.Type : DamageType.None;

    public void StartBattle()
    {
        TurnNumber = 0;
        tempBool = false;
    }

    public virtual void HealFull()
    {
        currentHp = GetMaxHp();
    }

    /// <summary>Единственное место, где уменьшается здоровье. dmg уже посчитан с учётом всех эффектов.</summary>
    public virtual void TakeDamage(int dmg, CharacterBase attacker)
    {
        currentHp = Mathf.Max(currentHp - Mathf.Max(dmg, 0), 0);
    }

    /// <summary>Один ход атаки по правилам ТЗ.</summary>
    public void DoAttack(CharacterBase defender)
    {
        TurnNumber++;

        if (!RollHit(defender))
        {
            Attack();
            return;
        }

        // Шаг 2: исходный урон = оружие + сила
        var ctx = new DamageContext(this, defender, TurnNumber, GetWeaponDamage(), strength, GetDamageType());

        foreach (var bonus in activeBonuses)          // шаг 3: эффекты атакующего
            bonus.OnAttack(ctx);
        foreach (var bonus in defender.activeBonuses) // шаг 4: эффекты цели
            bonus.OnDefense(ctx);

        tempBool = ctx.IsSpecialAttack;
        Attack();
        defender.TakeDamage(ctx.FinalDamage, this);   // шаг 5

        Debug.Log($"{name} -> {defender.name}: {ctx.FinalDamage} урона (ход {TurnNumber}), осталось {defender.currentHp} HP");
    }

    /// <summary>Шаг 1: бросок 1..(ловкость атакующего + ловкость цели); попадание, если бросок > ловкости цели.</summary>
    public bool RollHit(CharacterBase target)
    {
        int roll = Random.Range(1, agility + target.agility + 1);
        bool hit = roll > target.agility;
        Debug.Log($"{name} по {target.name}: бросок {roll} (макс {agility + target.agility}), ловкость цели {target.agility} -> {(hit ? "попал" : "промах")}");
        return hit;
    }
}
