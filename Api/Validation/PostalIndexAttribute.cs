using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Validation;

/// <summary>
/// Specifies that a data field contains a valid postal index
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PostalIndexAttribute() : RegularExpressionAttribute(@"^\d{2}-\d{3}$")
{
    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        $"{name} must be a valid postal index (example: 00-000)";
}
