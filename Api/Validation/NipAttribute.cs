using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Reservant.Api.Validation;

/// <summary>
/// Specifies that a data field contains a valid
/// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NipAttribute : ValidationAttribute
{
    private static readonly Regex Regex1 = new(@"^\d{10}$", RegexOptions.Compiled);
    private static readonly Regex Regex2 = new(@"^\d{3}-\d{2}-\d{2}-\d{3}$", RegexOptions.Compiled);

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is not string strValue)
        {
            return false;
        }

        return Regex1.IsMatch(strValue) || Regex2.IsMatch(strValue);
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        $"{name} must be a valid NIP";
}
