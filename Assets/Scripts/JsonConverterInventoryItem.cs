using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class JsonConverterInventoryItem : JsonConverter<InventoryItem>
{
    private InventoryManager _inventoryManager;

    public override bool CanWrite => false;

    public override InventoryItem ReadJson(
        JsonReader reader,
        Type objectType,
        InventoryItem existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (_inventoryManager == null)
        {
            _inventoryManager = UnityEngine.Object.FindObjectOfType<InventoryManager>();
        }

        var obj = JObject.Load(reader);

        return Deserialize(obj);
    }

    public override void WriteJson(
        JsonWriter writer,
        InventoryItem value,
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
            backpackInventoryItem.Inventory = new Inventory(
                backpackItem.Width,
                backpackItem.Height
            );

            var jObjectInventory = jObject[nameof(BackpackInventoryItem.Inventory)];

            foreach (
                var innerObject in jObjectInventory[nameof(Inventory.Items)].ToObject<JObject[]>()
            )
            {
                var innerInventoryItem = Deserialize(innerObject);
                backpackInventoryItem.Inventory.AddExistingItemAt(
                    innerInventoryItem,
                    innerInventoryItem.GridPosition.x,
                    innerInventoryItem.GridPosition.y,
                    innerInventoryItem.IsRotated
                );
            }

            inventoryItem = backpackInventoryItem;
        }
        else
        {
            inventoryItem = new InventoryItem();
        }

        inventoryItem.Id = jObject[nameof(InventoryItem.Id)].ToObject<string>();
        inventoryItem.ItemId = staticItemId;
        inventoryItem.GridPosition = jObject[
            nameof(InventoryItem.GridPosition)
        ].ToObject<Vector2Int>();
        inventoryItem.IsRotated = jObject[nameof(InventoryItem.IsRotated)].ToObject<bool>();

        return inventoryItem;
    }
}
