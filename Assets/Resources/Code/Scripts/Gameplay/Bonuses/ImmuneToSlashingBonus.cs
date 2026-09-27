using UnityEngine;

public class ImmuneToSlashingBonus : IBonus
{
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context)
    {
        if (attacker?.weapon != null && attacker.weapon.damageType == "Slashing")
        {
            context?.ShowLog($"{self.name} has immunity to slashing weapons!");
            self.TempDamageModifier = attacker.weapon.getDamage(); // отменяем урон от оружия
        }
    }

    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}
