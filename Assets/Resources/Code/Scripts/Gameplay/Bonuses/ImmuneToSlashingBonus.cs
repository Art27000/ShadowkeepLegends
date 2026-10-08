/// <summary>Слайм: рубящее оружие не наносит урона (урон от силы и эффектов остаётся).</summary>
public class ImmuneToSlashingBonus : BonusBase
{
    public override void OnDefense(DamageContext ctx)
    {
        if (ctx.DamageType == DamageType.Slashing)
        {
            ctx.WeaponDamage = 0;
            ctx.Log($"{ctx.Defender.name} has immunity to slashing weapons!");
        }
    }
}
