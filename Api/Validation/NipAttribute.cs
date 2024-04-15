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
    private static readonly int[] Weights = new[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is not string strValue)
        {
            return false;
        }
        
        if (!Regex1.IsMatch(strValue))
        {
            return false;
        }
        
        int sum = 0;
        for (int i = 0; i < Weights.Length; i++)
        {
            sum += Weights[i] * (strValue[i] - '0');
        }

        int checksum = sum % 11;
        if (checksum == 10)
        {
            checksum = 0;
        }
        
        return checksum == (strValue[9] - '0');
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        $"{name} must be a valid NIP";
}