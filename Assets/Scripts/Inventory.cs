using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory<T> : IInventory
    where T : class, IDynamicInventoryItem
{
    public event Action<Inventory<T>> OnCollectionChanged;

    public readonly List<T> Items = new();
    private readonly T[] _cells;
    private readonly Dictionary<T, int[]> _cellsMap;
    private readonly Grid2D _grid;

    private readonly int _width;
    private readonly int _height;

    IReadOnlyList<IDynamicInventoryItem> IInventory.Items => Items;

    public Func<T> CreateItemFunc;
    public Func<Vector2Int, T> CreateBackpackItemFunc;

    /// <summary>
    /// This constructor is used by Newtonsoft.Json to create empty instance, please avoid of using this constructor.
    /// </summary>
    public Inventory() { }

    public Inventory(int width, int height)
    {
        _width = width;
        _height = height;
        _cells = new T[_width * _height];
        _cellsMap = new Dictionary<T, int[]>();
        _grid = new Grid2D(_width, _height);
    }

    public Inventory(Vector2Int size)
        : this(size.x, size.y) { }

    public bool AddItem(IStaticInventoryItem item, out T newItem)
    {
        newItem = null;

        if (item.Width * item.Height > _width * _height)
        {
            return false;
        }

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var index = _grid.GridToIndex(x, y);

                if (_cells[index] != null)
                {
                    continue;
                }

                if (AddItemAt(item, x, y, false, out newItem))
                {
                    return true;
                }
            }
        }

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var index = _grid.GridToIndex(x, y);

                if (_cells[index] != null)
                {
                    continue;
                }

                if (AddItemAt(item, x, y, true, out newItem))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool AddItemAt(IStaticInventoryItem item, int x, int y, bool rotated, out T newItem)
    {
        newItem = null;

        var width = !rotated ? item.Width : item.Height;
        var height = !rotated ? item.Height : item.Width;

        if (!IsAreaEmpty(x, y, width, height))
        {
            return false;
        }

        if (item is IStaticBackpackInventoryItem backpackStaticItem)
        {
            newItem = CreateBackpackItemFunc(backpackStaticItem.GetBackpackInventorySize());
        }
        else
        {
            newItem = CreateItemFunc();
        }

        newItem.Id = Guid.NewGuid().ToString();
        newItem.GridPosition = new Vector2Int(x, y);
        newItem.ItemId = item.Id;
        newItem.IsRotated = rotated;

        // todo: avoid allocation ?
        var indexes = new int[item.Width * item.Height];
        PopulateIndexes(x, y, width, height, indexes);

        Items.Add(newItem);

        _cellsMap.Add(newItem, indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = newItem;
        }

        DispatchCollectionChangedEvent();

        return true;
    }

    public bool AddExistingItem(T inventoryItem)
    {
        if (inventoryItem == null)
        {
            throw new ArgumentNullException();
        }

        if (Contains(inventoryItem))
        {
            throw new ArgumentException();
        }

        var item = inventoryItem.Item;

        if (item.Width * item.Height > _width * _height)
        {
            return false;
        }

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var index = _grid.GridToIndex(x, y);

                if (_cells[index] != null)
                {
                    continue;
                }

                if (AddExistingItemAt(inventoryItem, x, y, false))
                {
                    return true;
                }
            }
        }

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var index = _grid.GridToIndex(x, y);

                if (_cells[index] != null)
                {
                    continue;
                }

                if (AddExistingItemAt(inventoryItem, x, y, true))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool AddExistingItemAt(T inventoryItem, int x, int y, bool rotated)
    {
        if (inventoryItem == null)
        {
            throw new ArgumentNullException();
        }

        if (Contains(inventoryItem))
        {
            throw new ArgumentException();
        }

        var item = inventoryItem.Item;
        var width = !rotated ? item.Width : item.Height;
        var height = !rotated ? item.Height : item.Width;

        if (!IsAreaEmpty(x, y, width, height))
        {
            return false;
        }

        // todo: avoid allocation ?
        var indexes = new int[item.Width * item.Height];
        PopulateIndexes(x, y, width, height, indexes);

        inventoryItem.GridPosition = new Vector2Int(x, y);
        inventoryItem.IsRotated = rotated;

        Items.Add(inventoryItem);

        _cellsMap.Add(inventoryItem, indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = inventoryItem;
        }

        DispatchCollectionChangedEvent();

        return true;
    }

    public bool IsAreaEmptyOrOccupiedByItem(
        int x,
        int y,
        int width,
        int height,
        IDynamicInventoryItem item = null
    )
    {
        var totalCount = width * height;
        var freeCount = 0;

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var xx = x + i;
                var yy = y + j;

                if (!_grid.IsInside(xx, yy))
                {
                    continue;
                }

                var index = _grid.GridToIndex(xx, yy);

                if (_cells[index] == null || _cells[index] == item)
                {
                    freeCount += 1;
                }
            }
        }

        return freeCount == totalCount;
    }

    public bool IsAreaEmpty(int x, int y, int width, int height)
    {
        var totalCount = width * height;
        var freeCount = 0;

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var xx = x + i;
                var yy = y + j;

                if (!_grid.IsInside(xx, yy))
                {
                    continue;
                }

                var index = _grid.GridToIndex(xx, yy);

                if (_cells[index] == null)
                {
                    freeCount += 1;
                }
            }
        }

        return freeCount == totalCount;
    }

    public bool RemoveItemById(string id, out T inventoryItem)
    {
        inventoryItem = null;

        var itemIndex = Items.FindIndex(item => item.Id == id);
        if (itemIndex == -1)
        {
            return false;
        }

        inventoryItem = Items[itemIndex];
        _cellsMap.Remove(inventoryItem, out var indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = null;
        }

        Items.RemoveAt(itemIndex);

        DispatchCollectionChangedEvent();

        return true;
    }

    public bool MoveItemByIdTo(string id, int x, int y, bool rotated)
    {
        var inventoryItem = Items.Find(item => item.Id == id);

        if (inventoryItem == null)
        {
            return false;
        }

        var item = inventoryItem.Item;
        var width = !rotated ? item.Width : item.Height;
        var height = !rotated ? item.Height : item.Width;

        if (!IsAreaEmptyOrOccupiedByItem(x, y, width, height, inventoryItem))
        {
            return false;
        }

        if (!_cellsMap.TryGetValue(inventoryItem, out var indexes))
        {
            return false;
        }

        foreach (var idx in indexes)
        {
            _cells[idx] = null;
        }

        var gridPosition = new Vector2Int(x, y);

        inventoryItem.GridPosition = gridPosition;

        PopulateIndexes(x, y, width, height, indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = inventoryItem;
        }

        inventoryItem.IsRotated = rotated;

        DispatchCollectionChangedEvent();

        return true;
    }

    public bool Contains(T inventoryItem)
    {
        return Items.Contains(inventoryItem);
    }

    public void Sort()
    {
        var list = new List<T>(Items);
        list.Sort(InventoryItemCompaper);

        Clear();

        for (int i = 0; i < list.Count; i++)
        {
            var added = AddExistingItem(list[i]);

            if (!added)
            {
                Debug.LogWarning(
                    $"Failed to add item {list[i].Id} during sorting process, this should never happen"
                );
            }
        }

        DispatchCollectionChangedEvent();
    }

    public void GetItemsDeeplyNonAlloc(List<IDynamicInventoryItem> results)
    {
        var stack = new Stack<IDynamicBackpackInventoryItem>();

        for (int i = 0; i < Items.Count; i++)
        {
            results.Add(Items[i]);

            if (Items[i] is IDynamicBackpackInventoryItem backpackItem)
            {
                stack.Push(backpackItem);
            }
        }

        while (stack.Count > 0)
        {
            var backpackItem = stack.Pop();

            for (int i = 0; i < backpackItem.Inventory.Items.Count; i++)
            {
                var innerItem = backpackItem.Inventory.Items[i];

                results.Add(innerItem);

                if (innerItem is IDynamicBackpackInventoryItem innerBackpackItem)
                {
                    stack.Push(innerBackpackItem);
                }
            }
        }
    }

    private void Clear()
    {
        _cellsMap.Clear();

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = null;
        }

        Items.Clear();
    }

    private void PopulateIndexes(int x, int y, int width, int height, int[] array)
    {
        var counter = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var xx = x + i;
                var yy = y + j;
                var idx = _grid.GridToIndex(xx, yy);
                array[counter++] = idx;
            }
        }
    }

    private void DispatchCollectionChangedEvent()
    {
        OnCollectionChanged?.Invoke(this);
    }

    private static int InventoryItemCompaper(IDynamicInventoryItem lhs, IDynamicInventoryItem rhs)
    {
        var lhsItem = lhs.Item;
        var rhsItem = rhs.Item;

        var lhsGridSize = lhsItem.GetGridSize();
        var rhsGridSize = rhsItem.GetGridSize();

        var lhsArea = lhsGridSize.x * lhsGridSize.y;
        var rhsArea = rhsGridSize.x * rhsGridSize.y;

        return rhsArea - lhsArea;
    }
}
