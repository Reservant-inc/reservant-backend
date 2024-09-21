using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Reservant.ErrorCodeDocs.Attributes;

/// <summary>
/// Use <see cref="MethodErrorCodesAttribute{TContaining}"/> instead.
/// 
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public partial class MethodErrorCodesAttribute : Attribute
{
    /// <summary>
    /// Name of the class containing the referenced method
    /// </summary>
    public Type ContainingType { get; init; }

    /// <summary>
    /// Name of the referenced method
    /// </summary>
    public string MethodName { get; init; }

    /// <summary>
    /// Construct a new MethodErrorCodesAttribute
    /// </summary>
    /// <param name="method">Method reference, must be in the form: nameof(ClassName.MethodName)</param>
    /// <param name="methodAsString">Do not use. Used to get the string representation of <paramref name="method"/></param>
    /// <exception cref="ArgumentException">If <paramref name="method"/> is in wrong format</exception>
    public MethodErrorCodesAttribute(
        Type containingType, string method,
        [CallerArgumentExpression(nameof(method))] string? methodAsString = null)
    {
        ArgumentNullException.ThrowIfNull(methodAsString, nameof(methodAsString));

        var match = MethodParameterRegex().Match(methodAsString);
        if (!match.Success)
        {
            throw new ArgumentException("Argument can only be in the form: nameof(ClassName.MethodName)", nameof(method));
        }

        ContainingType = containingType;
        MethodName = match.Groups[2].Value;
    }

    [GeneratedRegex(@"^nameof\((\w+).(\w+)\)$")]
    private static partial Regex MethodParameterRegex();
}

/// <summary>
/// Documents that an endpoint (or a method) can return error codes produced by the specified method
/// </summary>
/// <param name="method">Method reference, must be in the form: nameof(ClassName.MethodName)</param>
/// <param name="methodAsString">Do not use. Used to get the string representation of <paramref name="method"/></param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MethodErrorCodesAttribute<TContaining>(
    string method, [CallerArgumentExpression(nameof(method))] string? methodAsString = null)
    : MethodErrorCodesAttribute(typeof(TContaining), method, methodAsString);
