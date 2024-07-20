namespace ErrorCodeDocs;

/// <summary>
/// Documents that an endpoint (or a method) can return an error code
/// </summary>
/// <param name="propertyName">Property name for which the error code is returned</param>
/// <param name="errorCode">The error code returned</param>
/// <param name="description">Optional description of the reason the error code might be returned</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ErrorCodeAttribute(string? propertyName, string errorCode, string? description = null) : Attribute
{
    /// <summary>
    /// Property name for which the error code is returned
    /// </summary>
    public string? PropertyName => propertyName;

    /// <summary>
    /// The error code returned
    /// </summary>
    public string ErrorCode => errorCode;

    /// <summary>
    /// Optional description of the reason the error code might be returned
    /// </summary>
    public string? Description => description;
}
