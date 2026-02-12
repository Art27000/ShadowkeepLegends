using UnityEngine;

public abstract class EnemyController : CharacterBase
{
    private string reward;
    public override void TakeDamage(int amount, CharacterBase attacker)
    {
        base.TakeDamage(amount, attacker);
    }
    public abstract void PlayAttackAnimation();
}
