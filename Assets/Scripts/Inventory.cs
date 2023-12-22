using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Inventory
{
    public List<InventoryItem> Items = new();
    private readonly InventoryItem[] _cells;
    private readonly Dictionary<InventoryItem, int[]> _cellsMap;
    private readonly Grid2D _grid;

    private readonly int _width;
    private readonly int _height;

    public Inventory(int width, int height)
    {
        _width = width;
        _height = height;
        _cells = new InventoryItem[_width * _height];
        _cellsMap = new Dictionary<InventoryItem, int[]>();
        _grid = new Grid2D(_width, _height);
    }

    public Inventory(Vector2Int size)
        : this(size.x, size.y) { }

    public bool AddItem(InventoryItemSO item, out InventoryItem newItem)
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

                if (AddItemAt(item, x, y, out newItem))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool AddItemAt(InventoryItemSO item, int x, int y, out InventoryItem newItem)
    {
        newItem = null;

        var maxCount = item.Width * item.Height;
        var indexes = new List<int>();

        for (var i = 0; i < item.Width; i++)
        {
            for (var j = 0; j < item.Height; j++)
            {
                var xx = x + i;
                var yy = y + j;

                if (xx >= _width || yy >= _height)
                {
                    continue;
                }

                var index = _grid.GridToIndex(xx, yy);
                if (_cells[index] != null)
                {
                    continue;
                }

                indexes.Add(index);
            }
        }

        if (indexes.Count != maxCount)
        {
            return false;
        }

        if (item is BackpackInventoryItemSO backpackStaticItem)
        {
            var backpackItem = new BackpackInventoryItem
            {
                Inventory = new Inventory(
                    backpackStaticItem.BackpackWidth,
                    backpackStaticItem.BackpackHeight
                )
            };
            newItem = backpackItem;
        }
        else
        {
            newItem = new InventoryItem();
        }

        newItem.Id = Guid.NewGuid().ToString();
        newItem.GridPosition = new Vector2Int(x, y);
        newItem.ItemId = item.Id;

        Items.Add(newItem);

        // todo: avoid allocation
        _cellsMap.Add(newItem, indexes.ToArray());

        foreach (var idx in indexes)
        {
            _cells[idx] = newItem;
        }

        return true;
    }

    public bool IsAreaEmptyOrOccupiedByItem(
        int x,
        int y,
        int width,
        int height,
        InventoryItem item = null
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

    public bool RemoveItemById(string id)
    {
        var itemIndex = Items.FindIndex(item => item.Id == id);
        if (itemIndex == -1)
        {
            return false;
        }

        var inventoryItem = Items[itemIndex];
        _cellsMap.Remove(inventoryItem, out var indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = null;
        }

        Items.RemoveAt(itemIndex);

        return true;
    }

    public bool MoveItemByIdTo(string id, int x, int y)
    {
        var inventoryItem = Items.Find(item => item.Id == id);

        if (inventoryItem == null)
        {
            return false;
        }

        var w = inventoryItem.Item.Width;
        var h = inventoryItem.Item.Height;

        if (!IsAreaEmptyOrOccupiedByItem(x, y, w, h, inventoryItem))
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

        inventoryItem.GridPosition.x = x;
        inventoryItem.GridPosition.y = y;

        PopulateIndexes(x, y, w, h, indexes);

        foreach (var idx in indexes)
        {
            _cells[idx] = inventoryItem;
        }

        return true;
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
}
