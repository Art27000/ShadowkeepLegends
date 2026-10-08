/// <summary>Щит: -3 к получаемому урону, если сила персонажа выше силы атакующего.</summary>
public class ShieldBonus : BonusBase
{
    public override void OnDefense(DamageContext ctx)
    {
        if (ctx.Defender.strength > ctx.Attacker.strength)
        {
            ctx.Reduction += 3;
            ctx.Log("Active bonus: Shield");
        }
    }
}
