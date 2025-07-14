using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] Image Backimag;
    [SerializeField] Image iconImage;
    public TMP_Text countText;

    private InventoryItemData itemData;


    [SerializeField] private ItemBGSpriteDB bgSpriteDB;

    public void SetSlot(InventoryItemData data)
    {
        itemData = data;
        iconImage.sprite = data.icon;
        countText.text = data.quantity.ToString();
        if (Backimag != null && bgSpriteDB != null)
            Backimag.sprite = bgSpriteDB.GetBGSprite(data.itemType);

    }

    public void SetData(InventoryItemData data)
    {
        itemData = data;
        iconImage.sprite = data.icon;
        countText.text = data.quantity.ToString();
    }

    public void UpdateSlot(InventoryItemData data)
    {
        countText.text = data.quantity.ToString();
    }

    public void OnClick()
    {
        InventoryUIManager.Inst.ShowItemDetail(itemData);
    }
}
