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
    [Header("�ڑ��ݒ�")]
    public string serverIP = "localhost";
    public int serverPort = 12345;

    [Header("�\���ݒ�")]
    public Renderer targetRenderer;  // �摜��\������I�u�W�F�N�g��Renderer
    public bool autoCreateQuad = true;  // ������Quad���쐬
    public float cameraDistance = 10f;  // �J��������̋���
    public bool adjustToScreenSize = true;  // ��ʃT�C�Y�ɍ��킹�Ē���
    public bool stayInFrontOfCamera = true;  // �J�����̑O�ʂɌŒ�

    [Header("�f�o�b�O")]
    public bool showDebugInfo = true;

    private TcpClient tcpClient;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;
    private bool shouldStop = false;

    // ���C���X���b�h�ōX�V���邽�߂̕ϐ�
    private byte[] pendingImageData;
    private bool hasNewImageData = false;
    private readonly object imageLock = new object();

    // ��ʃT�C�Y�����p
    private GameObject displayQuad;
    private Camera mainCamera;
    private float lastAspect;
    private float lastFOV;

    void Start()
    {
        // ������Quad���쐬����ꍇ
        if (autoCreateQuad && targetRenderer == null)
        {
            CreateDisplayQuad();
        }

        // Python�ڑ��J�n
        ConnectToPython();
    }

    void CreateDisplayQuad()
    {
        // �J�������擾
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera == null)
        {
            Debug.LogError("�J������������܂���ł���");
            return;
        }

        // Quad�I�u�W�F�N�g���쐬
        displayQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayQuad.name = "PersonSilhouetteDisplay";

        // Renderer���擾
        targetRenderer = displayQuad.GetComponent<Renderer>();

        // �w�i�����Ή��̃V�F�[�_�[��ݒ�
        targetRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // �����_�[�I�[�_�[��ݒ肵�čőO�ʂɕ\��
        targetRenderer.material.renderQueue = 3000;

        // �����T�C�Y�ƃ|�W�V������ݒ�
        UpdateQuadTransform();

        if (showDebugInfo)
            Debug.Log($"�\���pQuad���쐬���܂���");
    }

    void UpdateQuadTransform()
    {
        if (displayQuad == null || mainCamera == null) return;

        // �J�����̑O�ʂɔz�u
        if (stayInFrontOfCamera)
        {
            displayQuad.transform.position = mainCamera.transform.position + mainCamera.transform.forward * cameraDistance;
            displayQuad.transform.rotation = mainCamera.transform.rotation;
        }

        // ��ʑS�̂��J�o�[����T�C�Y���v�Z
        if (adjustToScreenSize)
        {
            // �J�����̎���p�ƃA�X�y�N�g�䂩��K�؂ȃT�C�Y���v�Z
            float cameraHeight = 2.0f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Quad�̃T�C�Y��ݒ�i��ʑS�̂��J�o�[�j
            displayQuad.transform.localScale = new Vector3(cameraWidth * 2.0f, cameraHeight * 2.0f, 1.0f);
            displayQuad.transform.position = new Vector3(4.5f, 0f, 0f);

            // ���݂̒l��ۑ�
            lastAspect = mainCamera.aspect;
            lastFOV = mainCamera.fieldOfView;

            if (showDebugInfo)
                Debug.Log($"Quad�T�C�Y�X�V: {cameraWidth:F2} x {cameraHeight:F2} (�A�X�y�N�g��: {mainCamera.aspect:F2})");
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

            // ��M�X���b�h���J�n
            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();

            if (showDebugInfo)
                Debug.Log("Python�ڑ�����");
        }
        catch (Exception e)
        {
            Debug.LogError($"Python�ڑ����s: {e.Message}");
            isConnected = false;
        }
    }

    void ReceiveData()
    {
        byte[] buffer = new byte[1024 * 1024 * 4]; // 4MB �o�b�t�@
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

                        // ���s�ŋ�؂�ꂽ���b�Z�[�W������
                        string allMessages = messageBuilder.ToString();
                        string[] messages = allMessages.Split('\n');

                        // �Ō�̃��b�Z�[�W�͕s���S�ȉ\��������̂ŕێ�
                        messageBuilder.Clear();
                        if (messages.Length > 0 && !allMessages.EndsWith("\n"))
                        {
                            messageBuilder.Append(messages[messages.Length - 1]);
                        }

                        // ���S�ȃ��b�Z�[�W������
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
                    Thread.Sleep(10); // CPU�g�p����}��
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"�f�[�^��M�G���[: {e.Message}");
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
                // Base64���摜�o�C�g�z��ɕϊ��i�o�b�N�O���E���h�X���b�h�Ŏ��s�\�j
                byte[] imageBytes = Convert.FromBase64String(imageData.data);

                // ���C���X���b�h�ŏ������邽�߂Ƀf�[�^��ۑ�
                lock (imageLock)
                {
                    pendingImageData = imageBytes;
                    hasNewImageData = true;
                }

                if (showDebugInfo)
                    Debug.Log($"�摜��M: {imageBytes.Length} bytes");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���b�Z�[�W�����G���[: {e.Message}");
        }
    }

    void Update()
    {
        // �J�����ݒ肪�ύX���ꂽ�ꍇ�AQuad�̃T�C�Y���X�V
        if (adjustToScreenSize && mainCamera != null && displayQuad != null)
        {
            // �A�X�y�N�g��⎋��p�̕ύX�����o
            if (Mathf.Abs(mainCamera.aspect - lastAspect) > 0.001f ||
                Mathf.Abs(mainCamera.fieldOfView - lastFOV) > 0.1f)
            {
                UpdateQuadTransform();
            }
        }

        // ���C���X���b�h�ŉ摜���X�V
        if (hasNewImageData && targetRenderer != null)
        {
            lock (imageLock)
            {
                if (pendingImageData != null)
                {
                    // �����̃e�N�X�`����j��
                    if (targetRenderer.material.mainTexture != null)
                    {
                        DestroyImmediate(targetRenderer.material.mainTexture);
                    }

                    // �V�����e�N�X�`�����쐬�i���C���X���b�h�Ŏ��s�j
                    Texture2D newTexture = new Texture2D(2, 2);
                    newTexture.LoadImage(pendingImageData);

                    // �e�N�X�`����K�p
                    targetRenderer.material.mainTexture = newTexture;

                    // �t���O�����Z�b�g
                    hasNewImageData = false;
                    pendingImageData = null;
                }
            }
        }

        // R �L�[�ōĐڑ�
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReconnectToPython();
        }

        // U �L�[��Quad�T�C�Y���蓮�X�V
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
            receiveThread.Join(1000); // 1�b�ҋ@
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
            Debug.Log("Python�ڑ���ؒf���܂���");
    }

    void OnDestroy()
    {
        DisconnectFromPython();

        // �e�N�X�`�����N���[���A�b�v
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