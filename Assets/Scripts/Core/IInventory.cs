using System.Collections.Generic;

public interface IInventory
{
    IReadOnlyList<IDynamicInventoryItem> Items { get; }
}
