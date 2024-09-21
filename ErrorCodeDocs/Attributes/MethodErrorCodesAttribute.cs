namespace Reservant.ErrorCodeDocs.Attributes;

/// <summary>
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
/// <param name="containingType">Type that contains the method</param>
/// <param name="method">Name of the method</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MethodErrorCodesAttribute(Type containingType, string method) : Attribute
{
    /// <summary>
    /// Name of the class containing the referenced method
    /// </summary>
    public Type ContainingType { get; } = containingType;

    /// <summary>
    /// Name of the referenced method
    /// </summary>
    public string MethodName { get; } = method;
}

/// <summary>
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
/// <typeparam name="TContaining">Type that contains the method</typeparam>
/// <param name="method">Name of the method</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MethodErrorCodesAttribute<TContaining>(string method)
    : MethodErrorCodesAttribute(typeof(TContaining), method);
