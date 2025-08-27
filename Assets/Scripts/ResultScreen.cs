using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private GameObject Shadow_win;
    [SerializeField] private TMP_Text Shadow_score;
    [SerializeField] private GameObject Player_win;
    [SerializeField] private TMP_Text Player_score;
    [SerializeField] private SceneLoader Restart;
    [SerializeField] private SceneLoader title;
    [SerializeField] private GameObject[] yajirusi;
    private bool pressed = true;
    private int kaisuu = 0;
    // Start is called before the first frame update
    public void shadow_win()
    {
        Shadow_win.SetActive(true);
        Shadow_score.text = "score:" + GameManager.Instance.shadowScore;
    }
    public void player_win()
    {
        Player_win.SetActive(true);
        Player_score.text = "score:" + GameManager.Instance.playerScore;
    }

    private void Update()
    {
        if(Player_win.activeSelf&&Shadow_win.activeSelf)
        {
            Player_win.SetActive(false);
            //タイムアップした後に速度を0にするまでの0.数フレームの間にゴールすることがあるため。
        }
        if(Input.GetKey(KeyCode.JoystickButton0))
        {
            Restart.GameLoad();
        }
        if (Input.GetKey(KeyCode.JoystickButton1))
        {
            Restart.TitleLoad();
        }
        /*foreach (GameObject y in yajirusi)
        {
            y.SetActive(false);
        }
        if(!(Mathf.Abs(Input.GetAxisRaw("Vertical"))<0.2f) && pressed)
        {
            kaisuu += (int)(Mathf.Abs(Input.GetAxisRaw("Vertical")) / Input.GetAxisRaw("Vertical"));
            pressed = false;
        }
        else
        {
            pressed = true;
        }
            yajirusi[kaisuu % yajirusi.Length - 1].SetActive(true);*/
    }
}
