using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public sealed class NewtonsoftJsonSerialization : ISerialization
{
    private JsonConverter[] _jsonConverters = null;

    public NewtonsoftJsonSerialization(InventoryManager manager)
    {
        var list = new List<JsonConverter>
        {
            new JsonConverterVector2Int(),
            new JsonConverterInventoryItem(manager),
            new JsonConverterTetrisInventory()
        };

        _jsonConverters = list.ToArray();
    }

    public byte[] Serialize(TetrisInventory inventory)
    {
        var json = JsonConvert.SerializeObject(
            inventory,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                Converters = _jsonConverters,
            }
        );

        var bytes = Encoding.UTF8.GetBytes(json);

        return bytes;
    }

    public TetrisInventory Deserialize(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);

        var inventory = JsonConvert.DeserializeObject<TetrisInventory>(json, _jsonConverters);

        return inventory;
    }
}
