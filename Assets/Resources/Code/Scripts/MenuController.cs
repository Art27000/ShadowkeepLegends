using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public SpriteRenderer sr;

    public static CharacterClass index;
    public GameObject Choice;
    public GameObject Main;

    public Text targetText;
    public float speed = 2f;
    public float minBrightness = 0.2f;
    public float maxBrightness = 1f;

    private Color baseColor;
    public Text targetRendererText;
    public Color colorA;
    public Color colorB;
    public float duration = 2f;

    private void Start()
    {
        baseColor = targetText.color;
        StartCoroutine(ChangeColorLoop());
        if (GameController.isRestart)
        {
            OnStartClick();
        }
    }
    public void OnStartClick()
    {
        Main.SetActive(false);
        Choice.SetActive(true);
        //targetText.gameObject.SetActive(true);
        //StartCoroutine(Blink());
        //SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
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
                index = CharacterClass.Bandit; break;
            case "KNIGHT":
                index = CharacterClass.Knight; break;
            case "BARBARIAN":
                index = CharacterClass.Barbarian; break;
        }
        StartCoroutine(FillBack());
    }
    public void OnSettingsClick()
    {
        AdsManager.Instance.interstitialAds.ShowAd();
    }

    public void OnQuitClick()
    {
        SceneManager.LoadScene("HomeScene");
    }
    IEnumerator FillBack()
    {
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            sr.color = c;
            yield return null;
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
    }

    private IEnumerator ChangeColorLoop()
    {
        while (true)
        {
            // Переход от A к B
            yield return StartCoroutine(ChangeColor(colorA, colorB, duration));
            // Переход от B к A
            yield return StartCoroutine(ChangeColor(colorB, colorA, duration));
        }
    }

    private IEnumerator ChangeColor(Color start, Color end, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            targetRendererText.color = new Color(
            Mathf.Lerp(start.r, end.r, t),
            Mathf.Lerp(start.g, end.g, t),
            Mathf.Lerp(start.b, end.b, t),
            1f /* фиксируем альфу = 1 (полностью непрозрачный)*/);
            yield return null; // ждём следующий кадр
        }
    }
}
