/// <summary>Порыв к действию: в первый ход наносит двойной урон оружием (+ урон оружия ещё раз).</summary>
public class DoubleDamageBonus : BonusBase
{
    public override void OnAttack(DamageContext ctx)
    {
        if (ctx.TurnNumber == 1)
        {
            ctx.Bonus += ctx.WeaponDamage;
            ctx.Log("Active bonus: Impulse to action");
        }
    }
}
