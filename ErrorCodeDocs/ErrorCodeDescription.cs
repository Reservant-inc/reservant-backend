using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.ErrorCodeDocs;

/// <summary>
/// Describes an error code that can be returned from a method
/// </summary>
/// <param name="PropertyName">Property name for which the error code is returned</param>
/// <param name="ErrorCode">The error code returned</param>
/// <param name="ErrorCode">Optional description of the reason the error code might be returned</param>
public readonly record struct ErrorCodeDescription(
    string? PropertyName, string ErrorCode, string? Description = null)
    : IComparable<ErrorCodeDescription>
{
    /// <summary>
    /// Construct an instance based on an <see cref="ErrorCodeAttribute"/>
    /// </summary>
    internal ErrorCodeDescription(ErrorCodeAttribute attribute)
        : this(attribute.PropertyName, attribute.ErrorCode, attribute.Description) { }

    public override string ToString()
    {
        var description = Description is null ? "" : $" ({Description})";
        return $"\"{PropertyName}\": {ErrorCode}{description}";
    }

    public int CompareTo(ErrorCodeDescription other)
    {
        var propertyNameComparison = string.Compare(PropertyName, other.PropertyName, StringComparison.Ordinal);
        if (propertyNameComparison != 0)
        {
            return propertyNameComparison;
        }

        var errorCodeComparison = string.Compare(ErrorCode, other.ErrorCode, StringComparison.Ordinal);
        if (errorCodeComparison != 0)
        {
            return errorCodeComparison;
        }

        return string.Compare(Description, other.Description, StringComparison.Ordinal);
    }

    public static bool operator <(ErrorCodeDescription left, ErrorCodeDescription right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(ErrorCodeDescription left, ErrorCodeDescription right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(ErrorCodeDescription left, ErrorCodeDescription right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(ErrorCodeDescription left, ErrorCodeDescription right)
    {
        return left.CompareTo(right) >= 0;
    }
}
