using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Singleton { get; private set; }

    [SerializeField]
    private UnityEvent<InventoryItem> _onItemRemoved;

    [SerializeField]
    private InventoryView _view;

    [SerializeField]
    private Vector2Int _gridSize;

    [SerializeField]
    private InventoryItemCollectionSO _itemCollection;

    [SerializeField]
    private bool _loadOnStart;

    private Inventory _inventory;

    public Inventory RootInventory => _inventory;
    public InventoryItemCollectionSO ItemCollection => _itemCollection;

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
        _view.Stash.Bind(_inventory);
        // todo: temp solution, it should be null since it is referenced as "destinationInventoryId" and compared to null is this class
        _view.Stash.DynamicId = null;

        if (_loadOnStart)
        {
            Load();
        }
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
        return _itemCollection.Items.Find(item => item.Id == id);
    }

    public InventoryItem GetDynamicItemById(string id)
    {
        return GetDynamicItemById(id, out var _);
    }

    public InventoryItem GetDynamicItemById(string id, out Inventory parentInventory)
    {
        // todo: search inner inventories (Breadth-first search) (needs testing)
        parentInventory = null;

        var itemInStash = _inventory.Items.Find(item => item.Id == id);

        if (itemInStash != null)
        {
            parentInventory = _inventory;
            return itemInStash;
        }

        var queue = new Queue<BackpackInventoryItem>();

        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            if (_inventory.Items[i] is BackpackInventoryItem backpackItem)
            {
                queue.Enqueue(backpackItem);
            }
        }

        while (queue.Count > 0)
        {
            var backpackItem = queue.Dequeue();

            for (int i = 0; i < backpackItem.Inventory.Items.Count; i++)
            {
                var innerItem = backpackItem.Inventory.Items[i];

                if (innerItem.Id == id)
                {
                    parentInventory = backpackItem.Inventory;
                    return innerItem;
                }

                if (innerItem is BackpackInventoryItem innerBackpackItem)
                {
                    queue.Enqueue(innerBackpackItem);
                }
            }
        }

        return null;
    }

    [ContextMenu(nameof(AddDebugItem))]
    private void AddDebugItem()
    {
        AddItem(_itemCollection.Items[0]);
    }

    public bool TryRemoveItem(string id)
    {
        var dynamicItem = GetDynamicItemById(id, out var parentInventory);

        if (dynamicItem == null)
        {
            return true;
        }

        if (parentInventory.RemoveItemById(id, out var removedItem))
        {
            _onItemRemoved.Invoke(removedItem);
            return true;
        }

        return false;
    }

    public bool TransferItemToInventory(string dynamicItemId, string destinationInventoryId)
    {
        var inventoryItem = GetDynamicItemById(dynamicItemId, out var parentInventory);

        if (inventoryItem == null)
        {
            return false;
        }

        Inventory destinationInventory;

        if (destinationInventoryId != null)
        {
            var dynamicItemDropTarget = GetDynamicItemById(destinationInventoryId);

            if (
                dynamicItemDropTarget == null
                || dynamicItemDropTarget is not BackpackInventoryItem backpackItem
            )
            {
                return false;
            }

            destinationInventory = backpackItem.Inventory;
        }
        else
        {
            destinationInventory = _inventory;
        }

        if (destinationInventory.Contains(inventoryItem))
        {
            Debug.LogWarning("can't transfer item from same inventory (ok?)");
            return false;
        }

        if (destinationInventory.AddExistingItem(inventoryItem))
        {
            if (parentInventory.RemoveItemById(inventoryItem.Id, out var _))
            {
                return true;
            }
            else
            {
                Debug.Log("this should never happen");
            }
        }

        return false;
    }

    public bool MoveItemToInventory(
        string dynamicItemId,
        string destinationInventoryId,
        Vector2Int gridPosition,
        bool rotated
    )
    {
        var inventoryItem = GetDynamicItemById(dynamicItemId, out var parentInventory);

        if (inventoryItem == null)
        {
            return false;
        }

        Inventory destinationInventory;

        if (destinationInventoryId != null)
        {
            var dynamicItemDropTarget = GetDynamicItemById(destinationInventoryId);

            if (
                dynamicItemDropTarget == null
                || dynamicItemDropTarget is not BackpackInventoryItem backpackItem
            )
            {
                return false;
            }

            if (inventoryItem == backpackItem)
            {
                return false;
            }

            destinationInventory = backpackItem.Inventory;
        }
        else
        {
            destinationInventory = _inventory;
        }

        if (parentInventory == destinationInventory)
        {
            return parentInventory.MoveItemByIdTo(
                dynamicItemId,
                gridPosition.x,
                gridPosition.y,
                rotated
            );
        }
        else
        {
            if (
                destinationInventory.AddExistingItemAt(
                    inventoryItem,
                    gridPosition.x,
                    gridPosition.y,
                    rotated
                )
            )
            {
                if (parentInventory.RemoveItemById(inventoryItem.Id, out var _))
                {
                    return true;
                }
                else
                {
                    Debug.Log("this should never happen");
                }
            }
        }

        return false;
    }

    public bool AddItem(InventoryItemSO item)
    {
        return _inventory.AddItem(item, out var _);
    }

    #region Serialize / Deserialize
    private string JsonStringifyInventory()
    {
        return JsonConvert.SerializeObject(
            _inventory,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            }
        );
    }

    [ContextMenu(nameof(Save))]
    public void Save()
    {
        var path = GetSavePath();
        var json = JsonStringifyInventory();
        File.WriteAllText(path, json);
    }

    [ContextMenu(nameof(Load))]
    public void Load()
    {
        var path = GetSavePath();
        if (!File.Exists(path))
        {
            return;
        }

        var json = File.ReadAllText(path);
        var uninitializedInventory = JsonConvert.DeserializeObject<Inventory>(json);

        _inventory = new Inventory(_gridSize);

        for (int i = 0; i < uninitializedInventory.Items.Count; i++)
        {
            var item = uninitializedInventory.Items[i];
            _inventory.AddExistingItemAt(
                item,
                item.GridPosition.x,
                item.GridPosition.y,
                item.IsRotated
            );
        }

        if (Application.isPlaying)
        {
            _view.Stash.Bind(_inventory);
            _view.Stash.Sync();
        }
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "inventory.json");
    }
    #endregion
}
