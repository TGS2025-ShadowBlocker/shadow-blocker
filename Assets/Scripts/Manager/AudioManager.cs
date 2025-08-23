using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager Instance;

    // 現在再生中のBGM名を保持
    private string currentBGMName;

    private void Start()
    {
        // デバッグ用
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

    // シーン読み込み時にBGMを切り替える
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
        // 購読解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    /// <summary>
    /// シーン名と同じ名前のBGMを再生する。
    /// 既に同名のBGMが再生中であれば何もしない。
    /// 他のBGMは停止する。
    /// </summary>
    /// <param name="sceneName"></param>
    public void PlayBGMForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // 対象のBGMを探す
        Sound target = Array.Find(sounds, s => s.type == Sound.audioType.BGM && s.name == sceneName);

        if (target == null)
        {
            Debug.Log("No BGM found for scene: " + sceneName);
            // シーンに対応するBGMが無ければ現在再生中のBGMを停止する
            if (!string.IsNullOrEmpty(currentBGMName))
            {
                // 現在保持しているBGMを探して停止
                Sound playing = Array.Find(sounds, s => s.name == currentBGMName && s.type == Sound.audioType.BGM);
                if (playing != null && playing.source != null && playing.source.isPlaying)
                {
                    Stop(playing.name);
                }
                currentBGMName = null;
            }
            return;
        }

        // 既に同じBGMが再生中なら何もしない
        if (!string.IsNullOrEmpty(currentBGMName) && currentBGMName == target.name)
        {
            if (target.source != null && target.source.isPlaying) return;
        }

        // 他のBGMを停止（ただし保存フラグが立っているものは停止しても再生位置を保持する挙動は既存の Stop() に従う）
        foreach (Sound s in sounds)
        {
            if (s.type == Sound.audioType.BGM && s.name != target.name)
            {
                if (s.source != null && s.source.isPlaying)
                {
                    // 再生位置保存が有効なら Stop() が保存処理を行う
                    Stop(s.name);
                }
            }
        }

        // 対象を再生
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

        // 再生位置を保存
        if (s.isSaved)
        {
            s.savedTime = s.source.time;
        }

        s.source.Stop();
    }

    // 全てのAudioの再生を止める
    public void StopAll()
    {
        foreach (Sound s in sounds)
        {
            Stop(s.name);
        }
    }

    // BGMを停止する
    public void StopBGM()
    {
        foreach (Sound s in sounds)
        {
            if (s.type == Sound.audioType.BGM)
            {
                Stop(s.name);
            }
        }
        currentBGMName = null; // BGM停止時に現在のBGM名をクリア
    }

    // SEを停止する
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

    [Header("再生位置を記録")]
    public bool isSaved;
    public float savedTime;
}