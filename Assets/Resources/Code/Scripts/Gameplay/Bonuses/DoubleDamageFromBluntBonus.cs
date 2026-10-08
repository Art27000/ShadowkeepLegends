/// <summary>Скелет: получает вдвое больше урона от дробящего оружия.</summary>
public class DoubleDamageFromBluntBonus : BonusBase
{
    public override void OnDefense(DamageContext ctx)
    {
        if (ctx.DamageType == DamageType.Blunt)
        {
            ctx.Multiplier *= 2;
            ctx.Log($"{ctx.Defender.name} gets double damage from Blunt weapon!");
        }
    }
}
