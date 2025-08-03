using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class UnixEpochDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var seconds))
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        if (reader.TokenType == JsonTokenType.String && 
            long.TryParse(reader.GetString(), out seconds))
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        // fallback
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());
}
