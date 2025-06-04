using System;
using Newtonsoft.Json;

public sealed class JsonConverterTetrisInventory : JsonConverter<TetrisInventory>
{
    public override bool CanRead => false;

    public override TetrisInventory ReadJson(
        JsonReader reader,
        Type objectType,
        TetrisInventory existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        return null;
    }

    public override void WriteJson(
        JsonWriter writer,
        TetrisInventory value,
        JsonSerializer serializer
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Items");

        writer.WriteStartArray();

        foreach (var item in value.Items)
        {
            serializer.Serialize(writer, item);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
