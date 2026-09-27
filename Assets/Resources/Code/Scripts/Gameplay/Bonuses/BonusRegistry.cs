public static class BonusRegistry
{
    public static IBonus GetBonusForClassLevel(CharacterClass cls, int level)
    {
        switch (cls)
        {
            case CharacterClass.Bandit:
                if (level == 1) return new SneakAttackBonus();
                if (level == 2) return new AgilityBonus();
                if (level == 3) return new PoisonBonus();
                break;
            case CharacterClass.Knight:
                if (level == 1) return new DoubleDamageBonus();
                if (level == 2) return new ShieldBonus();
                if (level == 3) return new StrengthBonus();
                break;
            case CharacterClass.Barbarian:
                if (level == 1) return new RageBonus();
                if (level == 2) return new StoneSkinBonus();
                if (level == 3) return new EnduranceBonus();
                break;
        }
        return null;
    }
}
