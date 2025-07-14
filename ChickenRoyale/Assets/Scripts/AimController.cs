using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float aimFOV = 30f;
    public float aimSpeed = 10f;
    void Start()
    {

    }

    void Update()
    {
        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
            return;

        if (Input.GetMouseButton(1))
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, aimFOV, Time.deltaTime * aimSpeed);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, Time.deltaTime * aimSpeed);
        }
    }
}
