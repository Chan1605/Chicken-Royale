using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayFabLogin : MonoBehaviour
{
    public static bool IsLoggedIn { get; private set; } = false;
    public static PlayFabLogin Inst;
    public TextMeshProUGUI Logininfotxt;

    void Awake()
    {
        if (!Inst)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }


    public void LoginAndSetDisplayName(string displayName, System.Action onComplete = null)
    {
        PlayFabSettings.TitleId = "1EF77A";

        string customId;
        if (PlayerPrefs.HasKey("MyCustomId"))
        {
            customId = PlayerPrefs.GetString("MyCustomId");
        }
        else
        {
            customId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("MyCustomId", customId);
        }

        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CreateAccount = true,
            CustomId = customId
        },
        success =>
        {
            Debug.Log("PlayFab 로그인 성공");

            IsLoggedIn = true;
            CheckAccountValidity();

            if (success.NewlyCreated)
            {
                Debug.Log("신규 계정이 생성되었습니다.");
            }

            SetDisplayName(displayName, onComplete);
        },
        failure =>
        {
            Debug.LogError("PlayFab 로그인 실패: " + failure.GenerateErrorReport());

            // 문제가 있는 CustomId는 삭제
            PlayerPrefs.DeleteKey("MyCustomId");
            PlayerPrefs.Save();

            // 에러 메시지를 UI에 표시할 수도 있음
            if (Logininfotxt != null)
            {
                Logininfotxt.text = "로그인 실패: 다시 실행해주세요.";
            }

            IsLoggedIn = false;
        });
    }

    public void SetDisplayName(string newName, System.Action onComplete = null)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log("닉네임 설정 성공: " + result.DisplayName);

                // 닉네임 저장
                PlayerPrefs.SetInt("IsNicknameSet", 1);
                PlayerPrefs.SetString("MyNickname", newName);

                onComplete?.Invoke();
            },
         error =>
         {
             Debug.LogError("닉네임 설정 실패: " + error.GenerateErrorReport());

             string message = "닉네임 설정 실패";

             // 실패 사유에 따라 분기
             if (error.Error == PlayFabErrorCode.InvalidParams)
             {
                 if (request.DisplayName.Length < 3)
                     message = "닉네임은 최소 3자 이상이어야 합니다!";
                 else if (request.DisplayName.Contains(" "))
                     message = "닉네임에 공백(스페이스)을 포함할 수 없습니다!";
                 else
                     message = "유효하지 않은 닉네임입니다.";
             }
             else if (error.Error == PlayFabErrorCode.NameNotAvailable)
             {
                 message = "이미 사용 중인 닉네임입니다!";
             }

             Logininfotxt.text = message;
         });
    }

    public void GetDisplayName(System.Action<string> onSuccess)
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                string nickname = result.AccountInfo.TitleInfo.DisplayName;
                onSuccess?.Invoke(nickname); // null 체크 추가
            },
            error =>
            {
                Debug.LogError("닉네임 가져오기 실패: " + error.GenerateErrorReport());
            });
    }

    void CheckAccountValidity()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                Debug.Log("PlayFab 계정 유효: " + result.AccountInfo.PlayFabId);
            },
            error =>
            {
                Debug.LogError("PlayFab 계정 무효 또는 삭제됨: " + error.GenerateErrorReport());
            });
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("IsNicknameSet");
        PlayerPrefs.DeleteKey("MyNickname");
        //PlayerPrefs.DeleteAll();
    }

}

