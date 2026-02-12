using UnityEngine;
using PixelBattleText;
public class TestingPixelBattleText : MonoBehaviour
{
    public TextAnimation textAnimation;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            PixelBattleTextController.DisplayText(
           "Hello World!", textAnimation, Vector3.one * 0.5f);
    }
}
