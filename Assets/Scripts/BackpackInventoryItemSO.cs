using UnityEngine;

[CreateAssetMenu(fileName = "New BackpackInventoryItemSO", menuName = "SO/BackpackInventoryItemSO")]
public sealed class BackpackInventoryItemSO : InventoryItemSO, IStaticBackpackInventoryItem
{
    [Header("Backpack")]
    [SerializeField]
    private Vector2Int _backpackGridSize;

    public int BackpackWidth => _backpackGridSize.x;
    public int BackpackHeight => _backpackGridSize.y;
}
