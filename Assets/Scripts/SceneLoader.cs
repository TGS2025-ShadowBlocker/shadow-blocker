using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject goalUI;


    public void titleLoad()
    {
        SceneManager.LoadScene("title");
        GameManager.Instance.isGameActive = false;
    }

    public void gameLoad()
    {
        //SceneManager.LoadScene("game");
        player.transform.position = new Vector3(-7.0f,-3.5f,0.0f);
        goalUI.SetActive(false);
        GameManager.Instance.isGameActive = true;
    }
}
