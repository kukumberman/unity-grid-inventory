using System.Collections.Generic;

public interface IInventory
{
    IReadOnlyList<IDynamicInventoryItem> Items { get; }

    void Sort();

    void GetItemsDeeplyNonAlloc(List<IDynamicInventoryItem> items);
}
