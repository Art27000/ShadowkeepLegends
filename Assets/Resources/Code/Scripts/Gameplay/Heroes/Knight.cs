using UnityEngine;

public class Knight : PlayerController
{
    // Единственное отличие рыцаря: случайный выбор из двух анимаций атаки.
    public override void Attack()
    {
        m_animator.SetTrigger(Random.Range(0, 2) == 0 ? "Attack" : "Attack2");
    }
}
