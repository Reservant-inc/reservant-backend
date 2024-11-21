using Microsoft.Identity.Client;

namespace Reservant.Api.Models;

/// <summary>
/// Full phone number structure that contains country code and the phone number
/// </summary>
public class PhoneNumber
{
    /// <summary>
    /// normal constructor
    /// </summary>
    public PhoneNumber(string Code, string Number)
    {
        this.Code = Code;
        this.Number = Number;
    }
    /// <summary>
    /// country code
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// phone number
    /// </summary>
    public string Number { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Code + Number;
    }
}
