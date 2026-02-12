using Unity.VisualScripting;
using UnityEngine;

public class Knight : PlayerController
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
        base.AddClass(CharacterClass.Knight);
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
        int choice = Random.Range(0, 2);

        if (choice == 0)
            m_animator.SetTrigger("Attack");
        else
            m_animator.SetTrigger("Attack2");
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
