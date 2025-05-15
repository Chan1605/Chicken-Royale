using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScene")
        {
            Debug.Log("타이틀씬 로딩됨, 인게임 오브젝트 삭제 시작");

            // 확실히 존재하는지 체크하고 삭제
            TryDestroy("TPScam");
            TryDestroy("IngameBGM");
            TryDestroy("Player");
            TryDestroy("GameManager"); // GameManager도 씬 내에 있던 거라면 삭제 가능

            // 만약 GameManager를 삭제하면 다시 생성해야 할 수도 있어
        }
    }

    void TryDestroy(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}


