using Newtonsoft.Json;

namespace Cosmere.Tools.Models;

[JsonConverter(typeof(ColorInfoConverter))]
public class ColorInfo
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; } = 255;

    public ColorInfo() { }

    public ColorInfo(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public override string ToString()
    {
        return $"({R}, {G}, {B}, {A})";
    }
}

public class ColorInfoConverter : JsonConverter<ColorInfo>
{
    public override void WriteJson(JsonWriter writer, ColorInfo? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();
        writer.WriteValue(value.R);
        writer.WriteValue(value.G);
        writer.WriteValue(value.B);
        if (value.A != 255)
        {
            writer.WriteValue(value.A);
        }
        writer.WriteEndArray();
    }

    public override ColorInfo? ReadJson(JsonReader reader, Type objectType, ColorInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType == JsonToken.StartArray)
        {
            var values = new List<int>();
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                if (reader.Value != null && int.TryParse(reader.Value.ToString(), out int value))
                {
                    values.Add(value);
                }
            }

            if (values.Count >= 3)
            {
                var a = values.Count > 3 ? (byte)values[3] : (byte)255;
                return new ColorInfo((byte)values[0], (byte)values[1], (byte)values[2], a);
            }
        }
        else if (reader.TokenType == JsonToken.String && reader.Value is string hexValue)
        {
            // Handle hex color strings like "9966cc" or "#9966cc"
            var hex = hexValue.TrimStart('#');
            
            // Handle 3-digit shorthand hex codes
            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length == 6 && 
                byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                return new ColorInfo(r, g, b);
            }
        }

        throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing ColorInfo. Expected array of RGB values or hex string.");
    }
}