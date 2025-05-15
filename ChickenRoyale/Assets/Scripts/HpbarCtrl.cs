using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpbarCtrl : MonoBehaviour
{
    [SerializeField] GameObject m_hpbarPrefab;

    Dictionary<Transform, GameObject> m_HpbarDict = new Dictionary<Transform, GameObject>();
    Camera m_cam = null;

    void Awake()
    {
        m_cam = Camera.main;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            GameObject hpBar = Instantiate(m_hpbarPrefab, transform);
            hpBar.transform.position = m_cam.WorldToScreenPoint(enemyTransform.position + new Vector3(0, 3f, 0));

            m_HpbarDict.Add(enemyTransform, hpBar);

            EnemyCtrl enemyScript = enemy.GetComponent<EnemyCtrl>();
            if (enemyScript != null)
            {
                // if (enemyScript.m_MonType == EnemyCtrl.MonType.Dino) // 객체를 통해 접근
                // {
                //     //hpBar.transform.position = m_cam.WorldToScreenPoint(enemyTransform.position + new Vector3(0, 12f, 0));
                // }
                enemyScript.SetHpBar(hpBar.GetComponent<Image>());
            }
        }
    }

    void Update()
    {
        List<Transform> toRemove = new List<Transform>();

        foreach (var pair in m_HpbarDict)
        {
            Transform enemyTransform = pair.Key;
            GameObject hpBar = pair.Value;

            if (enemyTransform == null)
            {
                toRemove.Add(enemyTransform);
                Destroy(hpBar);             
                continue;
            }

            Vector3 worldPos = enemyTransform.position + new Vector3(0, 3f, 0);
            Vector3 screenPos = m_cam.WorldToScreenPoint(worldPos);

            float distance = Vector3.Distance(m_cam.transform.position, enemyTransform.position);


            if (screenPos.z < 0 || distance > 50 || GameMgr.inst.m_esc || GameMgr.inst.m_Gameover) 
            {
                hpBar.SetActive(false);
            }
            else
            {
                hpBar.SetActive(true);
                hpBar.transform.position = screenPos;
            }
        }

        foreach (Transform enemy in toRemove)
        {
            m_HpbarDict.Remove(enemy);
        }
    }

    public void Register(Transform enemyTransform, EnemyCtrl enemyScript)
    {
        if (m_HpbarDict.ContainsKey(enemyTransform)) return;

        GameObject hpBar = Instantiate(m_hpbarPrefab, transform);
        hpBar.transform.position = m_cam.WorldToScreenPoint(enemyTransform.position + new Vector3(0, 3f, 0));
        m_HpbarDict.Add(enemyTransform, hpBar);

        if (enemyScript != null)
        {
            enemyScript.SetHpBar(hpBar.GetComponent<Image>());
        }
    }
}
