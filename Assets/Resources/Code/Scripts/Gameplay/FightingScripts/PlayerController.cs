using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterClass
{
    Knight = 0,
    Bandit = 1,
    Barbarian = 2
}

/// <summary>
/// Общая логика всех играбельных героев. Конкретные классы (Bandit, Knight, Barbarian)
/// теперь только переопределяют то, чем реально отличаются (например, анимацию атаки).
/// Поле `data` перенесено сюда с сохранением имени, поэтому ссылки в префабах и сценах не сломаются.
/// </summary>
public abstract class PlayerController : CharacterBase
{
    private const int MinStat = 1;
    private const int MaxStatExclusive = 4; // int-перегрузка Random.Range: значения 1..3

    public PlayerData data;
    public HeroesContainer hc;
    public Dictionary<CharacterClass, int> classLevels = new Dictionary<CharacterClass, int>();

    protected Animator m_animator;

    public int TotalLevel => classLevels.Values.Sum();

    protected virtual void Awake()
    {
        m_animator = GetComponent<Animator>();
        weapon = data.weapon;
    }

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
        classLevels[cls] = GetLevel(cls) + 1;
    }

    public int GetLevel(CharacterClass cls)
    {
        return classLevels.TryGetValue(cls, out int level) ? level : 0;
    }

    public override int GetMaxHp()
    {
        // По ТЗ выносливость прибавляется к здоровью при каждом повышении уровня.
        int hp = endurance * TotalLevel;
        foreach (CharacterClass cls in System.Enum.GetValues(typeof(CharacterClass)))
            hp += GetLevel(cls) * GetHpPerLvl(cls);
        return hp;
    }

    public override void GenerateStats()
    {
        strength = Random.Range(MinStat, MaxStatExclusive);
        agility = Random.Range(MinStat, MaxStatExclusive);
        endurance = Random.Range(MinStat, MaxStatExclusive);
        maxHp = GetMaxHp();
        currentHp = maxHp;
        Debug.Log($"STR:{strength} AGIL:{agility} END:{endurance} MAXHP:{maxHp}");
    }

    public void LevelUp(CharacterClass cls)
    {
        AddClass(cls);
        Debug.Log($"Уровень повышен: {cls} -> {GetLevel(cls)}");

        IBonus bonus = BonusRegistry.GetBonusForClassLevel(cls, GetLevel(cls));
        if (bonus != null)
        {
            activeBonuses.Add(bonus);
            bonus.OnAcquired(this); // разовые бонусы (+1 к характеристике)
            Debug.Log("Получен новый бонус!");
        }

        HealFull();
    }

    // ---- Анимации (общие для всех героев) ----

    public void Die() => m_animator.SetBool("Death", true);
    public void Hurt() => m_animator.SetTrigger("Hurt");

    public override void Attack() => m_animator.SetTrigger("Attack");

    public virtual void PlayAttackAnimation() => Attack();

    public override void TakeDamage(int amount, CharacterBase attacker)
    {
        base.TakeDamage(amount, attacker);
        Hurt();
        if (currentHp <= 0)
            Die();
    }
}
