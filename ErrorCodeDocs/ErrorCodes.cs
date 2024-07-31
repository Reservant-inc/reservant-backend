using ErrorCodeDocs.Attributes;
using System.Reflection;

namespace ErrorCodeDocs;

/// <summary>
/// Methods to aggregate error codes for a selected method
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Get error codes that can be returned from a method
    /// </summary>
    /// <param name="method">The method</param>
    public static IEnumerable<ErrorCodeDescription> GetErrorCodes(MethodInfo method)
    {
        foreach (var errorCode in method.GetCustomAttributes<ErrorCodeAttribute>())
        {
            yield return new ErrorCodeDescription(errorCode);
        }

        foreach (var inheritedMethod in method.GetCustomAttributes(typeof(MethodErrorCodesAttribute<>)))
        {
            foreach (var errorCode in GetReferencedMethodErrorCodes(inheritedMethod))
            {
                yield return errorCode;
            }
        }
    }

    /// <summary>
    /// Get error codes that can be returned from the method referenced in a <see cref="MethodErrorCodesAttribute{TContaining}"/>
    /// </summary>
    /// <param name="attribute">The attribute, must inherit from <see cref="MethodErrorCodesAttribute"/></param>
    /// <exception cref="ArgumentException">The attribute does not inherit from <see cref="MethodErrorCodesAttribute"/></exception>
    /// <exception cref="InvalidOperationException">The method referenced was not found</exception>
    private static IEnumerable<ErrorCodeDescription> GetReferencedMethodErrorCodes(Attribute attribute)
    {
        if (attribute is not MethodErrorCodesAttribute methodAttribute)
        {
            throw new ArgumentException($"Attribute must be a {nameof(MethodErrorCodesAttribute)}", nameof(attribute));
        }

        var referencedMethod = methodAttribute.ContainingType
            .GetMethod(methodAttribute.MethodName)
            ?? throw new InvalidOperationException(
                $"Method not found {methodAttribute.MethodName} in type {methodAttribute.ContainingType.Name}");

        foreach (var errorCode in GetErrorCodes(referencedMethod))
        {
            yield return errorCode;
        }
    }
}
