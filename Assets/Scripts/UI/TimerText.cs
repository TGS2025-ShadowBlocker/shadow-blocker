using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerText : MonoBehaviour
{
    private float gameTime;

    [Header("Prefab / Parent Settings")]
    [Tooltip("If set, will load prefab from Resources/<this path> (without file extension).")]
    public string prefabResourcePath; // ��: "UI/ResultPanel" -> Resources/UI/ResultPanel.prefab

    [Tooltip("Optional: hierarchy path of parent GameObject (use GameObject.Find). Example: 'Canvas/ResultAnchor' or just 'Canvas'. Leave empty to instantiate at world (0,0,0).")]
    public string parentHierarchyPath;

    // fallback to GameManager.Instance.resultScreen if resource not set or not found
    private GameObject resultPrefab;
    private GameObject parentObject;
    private bool hasSpawnedResult = false;

    private void Start()
    {
        // GameManager����Q�[�����Ԃ��擾
        gameTime = GameManager.Instance.gameTime;

        // ���\�[�X�p�X���w�肳��Ă���΃��[�h�����݂�
        if (!string.IsNullOrEmpty(prefabResourcePath))
        {
            resultPrefab = Resources.Load<GameObject>(prefabResourcePath);
            if (resultPrefab == null)
            {
                Debug.LogWarning($"TimerText: Resources.Load failed for path '{prefabResourcePath}'. Falling back to GameManager.resultScreen if set.");
            }
        }

        // parentHierarchyPath ������ΒT���i�V�[�����̃��[�g���猟���j
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
        // �Q�[�����A�N�e�B�u�ȊԁA�^�C�}�[���X�V
        if (GameManager.Instance.isGameActive)
        {
            gameTime -= Time.deltaTime;

            // �^�C�}�[��0�ȉ��ɂȂ�����Q�[�����I��
            if (gameTime <= 0f && !hasSpawnedResult)
            {
                gameTime = 0f;
                GameManager.Instance.isGameActive = false;
                AudioManager.Instance.Play("gameover"); // �Q�[���I�[�o�[�����Đ�
                AudioManager.Instance.StopBGM(); // BGM���~
                AudioManager.Instance.Play("result"); // ���ʉ�ʂ�BGM���Đ�
                SpawnResultPrefab();
            }
        }

        // TextMeshPro�Ƀ^�C�}�[�̒l��\��
        var text = GetComponent<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = Mathf.Max(0f, gameTime).ToString("F2");
        }
    }

    private void SpawnResultPrefab()
    {
        hasSpawnedResult = true;

        // �D�揇: Resources ���烍�[�h���� prefab -> GameManager �� resultScreen -> null
        GameObject toInstantiate = resultPrefab ?? GameManager.Instance.resultScreen;
        if (toInstantiate == null) return;

        // �e���w�肳��Ă��Ȃ��ꍇ�́A�܂��V�[������ Canvas ��T���iUI �p�j
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
            // UI �v���n�u�� Canvas �̉��ɔz�u����ꍇ�� instantiateInWorldSpace=false ���g���ă��[�J�� transform ���ێ�����
            result = Instantiate(toInstantiate, parentTransform, false);
        }
        else
        {
            // �e��������Ȃ��ꍇ�̓��[���h���_�ɐ���
            result = Instantiate(toInstantiate, Vector3.zero, Quaternion.identity);
        }
        result.GetComponent<ResultScreen>().shadow_win();
    }
}
