using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAds interstitialAds;

    public static AdsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
