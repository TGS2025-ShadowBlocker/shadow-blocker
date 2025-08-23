using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject goalUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    public void TitleLoad()
    {
        SceneManager.LoadScene("title");
        GameManager.Instance.isGameActive = false;
    }

    public void GameLoad()
    {
        SceneManager.LoadScene("game");
        print("game");
        GameManager.Instance.isGameActive = true;
    }
}
