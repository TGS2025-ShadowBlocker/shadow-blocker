using UnityEngine;
/***********************

なんで変数名を何でもかんでも略そうとするの？
自分にしか分からない変数名書いて楽しい？ by suzuuuuu09

***********************/

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float knockbackPower;

    [Header("References")]
    [SerializeField] private GameObject goalResult;
    [SerializeField] private GameObject player;
    [SerializeField] private CameraController CameraController;
    public Vector3 startPosition;
    [SerializeField] private Animator anim;

    [Header("State")]
    private bool isGround = true;
    private bool hasSpawnedResult = false;
    private bool canKnockback = false;
    private bool end_first = true;
    private Vector2 moveVelocity;

    private Rigidbody2D rb;

    // Claude曰くこうすれば可読性が上がるらしい
    [Header("Consts")]
    private const float VELOCITY_THRESHOLD = 0.0001f;
    private const string GROUND_TAG = "ground";
    private const string GOAL_TAG = "goal";
    private const string DEATH_TAG = "death";
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        transform.position = startPosition;
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if(!GameManager.Instance.isGameActive) return;

        Movement();
    }

    private void Movement()
    {
        // Velocityを初期化する
        moveVelocity = Vector2.zero;
        
        // ゲームがアクティブな場合のみ移動処理を行う
        if (GameManager.Instance.isGameActive)
        {
            // 基本操作(左右移動とジャンプ)のVelocityを設定する
            BasicMovement();

            // Velocityを適用する
            ApplyMovement(moveVelocity);

        }
        else if(end_first)
        {
            ApplyMovement(Vector2.zero);
            end_first = false;
        }

            PlayAnim();
            //ゲームが終了した後も殴れるとおもろいので外に出します
            // ノックバックする
            Knockback();

        if (!Input.GetKey(KeyCode.F) && !Input.GetKey(KeyCode.R) && !Input.GetKey(KeyCode.C))
        {
            canKnockback = true;
        }
    }
    
    private void BasicMovement()
    {
        // 右に移動する
        if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
        {
            moveVelocity += Vector2.right * speed;
        }
        // 左に移動
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
        { 
            moveVelocity += Vector2.left * speed;
        }
        // ジャンプ！
        if (Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (isGround)
        {
            AudioManager.Instance.Play("jump"); // ジャンプ音を再生
            rb.AddForce(Vector2.up * jumpPower);
            isGround = false; // 接地判定これだと不具合起きる可能性があるって聞いたんですけどどうなんですかね
            //isGroundってtfしかないし、外でfalseにする理由ねぇかなって。一応言っとくとこれなくすと無限ジャンプ編入る
        }
    }
    
    private void ApplyMovement(Vector2 moveVelocity)
    {
        rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
    }

    private void PlayAnim()
    {
        if (!isGround)
        {
            anim.Play("jump");
        }
        else
        {
            if (moveVelocity.x == 0.0f)
            {
                anim.Play("boudati");
            }
            else if (moveVelocity.x > 0.0f)
            {
                anim.Play("run");
            }
            else if (moveVelocity.x < 0.0f)
            {
                anim.Play("run");
            }
        }
        if (moveVelocity.x > 0.0f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveVelocity.x < 0.0f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1.0f, transform.localScale.y, transform.localScale.z);
        }
    }


    private void Knockback()
    {
        if (!canKnockback) return;
        var tracker = GetTrackingDatas.Instance;
        if (tracker == null) return; // トラッキング用オブジェクトがシーンにない場合は何もしない

        if (tracker.IsKickActive)
        {
            moveVelocity = new Vector2(-1.0f, 1.0f).normalized * knockbackPower;
        }
        else if (tracker.IsPunchActive)
        {
            moveVelocity = new Vector2(-1.0f, 0.0f).normalized * knockbackPower;
        }

        rb.AddForce(moveVelocity);
    }
    
    private void Death()
    {
        /* // プレイヤーを再生成
        Instantiate(player, startPosition, Quaternion.identity);
        // 現在のプレイヤーを削除
        Destroy(gameObject);
        // ゲームを非アクティブにする 
        GameManager.Instance.isGameActive = false; */

        AudioManager.Instance.Play("player_death");
        transform.position = startPosition;
    }
    
    private void Goal()
    {
        if (hasSpawnedResult) return;
        hasSpawnedResult = true;
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.Play("result");
        AudioManager.Instance.Play("gameover");

        // ゴールしたときのUIを表示（優先: インスペクタで割り当てた goalResult -> GameManager.resultScreen -> 何もしない）
        if (goalResult != null)
        {
            // goalResult がシーン内の非アクティブなオブジェクトであれば単に有効化
            goalResult.SetActive(true);
        }
        else
        {
            GameObject toInstantiate = GameManager.Instance != null ? GameManager.Instance.resultScreen : null;
            if (toInstantiate != null)
            {
                var canvas = FindObjectOfType<Canvas>();
                GameObject result;
                if (canvas != null)
                {
                    result = Instantiate(toInstantiate, canvas.transform, false);
                }
                else
                {
                    result = Instantiate(toInstantiate, Vector3.zero, Quaternion.identity);
                }
                result.GetComponent<ResultScreen>().player_win();
            }
            else
            {
                Debug.LogWarning("PlayerController.Goal: No goalResult assigned and GameManager.resultScreen is null. Cannot show result UI.");
            }
        }

        // ゲームのアクティブ状態を変更
        GameManager.Instance.isGameActive = false;
        Debug.Log("goal");
    }
    
    // y軸の速度がほぼゼロかどうかを判定
    private bool IsVerticalVelocityZero()
    {
        float verticalVelocity = rb.velocity.y;
        return Mathf.Abs(verticalVelocity) < VELOCITY_THRESHOLD;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!GameManager.Instance.isGameActive) return;

        string collisionTag = collision.gameObject.tag;
        if(collisionTag == GROUND_TAG && IsVerticalVelocityZero())
        {
            isGround = true;
        }
        if (collisionTag == GOAL_TAG)
        {
            // ゴールしたときのUIを表示とゲームのアクティブ状態を変更
            Goal();
        }
        if (collisionTag == DEATH_TAG)
        {
            // プレイヤーの再生成と死んだプレイヤーの削除を行う
            Death();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!GameManager.Instance.isGameActive) return;

        string collisionTag = collision.gameObject.tag;
        if(collisionTag == GROUND_TAG)
        {
            isGround = false;
        }
    }
}
