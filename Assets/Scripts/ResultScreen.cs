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
    [SerializeField] private ScoreCounter scoreCounter;
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
            //�^�C���A�b�v������ɑ��x��0�ɂ���܂ł�0.���t���[���̊ԂɃS�[�����邱�Ƃ����邽�߁B
        }
    }
}
