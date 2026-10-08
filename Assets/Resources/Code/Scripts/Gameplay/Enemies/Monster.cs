using UnityEngine;

public class Monster : EnemyController
{
    public EnemyData data;

    private Animator m_animator;
    private bool hasAttack2;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        weapon = data.reward; // награда за победу, не оружие самого монстра

        foreach (var param in m_animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == "Attack2")
            {
                hasAttack2 = true;
                break;
            }
        }
    }

    public override void GenerateStats()
    {
        strength = data.strength;
        agility = data.agility;
        endurance = data.endurance;
        maxHp = GetMaxHp();
        currentHp = maxHp;
        activeBonuses = EnemyAbilityRegistry.GetAbilitiesForEnemy(data.enemyName);
    }

    // По ТЗ колонка «Здоровье» в таблице врагов уже и есть итоговое здоровье.
    public override int GetMaxHp() => data.maxHp;

    // Оружие монстра задаётся в EnemyData, а не полем weapon (там лежит награда).
    public override int GetWeaponDamage() => data.weaponDamage;
    public override DamageType GetDamageType() => DamageType.None;

    public void Die() => m_animator.SetTrigger("Death");
    public void Hurt() => m_animator.SetTrigger("Hurt");

    public override void Attack()
    {
        if (hasAttack2)
        {
            m_animator.SetTrigger(Random.Range(0, 2) == 0 ? "Attack" : "Attack2");
            return;
        }

        if (tempBool)
        {
            m_animator.SetTrigger("SpecialAttack");
            tempBool = false;
        }
        m_animator.SetTrigger("Attack");
    }

    public override void PlayAttackAnimation() => Attack();

    public override void TakeDamage(int amount, CharacterBase attacker)
    {
        base.TakeDamage(amount, attacker);
        Hurt();
        if (currentHp <= 0)
            Die();
    }
}
