using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMaker : MonoBehaviour
{
    [SerializeField] private GameObject[] items;
    [SerializeField] private GameObject player;
    private int game_time;
    private int cnt;
    private bool flag = true;
    // Start is called before the first frame update
    void Start()
    {
        game_time = (int)GameManager.Instance.gameTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.isGameActive)
        {
            if(flag)
            {
                Invoke("ItemRand", 1.0f);
                flag = false;
            }
        }
    }
    private void ItemRand()
    {
        flag = true;
        cnt++;
        int a = 0;
        if (game_time * 1 / 3 > cnt)
        {
            a = 10;
        }
        else
        {
            a = (int)(cnt - game_time / 3.0f) / (2 / 3 * game_time) * -8 + 10;
        }
        int r = Random.Range(0, a);
        if(r==0)
        {
            int s = Random.Range(0,items.Length);
            Instantiate(items[s],new Vector3(player.transform.position.x,6.0f,0.0f),Quaternion.identity);
        }
    }
}
