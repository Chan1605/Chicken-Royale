using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using PlayFab;
using PlayFab.ClientModels;

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
    private int m_HighKill = 0;

    [Header("----- UIinfo -----")]
    public TextMeshProUGUI m_GuideText;
    public TextMeshProUGUI m_KillText;
    public TextMeshProUGUI m_Grenadetxt;
    public int m_GranadeCount = 0;
    private int m_MaxGranade = 99;
    float GuideTimer = 0f;
    [Header("----- Buff -----")]
    private float duration;
    private float timer;
    private bool isBuffActive = false;
    public Image m_Buff;
    public Image m_Durimg;
    [HideInInspector] public bool m_Gameover = false;
    public TextMeshProUGUI m_UserName;

    [Header("----- Pause Panel -----")]
    public GameObject m_Panel;
    [Header("----- End Panel -----")]
    public GameObject m_EndPanel;
    public Button m_YesBtn;
    public Button m_NoBtn;
    public TextMeshProUGUI m_EndScoretxt;
    [Header("----- PopUp Panel -----")]
    public GameObject m_Popup;
    public TextMeshProUGUI m_infoText;
    private bool m_isPopup = false;
    private System.Action m_OnPopupConfirm;
    [Header("----- Rank Panel -----")]
    public GameObject m_RankPanel;
    public TextMeshProUGUI m_Rankname;
    public TextMeshProUGUI m_RankScore;
    public TextMeshProUGUI m_MyRankText;
    [SerializeField] private Sprite[] rankSprites;
    [SerializeField] private Image m_MedalImage;
    [SerializeField] private Transform rankContent;
    [SerializeField] private GameObject rankItemPrefab;
    [SerializeField] private Sprite[] m_RankMedals;
    [Header("----- Inventory -----")]
    public GameObject m_InvenPanel;
    public bool m_isInven = false;
    public int Gold { get; private set; }

    [Header("----- Tutorial -----")]
    public CanvasGroup m_tutorialGroup;
    public float m_showDuration = 10f;
    private bool m_hasShown = false;
    [Header("----- SkyBox -----")]
    public Material[] skyboxMaterials;
    [Header("수류탄 아이템 데이터")]
    [SerializeField] private InventoryItemData grenadeData;

    // Start is called before the first frame update
    void Start()
    {       
        m_Curgame = GameState.Start;
        m_GuideText.gameObject.SetActive(false);
        InventoryManager.Inst.AddItem(grenadeData, 10);
        m_GranadeCount = InventoryManager.Inst.GetItemCount("2");
        Gold = 0;
        m_HighScore = PlayerPrefs.GetInt("HighScore", 0);
        m_HighKill = PlayerPrefs.GetInt("HighKill", 0);

        PlayFabLogin.Inst.GetDisplayName((nickname) =>
        {
            m_UserName.text = "이름 : " + nickname;

            ScoreUpdate(0, 0);
        });
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
        if (SoundMgr.Instance != null)
        {
            SoundMgr.Instance.PlayBGM("InGamebgm", 1f);
        }

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

    }

    // Update is called once per frame
    void Update()
    {
        Crossharictrl();
        GuideInfo();
        BuffTimer();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_Curgame == GameState.End || m_isPopup || m_isInven)
                return;

            EscCtr();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (m_Curgame == GameState.Pause)
                return;
            if (m_isPopup)
                HideInvenPanel();
            else
                ShowInvenPanel();
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
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
            if (PlayFabLogin.IsLoggedIn)
            {
                SendScoreToPlayFab(m_HighScore);
                Debug.Log($"로그인 상태: {PlayFabLogin.IsLoggedIn}");
            }
            else
            {
                Debug.Log("아직 로그인되지 않았습니다.");
            }
        }


        m_ScoreTxt.text = "Score : " + m_CurScore;
        m_KillText.text = "Kill Count : " + m_Curkill;
    }

    void SendScoreToPlayFab(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
        {
            new()
            {
                StatisticName = "HighScore",
                Value = score
            }
        }
        };

        PlayFab.PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => Debug.Log("PlayFab 점수 업로드 성공"),
            error => Debug.LogError("PlayFab 점수 업로드 실패: " + error.GenerateErrorReport()));
    }

    void SendKillToPlayFab(int killCount)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
        {
            new()
            {
                StatisticName = "HighKill",
                Value = killCount
            }
        }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => { Debug.Log("킬 카운트 업데이트 성공"); },
            error => { Debug.LogError("킬 카운트 업데이트 실패: " + error.GenerateErrorReport()); });

    }


    void Crossharictrl()
    {
        if (PlayerCtrl.inst.m_isDie || m_Curgame == GameState.End || m_Curgame == GameState.Pause || m_isInven)
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
        m_GranadeCount = count;
        m_Grenadetxt.text = $"{m_GranadeCount}"+ " / "+ $"{m_MaxGranade}";
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
            .SetUpdate(true)
            .SetEase(Ease.OutBack);
        Time.timeScale = 0.0f;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

    }

    public void Resume()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
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
        m_EndScoretxt.text = "<color=white>Score : " + m_CurScore + "</color>\n" +
    "<color=yellow>Best Score : " + m_HighScore + "</color>";
        if (m_Curkill > m_HighKill)
        {
            m_HighKill = m_Curkill;
            PlayerPrefs.SetInt("HighKill", m_HighKill);
            if (PlayFabLogin.IsLoggedIn)
            {           
                SendKillToPlayFab(m_HighKill);
            }
        }
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void TitleBack()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_NoBtn.transform.DOShakePosition(0.3f, 10, 20, 90, false, true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                ClearIngameObjects();
                LoadingCtrl.LoadScene("TitleScene");
            });
    }
    public void ReGame()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_YesBtn.transform.DOScale(1.2f, 0.1f)
    .SetEase(Ease.OutQuad)
    .SetUpdate(true)
    .OnComplete(() =>
    {
        m_YesBtn.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InQuad);
        Time.timeScale = 1f;
        m_Curgame = GameState.Start;
        m_esc = false;
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

    public void ShowPopupPanel(string infoMessage, System.Action onConfirm)
    {
        m_isPopup = true;
        m_infoText.text = infoMessage;
        m_OnPopupConfirm = onConfirm;  // 콜백 저장

        m_Popup.SetActive(true);
        m_Popup.transform.localScale = Vector3.zero;

        m_Popup.transform.DOScale(Vector3.one, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack);

        Time.timeScale = 0.0f;
    }

    public void OnClick_TitleButton()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        ShowPopupPanel("타이틀로 돌아가시겠습니까?", TitleBack);
    }
    public void OnClick_ReGameButton()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        ShowPopupPanel("게임을 재시작 합니다!", ReGame);
    }

    public void OnClick_ResetScoreButton()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        ShowPopupPanel("최고점수가 삭제 됩니다!", RemoveScore);
    }
    public void OnClick_RankButton()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_RankPanel.SetActive(true);
        m_isPopup = true;
        m_RankPanel.transform.localScale = Vector3.zero;

        GetLeaderboard();         //전체 랭킹
        UpdateRankPanelInfo();    //본인 순위 

        m_RankPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack);
    }
    public void HideRankPanel()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_RankPanel.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           m_RankPanel.SetActive(false);
           m_isPopup = false;
       });
    }

    public void ShowInvenPanel()
    {
        m_InvenPanel.SetActive(true); // 반드시 활성화
        m_InvenPanel.transform.localScale = Vector3.zero;

        m_InvenPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack);

        m_isPopup = true;
        m_isInven = true;
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0.0f;
    }

    public void HideInvenPanel()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_InvenPanel.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           m_isPopup = false;
           m_isInven = false;
            Time.timeScale = 1.0f;
           if(m_Curgame == GameState.End)
               return;
           Cursor.lockState = CursorLockMode.Locked;
           Cursor.visible = false;        
       });
    }



    void UpdateRankPanelInfo()
    {
        if (PlayFabLogin.IsLoggedIn)
        {
            Debug.Log($"로그인 상태: {PlayFabLogin.IsLoggedIn}");
            // 닉네임 불러오기
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                string nickname = result.AccountInfo.TitleInfo.DisplayName;
                m_Rankname.text = $"{nickname}";
            },
            error =>
            {
                Debug.LogError("닉네임 불러오기 실패: " + error.GenerateErrorReport());
                m_Rankname.text = $"Unknown";
            });

            // 점수 불러오기
            PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            result =>
            {
                var stat = result.Statistics.Find(s => s.StatisticName == "HighScore");
                int highScore = stat != null ? stat.Value : 0;

                var highKillStat = result.Statistics.Find(s => s.StatisticName == "HighKill");
                int highKill = highKillStat != null ? highKillStat.Value : 0;


                m_RankScore.text = $"Kill Count : {highKill}\n {highScore}";
            },
            error =>
            {
                Debug.LogError("점수 불러오기 실패: " + error.GenerateErrorReport());
                m_RankScore.text = "0";
            });
            PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = "HighScore",
                MaxResultsCount = 1
            },
            result =>
            {
                int myRank = result.Leaderboard[0].Position + 1;
                m_MyRankText.text = myRank.ToString();
                if (myRank <= 3)
                {
                    m_MedalImage.gameObject.SetActive(true);
                    m_MedalImage.sprite = m_RankMedals[myRank - 1]; // 1등은 0번 인덱스
                    switch (myRank)
                    {
                        case 1:
                            m_MedalImage.rectTransform.sizeDelta = new Vector2(124, 108);
                            break;
                        case 2:
                            m_MedalImage.rectTransform.sizeDelta = new Vector2(111, 108);
                            break;
                        case 3:
                            m_MedalImage.rectTransform.sizeDelta = new Vector2(82, 108);
                            break;
                    }
                }
                else
                {
                    m_MedalImage.gameObject.SetActive(false);
                    m_MyRankText.gameObject.SetActive(true);
                }
            },
            error =>
            {
                Debug.LogError("자기 순위 불러오기 실패: " + error.GenerateErrorReport());
                m_MyRankText.text = "-";
                m_MedalImage.gameObject.SetActive(false);
            });
        }
        else
        {
            m_Rankname.text = $"Unknown";
            m_RankScore.text = "0";
            m_MyRankText.text = "-";
        }
    }

    public void GetLeaderboard()
    {
        var highScoreRequest = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        var killCountRequest = new GetLeaderboardRequest
        {
            StatisticName = "HighKill",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        Dictionary<string, int> killCountDict = new Dictionary<string, int>();

        // 먼저 KillCount 랭킹 가져오기
        PlayFabClientAPI.GetLeaderboard(killCountRequest,
        killResult =>
        {
            foreach (var item in killResult.Leaderboard)
            {
                if (!killCountDict.ContainsKey(item.PlayFabId))
                    killCountDict.Add(item.PlayFabId, item.StatValue);
            }

            // 그 다음 HighScore 랭킹 가져오기
            PlayFabClientAPI.GetLeaderboard(highScoreRequest,
            result =>
            {
                foreach (Transform child in rankContent)
                    Destroy(child.gameObject); // 기존 항목 제거

                foreach (var item in result.Leaderboard)
                {
                    GameObject entry = Instantiate(rankItemPrefab, rankContent);
                    var entryUI = entry.GetComponent<RankEntryUI>();

                    Sprite trophySprite = null;
                    if (item.Position < 3)
                        trophySprite = rankSprites[item.Position];

                    int killCount = killCountDict.ContainsKey(item.PlayFabId) ? killCountDict[item.PlayFabId] : 0;

                    entryUI.SetRankInfo(
                        rank: item.Position + 1,
                        playerName: item.DisplayName,
                        score: item.StatValue,
                        kill: killCount,
                        trophySprite: trophySprite
                    );
                }
            },
            error =>
            {
                Debug.LogError("HighScore 랭킹 불러오기 실패: " + error.GenerateErrorReport());
            });
        },
        error =>
        {
            Debug.LogError("KillCount 랭킹 불러오기 실패: " + error.GenerateErrorReport());
        });
    }


    public void OnClick_PopupOK()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_OnPopupConfirm.Invoke();   // null 체크 후 콜백 실행
        HidePopupPanel();
    }

    public void HidePopupPanel()
    {
        SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
        m_Popup.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           m_Popup.SetActive(false);
           m_isPopup = false;
           m_infoText.text = "";
       });
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
        m_Popup.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           m_Popup.SetActive(false);
           m_isPopup = false;
           SoundMgr.Instance.PlayEffSound("SFX_UI_Button_Click_Settings_2", 0.5f);
           PlayerPrefs.DeleteKey("HighScore");
           PlayerPrefs.DeleteKey("HighKill");
           ShowGuide("기록이 초기화 됐습니다!", 5f);
       });
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
