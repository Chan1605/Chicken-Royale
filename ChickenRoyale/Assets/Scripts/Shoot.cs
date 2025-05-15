using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class Shoot : MonoBehaviour
{
    public Transform m_tip; //총구 위치
    public Transform m_GrenadeSpawn; //수류탄 위치
    public GameObject m_bullet;
    public GameObject m_Grenade;
    public Camera m_playerCamera;
    public bool m_isAiming = false;
    public bool m_AimGrenade = false;
    [SerializeField] float m_GraSpeed;

    void Start()
    {
        m_isAiming = false;
        m_AimGrenade = false;
        m_GraSpeed = 100f;
    }
    void Update()
    {
        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
            return;


        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (GameMgr.inst.m_GranadeCount < 1)
            {
                GameMgr.inst.ShowGuide("수류탄이 부족합니다!", 1f);
                return;
            }
            GameMgr.inst.GreGuide(-1);
            if (m_AimGrenade)
                ThrowGrenade(true, 1);
            else
                ThrowGrenade(false, 1);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameMgr.inst.m_GranadeCount < 3)
            {
                GameMgr.inst.ShowGuide("수류탄이 부족합니다!", 1f);
                return;
            }
            GameMgr.inst.GreGuide(-3);
            if (m_AimGrenade)
                ThrowGrenade(true, 3);
            else
                ThrowGrenade(false, 3);
        }
    }

    void ThrowGrenade(bool isAiming, int count)
    {
        Vector3 dir = transform.forward;
        if (isAiming)
        {
            Vector3 target = GetAimTarget();
            m_GrenadeSpawn.LookAt(target);
            dir = transform.forward * m_GraSpeed * Time.deltaTime;//(target - m_GrenadeSpawn.position).normalized;        
        }
        if (count == 1)
        {
            SpawnGrenade(dir);
        }
        else
        {
            ThrowMultiple(dir);
        }
    }

    Vector3 GetAimTarget()
    {
        Ray ray = m_playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(100f); // 가상 타겟
        }
    }

    void SpawnGrenade(Vector3 direction)
    {
        GameObject grenade = Instantiate(m_Grenade, m_GrenadeSpawn.position, Quaternion.identity);
        GrenadeCtrl grenadeCtrl = grenade.GetComponent<GrenadeCtrl>();

        if (grenadeCtrl != null)
        {
            grenadeCtrl.SetForwardDir(direction + Vector3.up * 0.3f);
        }
    }
    void ThrowMultiple(Vector3 centerDirection)
    {
        for (float ang = -20.0f; ang <= 20.0f; ang += 20.0f)
        {
            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.up);
            Vector3 spreadDir = rot * centerDirection;

            SpawnGrenade(spreadDir + Vector3.up);
        }
    }


    public void Shooteven()
    {
        if (!m_isAiming)
        {
            Instantiate(m_bullet, m_tip.position, m_tip.rotation);
        }
        if (m_isAiming)
        {
            Vector3 target = GetAimTarget();

            m_tip.LookAt(target);
            Instantiate(m_bullet, m_tip.position, m_tip.rotation);
        }
    }





}
