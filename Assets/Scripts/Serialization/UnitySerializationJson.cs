using System.Text;
using Unity.Serialization.Json;

public sealed class UnitySerializationJson : ISerialization
{
    public byte[] Serialize(TetrisInventory inventory)
    {
        var json = JsonSerialization.ToJson(
            inventory,
            new JsonSerializationParameters { Minified = false }
        );

        var bytes = Encoding.UTF8.GetBytes(json);

        return bytes;
    }

    public TetrisInventory Deserialize(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);

        var inventory = JsonSerialization.FromJson<TetrisInventory>(json);

        return inventory;
    }
}
