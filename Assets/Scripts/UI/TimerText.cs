using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerText : MonoBehaviour
{
    private float gameTime;

    private void Start()
    {
        // GameManager����Q�[�����Ԃ��擾
        gameTime = GameManager.Instance.gameTime;
    }

    private void Update()
    {
        // �Q�[�����A�N�e�B�u�ȊԁA�^�C�}�[���X�V
        if (GameManager.Instance.isGameActive)
        {
            gameTime -= Time.deltaTime;

            // �^�C�}�[��0�ȉ��ɂȂ�����Q�[�����I��
            if (gameTime <= 0f)
            {
                gameTime = 0f;
                GameManager.Instance.isGameActive = false;
                // �����ŃQ�[���I���̏�����ǉ����邱�Ƃ��\
            }
        }

        // TextMeshPro�Ƀ^�C�}�[�̒l��\��
        GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.Max(0f, gameTime).ToString("F2");
    }
}
