using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
/***********************

なんで変数名を何でもかんでも略そうとするの？
自分にしか分からない変数名書いて楽しい？ by suzuuuuu09

***********************/

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float kickknockbackPower;
    [SerializeField] private float punchknockbackPower;
    [SerializeField] private float punch_cooldown;
    [SerializeField] private float kick_cooldown;
    [SerializeField] private float punch_range;
    [SerializeField] private float kick_range;
    [SerializeField] private float Touch_power;
    [SerializeField] private float hosi_time;
    [SerializeField] private float petto_time;

    [Header("References")]
    [SerializeField] private GameObject goalResult;
    [SerializeField] private GameObject player;
    [SerializeField] private CameraController CameraController;
    public Vector3 startPosition;
    [SerializeField] private Animator anim;
    [SerializeField] private ScoreCounter ScoreCounter;
    [SerializeField] private GameObject hosi_UI;
    [SerializeField] private Image hosi_circle;
    [SerializeField] private GameObject petto_UI;
    [SerializeField] private Image petto_circle;

    [Header("State")]
    private bool isGround = true;
    private bool hasSpawnedResult = false;
    private bool canKnockback = false;
    private bool end_first = true;
    private Vector2 moveVelocity;
    private bool kickActiveTime = true;
    private bool punchActiveTime = true;
    private bool knockback = false;

    private Rigidbody2D rb;

    // Claude曰くこうすれば可読性が上がるらしい
    [Header("Consts")]
    private const float VELOCITY_THRESHOLD = 0.3f;
    private const string GROUND_TAG = "ground";
    private const string GOAL_TAG = "goal";
    private const string DEATH_TAG = "death";
    private const string BONE_TAG = "bone";
    private const string HOSI_TAG = "hosi";
    private const string PETTO_TAG = "petto";
    private int playerLayer => LayerMask.NameToLayer("player");
    private int boneLayer => LayerMask.NameToLayer("bone");
    [SerializeField] private Camera mainCamera; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        transform.position = startPosition;
        anim = GetComponent<Animator>();
        Invoke("kickTrue", kick_cooldown);
        Invoke("punchTrue", punch_cooldown);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        if (!knockback)
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
            else if (end_first)
            {
                //ゲーム終了後最初の1フレームは速度を0にする
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
    }
    
    private void BasicMovement()
    {
        moveVelocity += Vector2.right * speed * Input.GetAxisRaw("Horizontal");
        // ジャンプ！
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0) || Input.GetKey(KeyCode.JoystickButton1) || Input.GetAxisRaw("Vertical") > 0.99f)
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
        if(knockback)
        {
            anim.Play("knokback");
        }
        else if (!isGround)
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


    private void kickTrue()
    {
        kickActiveTime = true;
    }
    private void punchTrue()
    {
        punchActiveTime = true;
    }
    private void knokbackFalse()
    {
        knockback = false;
    }
    private float PointDistance(LandmarkPoint land_p)
    {
        Vector3 World_land_point = mainCamera.ViewportToWorldPoint(new Vector3(land_p.x, land_p.y, 0.0f));
        float value = (World_land_point.x - transform.position.x) * (World_land_point.x - transform.position.x) +
                      (World_land_point.y - transform.position.y) * (World_land_point.y - transform.position.y);
        value = Mathf.Sqrt(value);
        return value;
    }
    private void Knockback()
    {
        Vector2 knokback = Vector2.zero;
        if (!canKnockback) return;
        var tracker = GetTrackingDatas.Instance;
        if (tracker == null) return; // トラッキング用オブジェクトがシーンにない場合は何もしない
        //左手もしくは右手の近くにplayerがいる
        if ((tracker.IsKickActive && kickActiveTime) && (PointDistance(tracker.LandmarksData["left_ankle"]) < kick_range || PointDistance(tracker.LandmarksData["left_ankle"]) < kick_range))
        {
            knokback = new Vector2(-1.0f, 1.0f).normalized * kickknockbackPower;
            kickActiveTime = false;
            Invoke("kickTrue", kick_cooldown);
            ScoreCounter.Attack();
            knockback = true;
            rb.velocity = new Vector2(0.0f, 0.0f);
        }
        else if ((tracker.IsPunchActive && punchActiveTime) && (PointDistance(tracker.LandmarksData["left_ankle"]) < punch_range || PointDistance(tracker.LandmarksData["left_ankle"]) < punch_range))
        {
            knokback = new Vector2(-1.0f, 1.0f / 200.0f).normalized * punchknockbackPower;
            punchActiveTime = false;
            Invoke("punchTrue", punch_cooldown);
            ScoreCounter.Attack();
            knockback = true;
            rb.velocity = new Vector2(0.0f, 0.0f);
        }
        rb.AddForce(knokback);
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
        ScoreCounter.playerDeath();
        CameraController.cameraReset();
    }
    
    private void Goal()
    {
        if (hasSpawnedResult) return;
        hasSpawnedResult = true;
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.Play("result");
        AudioManager.Instance.Play("gameover");
        ScoreCounter.playerGoal();

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

    private void Touch_Knokback(Collision2D collision)
    {
        rb.velocity = new Vector2(0.0f, 0.0f);
        Collider2D otherCollider = collision.collider;
        Vector2 contactPoint = collision.GetContact(0).point;

        float angularVelocity = collision.gameObject.GetComponent<bone_info>().angularVelocity;
        Vector2 velocity = collision.gameObject.GetComponent<bone_info>().velocity;

        // 2. オブジェクトの中心（ピボット）から衝突点へのベクトルを計算
        Vector2 pivot = transform.position;
        Vector2 radiusVector = contactPoint - pivot;

        // 3. 角速度をラジアン/秒に変換（物理計算で使うため）
        float angularVelocityRad = angularVelocity * Mathf.Deg2Rad;

        // 4. 回転によって生じる速度（接線速度）を計算
        //    これは2D空間における角速度ベクトルと半径ベクトルのクロス積（外積）に相当します。
        Vector2 tangentialVelocity = new Vector2(
            -angularVelocityRad * radiusVector.y, // -ωz * ry
             angularVelocityRad * radiusVector.x  //  ωz * rx
        );

        // 5. オブジェクト自体の移動速度（並進速度）と回転による接線速度を足し合わせる
        Vector2 velocityAtContactPoint = velocity + tangentialVelocity;
        velocityAtContactPoint *= Touch_power;
        if(velocityAtContactPoint.magnitude > 10.0f)
        {
            knockback = true;
            rb.AddForce(velocityAtContactPoint);
        }

    }

    // y軸の速度がほぼゼロかどうかを判定
    private bool IsVerticalVelocityZero()
    {
        float verticalVelocity = rb.velocity.y;
        return Mathf.Abs(verticalVelocity) < VELOCITY_THRESHOLD;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        string collisionTag = collision.gameObject.tag;
        if((collisionTag == GROUND_TAG || collisionTag == BONE_TAG) && IsVerticalVelocityZero())
        {
            isGround = true;
        }
        if((collisionTag == GROUND_TAG || collisionTag == BONE_TAG) && knockback)
        {
            Invoke("knokbackFalse", 0.2f);
        }
    }

    private void hosi()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, boneLayer, true);
        Invoke("hosi_end", hosi_time);
        StartCoroutine(WipeCircle(hosi_time,hosi_circle));
        hosi_UI.SetActive(true);

    }
    private void petto()
    {
        jumpPower = 130.0f;
        speed = 10.0f;
        Invoke("petto_end", petto_time);
        StartCoroutine(WipeCircle(petto_time,petto_circle));
        petto_UI.SetActive(true);
    }
    private System.Collections.IEnumerator WipeCircle(float wipeDuration,Image circleImage)
    {
        float elapsedTime = 0f;

        // wipeDuration秒かけて処理を行う
        while (elapsedTime < wipeDuration)
        {
            // 経過時間から進行度（0から1）を計算
            float progress = elapsedTime / wipeDuration;

            // Fill Amountを 1 -> 0 に変化させる
            circleImage.fillAmount = 1.0f - progress;

            // 経過時間を更新
            elapsedTime += Time.deltaTime;

            // 1フレーム待つ
            yield return null;
        }

        // 最後にきっちり0にする
        circleImage.fillAmount = 0;
    }
    private void hosi_end()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, boneLayer, false);
        hosi_UI.SetActive(false);
    }
    private void petto_end()
    {
        jumpPower = 100.0f;
        speed = 5.0f;
        petto_UI.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string collisionTag = collision.gameObject.tag;
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
        if(collisionTag == BONE_TAG)
        {
            Touch_Knokback(collision);
        }
        if(collisionTag == HOSI_TAG)
        {
            hosi();

        }
        if(collisionTag == PETTO_TAG)
        {
            petto();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string collisionTag = collision.gameObject.tag;
        if(collisionTag == GROUND_TAG || collisionTag == BONE_TAG)
        {
            isGround = false;
        }
    }
}
