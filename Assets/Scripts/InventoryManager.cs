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

    public void OnInventoryItemDragged(string id, Vector2Int gridPosition, bool rotated)
    {
        var inventoryItem = _inventory.Items.Find(item => item.Id == id);

        if (inventoryItem == null)
        {
            return;
        }

        var item = inventoryItem.Item;
        var width = !rotated ? item.Width : item.Height;
        var height = !rotated ? item.Height : item.Width;

        var allowed = _inventory.IsAreaEmptyOrOccupiedByItem(
            gridPosition.x,
            gridPosition.y,
            width,
            height,
            inventoryItem
        );

        _view.MarkCellsArea(gridPosition.x, gridPosition.y, width, height, allowed);
    }

    public void OnInventoryItemDropped(string id, Vector2Int gridPosition, bool rotated)
    {
        if (_inventory.MoveItemByIdTo(id, gridPosition.x, gridPosition.y, rotated))
        {
            var inventoryItem = _inventory.Items.Find(item => item.Id == id);
            _view.PlaceDraggedElementAt(
                inventoryItem.GridPosition.x,
                inventoryItem.GridPosition.y,
                inventoryItem.IsRotated
            );
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
            var itemElement = _view.CreateItemAt(newItem.GridPosition, newItem);
            _view.Stash.AddItemElement(itemElement);
        }
    }
}
