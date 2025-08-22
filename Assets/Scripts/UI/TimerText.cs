using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerText : MonoBehaviour
{
    private float gameTime;

    private void Start()
    {
        // GameManagerからゲーム時間を取得
        gameTime = GameManager.Instance.gameTime;
    }

    private void Update()
    {
        // ゲームがアクティブな間、タイマーを更新
        if (GameManager.Instance.isGameActive)
        {
            gameTime -= Time.deltaTime;

            // タイマーが0以下になったらゲームを終了
            if (gameTime <= 0f)
            {
                gameTime = 0f;
                GameManager.Instance.isGameActive = false;
                // ここでゲーム終了の処理を追加することも可能
            }
        }

        // TextMeshProにタイマーの値を表示
        GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.Max(0f, gameTime).ToString("F2");
    }
}
