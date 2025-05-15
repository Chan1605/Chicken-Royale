using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    public static PlayerCtrl inst = null;
    private float h;
    private float v;

    [Header("----- Player info -----")]
    [SerializeField] private float m_speed = 10.0f;
    [SerializeField] private float m_rotspeed = 10.0f;
    [SerializeField] private float m_gravity = 25f;
    [SerializeField] private float m_jumpForce = 10.0f;
    [SerializeField] private float m_firedur = 0.1f;
    public int m_attackdamage = 5;
    [SerializeField] private float m_buffDuration = 10.0f;
    private Coroutine m_buffCoroutine;
    [Header("----- HP UI -----")]
    [SerializeField] private float m_MaxHp = 100;
    [SerializeField] private float m_CurHp;
    public Image hpbar;
    [Header("----- Effect -----")]
    public GameObject m_BloodDecal;
    public GameObject m_BloodEffect;
    [SerializeField] private GameObject m_HealEffect;
    private Vector3 m_dir = Vector3.zero;
    private Vector3 m_Direction = Vector3.zero;
    private CharacterController m_cc;
    public Animator m_anim;
    public LayerMask m_layer;
    private Shoot m_shoot;
    private bool m_ground = false;
    [HideInInspector] public bool m_isDie = false;
    private float idletime = 0;
    [Header("----- Dash -----")]
    [SerializeField] private float m_dashSpeed = 25.0f;
    [SerializeField] private float m_dashDuration = 0.4f;
    [SerializeField] private float m_dashStaminaCost = 20f;
    [SerializeField] private float m_dashCooldown = 1.0f;
    private bool m_isDashing = false;
    private float m_dashTimer = 0f;
    private float m_dashCooldownTimer = 0f;
    [SerializeField] private float m_maxStamina = 100f;
    [SerializeField] private float m_curStamina;
    [SerializeField] private float m_staminaRegenRate = 15f; // 초당 회복량
    private bool m_isRecovering = false;
    [SerializeField] private float m_recoveryThreshold = 40f; // 회복 완료 조건
    [Header("----- Stamina UI -----")]
    public Slider staminaSlider;
    public TextMeshProUGUI staminaText;

    void Awake()
    {
        if (!inst)
        {
            inst = this;
        }
        else if (inst != null)
        {
            Destroy(inst);
        }
        if (!m_cc)
        {
            m_cc = GetComponent<CharacterController>();
        }
        m_CurHp = m_MaxHp;
        m_anim = GetComponent<Animator>();
        m_shoot = GetComponent<Shoot>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_curStamina = m_maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = m_maxStamina;
            staminaSlider.value = m_curStamina;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.inst.m_Curgame == GameMgr.GameState.End || m_isDie)
            return;

        IsGround();
        PlayerAnimCheck();

        m_firedur = m_firedur - Time.deltaTime;
        if (m_firedur <= 0.0f)
        {
            m_firedur = 0.0f;
            if (Input.GetMouseButton(0))
            {
                FireGun();
                m_firedur = 0.1f;
            }
        }


        HandleDash(); // 대시 상태 업데이트
        RegenerateStamina(); // 스태미너 회복
        UpdateStaminaUI(); //  스태미너 UI갱신

        m_dir.y -= m_gravity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (GameMgr.inst.m_Curgame == GameMgr.GameState.End || m_isDie)
            return;

        PlayerMove();
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            if (Input.GetKey(KeyCode.LeftShift) || m_isDashing || m_isRecovering)
            {
                if (!staminaSlider.gameObject.activeSelf)
                    staminaSlider.gameObject.SetActive(true);

            }
            else
            {
                if (staminaSlider.gameObject.activeSelf)
                    staminaSlider.gameObject.SetActive(false);
            }

            staminaSlider.value = m_curStamina;
        }
        if (m_isRecovering)
        {
            staminaSlider.fillRect.GetComponent<Image>().color = Color.red;
            GameMgr.inst.ShowGuide("스테미너 회복 중 입니다.", 1.0f);
        }
        else
            staminaSlider.fillRect.GetComponent<Image>().color = Color.yellow;

        if (staminaText != null)
        {
            staminaText.text = $"{(int)m_curStamina} / {m_maxStamina}";
        }
    }

    void PlayerMove()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 m_CamForward = Camera.main.transform.forward;
        m_CamForward.y = 0;
        m_CamForward.Normalize();

        Vector3 m_CamRight = Camera.main.transform.right;
        m_CamRight.y = 0;
        m_CamRight.Normalize();

        m_Direction = (m_CamForward * v) + (m_CamRight * h);

        if (m_Direction.sqrMagnitude > 0) //대각이동 속도 정규화
        {
            m_Direction.Normalize();
        }

        if (m_Direction.sqrMagnitude <= 0.01f)
        {
            m_isDashing = false;
        }

        float currentSpeed = m_isDashing ? m_dashSpeed : m_speed;

        if (m_Direction != Vector3.zero && m_ground)
        {
            m_anim.SetBool("isMove", true);

            float angle = Vector3.SignedAngle(transform.forward, m_Direction, Vector3.up);

            if (Mathf.Abs(angle) > 5.0f)
            {
                transform.forward = Vector3.Lerp(transform.forward, m_Direction, m_rotspeed * Time.deltaTime);
            }
            m_anim.SetFloat("idleTime", 0);
        }
        else
        {
            m_anim.SetBool("isMove", false);

            idletime += Time.deltaTime;
            if (idletime >= 5f)
            {
                m_anim.SetFloat("idleTime", idletime);
                idletime = 0;

                m_anim.SetBool("isBattle", false);
            }
        }

        m_cc.Move((m_Direction * currentSpeed + new Vector3(0, m_dir.y, 0)) * Time.deltaTime);
    }

    void PlayerAnimCheck()
    {
        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
            return;
        if (Input.GetButtonDown("Jump") && m_ground)
        {
            m_anim.SetBool("isJump", true);
            SoundMgr.Instance.PlayEffSound("Jump", 0.2f);
            m_dir.y = m_jumpForce;
            //float jumpTime = (2 * m_jumpForce) / m_gravity;         
        }
        else if (!m_ground)
        {
            m_anim.SetBool("isJump", false);
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(SmoothLookAt());
        }

    }


    void IsGround()
    {
        RaycastHit hit;

        if (Physics.Raycast(this.transform.position + (Vector3.up * 0.2f), Vector3.down, out hit, 0.4f, m_layer))
        {
            m_ground = true;
        }
        else
        {
            m_ground = false;
        }
    }


    IEnumerator SmoothLookAt()//(float duration)
    {
        while (Input.GetMouseButton(1))
        {
            m_shoot.m_isAiming = true;
            m_shoot.m_AimGrenade = true;

            //float elapsed = 0f;
            //Quaternion startRotation = transform.rotation;

            // 카메라의 Y축 방향만 가져오기
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // y축 제거 (캐릭터가 위를 바라보지 않도록)
            cameraForward.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

            // while (elapsed < duration)
            // {
            //     transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            //     elapsed += Time.deltaTime;
            //     yield return null;
            // }

            transform.rotation = targetRotation; // 마지막 보정

            yield return null; // 한 프레임 대기 후 다시 실행
        }
        m_shoot.m_isAiming = false;
        m_shoot.m_AimGrenade = false;
    }

    void FireGun()
    {
        m_anim.SetTrigger("fire");
        m_anim.SetBool("isBattle", true);
        m_anim.SetFloat("idleTime", 0f);

        SoundMgr.Instance.PlayEffSound("GunSound", 0.15f);
    }

    public void TakeDamage(int a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp <= 0.0f)
            m_CurHp = 0.0f;

        //CreateBloodEffect(this.transform.position);

        if (m_CurHp <= 0)
        {
            PlayerDie();
        }

        hpbar.fillAmount = m_CurHp / m_MaxHp;
        GameMgr.inst.m_HpTxt.text = m_CurHp + " / " + m_MaxHp;
    }


    void PlayerDie()
    {
        m_isDie = true;
        m_anim.SetTrigger("isDie");
        GameMgr.inst.NotifyPlayerDied();

        m_anim.StopPlayback();

        GameMgr.inst.m_Gameover = true;
        GameMgr.inst.m_Curgame = GameMgr.GameState.End;

        GameMgr.inst.GameOver();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("CoinPrefab"))
        {
            SoundMgr.Instance.PlayEffSound("Coin", 0.2f);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.name.Contains("DiaPrefab"))
        {
            SoundMgr.Instance.PlayEffSound("Buff", 0.4f);
            if (m_buffCoroutine != null)
                StopCoroutine(m_buffCoroutine);

            m_buffCoroutine = StartCoroutine(BuffAttackDamage(10, m_buffDuration));
            Destroy(other.gameObject);
        }
        else if (other.gameObject.name.Contains("ItemGrenadePrefab"))
        {
            GameMgr.inst.GreGuide(1);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.name.Contains("HealObj"))
        {
            SoundMgr.Instance.PlayEffSound("Coin", 0.2f);
            Heal(20,this.transform.position);
            Destroy(other.gameObject);
        }

    }

    private IEnumerator BuffAttackDamage(int buffAmount, float duration)
    {
        int originalDamage = m_attackdamage;
        m_attackdamage = buffAmount;
        GameMgr.inst.StartBuff(duration);

        yield return new WaitForSeconds(duration);

        m_attackdamage = originalDamage;
    }

    void OnCollisionEnter(Collision other)
    {
        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
            return;
        if (other.gameObject.tag == "EnemyBull")
        {
            EnemyBullet bullet = other.gameObject.GetComponent<EnemyBullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);  // 총알에 저장된 데미지로 체력 감소
                m_anim.SetTrigger("isHit");
            }
        }
    }

    void CreateBloodEffect(Vector3 pos)
    {
        GameObject blood1 =
        (GameObject)Instantiate(m_BloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = this.transform.position + (Vector3.up * 0.05f);
        //데칼의 회전값을 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));


        GameObject blood2 = (GameObject)Instantiate(m_BloodDecal, decalPos, decalRot);
        //데칼의 크기도 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        Destroy(blood2, 3.0f);
    }

    void FootStep()
    {
        SoundMgr.Instance.PlayEffSound("SFX_Movement_Footstep_Water_1", 0.5f);
    }

    void RegenerateStamina()
    {
        if (m_curStamina < m_maxStamina)
        {
            m_curStamina += m_staminaRegenRate * Time.deltaTime;
            if (m_curStamina > m_maxStamina)
                m_curStamina = m_maxStamina;
        }

        // 리커버리 타임 체크
        if (m_isRecovering && m_curStamina >= m_recoveryThreshold)
        {
            m_isRecovering = false;
        }
    }


    void HandleDash()
    {
        if (m_dashCooldownTimer > 0)
            m_dashCooldownTimer -= Time.deltaTime;

        if (m_isDashing)
        {
            m_dashTimer -= Time.deltaTime;
            if (m_dashTimer <= 0)
            {
                m_isDashing = false;
            }
            return;
        }
        if (
            Input.GetKeyDown(KeyCode.LeftShift) &&
            m_Direction.sqrMagnitude > 0.01f && // 정지 상태 대쉬 방지
            m_curStamina >= m_dashStaminaCost &&
            !m_isRecovering &&
            m_dashCooldownTimer <= 0
        )
        {
            m_isDashing = true;
            m_dashTimer = m_dashDuration;
            m_dashCooldownTimer = m_dashCooldown;
            m_curStamina -= m_dashStaminaCost;

            m_anim.SetTrigger("isDash");
            SoundMgr.Instance.PlayEffSound("SFX_UI_Swipe_Swoosh_Medium_1", 0.2f);

            if (m_curStamina <= 10f)
            {
                m_curStamina = 0f;
                m_isRecovering = true;

            }
        }
    }

    public void Heal(int amount,Vector3 pos)
    {
        m_CurHp += amount;
        if (m_CurHp > m_MaxHp)
            m_CurHp = m_MaxHp;

        GameObject HealInst = (GameObject)Instantiate(m_HealEffect, pos, Quaternion.identity);
        HealInst.GetComponent<ParticleSystem>().Play();
        Destroy(HealInst, 2.0f);

        hpbar.fillAmount = m_CurHp / m_MaxHp;
        GameMgr.inst.m_HpTxt.text = m_CurHp + " / " + m_MaxHp;
    }

}
