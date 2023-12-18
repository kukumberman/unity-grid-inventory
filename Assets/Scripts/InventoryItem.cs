using UnityEngine;

public sealed class InventoryItem
{
    public string Id;
    public InventoryItemSO Item;
    public Vector2Int GridPosition;
    public bool IsRotated;

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
