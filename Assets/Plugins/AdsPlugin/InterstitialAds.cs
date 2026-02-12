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
        LoadAd();
    }

    void LoadAd()
    {
        if (AdsInitializer.IsInitialized)
        {
            Debug.Log("Загрузка межстраничной рекламы...");
            Advertisement.Load(adID, this);
        }
    }
    IEnumerator TryLoadAd()
    {
        if (AdsInitializer.IsInitialized)
        {
            Debug.Log("Загрузка межстраничной рекламы...");
            Advertisement.Load(adID, this);

            // Ждём пока реклама загрузится
            yield return new WaitUntil(() => adLoaded);

            // Когда загрузилась — показываем
            ShowAd();
        }
        else
        {
            Debug.Log("Unity Ads ещё не инициализированы. Ждём...");
            // Ждём пока инициализация завершится
            yield return new WaitUntil(() => AdsInitializer.IsInitialized);
            // После инициализации пробуем снова
            yield return StartCoroutine(TryLoadAd());
        }
    }

    public void ShowAd()
    {
        if (adLoaded)
        {
            Debug.Log("Показ межстраничной рекламы...");
            Advertisement.Show(adID, this);
            adLoaded = false;
        }
        else
        {
            Debug.Log("Реклама ещё не загружена. Попробуем загрузить снова.");
            StartCoroutine(TryLoadAd());
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
        Debug.Log("Старт показа рекламы: " + placementId);

    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Клик по рекламе: " + placementId);

    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Реклама завершена.");
        TryLoadAd(); // сразу готовим следующую
    }
}
