using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Inst;
    private readonly Dictionary<string, InventoryItemData> inventory = new();
    private readonly Dictionary<string, InventorySlotUI> slotUIs = new();
    [Header("----- UI -----")]
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;
    public TextMeshProUGUI[] CurGold;

    void Awake()
    {
        if (Inst == null)
            Inst = this;
        else
            Destroy(gameObject); // 중복 방지
    }
    public void AddGold(int amount)
    {
        GameMgr.inst.AddGold(amount);
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        foreach (var text in CurGold)
        {
            if (text != null)
                text.text = GameMgr.inst.Gold.ToString("N0");
        }
    }
    public void AddItem(InventoryItemData newItem, int amount = 1)
    {
        if (inventory.ContainsKey(newItem.itemId))
        {
            inventory[newItem.itemId].quantity += amount;
            slotUIs[newItem.itemId].UpdateSlot(inventory[newItem.itemId]);
        }
        else
        {
            InventoryItemData copy = Instantiate(newItem);
            copy.quantity = amount;
            inventory.Add(copy.itemId, copy);

            GameObject slot = Instantiate(slotPrefab, slotParent);
            InventorySlotUI ui = slot.GetComponent<InventorySlotUI>();
            ui.SetSlot(copy);

            slotUIs.Add(copy.itemId, ui);
        }

        if (newItem.itemId == "2")
        {
            GameMgr.inst.GreGuide(inventory["2"].quantity);
        }
    }

    public void RefreshUI()
    {
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        foreach (InventoryItemData item in inventory.Values)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            go.GetComponent<InventorySlotUI>().SetSlot(item);
        }
    }

    public void RemoveItem(string itemId)
    {
        if (!inventory.ContainsKey(itemId))
        {
            return;
        }

        if (inventory.ContainsKey(itemId))
        {
            inventory.Remove(itemId);
        }

        if (slotUIs.ContainsKey(itemId))
        {
            InventorySlotUI ui = slotUIs[itemId];
            slotUIs.Remove(itemId);
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
        }
        RefreshUI();
    }

    public int GetItemCount(string itemId)
    {
        if (inventory.ContainsKey(itemId))
            return inventory[itemId].quantity;
        return 0;
    }

    public void UseItem(string itemId, int amount = 1)
    {
        if (!inventory.ContainsKey(itemId))
            return;

        inventory[itemId].quantity -= amount;
        if (inventory[itemId].quantity <= 0)
        {
            RemoveItem(itemId);
        }
        else  // 남아 있다면 UI만 갱신
        {
            slotUIs[itemId].UpdateSlot(inventory[itemId]);
        }
        if (itemId == "2") // 인게임 UI 수류탄 수량 동기화
        {
            GameMgr.inst.GreGuide(GetItemCount("2")); // 없으면 0 반환
        }
    }


}
