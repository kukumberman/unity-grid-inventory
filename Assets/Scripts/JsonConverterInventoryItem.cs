using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class JsonConverterInventoryItem : JsonConverter<IDynamicInventoryItem>
{
    private readonly InventoryManager _inventoryManager;

    public JsonConverterInventoryItem(InventoryManager manager)
    {
        _inventoryManager = manager;
    }

    public override bool CanWrite => false;

    public override IDynamicInventoryItem ReadJson(
        JsonReader reader,
        Type objectType,
        IDynamicInventoryItem existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var obj = JObject.Load(reader);

        return Deserialize(obj);
    }

    public override void WriteJson(
        JsonWriter writer,
        IDynamicInventoryItem value,
        JsonSerializer serializer
    )
    {
        return;
    }

    private InventoryItem Deserialize(JObject jObject)
    {
        // todo: avoid this type of deserialization method
        // 1. jObject.ToObject<BackpackInventoryItem>() - crashes editor
        // - sometimes StackOverflowException is shown when debugging
        // - but in locals tab just null
        // 2. jObject[key] is case sensitive

        var staticItemId = jObject[nameof(InventoryItem.ItemId)].ToObject<string>();
        var staticItem = _inventoryManager.GetStaticItemById(staticItemId);

        InventoryItem inventoryItem;

        if (staticItem is BackpackInventoryItemSO backpackItem)
        {
            var backpackInventoryItem = new BackpackInventoryItem();
            backpackInventoryItem.Inventory = new TetrisInventory(
                backpackItem.BackpackGridSize.x,
                backpackItem.BackpackGridSize.y
            );

            var jObjectInventory = jObject[nameof(BackpackInventoryItem.Inventory)];

            foreach (
                var innerObject in jObjectInventory[nameof(IInventory.Items)].ToObject<JObject[]>()
            )
            {
                var innerInventoryItem = Deserialize(innerObject);
                var added = backpackInventoryItem.Inventory.AddExistingItemAt(
                    innerInventoryItem,
                    innerInventoryItem.GridPosition.x,
                    innerInventoryItem.GridPosition.y,
                    innerInventoryItem.IsRotated
                );

                if (!added)
                {
                    Debug.LogWarning(innerInventoryItem.Id);
                }
            }

            inventoryItem = backpackInventoryItem;
        }
        else
        {
            inventoryItem = new InventoryItem();
        }

        inventoryItem.Id = jObject[nameof(InventoryItem.Id)].ToObject<string>();
        inventoryItem.ItemId = staticItemId;
        inventoryItem.GridPosition = jObject[nameof(InventoryItem.GridPosition)]
            .ToObject<Vector2Int>();
        inventoryItem.IsRotated = jObject[nameof(InventoryItem.IsRotated)].ToObject<bool>();

        return inventoryItem;
    }
}
