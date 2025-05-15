using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCtrl : MonoBehaviour
{
    Transform m_Itemtr;
    Transform m_Target;
    [SerializeField] float m_Rotspeed;
    [SerializeField] float m_Speed;
    float m_distance;
    // Start is called before the first frame update
    void Start()
    {
        m_Itemtr = GetComponent<Transform>();
        m_Target = GameObject.FindWithTag("Player").GetComponent<Transform>();

        m_Speed = 20f;
        m_Rotspeed = 200f;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Target == null)
            return;

        m_distance = Vector3.Distance(m_Target.position, m_Itemtr.position);

        if (m_distance <= 5f)
        {
            Vector3 a_Dispos = m_Target.transform.position - m_Itemtr.transform.position;

            Vector3 a_Movestep = a_Dispos.normalized * m_Speed * Time.deltaTime;
            m_Itemtr.transform.Translate(a_Movestep, Space.World);
        }


        transform.Rotate(0.0f, m_Rotspeed * Time.deltaTime, 0.0f);
    }
}
