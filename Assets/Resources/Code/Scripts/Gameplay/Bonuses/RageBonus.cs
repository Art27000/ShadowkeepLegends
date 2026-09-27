using UnityEngine;

public class RageBonus : IBonus
{
    private int temp = 0;
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context)
    {
        if (context != null && temp < 3)
        {
            self.TempDamageModifier = 2;
            context.ShowLog("Active bonus: Rage");
            temp++;
        }
        else if (context != null)
        {
            self.TempDamageModifier = -1;
        }
        else
        {
            if (context == null)
                Debug.Log("context is null");
            if (temp < 3)
                Debug.Log("HeroTurnCount <= 3");
        }

    }

    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}

