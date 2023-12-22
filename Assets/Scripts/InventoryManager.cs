using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Singleton { get; private set; }

    [SerializeField]
    private InventoryView _view;

    [SerializeField]
    private Vector2Int _gridSize;

    [SerializeField]
    private List<InventoryItemSO> _allItems;

    [SerializeField]
    private InventoryItemSO _debugItem;

    private Inventory _inventory;

    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

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

    public InventoryItemSO GetStaticItemById(string id)
    {
        return _allItems.Find(item => item.Id == id);
    }

    public InventoryItem GetDynamicItemById(string id)
    {
        // todo: search inner inventories

        return _inventory.Items.Find(item => item.Id == id);
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
            _view.RemoveItemElementByDynamicId(id);
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
