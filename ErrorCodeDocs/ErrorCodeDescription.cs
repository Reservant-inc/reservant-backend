using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.ErrorCodeDocs;

/// <summary>
/// Describes an error code that can be returned from a method
/// </summary>
public struct ErrorCodeDescription
{
    /// <summary>
    /// Property name for which the error code is returned
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// The error code returned
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Optional description of the reason the error code might be returned
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Construct an instance based on an <see cref="ErrorCodeAttribute"/>
    /// </summary>
    public ErrorCodeDescription(ErrorCodeAttribute attribute)
    {
        PropertyName = attribute.PropertyName;
        ErrorCode = attribute.ErrorCode;
        Description = attribute.Description;
    }
}
