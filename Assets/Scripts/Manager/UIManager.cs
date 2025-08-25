using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static GameObject resultScreen;

    private void Start()
    {

    }

    private void Update()
    {
        // ここでUIの更新処理を行うことができます
        // 例えば、ゲームの状態に応じてUIを表示/非表示にするなど
        if (GameManager.Instance.isGameActive)
        {
            //resultScreen.SetActive(false);
        }
        else
        {
            //resultScreen.SetActive(true);
        }
    }
}
