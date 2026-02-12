using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum CharacterClass
{
    Knight = 0,
    Bandit = 1,
    Barbarian = 2
}
public abstract class PlayerController : CharacterBase
{
    public Dictionary<CharacterClass, int> classLevels = new Dictionary<CharacterClass, int>();
    public HeroesContainer hc;
    public void SetHc(HeroesContainer container)
    {
        hc = container;
    }
    public virtual int GetHpPerLvl(CharacterClass cls)
    {
        return hc.heroes[(int)cls].HP_Per_Lvl;
    }

    public virtual void AddClass(CharacterClass cls)
    {
        if (!classLevels.ContainsKey(cls))
            classLevels[cls] = 1;
        else
            classLevels[cls]++;

    }
    public int GetLevel(CharacterClass cls)
    {
        return classLevels.ContainsKey(cls) ? classLevels[cls] : 0;
    }
    public override int GetMaxHp()
    {
        return GetLevel(CharacterClass.Knight) * GetHpPerLvl(CharacterClass.Knight)
            + GetLevel(CharacterClass.Bandit) * GetHpPerLvl(CharacterClass.Bandit) + GetLevel(CharacterClass.Barbarian) * GetHpPerLvl(CharacterClass.Barbarian)
            + endurance;
    }
    public int TotalLevel => classLevels.Values.Sum();
    public override void GenerateStats()
    {
        strength = (int)Random.Range(1, 4);
        agility = (int)Random.Range(1, 4);
        endurance = (int)Random.Range(1, 4);
        maxHp = GetMaxHp();
        currentHp = maxHp;
        Debug.Log("STR:" + strength + " AGIL:" + agility + " END:" + endurance + " MAXHP:" + maxHp);
    }
    public abstract void PlayAttackAnimation();
    public void LevelUp(CharacterClass cls)
    {
            Debug.Log("Уровень повышен! Добавляем максимальное хп и выбирам новый класс");
            AddClass(cls);
            IBonus bonus = BonusRegistry.GetBonusForClassLevel(cls, GetLevel(cls));
        Debug.Log("Текущий уровень: " + GetLevel(cls));
        if (bonus != null)
        {
            activeBonuses.Add(bonus);
            Debug.Log("Добавили бонус персонажу!");
        }
        StartTurnBonus(); //Вызываем бонус(например +1 к ловкости) при повышении уровня
        HealFull();
            Debug.Log("Герою добавлено " + (GetHpPerLvl(cls) + endurance) + "HP. Из них " + endurance + " составляет ENDURANCE");
    }
    public override void TakeDamage(int amount, CharacterBase attacker)
    {
        base.TakeDamage(amount, attacker);
    }
}
