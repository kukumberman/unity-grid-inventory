using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItemSO", menuName = "SO/InventoryItemSO")]
public sealed class InventoryItemSO : ScriptableObject
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private Vector2Int _gridSize;

    [SerializeField]
    private Sprite _sprite;

    public string Id => _id;
    public int Width => _gridSize.x;
    public int Height => _gridSize.y;
    public Sprite Sprite => _sprite;
    public Vector2Int GridSize => _gridSize;
}
