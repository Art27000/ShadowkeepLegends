using System.Collections;
using UnityEngine;

public class Monster : EnemyController
{
    private Animator m_animator;
    public EnemyData data;
    void Start()
    {
        m_animator = GetComponent<Animator>();
        weapon = data.reward;
    }
    public override void GenerateStats()
    {
        Debug.Log("Запустили генерацию статов моба");
        strength = data.strength;
        agility = data.agility;
        endurance = data.endurance;
        maxHp = GetMaxHp();
        currentHp = maxHp;
        activeBonuses = EnemyAbilityRegistry.GetAbilitiesForEnemy(data.enemyName);
    }
    public override int GetMaxHp()
    {
        return data.maxHp + data.endurance;
    }

    public void Die()
    {
        m_animator.SetTrigger("Death");
    }
    public void Hurt()
    {
        m_animator.SetTrigger("Hurt");
    }

    public override void Attack()
    {
        bool hasAttack2 = false;
        foreach (var param in m_animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == "Attack2")
            {
                hasAttack2 = true;
                break;
            }
        }

        if (hasAttack2)
        {
            string trigger = (Random.Range(0, 2) == 0) ? "Attack" : "Attack2";
            m_animator.SetTrigger(trigger);
        }
        else
        {
            if (tempBool)
            {
                m_animator.SetTrigger("SpecialAttack");
                tempBool = false;
            }
            m_animator.SetTrigger("Attack");
        }
    }
    public override int CalculateDamage()
    {
        Debug.Log("ПРИШЛО УРОНА ОТ ОРУЖИЯ: " + data.weaponDamage + " а урона от силы: " + strength);
        return data.weaponDamage + strength + TempDamageModifier;
    }
    public override void PlayAttackAnimation()
    {
        Attack();
    }
    public override void TakeDamage(int amount, CharacterBase attacker)
    {
        base.TakeDamage(amount, attacker);
        if (currentHp <= 0)
        {
            Hurt();
            Die();
        }
        else
        {
            Hurt();
        }
    }
    public override void TakeDamage()
    {
        base.TakeDamage();
        if (currentHp <= 0)
        {
            Hurt();
            Die();
        }
        else
        {
            Hurt();
        }
    }
}
