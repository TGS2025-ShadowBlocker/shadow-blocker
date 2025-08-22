using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class ImageData
{
    public string type;
    public string data;
    public float timestamp;
}

public class PersonSilhouetteReceiver : MonoBehaviour
{
    [Header("接続設定")]
    public string serverIP = "localhost";
    public int serverPort = 12345;

    [Header("表示設定")]
    public Renderer targetRenderer;  // 画像を表示するオブジェクトのRenderer
    public bool autoCreateQuad = true;  // 自動でQuadを作成
    public float cameraDistance = 10f;  // カメラからの距離
    public bool adjustToScreenSize = true;  // 画面サイズに合わせて調整
    public bool stayInFrontOfCamera = true;  // カメラの前面に固定

    [Header("デバッグ")]
    public bool showDebugInfo = true;

    private TcpClient tcpClient;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;
    private bool shouldStop = false;

    // メインスレッドで更新するための変数
    private byte[] pendingImageData;
    private bool hasNewImageData = false;
    private readonly object imageLock = new object();

    // 画面サイズ調整用
    private GameObject displayQuad;
    private Camera mainCamera;
    private float lastAspect;
    private float lastFOV;

    void Start()
    {
        // 自動でQuadを作成する場合
        if (autoCreateQuad && targetRenderer == null)
        {
            CreateDisplayQuad();
        }

        // Python接続開始
        ConnectToPython();
    }

    void CreateDisplayQuad()
    {
        // カメラを取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera == null)
        {
            Debug.LogError("カメラが見つかりませんでした");
            return;
        }

        // Quadオブジェクトを作成
        displayQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayQuad.name = "PersonSilhouetteDisplay";

        // Rendererを取得
        targetRenderer = displayQuad.GetComponent<Renderer>();

        // 背景透明対応のシェーダーを設定
        targetRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // レンダーオーダーを設定して最前面に表示
        targetRenderer.material.renderQueue = 3000;

        // 初期サイズとポジションを設定
        UpdateQuadTransform();

        if (showDebugInfo)
            Debug.Log($"表示用Quadを作成しました");
    }

    void UpdateQuadTransform()
    {
        if (displayQuad == null || mainCamera == null) return;

        // カメラの前面に配置
        if (stayInFrontOfCamera)
        {
            displayQuad.transform.position = mainCamera.transform.position + mainCamera.transform.forward * cameraDistance;
            displayQuad.transform.rotation = mainCamera.transform.rotation;
        }

        // 画面全体をカバーするサイズを計算
        if (adjustToScreenSize)
        {
            // カメラの視野角とアスペクト比から適切なサイズを計算
            float cameraHeight = 2.0f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Quadのサイズを設定（画面全体をカバー）
            displayQuad.transform.localScale = new Vector3(cameraWidth * 2.0f, cameraHeight * 2.0f, 1.0f);
            displayQuad.transform.position = new Vector3(4.5f, 0f, 0f);

            // 現在の値を保存
            lastAspect = mainCamera.aspect;
            lastFOV = mainCamera.fieldOfView;

            if (showDebugInfo)
                Debug.Log($"Quadサイズ更新: {cameraWidth:F2} x {cameraHeight:F2} (アスペクト比: {mainCamera.aspect:F2})");
        }
    }

    void ConnectToPython()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIP, serverPort);
            stream = tcpClient.GetStream();
            isConnected = true;

            // 受信スレッドを開始
            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();

            if (showDebugInfo)
                Debug.Log("Python接続成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"Python接続失敗: {e.Message}");
            isConnected = false;
        }
    }

    void ReceiveData()
    {
        byte[] buffer = new byte[1024 * 1024 * 4]; // 4MB バッファ
        StringBuilder messageBuilder = new StringBuilder();

        while (isConnected && !shouldStop)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(receivedData);

                        // 改行で区切られたメッセージを処理
                        string allMessages = messageBuilder.ToString();
                        string[] messages = allMessages.Split('\n');

                        // 最後のメッセージは不完全な可能性があるので保持
                        messageBuilder.Clear();
                        if (messages.Length > 0 && !allMessages.EndsWith("\n"))
                        {
                            messageBuilder.Append(messages[messages.Length - 1]);
                        }

                        // 完全なメッセージを処理
                        int endIndex = allMessages.EndsWith("\n") ? messages.Length : messages.Length - 1;
                        for (int i = 0; i < endIndex; i++)
                        {
                            if (!string.IsNullOrEmpty(messages[i]))
                            {
                                ProcessMessage(messages[i]);
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10); // CPU使用率を抑制
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"データ受信エラー: {e.Message}");
                break;
            }
        }
    }

    void ProcessMessage(string message)
    {
        try
        {
            ImageData imageData = JsonConvert.DeserializeObject<ImageData>(message);

            if (imageData.type == "image")
            {
                // Base64を画像バイト配列に変換（バックグラウンドスレッドで実行可能）
                byte[] imageBytes = Convert.FromBase64String(imageData.data);

                // メインスレッドで処理するためにデータを保存
                lock (imageLock)
                {
                    pendingImageData = imageBytes;
                    hasNewImageData = true;
                }

                if (showDebugInfo)
                    Debug.Log($"画像受信: {imageBytes.Length} bytes");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"メッセージ処理エラー: {e.Message}");
        }
    }

    void Update()
    {
        // カメラ設定が変更された場合、Quadのサイズを更新
        if (adjustToScreenSize && mainCamera != null && displayQuad != null)
        {
            // アスペクト比や視野角の変更を検出
            if (Mathf.Abs(mainCamera.aspect - lastAspect) > 0.001f ||
                Mathf.Abs(mainCamera.fieldOfView - lastFOV) > 0.1f)
            {
                UpdateQuadTransform();
            }
        }

        // メインスレッドで画像を更新
        if (hasNewImageData && targetRenderer != null)
        {
            lock (imageLock)
            {
                if (pendingImageData != null)
                {
                    // 既存のテクスチャを破棄
                    if (targetRenderer.material.mainTexture != null)
                    {
                        DestroyImmediate(targetRenderer.material.mainTexture);
                    }

                    // 新しいテクスチャを作成（メインスレッドで実行）
                    Texture2D newTexture = new Texture2D(2, 2);
                    newTexture.LoadImage(pendingImageData);

                    // テクスチャを適用
                    targetRenderer.material.mainTexture = newTexture;

                    // フラグをリセット
                    hasNewImageData = false;
                    pendingImageData = null;
                }
            }
        }

        // R キーで再接続
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReconnectToPython();
        }

        // U キーでQuadサイズを手動更新
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateQuadTransform();
        }
    }

    void ReconnectToPython()
    {
        DisconnectFromPython();
        ConnectToPython();
    }

    void DisconnectFromPython()
    {
        shouldStop = true;
        isConnected = false;

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(1000); // 1秒待機
        }

        if (stream != null)
        {
            stream.Close();
        }

        if (tcpClient != null)
        {
            tcpClient.Close();
        }

        if (showDebugInfo)
            Debug.Log("Python接続を切断しました");
    }

    void OnDestroy()
    {
        DisconnectFromPython();

        // テクスチャをクリーンアップ
        if (targetRenderer != null && targetRenderer.material.mainTexture != null)
        {
            DestroyImmediate(targetRenderer.material.mainTexture);
        }
    }

    void OnApplicationQuit()
    {
        DisconnectFromPython();
    }
}