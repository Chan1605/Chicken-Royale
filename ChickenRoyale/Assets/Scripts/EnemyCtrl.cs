using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEditor;

public class EnemyCtrl : MonoBehaviour
{
    public enum Monstate { idle, patrol, trace, attack, hit, die };
    [SerializeField] Monstate m_MonSt = Monstate.idle;
    public enum MonType { Bear, Dino };
    [SerializeField] private Renderer[] m_Renders;
    [SerializeField] private Material[] m_newMaterials;
    public MonType m_MonType;
    private Animator m_Anim;
    private bool m_isDie = false;
    private Transform m_target;
    private Transform m_montr;
    private Rigidbody m_rigid;
    private float m_AI_Delay = 0f;

    [Header("----- Monster info -----")]
    public float m_MaxHp = 100;
    public float m_CurHp;
    public int m_Eattackdamage = 1;
    [SerializeField] private float m_monspeed = 7.0f;
    [SerializeField] private float m_RotSpeed = 6.0f;
    [SerializeField] private float m_attackdist = 15f;
    [SerializeField] private float m_tracedist = 20f;
    [SerializeField] private float m_curdelay = 0f;
    [SerializeField] private float m_maxdelay = 1f;
    private int m_DeathCount = 0;
    [Header("----- Blood Effect -----")]
    public GameObject m_BloodDecal;
    public GameObject m_BloodEffect;
    [Header("----- MonBullet -----")]
    public GameObject[] m_EnemyBull; // 적 총알 오브젝트
    private int m_EnemyBulletIndex = 0;
    public Transform m_Enemytip; // 총구 위치
    [Header("----- Patrol-----")]
    [SerializeField] private Transform WayPoints;
    private int curpos = 0;
    //private NavMeshAgent m_Nav;
    private Image hpBar;
    [Header("----- Sell ItemData -----")]
    public List<DropItemInfo> dropItemList = new List<DropItemInfo>();
    public int dropAmount = 1;
    [System.Serializable]
    public class DropItemInfo
    {
        public InventoryItemData itemData;
        [Range(0f, 1f)]
        public float dropChance; 
    }

    void Awake()
    {
        m_Anim = GetComponent<Animator>();

        m_montr = GetComponent<Transform>();

        m_target = GameObject.FindWithTag("Player").GetComponent<Transform>();

        m_rigid = GetComponent<Rigidbody>();

        //m_Nav = GetComponent<NavMeshAgent>();

    }

    // Start is called before the first frame update
    void Start()
    {
        m_CurHp = m_MaxHp;
        m_curdelay = 0f;
        hpBar.GetComponent<Image>().fillAmount = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDie)
            return;

        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
            return;

        if (m_target.gameObject.activeSelf == false)
            m_target = GameObject.FindWithTag("Player").GetComponent<Transform>();

        CheckMonStateUpdate();
        MonStateUpdate();

