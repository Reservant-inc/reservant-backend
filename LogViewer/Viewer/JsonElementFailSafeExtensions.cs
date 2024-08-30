using System.Text.Json;

namespace LogsViewer.Viewer;

/// <summary>
/// Extension methods for fail-safe fluent JsonElement querying
/// </summary>
internal static class JsonElementFailSafeExtensions
{
    /// <summary>
    /// Get property of a JSON object. Return default(JsonElement) if it is invalid,
    /// not an object or has no such property
    /// </summary>
    public static JsonElement Prop(this JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        if (!element.TryGetProperty(propertyName, out var value))
        {
            return default;
        }

        return value;
    }

    /// <summary>
    /// Return the element as string. Return null if it is invalid or not a string
    /// </summary>
    public static string? AsString(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return element.GetString();
    }

    /// <summary>
    /// Return the element as int. Return null if it is invalid or not an int
    /// </summary>
    public static int? AsInt32(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        if (!element.TryGetInt32(out var result))
        {
            return null;
        }

        return result;
    }
}
