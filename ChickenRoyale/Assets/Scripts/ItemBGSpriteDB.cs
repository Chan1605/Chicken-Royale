using UnityEngine;

[CreateAssetMenu(fileName = "ItemBGSpriteDB", menuName = "Inventory/Item BG Sprite DB")]
public class ItemBGSpriteDB : ScriptableObject
{
    [System.Serializable]
    public struct BGSpriteEntry
    {
        public ItemType type;
        public Sprite bgSprite;
    }

    public BGSpriteEntry[] entries;

    public Sprite GetBGSprite(ItemType type)
    {
        foreach (var entry in entries)
        {
            if (entry.type == type)
                return entry.bgSprite;
        }
        return null;
    }
}
