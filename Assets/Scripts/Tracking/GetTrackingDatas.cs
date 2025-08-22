using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/**
 * ���t�@�C������GetTrackingDatas.Instance.IsKickActive�Ƃ�TrackingDatas.Instance.IsPunchActive�Ŏ擾�o����
*/

public class GetTrackingDatas : MonoBehaviour
{
    public static GetTrackingDatas Instance { get; private set; }

    private const string LocalServerUrl = "http://localhost:8000/status";

    private float requestInterval = 0.1f; // ���N�G�X�g�̊��o�i�b�j

    public bool kick = false;
    public bool punch = false;

    private void Awake()
    {
        // Singleton�p�^�[���̎���
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
            yield return request.SendWebRequest(); // yield �̃^�C�|���C��

            if (request.result == UnityWebRequest.Result.Success) // Result �̃^�C�|���C��
            {
                string json = request.downloadHandler.text;

                // JSON��ServerStatus�I�u�W�F�N�g�Ƀf�V���A���C�Y
                ServerStatus status = JsonConvert.DeserializeObject<ServerStatus>(json);

                // �擾�����l��ϐ��Ɋi�[
                kick = status.current_actions.kick;
                punch = status.current_actions.punch;

                Debug.Log($"Kick: {kick}, Punch: {punch}");
            }
            else
            {
                Debug.LogError($"���N�G�X�g�G���[: {request.error}");
            }

            request.Dispose(); // ���������[�N��h��

            // �w�肵���Ԋu�����ҋ@
            yield return new WaitForSeconds(requestInterval);
        }
    }

    // ���̃X�N���v�g����l���擾���邽�߂̃v���p�e�B
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