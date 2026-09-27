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
    public int TempDamageModifier { get; set; } = 0;
    public int TempSpecialDamageModifier { get; set; } = 1;
    public bool isPoisoned = false;
    public int TempStrengthModifier { get; set; } = 0;
    public int TempAgilityModifier { get; set; } = 0;
    public int TempEnduranceModifier { get; set; } = 0;
    public int baseDamage { get; private set; }
    public bool tempBool = false;
    public int poisonStacks = 0;
    public bool ignoreWeaponPart = false;




    public abstract void GenerateStats();
    public abstract int GetMaxHp();
    public string getWeaponName()
    {
        return weapon.weaponName;
    }
    public Button GetWeapon()
    {
        return weapon.weaponObj;
    }

    public virtual void HealFull()
    {
        currentHp = GetMaxHp();
        Debug.Log("хп восстановлено до максимального уровня: " + currentHp);
    }
    public void StartTurnBonus()
    {
        foreach (var bonus in activeBonuses)
            bonus.OnTurnStart(this, GameController.Instance);
    }
    public virtual void TakeDamage(int dmg, CharacterBase attacker)
    {
        foreach (var bonus in activeBonuses)
            bonus.OnDefense(this, attacker, GameController.Instance);

        if (ignoreWeaponPart && attacker?.weapon != null)
        {
            dmg -= attacker.weapon.getDamage();
            if (dmg < 0) dmg = 0;
            ignoreWeaponPart = false;
        }

        currentHp = Mathf.Max(currentHp - Mathf.Max(dmg - TempDamageModifier, 0), 0);
        TempDamageModifier = 0;
    }

    public virtual void TakeDamage()
    {
        if (isPoisoned)
        {
            currentHp = Mathf.Max(currentHp - poisonStacks, 0);
            Debug.Log($"{name} получает {poisonStacks} урона от яда!");

            if (currentHp == 0)
            {
                isPoisoned = false;
                poisonStacks = 0;
                Debug.Log("POISON OFF");
            }
        }
    }

    public void DoAttack(CharacterBase defender, CharacterBase attacker) 
    {
        foreach (var bonus in activeBonuses)
            bonus.OnAttack(this, defender, GameController.Instance);
        if (CalculateHitChance(defender))
        {
            defender.TakeDamage(CalculateDamage(), attacker);
            Debug.Log($"{this.name} атакует {defender.name}, у {defender.name} осталось {defender.currentHp} HP");
        }
    }
    public abstract void Attack();

    public bool CalculateHitChance(CharacterBase target)
    {
        int maxRange = target.agility + this.agility;
        int roll = Random.Range(1, maxRange + 1);
        Debug.Log("roll (max/roll)= " + maxRange + "/" + roll + " " + "target.getAgility() = " + target.agility);
        if (roll <= target.agility)
        {
            Debug.Log($"{name} промахнулся по {target.name}! (бросок {roll} ≤ {target.agility})");
            return false; // атака не попала
        }
        else
        {
            Debug.Log($"{name} попал по {target.name}! (бросок {roll} > {target.agility})");
            return true; // атака успешна
        }
    }
    public virtual int CalculateDamage()
    {
        baseDamage = weapon.getDamage() + strength;
        int totalDamage = baseDamage + TempDamageModifier;
        totalDamage = totalDamage*TempSpecialDamageModifier;
        Debug.Log("Итоговый урон: " + totalDamage + " ПРИШЛО УРОНА ОТ ОРУЖИЯ: " 
            + weapon.getDamage() + " а урона от силы: " + strength + "урона от бонусов: " + (totalDamage - baseDamage));
        TempDamageModifier = 0;
        TempSpecialDamageModifier = 1;
        return totalDamage;
    }
}
