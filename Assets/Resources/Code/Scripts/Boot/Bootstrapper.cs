using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lives in the Init scene (build index 0). Waits for AdsInitializer to finish
/// (with a timeout so a slow/offline network never blocks the app forever),
/// then asynchronously loads the Menu scene.
///
/// The AdsManager GameObject (which carries AdsManager + AdsInitializer +
/// InterstitialAds) should be moved from Menu.unity into Init.unity. Its own
/// Awake() already calls DontDestroyOnLoad, so it will survive into Menu and
/// every scene after that without needing to be re-created.
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Menu";
    [SerializeField] private float maxAdsInitWait = 5f;
    [SerializeField] private float minSplashTime = 1f;

    private IEnumerator Start()
    {
        float startTime = Time.unscaledTime;
        float waited = 0f;

        while (!AdsInitializer.IsInitialized && waited < maxAdsInitWait)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!AdsInitializer.IsInitialized)
        {
            Debug.LogWarning("Bootstrapper: Ads did not initialize within " +
                              maxAdsInitWait + "s, continuing without waiting further.");
        }

        // Keep the splash on screen for at least minSplashTime, even if init was instant,
        // so it doesn't flash by for one frame on a fast device/network.
        float elapsed = Time.unscaledTime - startTime;
        if (elapsed < minSplashTime)
        {
            yield return new WaitForSecondsRealtime(minSplashTime - elapsed);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        while (op != null && !op.isDone)
        {
            yield return null;
        }
    }
}
