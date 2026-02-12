using System.Collections;
using UnityEngine;

public class Barbarian : PlayerController
{
    public PlayerData data;
    private Animator m_animator;
    void Awake()
    {
        m_animator = GetComponent<Animator>();
        weapon = data.weapon;
        Debug.Log("Weapon в CharacterBase теперь: " + weapon);
    }
    public override void AddClass(CharacterClass cls)
    {
        base.AddClass(CharacterClass.Barbarian);
    }
    public void Die()
    {
        m_animator.SetBool("Death", true);
    }
    public void Hurt()
    {
        m_animator.SetTrigger("Hurt");
    }
    public override void Attack()
    {
        m_animator.SetTrigger("Attack");
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
}
