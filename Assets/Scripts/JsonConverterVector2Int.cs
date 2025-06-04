using System;
using Newtonsoft.Json;
using UnityEngine;

// https://gist.github.com/XCVG/4cf4d98218e0dc090b026292291deecb

public sealed class JsonConverterVector2Int : JsonConverter<Vector2Int>
{
    public override Vector2Int ReadJson(
        JsonReader reader,
        Type objectType,
        Vector2Int existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        Vector2Int result = default(Vector2Int);

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                switch (reader.Value.ToString())
                {
                    case "x":
                        result.x = reader.ReadAsInt32().Value;
                        break;
                    case "y":
                        result.y = reader.ReadAsInt32().Value;
                        break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                break;
            }
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("x");
        writer.WriteValue(value.x);

        writer.WritePropertyName("y");
        writer.WriteValue(value.y);

        writer.WriteEndObject();
    }
}
