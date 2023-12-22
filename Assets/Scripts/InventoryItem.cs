using UnityEngine;

public class InventoryItem
{
    public string Id;
    public string ItemId;
    public Vector2Int GridPosition;
    public bool IsRotated;

    private InventoryItemSO _item;

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public InventoryItemSO Item
    {
        get
        {
            if (_item == null)
            {
                _item = InventoryManager.Singleton.GetStaticItemById(ItemId);
            }

            return _item;
        }
    }
}
