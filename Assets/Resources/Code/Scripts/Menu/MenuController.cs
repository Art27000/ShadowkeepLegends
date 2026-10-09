using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static CharacterClass index;

    public GameObject Choice;
    public GameObject Main;

    private void Start()
    {
        // Флаг одноразовый: прочитали и сразу сбросили, чтобы он не «залипал»
        // и не открывал выбор класса при следующих заходах в меню.
        bool restart = GameController.isRestart;
        GameController.isRestart = false;

        if (restart)
            OnStartClick(); // после рестарта сразу на выбор класса
        else
            OnBackClick();  // всегда гарантируем: Main открыт, Choice закрыт
    }

    public void OnStartClick()
    {
        Main.SetActive(false);
        Choice.SetActive(true);
    }

    public void OnBackClick()
    {
        Main.SetActive(true);
        Choice.SetActive(false);
    }

    public void OnChoiceClick(string name)
    {
        switch (name)
        {
            case "BANDIT":
                index = CharacterClass.Bandit;
                break;
            case "KNIGHT":
                index = CharacterClass.Knight;
                break;
            case "BARBARIAN":
                index = CharacterClass.Barbarian;
                break;
            default:
                Debug.LogWarning($"MenuController: unknown character class '{name}'.");
                return;
        }

        ScreenFader.Instance.FadeToScene("SampleScene");
    }

    public void OnSettingsClick()
    {
        //temporary for testing
        if (AdsManager.Instance == null || AdsManager.Instance.interstitialAds == null)
        {
            Debug.LogWarning("MenuController: AdsManager not ready yet.");
            return;
        }
        AdsManager.Instance.interstitialAds.ShowAd();
    }

    public void OnQuitClick()
    {
        //temporary scene for testing new location
        ScreenFader.Instance.FadeToScene("HomeScene");
    }
}
