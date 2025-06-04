using System;
using UnityEngine;

public sealed class TetrisInventory : Inventory<InventoryItem>
{
    public event Action<TetrisInventory> OnInventoryChanged;

    public TetrisInventory()
    {
        Ctor();
    }

    public TetrisInventory(Vector2Int gridSize)
        : base(gridSize)
    {
        Ctor();
    }

    public TetrisInventory(int width, int height)
        : base(width, height)
    {
        Ctor();
    }

    private void Ctor()
    {
        CreateItemFunc = () => new InventoryItem();

        CreateBackpackItemFunc = (gridSize) =>
        {
            return new BackpackInventoryItem { Inventory = new TetrisInventory(gridSize) };
        };

        OnCollectionChanged += Inventory_OnCollectionChanged;
    }

    private void Inventory_OnCollectionChanged(Inventory<InventoryItem> inventory)
    {
        OnInventoryChanged?.Invoke(this);
    }
}
