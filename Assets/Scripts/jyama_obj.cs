using UnityEngine;

public class jyama_obj : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool ue;//変数名、上
    private bool migi;//変数名、右
    private float x;//変数名、x
    private float y_max = 3.0f;
    private float y_min = -2.0f;
    [SerializeField] private float speed;
    public int jyama_type;
    void Start()
    {
        jyama_set();
    }

    public void jyama_set()
    {
        transform.position = new Vector3(Random.Range(-1.0f, 47.0f), Random.Range(y_min, y_max), 0.0f);
        if (jyama_type == 1)
        {
            if (2.0f - transform.position.y < transform.position.y + 4.0f)
            {
                ue = true;
            }
            else
            {
                ue = false;
            }
            x = transform.position.x;
            migi = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(jyama_type==1)
        {
            if (ue)
            {
                transform.position += new Vector3(0.0f, speed * Time.deltaTime,0.0f);
                if(transform.position.y>y_max)
                {
                    ue = false;
                }
            }
            else
            {
                transform.position -= new Vector3(0.0f, speed * Time.deltaTime, 0.0f);
                if(transform.position.y<y_min)
                {
                    ue = true;
                }
            }
            if(migi)
            {
                transform.position -= new Vector3(speed * Time.deltaTime*0.2f, 0.0f, 0.0f);
                if(x - transform.position.x > 2f)
                {
                    migi = false;
                }
            }
            else
            {
                transform.position += new Vector3(speed * Time.deltaTime * 0.2f, 0.0f, 0.0f);
                if (x - transform.position.x > -2f)
                {
                    migi = true;
                }
            }
        }
        if(jyama_type==2)
        {
            transform.Rotate(0.0f,0.0f,speed* Time.deltaTime*18.0f);
        }
    }
}
