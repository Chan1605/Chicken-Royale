using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class GameMgr : MonoBehaviour
{
    public enum GameState { Start, Pause, End };
    public GameState m_Curgame = GameState.Start;
    [HideInInspector] public static GameMgr inst;
    [SerializeField] public bool m_esc = false;
    [Header("----- Player -----")]
    public TextMeshProUGUI m_HpTxt;
    public GameObject m_Crosshair;
    public GameObject[] m_Coin;

    [Header("----- Enemy Spawn -----")]
    public List<EnemyCtrl> enemies = new List<EnemyCtrl>();  // 적 리스트
    public Transform[] m_Points;
    public GameObject[] m_EnemyPrefabs;
    public float m_CreateTime = 1.0f;
    public List<GameObject> m_EnemyPool = new List<GameObject>();
    public int m_Maxenemy = 15;
    [Header("----- Score -----")]
    public TextMeshProUGUI m_ScoreTxt;
    [SerializeField] int m_CurScore = 0;
    [SerializeField] int m_Curkill = 0;
    private int m_HighScore = 0;

    [Header("----- UIinfo -----")]
    public TextMeshProUGUI m_GuideText;
    public TextMeshProUGUI m_KillText;
    public TextMeshProUGUI m_Grenadetxt;
    public int m_GranadeCount = 0;
    float GuideTimer = 0f;
    [Header("----- Buff -----")]
    private float duration;
    private float timer;
    private bool isBuffActive = false;
    public Image m_Buff;
    public Image m_Durimg;
    [HideInInspector] public bool m_Gameover = false;

    [Header("----- Pause Panel -----")]
    public GameObject m_Panel;
    public Button m_TitleBtn;
    public Button m_Reset;
    [Header("----- End Panel -----")]
    public GameObject m_EndPanel;
    public Button m_YesBtn;
    public Button m_NoBtn;
    public TextMeshProUGUI m_EndScoretxt;
    [Header("----- Tutorial -----")]
    public CanvasGroup m_tutorialGroup;
    public float m_showDuration = 10f;
    private bool m_hasShown = false;
    [Header("----- SkyBox -----")]
    public Material[] skyboxMaterials;

    // Start is called before the first frame update
    void Start()
    {
        m_Curgame = GameState.Start;
        m_GuideText.gameObject.SetActive(false);
        m_GranadeCount = 10;
        m_HighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (skyboxMaterials.Length > 0)
        {
            int index = Random.Range(0, skyboxMaterials.Length);
            Material selectedSkybox = skyboxMaterials[index];

            Debug.Log("Selected Skybox: " + selectedSkybox.name);

            RenderSettings.skybox = selectedSkybox;
            DynamicGI.UpdateEnvironment();
        }
        if (!m_hasShown)
        {
            ShowTutorial();
            m_hasShown = true;
        }

        SoundMgr.Instance.PlayBGM("InGamebgm", 1f);
        ScoreUpdate(0, 0);
        GreGuide(0);
        m_Crosshair.SetActive(false);
        m_Points = GameObject.Find("EnemyGroup").GetComponentsInChildren<Transform>();
        for (int i = 0; i < m_Maxenemy; i++)
        {
            int RandIdx = Random.Range(0, m_EnemyPrefabs.Length);
            GameObject Ranprefab = m_EnemyPrefabs[RandIdx];
            GameObject monster = Instantiate(Ranprefab);
            monster.name = "Enemy" + i.ToString();
            monster.SetActive(false);
            m_EnemyPool.Add(monster);
        }
        if (m_Points.Length > 0)
        {
            if (m_Curgame != GameState.Start)
                return;

            StartCoroutine(this.CreateMonster());
        }
        if (m_TitleBtn != null)
            m_TitleBtn.onClick.AddListener(TitleBack);
        if (m_Reset != null)
            m_Reset.onClick.AddListener(RemoveScore);

    }

    // Update is called once per frame
    void Update()
    {
        Crossharictrl();
        GuideInfo();
        BuffTimer();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_Curgame == GameState.End)
                return;

            EscCtr();
        }
    }


    void OnEnable()
    {
        if (inst == null)
        {
            inst = this;
        }
        else if (inst != this)
        {
            Destroy(gameObject);
            return;
        }

        // UI 등 재할당
        if (m_HpTxt == null)
            m_HpTxt = GameObject.Find("HpTxt").GetComponent<TextMeshProUGUI>();

        if (m_ScoreTxt == null)
            m_ScoreTxt = GameObject.Find("ScoreTxt").GetComponent<TextMeshProUGUI>();

        if (m_GuideText == null)
            m_GuideText = GameObject.Find("GuideText").GetComponent<TextMeshProUGUI>();

        if (m_Crosshair == null)
            m_Crosshair = GameObject.Find("Crosshair");

        if (m_TitleBtn == null)
        {
            GameObject panel = GameObject.Find("PausePanel");
            if (panel != null)
                m_TitleBtn = panel.transform.Find("TitleBtn").GetComponent<Button>();
        }

        if (m_TitleBtn != null)
            m_TitleBtn.onClick.AddListener(TitleBack);
    }


    public void BuffTimer()
    {
        if (!isBuffActive)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            isBuffActive = false;
            m_Buff.gameObject.SetActive(false);
        }

        m_Durimg.fillAmount = timer / duration;
    }

    public void StartBuff(float buffDuration)
    {
        duration = buffDuration;
        timer = buffDuration;
        isBuffActive = true;
        m_Durimg.fillAmount = 1.0f;
        m_Buff.gameObject.SetActive(true);
    }


    public void ScoreUpdate(int score, int kill)
    {
        m_CurScore += score;
        m_Curkill += kill;
        if (m_CurScore < 0)
        {
            m_CurScore = 0;
        }
        else if (m_Curkill < 0)
        {
            m_Curkill = 0;
        }
        if (m_CurScore > m_HighScore)
        {
            m_HighScore = m_CurScore;
            PlayerPrefs.SetInt("HighScore", m_HighScore);
            PlayerPrefs.Save();
        }

        m_ScoreTxt.text = "Score : " + m_CurScore; //+ "\n HighScore : " + m_HighScore;
        m_KillText.text = "잡은 곰 : " + m_Curkill;
    }


    void Crossharictrl()
    {
        if (PlayerCtrl.inst.m_isDie || GameMgr.inst.m_Curgame == GameMgr.GameState.End)
        {
            m_Crosshair.SetActive(false);
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            m_Crosshair.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_Crosshair.SetActive(false);
        }
    }
    public void ShowGuide(string message, float duration = 1.0f)
    {
        m_GuideText.text = message;
        m_GuideText.gameObject.SetActive(true);
        GuideTimer = duration;
    }

    public void GreGuide(int count)
    {
        m_GranadeCount += count;
        if (m_GranadeCount < 0)
            m_GranadeCount = 0;

        m_Grenadetxt.text = "x " + m_GranadeCount;
    }

    public void GuideInfo()
    {
        if (!m_GuideText.gameObject.activeSelf)
            return;

        GuideTimer -= Time.deltaTime;
        if (GuideTimer <= 0.0f)
        {
            m_GuideText.text = "";
            m_GuideText.gameObject.SetActive(false);
            GuideTimer = 0.0f;
        }
    }

    public void IsPause()
    {
        m_Curgame = GameState.Pause;
        m_esc = true;

        m_Panel.SetActive(true);
        m_Panel.transform.localScale = Vector3.zero;
        // 타임스케일 멈추기 전에 애니메이션 실행
        m_Panel.transform.DOScale(Vector3.one, 0.5f)
            .SetUpdate(true)  // ⬅️ 이걸 써야 타임스케일 0에서도 애니메이션이 작동함
            .SetEase(Ease.OutBack);
        Time.timeScale = 0.0f;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

    }

    public void Resume()
    {
        m_Panel.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           OnPauseOpened();
           m_Panel.SetActive(false);
           Time.timeScale = 1f;
           m_Curgame = GameState.Start;
           m_esc = false;
           Cursor.lockState = CursorLockMode.Locked;
           Cursor.visible = false;
       });

    }

    void EscCtr()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_esc)
            {
                Resume();
            }
            else
            {
                IsPause();
                OnPauseOpened();
            }
        }
    }
    public void GameOver()
    {
        ShowGameOverPanel();
        m_ScoreTxt.gameObject.SetActive(false);
        m_EndPanel.SetActive(true);
        m_YesBtn.onClick.AddListener(ReGame);
        m_NoBtn.onClick.AddListener(TitleBack);
        m_EndScoretxt.text = "<color=blue>획득 점수 : " + m_CurScore + "</color>\n" +
    "<color=red>최고 점수 : " + m_HighScore + "</color>";
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    void TitleBack()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_NoBtn.transform.DOShakePosition(0.3f, 10, 20, 90, false, true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                ClearIngameObjects();
                LoadingCtrl.LoadScene("TitleScene");
                // 타이틀로
            });
    }
    void ReGame()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_YesBtn.transform.DOScale(1.2f, 0.1f)
    .SetEase(Ease.OutQuad)
    .SetUpdate(true)
    .OnComplete(() =>
    {
        m_YesBtn.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InQuad);
        ClearIngameObjects();
        LoadingCtrl.LoadScene("Game_Scene_Field");
    });
    }

    void ClearIngameObjects()
    {
        if (GameMgr.inst != null)
            Destroy(GameMgr.inst.gameObject);

        if (SoundMgr.Instance != null)
            Destroy(SoundMgr.Instance.gameObject);

        TPScam cam = FindObjectOfType<TPScam>();
        if (cam != null)
            Destroy(cam.gameObject);
    }

    void ShowGameOverPanel()
    {
        m_EndPanel.transform.localScale = Vector3.zero;
        m_EndPanel.SetActive(true);

        // 부드럽게 커지며 등장 (0.5초 동안)
        m_EndPanel.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
    }


    public IEnumerator CreateMonster()
    {

        while (!m_Gameover)
        {

            yield return new WaitForSeconds(m_CreateTime);//몬스터 생성 주기 시간만큼 메인 루프에 양보


            if (m_Curgame == GameState.End)   //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
                yield break;

            foreach (GameObject monster in m_EnemyPool)
            {
                if (!monster.activeSelf) //비활성화 여부로 사용 가능한 몬스터를 판단
                {
                    int idx = Random.Range(1, m_Points.Length);//몬스터를 출현시킬 위치의 인덱스값을 추출 
                    monster.transform.position = m_Points[idx].position; //몬스터의 출현위치를 설정            
                    monster.SetActive(true);  //몬스터를 활성화함               
                    monster.GetComponent<EnemyCtrl>().InitMonster();
                    break; //오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
                }

            }

        }
    }

    public void RegisterEnemy(EnemyCtrl enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyCtrl enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
    }

    public void NotifyPlayerDied()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.OnPlayerDie();
        }
    }

    public void ShowTutorial()
    {
        // m_tutorialGroup.alpha = 0;
        m_tutorialGroup.alpha = 1f;
        m_showDuration = 10;
        m_tutorialGroup.gameObject.SetActive(true);
        m_tutorialGroup.DOFade(1, 1f);


        // 10초 후 자동 숨김
        DOVirtual.DelayedCall(m_showDuration, () =>
        {
            HideTutorial();
        });
    }

    public void HideTutorial()
    {
        m_tutorialGroup.DOFade(0, 1f).OnComplete(() =>
        {
            m_tutorialGroup.gameObject.SetActive(false);
        });
    }

    public void OnPauseOpened()
    {
        ShowTutorial(); // ESC 누르면 다시 보기
    }

    public void RemoveScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        ShowGuide("기록이 초기화 됐습니다!", 5f);
    }

    public static bool IsPointerOverUIObject() //UGUI가 클릭되지 않도록 설정
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
       List<RaycastResult> results = new List<RaycastResult>();
       for (int i = 0; i < Input.touchCount; ++i)
       {
            a_EDCurPos.position = Input.GetTouch(i).position;  
            results.Clear();
            EventSystem.current.RaycastAll(a_EDCurPos, results);

            if (0 < results.Count)
                return true;
       }

       return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif

    }


}
