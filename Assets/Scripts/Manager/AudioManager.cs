using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager Instance;

    // ���ݍĐ�����BGM����ێ�
    private string currentBGMName;

    private void Start()
    {
        // �f�o�b�O�p
        // Play("jump");
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

    // �V�[���ǂݍ��ݎ���BGM��؂�ւ���
    SceneManager.sceneLoaded += OnSceneLoaded;

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void OnDestroy()
    {
        // �w�ǉ���
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    /// <summary>
    /// �V�[�����Ɠ������O��BGM���Đ�����B
    /// ���ɓ�����BGM���Đ����ł���Ή������Ȃ��B
    /// ����BGM�͒�~����B
    /// </summary>
    /// <param name="sceneName"></param>
    public void PlayBGMForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // �Ώۂ�BGM��T��
        Sound target = Array.Find(sounds, s => s.type == Sound.audioType.BGM && s.name == sceneName);

        if (target == null)
        {
            Debug.Log("No BGM found for scene: " + sceneName);
            // �V�[���ɑΉ�����BGM��������Ό��ݍĐ�����BGM���~����
            if (!string.IsNullOrEmpty(currentBGMName))
            {
                // ���ݕێ����Ă���BGM��T���Ē�~
                Sound playing = Array.Find(sounds, s => s.name == currentBGMName && s.type == Sound.audioType.BGM);
                if (playing != null && playing.source != null && playing.source.isPlaying)
                {
                    Stop(playing.name);
                }
                currentBGMName = null;
            }
            return;
        }

        // ���ɓ���BGM���Đ����Ȃ牽�����Ȃ�
        if (!string.IsNullOrEmpty(currentBGMName) && currentBGMName == target.name)
        {
            if (target.source != null && target.source.isPlaying) return;
        }

        // ����BGM���~�i�������ۑ��t���O�������Ă�����̂͒�~���Ă��Đ��ʒu��ێ����鋓���͊����� Stop() �ɏ]���j
        foreach (Sound s in sounds)
        {
            if (s.type == Sound.audioType.BGM && s.name != target.name)
            {
                if (s.source != null && s.source.isPlaying)
                {
                    // �Đ��ʒu�ۑ����L���Ȃ� Stop() ���ۑ��������s��
                    Stop(s.name);
                }
            }
        }

        // �Ώۂ��Đ�
        Play(target.name);
        currentBGMName = target.name;
    }

    public void ToggleSavePosition(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " is not found!");
            return;
        }

        s.isSaved = !s.isSaved;
        Debug.Log("Save position for sound " + name + ": " + s.isSaved);
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " is not found!");
            return;
        }

        if (s.isSaved && s.savedTime > 0f && s.source.time != s.savedTime)
        {
            s.source.time = s.savedTime;
        }

        print("Playing sound: " + name + " at time: " + s.source.time + " (saved: " + s.isSaved + ", savedTime: " + s.savedTime + ")");
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " is not found!");
            return;
        }

        // �Đ��ʒu��ۑ�
        if (s.isSaved)
        {
            s.savedTime = s.source.time;
        }

        s.source.Stop();
    }

    // �S�Ă�Audio�̍Đ����~�߂�
    public void StopAll()
    {
        foreach (Sound s in sounds)
        {
            Stop(s.name);
        }
    }

    // BGM���~����
    public void StopBGM()
    {
        foreach (Sound s in sounds)
        {
            if (s.type == Sound.audioType.BGM)
            {
                Stop(s.name);
            }
        }
        currentBGMName = null; // BGM��~���Ɍ��݂�BGM�����N���A
    }

    // SE���~����
    public void StopSE()
    {
        foreach (Sound s in sounds)
        {
            if (s.type == Sound.audioType.SE)
            {
                Stop(s.name);
            }
        }
    }
}

// Sound class
[System.Serializable]
public class Sound
{
    public string name;
    public enum audioType
    {
        BGM,
        SE
    }
    public audioType type;
    public AudioClip clip;

    [Range(0, 1f)]
    public float volume;
    [Range(0, 3f)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;

    public bool loop;

    [Header("�Đ��ʒu���L�^")]
    public bool isSaved;
    public float savedTime;
}