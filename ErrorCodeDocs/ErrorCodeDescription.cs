using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.ErrorCodeDocs;

/// <summary>
/// Describes an error code that can be returned from a method
/// </summary>
/// <param name="PropertyName">Property name for which the error code is returned</param>
/// <param name="ErrorCode">The error code returned</param>
/// <param name="ErrorCode">Optional description of the reason the error code might be returned</param>
public record struct ErrorCodeDescription(string? PropertyName, string ErrorCode, string? Description = null)
{
    /// <summary>
    /// Construct an instance based on an <see cref="ErrorCodeAttribute"/>
    /// </summary>
    public ErrorCodeDescription(ErrorCodeAttribute attribute)
        : this(attribute.PropertyName, attribute.ErrorCode, attribute.Description) { }

    public override string ToString()
    {
        var description = Description is null ? "" : $" ({Description})";
        return $"\"{PropertyName}\": {ErrorCode}{description}";
    }
}
