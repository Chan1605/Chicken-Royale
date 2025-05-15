using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Test : PoolAble
{
    public float speed = 5f;

    void Awake()
    {      
    }
    void Update()
    {

        // 총알이 많이 날라가면 삭제 해주기
        if (this.transform.position.y > 5)
        {
            //Destroy(this.gameObject);
            ReleaseObject();
        }

        this.transform.Translate(Vector3.forward * this.speed * Time.deltaTime);

    }
}
