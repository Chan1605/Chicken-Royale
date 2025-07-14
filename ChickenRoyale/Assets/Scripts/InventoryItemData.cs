using UnityEngine;

public enum ItemType
{
    Yellow,
    Green,
    Purple,
    Blue,
    Brown
}

public enum ItemBehaviorType
{
    Use,  
    Sell, 
    None  
}


[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public int quantity;
    public ItemType itemType;
    public string description;
    public int attack;
    public int defense;
    public int sellPrice;
    public ItemBehaviorType behaviorType;
}
