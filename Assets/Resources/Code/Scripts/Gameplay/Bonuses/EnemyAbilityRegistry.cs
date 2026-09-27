using System.Collections.Generic;

public static class EnemyAbilityRegistry
{
    public static List<IBonus> GetAbilitiesForEnemy(string enemyName)
    {
        var abilities = new List<IBonus>();

        switch (enemyName)
        {
            case "Skeleton":
                abilities.Add(new DoubleDamageFromBluntBonus()); // у€звимость к дроб€щему
                break;

            case "Slime":
                abilities.Add(new ImmuneToSlashingBonus()); // иммунитет к руб€щему
                break;

            case "Ghost":
                abilities.Add(new SneakAttackBonus()); // скрыта€ атака
                break;

            case "Golem":
                abilities.Add(new StoneSkinBonus()); // каменна€ кожа
                break;

            case "Dragon":
                abilities.Add(new FireBreathBonus()); // дыхание огнЄм
                break;
        }

        return abilities;
    }
}
