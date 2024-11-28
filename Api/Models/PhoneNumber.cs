namespace Reservant.Api.Models;

/// <summary>
/// Full phone number structure that contains country code and the phone number
/// </summary>
public record PhoneNumber(string Code, string Number)
{
    /// <inheritdoc/>
    public override string ToString() => Code + Number;
}
