using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class My_jinja : MonoBehaviour
{
    public Camera cam;
    public Camera yobi_cam;
    public CameraSensorComponent cameraSensor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null && !cam.enabled)
        {
            cam.enabled = true;
        }
        if(cam == null)
        {
            cameraSensor.Camera = yobi_cam;
        }
    }
    void Awake()
    {
        if (cameraSensor != null && cam != null)
        {
            cameraSensor.Camera = cam;
        }
    }
}
