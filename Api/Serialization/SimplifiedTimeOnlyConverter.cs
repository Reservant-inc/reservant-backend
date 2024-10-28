using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reservant.Api.Serialization;

/// <summary>
/// JSON converter for TimeOnly that uses the HH:mm format
/// </summary>
public class SimplifiedTimeOnlyConverter : JsonConverter<TimeOnly>
{
    private const string TimeFormat = "HH:mm";

    /// <inheritdoc />
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var timeString = reader.GetString();
        if (timeString is null)
        {
            throw new JsonException("Value cannot be null");
        }

        if (TimeOnly.TryParseExact(timeString, TimeFormat, out var result))
        {
            throw new JsonException($"Invalid time format: {timeString}, expected {TimeFormat}");
        }

        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(TimeFormat, CultureInfo.InvariantCulture));
    }
}
