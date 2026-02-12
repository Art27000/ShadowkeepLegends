using Unity.VisualScripting;
using UnityEngine;

public class FireBreathBonus : IBonus
{
    private int turnCounter = 0;

    public void OnTurnStart(CharacterBase self, GameController context) { }
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context)
    {
        turnCounter++;
        if (turnCounter % 3 == 0)
        {
            if (target != null)
            {
                self.TempDamageModifier = 3;
                self.tempBool = true;
            }
        }
    }
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
}
