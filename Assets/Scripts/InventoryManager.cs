using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private InventoryView _view;

    [SerializeField]
    private Vector2Int _gridSize;

    [SerializeField]
    private InventoryItemSO _debugItem;

    private Inventory _inventory;

    private void Start()
    {
        _inventory = new Inventory(_gridSize);

        _view.CreateGrid(_gridSize);

        AddItem(_debugItem);
        AddItem(_debugItem);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddDebugItem();
        }
    }

    [ContextMenu(nameof(AddDebugItem))]
    private void AddDebugItem()
    {
        AddItem(_debugItem);
    }

    public void OnInventoryItemClicked(string id)
    {
        if (_inventory.RemoveItemById(id))
        {
            _view.RemoveItemElementByUniqueId(id);
        }
    }

    public void OnInventoryItemDragged(string id, Vector2Int gridPosition)
    {
        var inventoryItem = _inventory.Items.Find(item => item.Id == id);

        if (inventoryItem == null)
        {
            return;
        }

        var w = inventoryItem.Item.Width;
        var h = inventoryItem.Item.Height;

        var allowed = _inventory.IsAreaEmptyOrOccupiedByItem(
            gridPosition.x,
            gridPosition.y,
            w,
            h,
            inventoryItem
        );

        _view.MarkCellsArea(gridPosition.x, gridPosition.y, w, h, allowed);
    }

    public void OnInventoryItemDropped(string id, Vector2Int gridPosition)
    {
        if (_inventory.MoveItemByIdTo(id, gridPosition.x, gridPosition.y))
        {
            var inventoryItem = _inventory.Items.Find(item => item.Id == id);
            _view.PlaceDraggedElementAt(inventoryItem.GridPosition.x, inventoryItem.GridPosition.y);
        }
        else
        {
            _view.ResetDraggedElement();
        }
    }

    private void AddItem(InventoryItemSO item)
    {
        if (_inventory.AddItem(item, out var newItem))
        {
            _view.CreateItemAt(newItem.GridPosition, newItem, newItem.IsRotated);
        }
    }
}
