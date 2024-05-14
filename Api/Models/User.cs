using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// One class for all user classes.
/// </summary>
public class User : IdentityUser, ISoftDeletable
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

    /// <summary>
    /// ID of the RestaurantOwner who employs the user. For restaurant employees
    /// </summary>
    public string? EmployerId { get; set; }

    /// <summary>
    /// Employer of the user. For restaurant employees
    /// </summary>
    public User? Employer { get; set; }

    /// <summary>
    /// Navigational collection for the employments. For restaurant employees
    /// </summary>
    /// <returns></returns>
    public ICollection<Employment>? Employments { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    public string? PhotoFileName { get; set; }

    /// <summary>
    /// Navigation property for the photo upload
    /// </summary>
    public FileUpload? Photo { get; set; }
    
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
