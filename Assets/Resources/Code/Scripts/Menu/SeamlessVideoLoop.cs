using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SeamlessVideoLoop : MonoBehaviour
{
    [SerializeField] private VideoPlayer playerA, playerB;
    [SerializeField] private RawImage imageA, imageB;
    [SerializeField] private float crossfadeDuration = 1.5f;

    private VideoPlayer active, standby;
    private RawImage activeImg, standbyImg;
    private bool fading;

    void Start()
    {
        active = playerA; standby = playerB;
        activeImg = imageA; standbyImg = imageB;
        SetAlpha(activeImg, 1); SetAlpha(standbyImg, 0);
        active.Play();
    }

    void Update()
    {
        if (!fading && active.length - active.time <= crossfadeDuration)
        {
            standby.time = 0;
            standby.Play();
            fading = true;
            StartCoroutine(Crossfade());
        }
    }

    System.Collections.IEnumerator Crossfade()
    {
        float t = 0;
        while (t < crossfadeDuration)
        {
            t += Time.deltaTime;
            float k = t / crossfadeDuration;
            SetAlpha(activeImg, 1 - k);
            SetAlpha(standbyImg, k);
            yield return null;
        }
        active.Stop();
        (active, standby) = (standby, active);
        (activeImg, standbyImg) = (standbyImg, activeImg);
        fading = false;
    }

    void SetAlpha(RawImage img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }
}