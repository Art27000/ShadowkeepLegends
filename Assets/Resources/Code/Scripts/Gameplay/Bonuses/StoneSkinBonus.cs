/// <summary>Каменная кожа: получаемый урон снижается на значение выносливости.</summary>
public class StoneSkinBonus : BonusBase
{
    public override void OnDefense(DamageContext ctx)
    {
        ctx.Reduction += ctx.Defender.endurance;
        ctx.Log("Active bonus: Stone Skin");
    }
}
