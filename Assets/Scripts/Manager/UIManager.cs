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
        // ������UI�̍X�V�������s�����Ƃ��ł��܂�
        // �Ⴆ�΁A�Q�[���̏�Ԃɉ�����UI��\��/��\���ɂ���Ȃ�
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
