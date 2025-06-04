public sealed class BackpackInventoryItem : InventoryItem, IDynamicBackpackInventoryItem
{
    public TetrisInventory Inventory;

    IInventory IDynamicBackpackInventoryItem.Inventory => Inventory;
}
