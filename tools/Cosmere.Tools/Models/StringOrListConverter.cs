using Newtonsoft.Json;

namespace Cosmere.Tools.Models;

public class StringOrListConverter : JsonConverter<List<string>?> {
    public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        if (value.Count == 1) {
            writer.WriteValue(value[0]);
        } else {
            serializer.Serialize(writer, value);
        }
    }

    public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType == JsonToken.String) {
            return new List<string> { reader.Value?.ToString() ?? string.Empty };
        }

        if (reader.TokenType == JsonToken.StartArray) {
            return serializer.Deserialize<List<string>>(reader);
        }

        throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing StringOrList");
    }
}
