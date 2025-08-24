using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerText : MonoBehaviour
{
    private float gameTime;

    [Header("Prefab / Parent Settings")]
    [Tooltip("If set, will load prefab from Resources/<this path> (without file extension).")]
    public string prefabResourcePath; // 例: "UI/ResultPanel" -> Resources/UI/ResultPanel.prefab

    [Tooltip("Optional: hierarchy path of parent GameObject (use GameObject.Find). Example: 'Canvas/ResultAnchor' or just 'Canvas'. Leave empty to instantiate at world (0,0,0).")]
    public string parentHierarchyPath;

    // fallback to GameManager.Instance.resultScreen if resource not set or not found
    private GameObject resultPrefab;
    private GameObject parentObject;
    private bool hasSpawnedResult = false;

    private void Start()
    {
        // GameManagerからゲーム時間を取得
        gameTime = GameManager.Instance.gameTime;

        // リソースパスが指定されていればロードを試みる
        if (!string.IsNullOrEmpty(prefabResourcePath))
        {
            resultPrefab = Resources.Load<GameObject>(prefabResourcePath);
            if (resultPrefab == null)
            {
                Debug.LogWarning($"TimerText: Resources.Load failed for path '{prefabResourcePath}'. Falling back to GameManager.resultScreen if set.");
            }
        }

        // parentHierarchyPath があれば探す（シーン内のルートから検索）
        if (!string.IsNullOrEmpty(parentHierarchyPath))
        {
            parentObject = GameObject.Find(parentHierarchyPath);
            if (parentObject == null)
            {
                Debug.LogWarning($"TimerText: parent not found at hierarchy path '{parentHierarchyPath}'. Will instantiate without parent.");
            }
        }
    }

    private void Update()
    {
        // ゲームがアクティブな間、タイマーを更新
        if (GameManager.Instance.isGameActive)
        {
            gameTime -= Time.deltaTime;

            // タイマーが0以下になったらゲームを終了
            if (gameTime <= 0f && !hasSpawnedResult)
            {
                gameTime = 0f;
                GameManager.Instance.isGameActive = false;
                AudioManager.Instance.Play("gameover"); // ゲームオーバー音を再生
                AudioManager.Instance.StopBGM(); // BGMを停止
                AudioManager.Instance.Play("result"); // 結果画面のBGMを再生
                SpawnResultPrefab();
            }
        }

        // TextMeshProにタイマーの値を表示
        var text = GetComponent<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = Mathf.Max(0f, gameTime).ToString("F2");
        }
    }

    private void SpawnResultPrefab()
    {
        hasSpawnedResult = true;

        // 優先順: Resources からロードした prefab -> GameManager の resultScreen -> null
        GameObject toInstantiate = resultPrefab ?? GameManager.Instance.resultScreen;
        if (toInstantiate == null) return;

        // 親が指定されていない場合は、まずシーン内の Canvas を探す（UI 用）
        Transform parentTransform = parentObject != null ? parentObject.transform : null;
        if (parentTransform == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                parentTransform = canvas.transform;
            }
        }

        GameObject result;
        if (parentTransform != null)
        {
            // UI プレハブを Canvas の下に配置する場合は instantiateInWorldSpace=false を使ってローカル transform を維持する
            result = Instantiate(toInstantiate, parentTransform, false);
        }
        else
        {
            // 親が見つからない場合はワールド原点に生成
            result = Instantiate(toInstantiate, Vector3.zero, Quaternion.identity);
        }
        result.GetComponent<ResultScreen>().shadow_win();
    }
}
