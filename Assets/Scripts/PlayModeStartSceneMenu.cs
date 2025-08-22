#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

// source: https://garnetcode.jp/blog/2024/06/playmode_startscene/

public static class PlayModeStartSceneMenu
{
    private const string SetMenuPath = "Assets/PlayModeStartScene/Set";
    private const string ClearMenuPath = "Assets/PlayModeStartScene/Clear";
    
    /// <summary>
    /// スタートシーンを設定する
    /// </summary>
    [MenuItem(SetMenuPath, priority = 0)]
    public static void SetPlayModeStartScene()
    {
        EditorSceneManager.playModeStartScene = Selection.activeObject as SceneAsset;
    }
    
    [MenuItem(SetMenuPath, true)]
    public static bool PlayModeStartSceneValidate()
    {
        // シーンが設定されていたらチェックを入れる
        Menu.SetChecked(SetMenuPath, EditorSceneManager.playModeStartScene != null);
        
        // シーンアセットを選択している場合、選択中のシーンを設定可能にする
        return Selection.count == 1 && Selection.activeObject is SceneAsset;
    }
    
    /// <summary>
    /// スタートシーンをクリアする
    /// </summary>
    [MenuItem(ClearMenuPath, priority = 1)]
    public static void ClearPlayModeStartScene()
    {
        EditorSceneManager.playModeStartScene = null;
    }
}
#endif