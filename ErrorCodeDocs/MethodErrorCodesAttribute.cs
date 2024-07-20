using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ErrorCodeDocs;

/// <summary>
/// Documents that an endpoint (or a method) can return error codes
/// produced by the validator for the type <typeparamref name="TValidated"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public partial class MethodErrorCodesAttribute : Attribute
{
    /// <summary>
    /// Name of the referenced method in the form ClassName.MethodName
    /// </summary>
    public string FullMethodName { get; init; }

    /// <summary>
    /// Construct a new MethodErrorCodesAttribute
    /// </summary>
    /// <param name="method">Method reference, must be in the form: nameof(ClassName.MethodName)</param>
    /// <param name="methodAsString">Do not use. Used to get the string representation of <paramref name="method"/></param>
    /// <exception cref="ArgumentException">If <paramref name="method"/> is in wrong format</exception>
    public MethodErrorCodesAttribute(string method, [CallerArgumentExpression(nameof(method))] string? methodAsString = null)
    {
        ArgumentNullException.ThrowIfNull(methodAsString, nameof(methodAsString));

        var match = MethodParameterRegex().Match(methodAsString);
        if (!match.Success)
        {
            throw new ArgumentException("Argument can only be in the form: nameof(ClassName.MethodName)", nameof(method));
        }

        FullMethodName = match.Groups[0].Value;
    }

    [GeneratedRegex(@"^nameof\((\w+.\w+)\)$")]
    private static partial Regex MethodParameterRegex();
}
