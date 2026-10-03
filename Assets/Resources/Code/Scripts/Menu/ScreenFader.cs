using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent, reusable fade-to-black overlay for scene transitions.
///
/// Lives on a full-screen UI Image + CanvasGroup, one level under its own Canvas
/// (Render Mode: Screen Space - Overlay, high Sort Order so it always draws above
/// everything else, in every scene). Created once in the Init scene and carried
/// forward via DontDestroyOnLoad, exactly like AdsManager.
///
/// Any script anywhere can call:
///     ScreenFader.Instance.FadeToScene("SampleScene");
/// or, for finer control:
///     ScreenFader.Instance.FadeOut(() => { ... });
///     ScreenFader.Instance.FadeIn();
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private float defaultDuration = 1f;

    private CanvasGroup canvasGroup;
    private Coroutine running;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>Fades to opaque, loads a scene asynchronously, then fades back in.</summary>
    public void FadeToScene(string sceneName, float? duration = null)
    {
        FadeOut(() =>
        {
            StartCoroutine(LoadSceneAndFadeIn(sceneName, duration ?? defaultDuration));
        }, duration);
    }

    public void FadeOut(Action onComplete = null, float? duration = null)
    {
        Restart(Fade(0f, 1f, duration ?? defaultDuration, onComplete));
    }

    public void FadeIn(Action onComplete = null, float? duration = null)
    {
        Restart(Fade(1f, 0f, duration ?? defaultDuration, onComplete));
    }

    private IEnumerator LoadSceneAndFadeIn(string sceneName, float duration)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (op != null && !op.isDone)
        {
            yield return null;
        }
        FadeIn(null, duration);
    }

    private void Restart(IEnumerator routine)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(routine);
    }

    private IEnumerator Fade(float from, float to, float duration, Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        float t = 0f;
        canvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;

        bool stillBlocking = to >= 1f; // stay opaque+blocking if we ended fully faded out
        canvasGroup.blocksRaycasts = stillBlocking;
        running = null;

        onComplete?.Invoke();
    }
}
