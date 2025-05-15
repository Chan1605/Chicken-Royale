using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingCtrl : MonoBehaviour
{
    static string nextScene;
    [SerializeField] Image m_Loadimg;  
    [SerializeField] TextMeshProUGUI m_Tiptxt;
    [SerializeField] Sprite[] m_Randimg;
    public List<string> m_texts = new List<string>();
    

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Awake() 
    {
        m_Loadimg.fillAmount = 0f;
    }
    void Start()
    {        
        StartCoroutine(LoadScene());        

        m_Tiptxt.text = m_texts[Random.Range(0,m_texts.Count)];
        int idx = Random.Range(0,m_Randimg.Length);  
        GetComponentInChildren<Image>().sprite = m_Randimg[idx];

    }


    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        float mintimer = 3f;
        while (!op.isDone)
        {
            yield return null;

            if (op.progress < 0.9f)
            {
                m_Loadimg.fillAmount = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / mintimer);
                m_Loadimg.fillAmount = progress;//Mathf.Lerp(0.0f, 1.0f, timer);
                if (timer >= mintimer)//(m_Loadimg.fillAmount >= 1.0f)
                {
                    op.allowSceneActivation = true;            
                    yield break;
                }
            }
        }

    }
}
