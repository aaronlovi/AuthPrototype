using System.Text.Json;
using System.Text.Json.Serialization;

namespace OAuthToolkit.Shared;

public static class Conventions {
    public static readonly JsonSerializerOptions SerializationOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, SerializationOptions);
    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, SerializationOptions);
}
