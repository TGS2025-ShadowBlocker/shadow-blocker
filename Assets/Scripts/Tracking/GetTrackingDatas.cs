using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/**
 * 他ファイルからGetTrackingDatas.Instance.IsKickActiveとかTrackingDatas.Instance.IsPunchActiveで取得出来る
*/

public class GetTrackingDatas : MonoBehaviour
{
    public static GetTrackingDatas Instance { get; private set; }

    private const string LocalServerUrl = "http://localhost:8000/status";

    private float requestInterval = 0.1f; // リクエストの感覚（秒）

    public bool kick = false;
    public bool punch = false;

    private void Awake()
    {
        // Singletonパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(GetLocalData());
    }

    private IEnumerator GetLocalData()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(LocalServerUrl);
            yield return request.SendWebRequest(); // yield のタイポを修正

            if (request.result == UnityWebRequest.Result.Success) // Result のタイポを修正
            {
                string json = request.downloadHandler.text;

                // JSONをServerStatusオブジェクトにデシリアライズ
                ServerStatus status = JsonConvert.DeserializeObject<ServerStatus>(json);

                // 取得した値を変数に格納
                kick = status.current_actions.kick;
                punch = status.current_actions.punch;

                Debug.Log($"Kick: {kick}, Punch: {punch}");
            }
            else
            {
                Debug.LogError($"リクエストエラー: {request.error}");
            }

            request.Dispose(); // メモリリークを防ぐ

            // 指定した間隔だけ待機
            yield return new WaitForSeconds(requestInterval);
        }
    }

    // 他のスクリプトから値を取得するためのプロパティ
    public bool IsKickActive => kick;
    public bool IsPunchActive => punch;
}

[System.Serializable]
public class CurrentActions
{
    public bool kick;
    public bool punch;
}

[System.Serializable]
public class ServerStatus
{
    public CurrentActions current_actions;
}