using UnityEngine;

public class EnduranceBonus : IBonus
{
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context)
    {
        if (context != null)
        {
            self.endurance++;
            context.ShowLog("Active bonus: +1 Endurance");
        }
        else
        {
            if (context == null)
                Debug.Log("context is null");
        }
    }
}
