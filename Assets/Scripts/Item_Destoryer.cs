using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Destoryer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            AudioManager.Instance.Play("itemPick");
        }
    }
}
