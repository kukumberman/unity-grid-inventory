using UnityEngine;

public interface IDynamicInventoryItem
{
    string Id { get; set; }

    string ItemId { get; set; }

    Vector2Int GridPosition { get; set; }

    bool IsRotated { get; set; }

    IStaticInventoryItem Item { get; }
}
