using UnityEngine;

public class DoubleDamageFromBluntBonus : IBonus
{
    public void OnDefense(CharacterBase self, CharacterBase attacker, GameController context)
    {
        if (attacker?.weapon != null && attacker.weapon.damageType == "Blunt")
        {
            int extra = attacker.weapon.getDamage(); // добавляем ещё столько же урона
            self.currentHp = Mathf.Max(self.currentHp - extra, 0);
            context.ShowLog($"{self.name} gets double damage from Blunt weapon!");
        }
    }

    public void OnAttack(CharacterBase self, CharacterBase target, GameController context) { }
    public void OnTurnStart(CharacterBase self, GameController context) { }
}

