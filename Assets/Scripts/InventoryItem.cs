using Newtonsoft.Json;
using UnityEngine;

public class InventoryItem : IDynamicInventoryItem
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

    [JsonIgnore]
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

    string IDynamicInventoryItem.Id
    {
        get => Id;
        set => Id = value;
    }

    string IDynamicInventoryItem.ItemId
    {
        get => ItemId;
        set => ItemId = value;
    }

    Vector2Int IDynamicInventoryItem.GridPosition
    {
        get => GridPosition;
        set => GridPosition = value;
    }

    bool IDynamicInventoryItem.IsRotated
    {
        get => IsRotated;
        set => IsRotated = value;
    }

    IStaticInventoryItem IDynamicInventoryItem.Item => Item;
}
