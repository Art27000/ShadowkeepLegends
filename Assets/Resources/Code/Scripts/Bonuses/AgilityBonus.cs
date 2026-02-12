using UnityEngine;

public class AgilityBonus : IBonus
{
    private int temp = 0;
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context)
    {
        if (context != null && temp < 1)
        {
            self.agility++;
            context.ShowLog("Active bonus: +1 Agility");
            temp++;
        }
        else
        {
            if (context == null)
                Debug.Log("context is null");
        }
    }
}
