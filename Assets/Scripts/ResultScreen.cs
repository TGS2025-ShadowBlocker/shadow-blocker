using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private GameObject Shadow_win;
    [SerializeField] private GameObject Player_win;
    // Start is called before the first frame update
    public void shadow_win()
    {
        Shadow_win.SetActive(true);
    }
    public void player_win()
    {
        Player_win.SetActive(true);
    }

    private void Update()
    {
        if(Player_win.activeSelf&&Shadow_win.activeSelf)
        {
            Player_win.SetActive(false);
            //タイムアップした後に速度を0にするまでの0.数フレームの間にゴールすることがあるため。
        }
    }
}
