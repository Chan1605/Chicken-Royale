using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    [SerializeField] private Image trophyImage;
    [SerializeField] private Image rankImage;       

    public void SetRankInfo(int rank, string playerName, int score, int kill,Sprite trophySprite)
    {
        nameText.text = playerName;
        scoreText.text = $"Kill Count : {kill}\n" + score.ToString();
        trophyImage.sprite = trophySprite;

        if (rank <= 3)
        {
            rankText.gameObject.SetActive(false);
            rankImage.gameObject.SetActive(true);
            rankImage.sprite = trophySprite;

            switch (rank)
            {
                case 1:
                    rankImage.rectTransform.sizeDelta = new Vector2(124, 108);
                    break;
                case 2:
                    rankImage.rectTransform.sizeDelta = new Vector2(111, 108);
                    break;
                case 3:
                    rankImage.rectTransform.sizeDelta = new Vector2(82, 108);
                    break;
            }
        }
        else
        {
            rankImage.gameObject.SetActive(false);
            rankText.gameObject.SetActive(true);
            rankText.text = rank.ToString();

            rankImage.rectTransform.sizeDelta = new Vector2(124, 108);
        }
    }




}
