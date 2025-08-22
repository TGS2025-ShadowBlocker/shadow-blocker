using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MyAgent : Agent
{
    public Vector3 startPosition;
    private Rigidbody2D rb;

    public float speed = 5f;
    public float jumpPower = 100f;
    private int stepCount = 0;
    private float maxPenalty = 0.001f; // 最大で与える罰の大きさ
    private int maxStepLimit = 2000;  // 最大ステップ数
    private bool isGround = true;
    private const float VELOCITY_THRESHOLD = 0.0001f;
    private float beforx;
    [SerializeField] private GameObject goal;
    [SerializeField] private float start_goal_dis;
    [SerializeField] private List<GameObject> jyamas = new List<GameObject>();
    private float max_score;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Agent や Camera に付ける
    }
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        // プレイヤーとゴールの位置をリセット
        transform.position = startPosition;
        beforx = startPosition.x;
        rb.velocity = Vector2.zero;
        start_goal_dis = Mathf.Abs(startPosition.x - goal.transform.position.x);
        foreach (GameObject jyama in jyamas)
        {
            jyama.GetComponent<jyama_obj>().jyama_set();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 相対位置、速度、接地状態
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(isGround ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0]; // 左右移動
        float jump = actions.ContinuousActions[1];  // ジャンプ入力（0〜1）

        if(moveX>0.8f)
        {
            rb.velocity = new Vector2(speed * 1.5f, rb.velocity.y);
        }
        else if(moveX<-0.8f)
        {
            rb.velocity = new Vector2(speed * -1.5f, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveX * speed * 1.25f * 1.5f, rb.velocity.y);
        }
        if (jump > 0.8f && isGround && IsVerticalVelocityZero())
        {
            rb.AddForce(Vector2.up * jumpPower);
            isGround = false;
            AddReward(-0.01f);
        }
        if(beforx - transform.position.x<-0.002f)
        {
            AddReward(-0.03f);
        }
        float goal_dis = transform.position.x - goal.transform.position.x;
        AddReward((start_goal_dis-Mathf.Abs(goal_dis)) * 0.004f);
        /*float timePenalty = (float)stepCount / maxStepLimit * maxPenalty;
        AddReward(-timePenalty);*/

        stepCount++;

    }
    private bool IsVerticalVelocityZero()
    {
        float verticalVelocity = rb.velocity.y;
        return Mathf.Abs(verticalVelocity) < VELOCITY_THRESHOLD;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGround = true;
        }
        else if (collision.gameObject.CompareTag("goal"))
        {
            AddReward(10.0f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("death"))
        {
            AddReward(-10.0f);
            EndEpisode();
        }
        else if(collision.gameObject.CompareTag("jyama"))
        {
            AddReward(-0.01f);
        }
    }
}
