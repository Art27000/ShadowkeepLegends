/// <summary>Каждый 3-й ход дышит огнём: +3 урона.</summary>
public class FireBreathBonus : BonusBase
{
    public override void OnAttack(DamageContext ctx)
    {
        if (ctx.TurnNumber % 3 == 0)
        {
            ctx.Bonus += 3;
            ctx.IsSpecialAttack = true;
        }
    }
}
