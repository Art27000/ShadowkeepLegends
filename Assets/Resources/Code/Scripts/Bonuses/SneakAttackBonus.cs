using UnityEngine;

public class SneakAttackBonus : IBonus
{
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context)
    {
        if (self.agility > target.agility)
        {
            self.TempDamageModifier = 1;
            context.ShowLog("Active bonus: Sneak Attack");
        }

    }

    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}

