using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Reservant.Api.Models;

/// <summary>
/// One class for all user classes.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Imię.
    /// </summary>
    [Required, ProtectedPersonalData, StringLength(30)]
    public required string FirstName { get; set; }

    /// <summary>
    /// Nazwisko.
    /// </summary>
    [Required, ProtectedPersonalData, StringLength(30)]
    public required string LastName { get; set; }

    /// <summary>
    /// Data rejestracji.
    /// </summary>
    [Required]
    public required DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Data urodzenia. For Customers.
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Reputacja. For Customers.
    /// </summary>
    public int? Reputation { get; set; }

    /// <summary>
    /// Wiek. For Customers.
    /// </summary>
    public int? Age
    {
        get
        {
            if (BirthDate is null)
            {
                return null;
            }

            var today = DateTime.Today;

            var age = today.Year - BirthDate.Value.Year;
            if (today.DayOfYear < BirthDate.Value.DayOfYear)
            {
                age -= 1;
            }

            return age;
        }
    }
}
