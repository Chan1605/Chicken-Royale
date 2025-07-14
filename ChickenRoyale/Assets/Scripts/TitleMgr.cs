using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;


public class TitleMgr : MonoBehaviour
{
    [Header("----- Guide Text -----")]
    public TextMeshProUGUI m_Guidetxt;
    public Color color1 = Color.blue;
    public Color color2 = Color.cyan;
    public float colorChangeSpeed;
    public float floatAmplitude;
    public float floatFrequency;
    private Vector3 startPos;
    public GameObject nameInputPanel;
    [SerializeField] public TMP_InputField nameInput;
    private bool isLoading = false;
    private bool isSceneLoading = false;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;

        if (m_Guidetxt == null)
            m_Guidetxt = GetComponent<TextMeshProUGUI>();

        startPos = m_Guidetxt.rectTransform.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // 색상 변화 및 떠다니는 효과 유지
        float t = Mathf.PingPong(Time.time * colorChangeSpeed, 1f);
        m_Guidetxt.color = Color.Lerp(color1, color2, t);
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        m_Guidetxt.rectTransform.anchoredPosition = startPos + new Vector3(0, offsetY, 0);

        if (Input.anyKeyDown && !nameInputPanel.activeSelf && !isLoading)
        {
            isLoading = true;
            // 닉네임이 이미 저장되어 있다면 → 바로 게임 씬으로
            if (PlayerPrefs.HasKey("MyNickname"))
            {
                
                string savedNickname = PlayerPrefs.GetString("MyNickname");
                PlayFabLogin.Inst.LoginAndSetDisplayName(savedNickname, () =>
                {
                    LoadingCtrl.LoadScene("Game_Scene_Field");
                });
            }
            else
            {
                // 최초 진입 시에만 패널 표시
                ShowLoginPanel();
                isLoading = false;                
            }
        }
    }


    void OnEnable()
    {
        if (PlayFabLogin.Inst != null)
        {
            var text = GetComponent<TextMeshProUGUI>();
            if (text != null)
                PlayFabLogin.Inst.Logininfotxt = text;
        }
    }

    public void OnClick_StartGame()
    {
        if (isSceneLoading) 
            return;
        EventSystem.current.SetSelectedGameObject(null); // 포커스 강제 해제

        string finalName = nameInput.text;

        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogWarning("닉네임을 입력해주세요.");
            return;
        }
        
        
        PlayFabLogin.Inst.LoginAndSetDisplayName(finalName, () =>
        {
            isSceneLoading = true;
            // 닉네임 저장
            PlayerPrefs.SetString("MyNickname", nameInput.text);    

            LoadingCtrl.LoadScene("Game_Scene_Field");
        });

    }

    public void HidePanel()
    {
        nameInputPanel.transform.DOScale(Vector3.zero, 0.3f)
       .SetUpdate(true)
       .SetEase(Ease.InBack)
       .OnComplete(() =>
       {
           nameInputPanel.SetActive(false);
           m_Guidetxt.gameObject.SetActive(true);
           nameInput.text = "";
           PlayFabLogin.Inst.Logininfotxt.text = "닉네임을 입력해주세요 !";
       });
    }


    public void ShowLoginPanel()
    {
        nameInputPanel.SetActive(true);
        m_Guidetxt.gameObject.SetActive(false);


        nameInputPanel.transform.localScale = Vector3.zero;

        nameInputPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack);

    }



}
