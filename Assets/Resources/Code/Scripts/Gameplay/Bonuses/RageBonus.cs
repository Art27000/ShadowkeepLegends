/// <summary>Ярость: +2 к урону в первые 3 хода, потом -1 к урону.</summary>
public class RageBonus : BonusBase
{
    public override void OnAttack(DamageContext ctx)
    {
        ctx.Bonus += ctx.TurnNumber <= 3 ? 2 : -1;
        if (ctx.TurnNumber <= 3) ctx.Log("Active bonus: Rage");
    }
}
