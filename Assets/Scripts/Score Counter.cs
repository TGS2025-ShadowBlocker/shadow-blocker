using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField] private GameObject result;
    [SerializeField] private TMP_Text Timer;
    [SerializeField] private int base_score;
    private int player_score;
    private int shadow_score;
    private int playerDeathCnt;
    private float playerGoalTime;
    private int AttackCnt;

    private void Start()
    {
        playerGoalTime = GameManager.Instance.gameTime;
        playerDeathCnt = 0;
        var tracker = GetTrackingDatas.Instance;
        player_score = 0;
        shadow_score = 0;
    }

// Update is called once per frame
void Update()
    {
        player_score = base_score - 50*playerDeathCnt + (int)(10*playerGoalTime);
        shadow_score = base_score + 100*playerDeathCnt + 50*AttackCnt;
        GameManager.Instance.playerScore = player_score;
        GameManager.Instance.shadowScore = shadow_score;
    }

    public void playerDeath()
    {
        playerDeathCnt++;
    }
    public void playerGoal()
    {
        playerGoalTime = float.Parse(Timer.text);
    }
    public void Attack()
    {
        AttackCnt++;
    }
}
