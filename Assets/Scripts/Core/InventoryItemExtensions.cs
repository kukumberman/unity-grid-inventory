using UnityEngine;

public static class InventoryItemExtensions
{
    public static Vector2Int GetGridSize(this IStaticInventoryItem item)
    {
        return new Vector2Int(item.Width, item.Height);
    }

    public static Vector2Int GetBackpackInventorySize(this IStaticBackpackInventoryItem item)
    {
        return new Vector2Int(item.BackpackWidth, item.BackpackHeight);
    }

    public static Sprite GetSprite(this IDynamicInventoryItem item)
    {
        var staticItem = item.Item as InventoryItemSO;

        if (staticItem == null)
        {
            return null;
        }

        return staticItem.Sprite;
    }
}
