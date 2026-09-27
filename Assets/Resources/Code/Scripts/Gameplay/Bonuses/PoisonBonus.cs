using UnityEngine;

public class PoisonBonus : IBonus
{
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context)
    {
        if (target.currentHp <= 0) return;

        if (!target.isPoisoned)
        {
            target.isPoisoned = true;
            target.poisonStacks = 1;
            context?.ShowLog($"{target.name} is poisoned!");
        }
        else
        {
            target.poisonStacks++;
            context?.ShowLog($"{target.name} gets posion buff (x{target.poisonStacks})!");
        }

        target.TempSpecialDamageModifier = target.poisonStacks;
    }

    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}

