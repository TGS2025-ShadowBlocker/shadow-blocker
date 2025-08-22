using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraContoller2 : MonoBehaviour
{
    [SerializeField] private Vector3 posi;
    [SerializeField] private GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        posi = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = posi + Player.transform.position;
    }
}
