using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Inst;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI AttText;
    [SerializeField] private TextMeshProUGUI DefText;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image BackImg;
    [SerializeField] private GameObject sellButton;
    [SerializeField] private GameObject AllsellButton;
    [SerializeField] private TMP_Text priceText;
    [Header("Background Sprite DB")]
    [SerializeField] private ItemBGSpriteDB bgSpriteDB;
    private InventoryItemData currentItem;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        sellButton.GetComponent<Button>().onClick.AddListener(OnClickSellItem);
        AllsellButton.GetComponent<Button>().onClick.AddListener(OnClickSellAll);
    }



    public void OnClickSellItem()
    {
        if (currentItem == null)
        {
            Debug.Log("현재 아이템이 null입니다");
            return;
        }
        int sellPrice = currentItem.sellPrice;//GetSellPrice(currentItem); // 가격 계산
        currentItem.quantity--;
        // 골드 증가
        GameMgr.inst.AddGold(sellPrice);
        InventoryManager.Inst.UpdateGoldUI();

        if (currentItem.quantity <= 0)
        {
            string itemIdToRemove = currentItem.itemId;
            currentItem = null;

            InventoryManager.Inst.RemoveItem(itemIdToRemove);
            detailPanel.SetActive(false);
        }
        else
        {
            InventoryManager.Inst.RefreshUI();         // UI 슬롯 갱신
            ShowItemDetail(currentItem);               // 디테일 갱신
        }

    }

    public void OnClickSellAll()
    {
        SellAllOfCurrentItem();
    }
    public void SellAllOfCurrentItem()
    {
        if (currentItem == null)
        {
            Debug.Log("판매할 아이템이 없습니다.");
            return;
        }

        int quantity = currentItem.quantity;
        int sellPrice = currentItem.sellPrice;

        int totalGold = sellPrice * quantity;

        // 골드 추가
        GameMgr.inst.AddGold(totalGold);

        // 아이템 제거
        string itemIdToRemove = currentItem.itemId;
        currentItem = null;

        InventoryManager.Inst.RemoveItem(itemIdToRemove);
        InventoryManager.Inst.UpdateGoldUI();

        detailPanel.SetActive(false);
        Debug.Log($"'{itemIdToRemove}' {quantity}개를 판매하고 {totalGold}골드를 얻었습니다.");
    }

    private int GetSellPrice(InventoryItemData item)
    {
        // 임시 예시: 등급별 가격
        return item.itemType switch
        {
            ItemType.Purple => 1000,
            ItemType.Green => 500,
            ItemType.Brown => 100,
            _ => 10
        };
    }


    public void ShowItemDetail(InventoryItemData data)
    {
        if (data == null)
            return;

        currentItem = data;
        nameText.text = data.itemName;
        descText.text = data.description;
        AttText.text = $"{data.attack}";
        DefText.text = $"{data.defense}";
        int sellPrice = GetSellPrice(data);
        priceText.text = $"{sellPrice}";

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        if (BackImg != null && bgSpriteDB != null)
        {
            BackImg.sprite = bgSpriteDB.GetBGSprite(data.itemType);
        }

        switch (data.behaviorType)
        {
            case ItemBehaviorType.Use:
                //useButton.SetActive(true);
                sellButton.SetActive(false);
                AllsellButton.SetActive(false);
                break;

            case ItemBehaviorType.Sell:
                //useButton.SetActive(false);
                sellButton.SetActive(true);
                AllsellButton.SetActive(true);
                break;
            case ItemBehaviorType.None:
                //useButton.SetActive(false);
                sellButton.SetActive(false);
                AllsellButton.SetActive(false);
                break;
        }

        bool showStats = data.behaviorType == ItemBehaviorType.None;
        AttText.transform.parent.gameObject.SetActive(showStats);
        DefText.transform.parent.gameObject.SetActive(showStats);

        detailPanel.SetActive(true);
    }


}
