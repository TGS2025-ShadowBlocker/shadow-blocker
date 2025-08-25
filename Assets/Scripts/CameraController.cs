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
        //ここの制御は後々
        if (player.GetComponent<PlayerController>() != null)
        {
            //ゴールの条件、goalのオブジェクトが0カメラの0中心に配置されてたから動いてる
            if (player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x > camearaStart && //スタート側の端でカメラ止める条件
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x && ////ゴールの端でカメラ止める条件
                player.transform.position.x - transform.position.x > -1 * magnification * player.GetComponent<PlayerController>().startPosition.x) //カメラかくかくさせる条件
            {
                transform.position = new Vector3(player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
            if (player.transform.position.x - magnification * player.GetComponent<PlayerController>().startPosition.x > camearaStart &&
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x &&
                player.transform.position.x - transform.position.x < magnification * player.GetComponent<PlayerController>().startPosition.x)//後に動かすやつ。上は前に動かすやつ。統合思いつかんかった
            {
                transform.position = new Vector3(player.transform.position.x + magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
            //cameraの座標を画面切り替わりの整数にするためのやつ。これなくすとcameraの座標が14.02131とかなってきしょい
            if (transform.position.x % 2 * magnification * player.GetComponent<PlayerController>().startPosition.x != 0)
            {
                transform.position = new Vector3((transform.position.x / (2 * magnification * player.GetComponent<PlayerController>().startPosition.x) - ((transform.position.x / (2 * magnification * player.GetComponent<PlayerController>().startPosition.x)) % 1)) * 2 * magnification * player.GetComponent<PlayerController>().startPosition.x, 0.0f, -10.0f);
            }
        }
        else
        {
            //ゴールの条件、goalのオブジェクトが0カメラの0中心に配置されてたから動いてる
            if (player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x > camearaStart && //スタート側の端でカメラ止める条件
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x && ////ゴールの端でカメラ止める条件
                player.transform.position.x - transform.position.x > -1 * magnification * player.GetComponent<MyAgent>().startPosition.x) //カメラかくかくさせる条件
            {
                transform.position = new Vector3(player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
            if (player.transform.position.x - magnification * player.GetComponent<MyAgent>().startPosition.x > camearaStart &&
                player.transform.position.x - goal.transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x &&
                player.transform.position.x - transform.position.x < magnification * player.GetComponent<MyAgent>().startPosition.x)//後に動かすやつ。上は前に動かすやつ。統合思いつかんかった
            {
                transform.position = new Vector3(player.transform.position.x + magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
            //cameraの座標を画面切り替わりの整数にするためのやつ。これなくすとcameraの座標が14.02131とかなってきしょい
            if (transform.position.x % 2 * magnification * player.GetComponent<MyAgent>().startPosition.x != 0)
            {
                transform.position = new Vector3((transform.position.x / (2 * magnification * player.GetComponent<MyAgent>().startPosition.x) - ((transform.position.x / (2 * magnification * player.GetComponent<MyAgent>().startPosition.x)) % 1)) * 2 * magnification * player.GetComponent<MyAgent>().startPosition.x, 0.0f, -10.0f);
            }
        }

    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Agent や Camera に付ける
    }

    public void cameraReset()
    {
        this.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
    }
}
