using UnityEngine;
using TMPro;

public class AnyKeyDetector : MonoBehaviour
{
    private bool isAnyKeyPressed = false;
    private TMP_Text myText;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private float blinkSpeed;
    [SerializeField] private float minAlpha;

    void Start()
    {
        // スクリプトがアタッチされているゲームオブジェクトからTMP_Textコンポーネントを取得
        myText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if(!isAnyKeyPressed)
        {
            float sinValue = Mathf.Sin(Time.time * blinkSpeed);
            myText.alpha = Mathf.Min((sinValue + 1) * (1.0f-minAlpha) / 2 + minAlpha + (1.0f - minAlpha) / 3 , 1.0f);
        }
        if (Input.anyKeyDown)
        {
            myText.alpha *= 0.8f;
        }
        if (Input.anyKey)
        {
            isAnyKeyPressed = true;
        }
        else if (isAnyKeyPressed)
        {
            sceneLoader.GameLoad();
        }
    }
}