using UnityEngine;

public class StoneSkinBonus : IBonus
{
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context)
    {
        if (context != null)
        {
            self.TempDamageModifier = self.endurance;
            context.ShowLog("Active bonus: Stone Skin");
        }
        else
        {
            if (context == null)
                Debug.Log("context is null");
        }
    }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}
