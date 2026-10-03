using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static CharacterClass index;

    public GameObject Choice;
    public GameObject Main;

    private void Start()
    {
        if (GameController.isRestart)
        {
            OnStartClick();
        }
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
