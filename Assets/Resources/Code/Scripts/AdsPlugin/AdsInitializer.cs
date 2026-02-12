using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string androidGameID = "5961444";
    [SerializeField] string iOSGameID = "5961445";
    [SerializeField] bool testMode = true; // для отладки лучше оставить true
    private string gameID;

    public static bool IsInitialized { get; private set; }

    private void Awake()
    {
        gameID = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSGameID : androidGameID;
        Advertisement.Initialize(gameID, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads инициализированы успешно.");
        IsInitialized = true;
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Ошибка инициализации Unity Ads: {error} - {message}");
        IsInitialized = false;
    }
}
