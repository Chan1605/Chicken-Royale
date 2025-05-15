using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class GrenadeCtrl : MonoBehaviour
{
    Transform m_transform;
    Vector3 m_Forpos = Vector3.zero;
    [SerializeField] float m_timer = 2.0f;
    [SerializeField] float m_speed = 100f;
    [Header("----- Explosion -----")]
    public GameObject m_Explosion;
    bool isRot = true;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = GetComponent<Transform>();

        transform.forward = m_Forpos;

        transform.eulerAngles = new Vector3(20.0f, transform.eulerAngles.y, transform.eulerAngles.z);

        GetComponent<Rigidbody>().AddForce(m_Forpos * m_speed);


    }

    // Update is called once per frame
    void Update()
    {
        m_timer -= Time.deltaTime;
        if (m_timer <= 0.0f)
        {
            ExpGrenade();
        }

        if (isRot)
        {
            transform.Rotate(new Vector3(Time.deltaTime * 190.0f, 0.0f, 0.0f),
                            Space.Self);
        }
    }


    void OnCollisionEnter(Collision coll)
    {
        isRot = false;
    }


    void ExpGrenade()
    {
        GameObject explosion = Instantiate(m_Explosion,
                       m_transform.position, Quaternion.identity);
        Destroy(explosion,
            explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        SoundMgr.Instance.PlayEffSound("Explosion",0.3f);

        //지정한 원점을 중심으로 10.0f 반경 내에 들어와 있는 Collider 객체 추출
        Collider[] colls = Physics.OverlapSphere(m_transform.position, 12.0f);

        ////추출한 Collider 객체에 폭발력 전달
        EnemyCtrl a_MonCtrl = null;
        foreach (Collider coll in colls)
        {
            a_MonCtrl = coll.GetComponent<EnemyCtrl>();
            if (a_MonCtrl == null)
                continue;

            a_MonCtrl.TakeDamage(150);
        }
        Destroy(gameObject);
    }

    public void SetForwardDir(Vector3 a_Dir)
    {
        m_Forpos = new Vector3(a_Dir.x, a_Dir.y + 0.3f, a_Dir.z);
    }


}
