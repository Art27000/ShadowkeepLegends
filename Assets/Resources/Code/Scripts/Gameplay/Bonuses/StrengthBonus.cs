public class StrengthBonus : BonusBase
{
    public override void OnAcquired(CharacterBase self)
    {
        self.strength++;
        if (GameController.Instance != null) GameController.Instance.ShowLog("Active bonus: +1 Strength");
    }
}
