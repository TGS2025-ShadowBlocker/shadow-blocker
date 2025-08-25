using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] goals;
    [SerializeField] private GameObject goal;
    [SerializeField] private float camearaStart;
    [SerializeField] private float magnification;

    void Update()
    {
        foreach(GameObject g in goals)
        {
            if(g.activeInHierarchy)
            {
                goal = g;
            }
        }
        //�����̐���͌�X
        if (player.GetComponent<PlayerController>() != null)
        {
            //�S�[���̏����Agoal�̃I�u�W�F�N�g��0�J������0���S�ɔz�u����Ă����瓮���Ă�
            if (player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x > camearaStart && //�X�^�[�g���̒[�ŃJ�����~�߂����
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x && ////�S�[���̒[�ŃJ�����~�߂����
                player.transform.position.x - transform.position.x > -1 * magnification * player.GetComponent<PlayerController>().startPosition.x) //�J���������������������
            {
                transform.position = new Vector3(player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
            if (player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x > camearaStart &&
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x &&
                player.transform.position.x - transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x)//��ɓ�������B��͑O�ɓ�������B�����v�����񂩂���
            {
                transform.position = new Vector3(player.transform.position.x + magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
            //camera�̍��W����ʐ؂�ւ��̐����ɂ��邽�߂̂�B����Ȃ�����camera�̍��W��14.02131�Ƃ��Ȃ��Ă����傢
            if (transform.position.x % 2 * magnification * player.GetComponent<PlayerController>().startPosition.x != 0)
            {
                transform.position = new Vector3((transform.position.x / (2 * magnification * player.GetComponent<PlayerController>().startPosition.x) - ((transform.position.x / (2 * magnification * player.GetComponent<PlayerController>().startPosition.x)) % 1)) * 2 * magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
        }
        else
        {
            //�S�[���̏����Agoal�̃I�u�W�F�N�g��0�J������0���S�ɔz�u����Ă����瓮���Ă�
            if (player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x > camearaStart && //�X�^�[�g���̒[�ŃJ�����~�߂����
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x && ////�S�[���̒[�ŃJ�����~�߂����
                player.transform.position.x - transform.position.x > -1 * magnification * player.GetComponent<MyAgent>().startPosition.x) //�J���������������������
            {
                transform.position = new Vector3(player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
            if (player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x > camearaStart &&
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x &&
                player.transform.position.x - transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x)//��ɓ�������B��͑O�ɓ�������B�����v�����񂩂���
            {
                transform.position = new Vector3(player.transform.position.x + magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
            //camera�̍��W����ʐ؂�ւ��̐����ɂ��邽�߂̂�B����Ȃ�����camera�̍��W��14.02131�Ƃ��Ȃ��Ă����傢
            if (transform.position.x % 2 * magnification * player.GetComponent<MyAgent>().startPosition.x != 0)
            {
                transform.position = new Vector3((transform.position.x / (2 * magnification * player.GetComponent<MyAgent>().startPosition.x) - ((transform.position.x / (2 * magnification * player.GetComponent<MyAgent>().startPosition.x)) % 1)) * 2 * magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
        }

    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Agent �� Camera �ɕt����
    }

    public void cameraReset()
    {
        this.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
    }
}
