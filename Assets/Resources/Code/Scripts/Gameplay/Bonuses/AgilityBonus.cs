public class AgilityBonus : BonusBase
{
    public override void OnAcquired(CharacterBase self)
    {
        self.agility++;
        if (GameController.Instance != null) GameController.Instance.ShowLog("Active bonus: +1 Agility");
    }
}
