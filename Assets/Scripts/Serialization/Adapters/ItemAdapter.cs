using Unity.Collections;
using Unity.Serialization.Binary;
using UnityEngine;

public sealed class ItemAdapter : IBinaryAdapter<InventoryItem>
{
    private readonly InventoryManager _manager;

    public ItemAdapter(InventoryManager manager)
    {
        _manager = manager;
    }

    public unsafe void Serialize(
        in BinarySerializationContext<InventoryItem> context,
        InventoryItem item
    )
    {
        context.Writer->Add(new FixedString64Bytes(item.Id));
        context.Writer->Add(new FixedString64Bytes(item.ItemId));
        context.Writer->Add(item.GridPosition);
        context.Writer->Add(item.IsRotated);

        if (item is BackpackInventoryItem backpack)
        {
            context.Writer->Add(backpack.Inventory.Items.Count);

            foreach (var childItem in backpack.Inventory.Items)
            {
                context.SerializeValue(childItem);
            }
        }
    }

    public unsafe InventoryItem Deserialize(in BinaryDeserializationContext<InventoryItem> context)
    {
        var id = context.Reader->ReadNext<FixedString64Bytes>().ToString();
        var itemId = context.Reader->ReadNext<FixedString64Bytes>().ToString();
        var gridPosition = context.Reader->ReadNext<Vector2Int>();
        var isRotated = context.Reader->ReadNext<bool>();

        InventoryItem item = null;

        var staticItem = _manager.GetStaticItemById(itemId);

        if (staticItem is IStaticBackpackInventoryItem)
        {
            var backpack = new BackpackInventoryItem { Inventory = new TetrisInventory() };

            item = backpack;

            var count = context.Reader->ReadNext<int>();

            for (int i = 0; i < count; i++)
            {
                var childItem = context.DeserializeValue<InventoryItem>();
                backpack.Inventory.Items.Add(childItem);
            }
        }
        else
        {
            item = new InventoryItem();
        }

        item.Id = id;
        item.ItemId = itemId;
        item.GridPosition = gridPosition;
        item.IsRotated = isRotated;

        return staticItem != null ? item : null;
    }
}
