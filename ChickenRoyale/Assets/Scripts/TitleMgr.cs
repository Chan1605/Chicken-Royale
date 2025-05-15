using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TitleMgr : MonoBehaviour
{
    public Image Fadeimage;
    public Button ReAnimBtn;
    public Button PlayBtn;
    public GameObject TitleAnim;
    public GameObject FadePanel;
    float fadeCount = 0;
    [Header("----- Guide Text -----")]
    public TextMeshProUGUI m_Guidetxt;
    public Color color1 = Color.blue;
    public Color color2 = Color.cyan;
    public float colorChangeSpeed;
    public float floatAmplitude;
    public float floatFrequency;
    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;

        if (m_Guidetxt == null)
            m_Guidetxt = GetComponent<TextMeshProUGUI>();

        startPos = m_Guidetxt.rectTransform.anchoredPosition;

        if (ReAnimBtn != null)
            ReAnimBtn.onClick.AddListener(AnimStart);
        if (PlayBtn != null)
            PlayBtn.onClick.AddListener(GameStart);
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 색상 점점 바뀌게 (PingPong 방식)
        float t = Mathf.PingPong(Time.time * colorChangeSpeed, 1f);
        m_Guidetxt.color = Color.Lerp(color1, color2, t);

        // 텍스트 살짝 위아래로 떠다니게
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        m_Guidetxt.rectTransform.anchoredPosition = startPos + new Vector3(0, offsetY, 0);
        if (Input.anyKeyDown)
        {
            LoadingCtrl.LoadScene("Game_Scene_Field");
        }
    }

    void GameStart()
    {
        //StartCoroutine(FadeCoroutine());       
        LoadingCtrl.LoadScene("Game_Scene_Field");
        
    }

    void AnimStart()
    {
        TitleAnim.gameObject.SetActive(true);
    }

    IEnumerator FadeCoroutine()
    {
        FadePanel.SetActive(true);
        fadeCount = 0;
        while (fadeCount < 1.0f)
        {
            fadeCount += 0.01f;
            yield return new WaitForSeconds(0.01f);
            Fadeimage.color = new Color(0, 0, 0, fadeCount);
        }
    }

}
