public class EnduranceBonus : BonusBase
{
    public override void OnAcquired(CharacterBase self)
    {
        self.endurance++;
        if (GameController.Instance != null) GameController.Instance.ShowLog("Active bonus: +1 Endurance");
    }
}
