/// <summary>Яд: +1 урона на втором ходу, +2 на третьем и так далее (по ТЗ).</summary>
public class PoisonBonus : BonusBase
{
    public override void OnAttack(DamageContext ctx)
    {
        if (ctx.TurnNumber >= 2)
        {
            ctx.Bonus += ctx.TurnNumber - 1;
            ctx.Log("Active bonus: Poison");
        }
    }
}
