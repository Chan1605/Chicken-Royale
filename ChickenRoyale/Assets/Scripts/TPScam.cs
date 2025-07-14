using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPScam : MonoBehaviour
{
    [Header("----- Camera Info -----")]
    public Transform target;        
    public float targetHeight = 1.2f;
    public float targetSide = -0.15f;
    public float distance = 4.0f;
    public float maxDistance = 6;
    public float minDistance = 1.0f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -10;
    public float yMaxLimit = 70;
    public float zoomRate = 80;
    public float rotationDampening = 3.0f;
    private float x = 20.0f;
    private float y = 0.0f;
    public Quaternion aim;
    public float aimAngle = 8;
    RaycastHit hit;

    [HideInInspector]
    public float shakeValue = 0.0f;
    [HideInInspector]
    public bool onShaking = false;
    private float shakingv = 0.0f;
    private Shoot shootcs;


    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        if (!shootcs)
        {
            shootcs = FindObjectOfType<Shoot>();
        }
        if (!target)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
        //Screen.lockCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }

    void Update()
    {

    }

    void LateUpdate()
    {
        if(GameMgr.inst.m_Curgame == GameMgr.GameState.Pause)
            return;
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        distance -= (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * zoomRate * Mathf.Abs(distance);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // Rotate Camera
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        transform.rotation = rotation;
        aim = Quaternion.Euler(y - aimAngle, x, 0);

        Vector3 cameraForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 playerForward = new Vector3(target.transform.forward.x, 0, target.transform.forward.z).normalized;

        float angleDiff = Vector3.Angle(cameraForward, playerForward);

        // 기본 카메라 위치 계산
        Vector3 position = target.position - (rotation * new Vector3(targetSide, 0, 1) * distance + new Vector3(0, -targetHeight, 0));

        if (shootcs.m_isAiming)
        {
            yMinLimit = -10;
                    
            RaycastHit hit;   // Raycast로 지면 체크
            Vector3 trueTargetPosition = target.position - new Vector3(targetSide, -targetHeight, 0);
            if (Physics.Raycast(target.position, (position - target.position).normalized, out hit, distance + 0.1f))
            {
                position = hit.point + Vector3.up * 0.3f; // 카메라를 지면 위로 약간 올림
            }
        }
        else
        {
            yMinLimit = 0;
        }
        transform.position = position;


        // if (Physics.Linecast(trueTargetPosition, transform.position, out hit))
        // {
        //     if (hit.transform.tag == "Wall")
        //     {
        //         transform.position = hit.point + hit.normal * 0.1f;   //put it at the position that it hit
        //     }
        // }
        // if (onShaking)
        // {
        //     shakeValue = Random.Range(-shakingv, shakingv) * 0.2f;
        //     transform.position += new Vector3(0, shakeValue, 0);
        // }
    }




    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);

    }


    public void Shake(float val, float dur)
    {
        if (onShaking)
        {
            return;
        }
        shakingv = val;
        StartCoroutine(Shaking(dur));
    }

    public IEnumerator Shaking(float dur)
    {
        onShaking = true;
        yield return new WaitForSeconds(dur);
        shakingv = 0;
        shakeValue = 0;
        onShaking = false;
    }

    public void SetNewTarget(Transform p)
    {
        target = p;
    }

    void OnEnable()
    {
        shakingv = 0;
        shakeValue = 0;
        onShaking = false;
    }
}


