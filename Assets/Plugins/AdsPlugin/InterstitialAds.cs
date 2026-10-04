using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string androidAdID = "Interstitial_Android";
    [SerializeField] string iOSAdID = "Interstitial_iOS";
    private string adID;
    private bool adLoaded = false;

    private void Start()
    {
        adID = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSAdID : androidAdID;
        StartCoroutine(LoadWhenReady());
    }

    // Loads an ad in the background. Does NOT show it — loading and showing
    // are separate concerns now. Safe to call multiple times (e.g. to preload
    // the next ad after one finishes).
    private IEnumerator LoadWhenReady()
    {
        if (!AdsInitializer.IsInitialized)
        {
            Debug.Log("Unity Ads ещё не инициализирован. Ждём...");
            yield return new WaitUntil(() => AdsInitializer.IsInitialized);
        }

        Debug.Log("Загружаем интерстициальную рекламу...");
        Advertisement.Load(adID, this);
    }

    // The ONLY method that actually shows an ad. Call this from a player
    // action (e.g. a button click), never automatically from a load routine.
    public void ShowAd()
    {
        if (adLoaded)
        {
            Debug.Log("Показ интерстициальной рекламы...");
            Advertisement.Show(adID, this);
            adLoaded = false;
        }
        else
        {
            Debug.Log("Реклама ещё не загружена. Запускаем загрузку, показ произойдёт при следующем вызове ShowAd().");
            StartCoroutine(LoadWhenReady());
        }
    }

    // Callbacks
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Реклама загружена: " + placementId);
        adLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Ошибка загрузки рекламы: {error} - {message}");
        adLoaded = false;
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ошибка показа рекламы: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Начат показ рекламы: " + placementId);
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Клик по рекламе: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Показ рекламы завершён.");
        StartCoroutine(LoadWhenReady()); // preload the next ad, doesn't auto-show
    }
}
