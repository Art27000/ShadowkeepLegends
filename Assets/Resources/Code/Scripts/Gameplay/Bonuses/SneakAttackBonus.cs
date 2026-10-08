/// <summary>Скрытая атака: +1 к урону, если ловкость атакующего выше ловкости цели.</summary>
public class SneakAttackBonus : BonusBase
{
    public override void OnAttack(DamageContext ctx)
    {
        if (ctx.Attacker.agility > ctx.Defender.agility)
        {
            ctx.Bonus += 1;
            ctx.Log("Active bonus: Sneak Attack");
        }
    }
}
