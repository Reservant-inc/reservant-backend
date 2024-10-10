using System.Diagnostics.CodeAnalysis;

namespace Reservant.ErrorCodeDocs.Attributes;

/// <summary>
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
/// <param name="containingType">Type that contains the method</param>
/// <param name="methodName">Name of the method</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[SuppressMessage("Performance", "CA1813:Avoid unsealed attributes")]
public class MethodErrorCodesAttribute(Type containingType, string methodName) : Attribute
{
    /// <summary>
    /// Name of the class containing the referenced method
    /// </summary>
    public Type ContainingType { get; } = containingType;

    /// <summary>
    /// Name of the referenced method
    /// </summary>
    public string MethodName { get; } = methodName;
}

/// <summary>
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
/// <typeparam name="TContaining">Type that contains the method</typeparam>
/// <param name="methodName">Name of the method</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MethodErrorCodesAttribute<TContaining>(string methodName)
    : MethodErrorCodesAttribute(typeof(TContaining), methodName);
