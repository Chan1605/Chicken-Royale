using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject m_hpbar;

    List<Transform> m_ObjcetList = new List<Transform>();
    List<GameObject> m_Hpbarlist = new List<GameObject>();
    Camera m_cam = null;
    // Start is called before the first frame update
    void Start()
    {
        m_cam = Camera.main;

         GameObject[] t_objects = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i=0; i < t_objects.Length; i++)
        {
            m_ObjcetList.Add(t_objects[i].transform);
            GameObject t_hpbar = Instantiate(m_hpbar,t_objects[i].transform.position,Quaternion.identity,transform);
            m_Hpbarlist.Add(t_hpbar);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<m_ObjcetList.Count; i++)
        {
            m_Hpbarlist[i].transform.position = m_cam.WorldToScreenPoint(m_ObjcetList[i].position+ new Vector3(0,3f,0));
        }
    }
}