        // if (m_Nav.remainingDistance <= 0.2f)
        // {
        //     curpos++;
        //     if (curpos >= WayPoints.childCount)
        //     {
        //         curpos = 0;
        //     }
        //     m_Nav.SetDestination(WayPoints.GetChild(curpos).position);
        // }

    }

    public void InitMonster()
    {
        m_CurHp = m_MaxHp;           // 체력 초기화
        m_isDie = false;
        m_MonSt = Monstate.idle;     // 기본 상태로

        HpbarCtrl hpbarCtrl = FindObjectOfType<HpbarCtrl>();
        hpbarCtrl.Register(this.transform, this);
        hpBar.GetComponent<Image>().fillAmount = 1.0f;

        switch (m_DeathCount)
        {
            case >= 2:
                m_EnemyBulletIndex = 2;
                m_maxdelay = 0.1f;
                m_Eattackdamage = 20;
                m_tracedist = 100f;
                m_attackdist = 20f;
                m_monspeed = 10f;

                ChangeToDeadMaterials();
                break;
            case >= 1:
                m_EnemyBulletIndex = 1;
                m_maxdelay = 0.2f;
                m_Eattackdamage = 10;
                m_tracedist = 50f;
                m_monspeed = 8f;
                m_attackdist = 17f;
                ChangeTintColor(Color.red);
                break;
            default:
                m_EnemyBulletIndex = 0;
                m_maxdelay = 0.3f;
                m_Eattackdamage = 1;
                break;
        }
    }

    // private IEnumerator EnemyAttackBullet(int bulletIndex)
    // {
    //     m_EnemyBulletIndex = bulletIndex;   

    //     yield return null;
    // }

    public void SetHpBar(Image hpBarImage)
    {
        hpBar = hpBarImage;
        UpdateHpBar();
    }


    void CheckMonStateUpdate()
    {
        if (m_isDie)
            return;

        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        float m_dist = Vector3.Distance(m_target.position, m_montr.position);

        if (m_dist <= m_attackdist) //공격 사거리 안
        {
            m_MonSt = Monstate.attack;
            //m_Nav.isStopped = true;
        }
        else if (m_dist <= m_tracedist)//추적 사거리 안
        {
            m_MonSt = Monstate.trace;
            //m_Nav.isStopped = false;
        }
        else
        {
            m_MonSt = Monstate.idle;
        }
    }


    void OnCollisionEnter(Collision coll)
    {
        if (m_MonSt == Monstate.die || m_isDie)
            return;

        if (coll.gameObject.tag == "BULL")
        {

            TakeDamage(PlayerCtrl.inst.m_attackdamage);
            m_Anim.SetTrigger("isHit");
            CreateBloodEffect(this.transform.position);

            //StartCoroutine(ResetHitCoroutine());
        }
    }


    void ChangeTintColor(Color color)
    {
        if (m_Renders == null || m_Renders.Length == 0)
            return;

        foreach (Renderer renderer in m_Renders)
        {
            if (renderer == null) continue;

            Material mat = renderer.material;

            if (mat.HasProperty("PBRMaskTint"))
            {
                mat.SetColor("PBRMaskTint", color);
            }
            else if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
            else if (mat.HasProperty("_TintColor"))
            {
                mat.SetColor("_TintColor", color);
            }
            else
            {
                Debug.LogWarning($"[{renderer.name}] Tint 속성 없음");
            }
        }
    }


    public void ChangeToDeadMaterials()
    {
        for (int i = 0; i < m_Renders.Length && i < m_newMaterials.Length; i++)
        {
            if (m_Renders[i] != null && m_newMaterials[i] != null)
            {
                m_Renders[i].material = m_newMaterials[i];
            }
        }
    }

    IEnumerator ResetHitCoroutine()
    {
        yield return null;//new WaitForSeconds(0.5f);
        m_Anim.SetTrigger("isHit");
    }

    void MonStateUpdate()
    {
        if (m_isDie)
            return;

        switch (m_MonSt)
        {
            case Monstate.idle:
                // if (m_Nav.remainingDistance <= 0.2f)
                // {
                //     curpos++;
                //     if (curpos >= WayPoints.childCount)
                //     {
                //         curpos = 0;
                //     }
                //     m_Nav.SetDestination(WayPoints.GetChild(curpos).position);
                // }
                m_Anim.SetBool("isTrace", false);
                break;
            case Monstate.trace:
                {
                    m_Anim.SetBool("isTrace", true);
                    Vector3 m_MoveDir = Vector3.zero;
                    m_MoveDir = m_target.transform.position - m_montr.transform.position;
                    m_MoveDir.y = 0;

                    Vector3 a_Monstep = (m_MoveDir.normalized *
                                               Time.deltaTime * m_monspeed);
                    transform.Translate(a_Monstep, Space.World);

                    if (0.1f < m_MoveDir.magnitude)
                    {
                        Quaternion a_TargetRot;
                        float m_RotSpeed = 7.0f;
                        a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                        transform.rotation =
                            Quaternion.Slerp(transform.rotation, a_TargetRot,
                                             Time.deltaTime * m_RotSpeed);
                    }

                    m_Anim.SetBool("isTrace", true);
                }
                break;
            case Monstate.attack:
                {
                    m_curdelay += Time.deltaTime;
                    if (m_curdelay < m_maxdelay)
                        return;
                    m_curdelay = 0f;
                    if (m_MonType == MonType.Dino)
                    {
                        DinoAttack();

                    }
                    else if (m_MonType == MonType.Bear)
                    {
                        BearAttack();
                    }
                }
                break;
        }
    }

    public void TakeDamage(int a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Value;
        if (m_CurHp <= 0.0f)
            m_CurHp = 0.0f;
        if (hpBar != null)
        {
            UpdateHpBar();
        }
        if (m_CurHp <= 0.0f)
        {
            EnemyDie();

            if (GameMgr.inst.m_Coin != null)
            {
                int idx = Random.Range(0, GameMgr.inst.m_Coin.Length);
                GameObject a_ItemObj = Instantiate(GameMgr.inst.m_Coin[idx]);
                a_ItemObj.transform.position = this.transform.position + Vector3.up;

                Itempickup pickup = a_ItemObj.GetComponent<Itempickup>();
                if (pickup != null && pickup.itemData != null)
                {
                    switch (pickup.itemData.itemId)
                    {
                        case "0": //코인
                            pickup.amount = (m_DeathCount >= 2) ? 200 : (m_DeathCount == 1 ? 150 : 100);
                            break;
                        case "1": //다이아
                            pickup.amount = 1;
                            break;
                        case "2": //수류탄
                            pickup.amount = 1;
                            break;
                        case "3": //힐
                            pickup.amount = 1;
                            break;
                    }
                }

                Destroy(a_ItemObj, 10f);
            }
            StartCoroutine(DisableAfterDelay(1.0f)); // 2초 뒤에 비활성화
        }
    }

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    void CreateBloodEffect(Vector3 pos)
    {
        GameObject blood1 =
        (GameObject)Instantiate(m_BloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = m_montr.position + (Vector3.up * 0.05f);
        //데칼의 회전값을 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //데칼 프리팹 생성
        GameObject blood2 = (GameObject)Instantiate(m_BloodDecal, decalPos, decalRot);
        //데칼의 크기도 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //3초 후에 혈흔효과 프리팹을 삭제
        Destroy(blood2, 3.0f);
    }
    public void BearShoot()
    {
        float dist = Vector3.Distance(m_target.position, m_montr.position);

        if (dist <= m_attackdist)  // 공격 사거리 체크
        {
            GameObject bullet = Instantiate(m_EnemyBull[m_EnemyBulletIndex], m_Enemytip.position, m_Enemytip.rotation);
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
            {
                enemyBullet.SetDamage(this.m_Eattackdamage);  // 적 자신의 공격력 넣기
            }
        }
    }

    void BearAttack()
    {
        m_Anim.SetBool("isTrace", false);
        m_Anim.SetTrigger("fire");

        Vector3 a_CacVLen = m_target.position - m_montr.position;
        a_CacVLen.y = 0.0f;
        Quaternion a_TargetRot =
                    Quaternion.LookRotation(a_CacVLen.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                  a_TargetRot, Time.deltaTime * m_RotSpeed);

    }

    void DinoAttack()
    {
        m_Anim.SetBool("isTrace", false);
        m_Anim.SetBool("isAttack", true);

        Vector3 a_CacVLen = m_target.position - m_montr.position;
        a_CacVLen.y = 0.0f;
        Quaternion a_TargetRot =
                    Quaternion.LookRotation(a_CacVLen.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                  a_TargetRot, Time.deltaTime * m_RotSpeed);
    }

    private void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = m_CurHp / m_MaxHp;
        }
    }

    void OnEnable()
    {
        GameMgr.inst.RegisterEnemy(this);
    }

    void OnDisable()
    {
        GameMgr.inst.UnregisterEnemy(this);
    }

    public void OnPlayerDie()
    {
        if (m_isDie == true)
            return;

        StopAllCoroutines();

        m_Anim.SetTrigger("IsPlayerDie");
    }

    void DropRandomItem()
    {
        if (dropItemList == null || dropItemList.Count == 0) 
        return;

        float totalChance = 0f;
        foreach (var drop in dropItemList)
            totalChance += drop.dropChance;

        float rand = Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var drop in dropItemList)
        {
            cumulative += drop.dropChance;
            if (rand <= cumulative)
            {
                InventoryManager.Inst.AddItem(drop.itemData, dropAmount);
                break;
            }
        }
    }

    void EnemyDie()
    {
        m_isDie = true;
        m_MonSt = Monstate.die;
        m_Anim.SetTrigger("isDie");
        if (m_DeathCount >= 1)
            GameMgr.inst.ScoreUpdate(150, 1);
        else
            GameMgr.inst.ScoreUpdate(100, 1);
        m_DeathCount++;

        DropRandomItem(); 

        m_Anim.StopPlayback();

    }


}
