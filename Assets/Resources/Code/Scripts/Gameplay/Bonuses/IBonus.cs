using UnityEngine;

public interface IBonus
{
    // Метод, который вызывается, когда бонус должен сработать
    public void OnAttack(CharacterBase self, CharacterBase target, GameController context);

    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context);
    public void OnTurnStart(CharacterBase self, GameController context);
}
