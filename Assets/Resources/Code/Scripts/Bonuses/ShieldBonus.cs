using UnityEngine;

public class ShieldBonus : IBonus
{
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context)
    {
        if (context != null && (self.strength > attacker.strength))
        {
            self.TempDamageModifier = 3;
            context.ShowLog("Active bonus: Shield");
        }
        else if(context != null)
        {
            self.TempDamageModifier = 0;
        }
        else
        {
            if (context == null)
                Debug.Log("context is null");
        }
    }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}
