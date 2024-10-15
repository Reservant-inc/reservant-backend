using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Available Hours with empty tables
/// </summary>
public class AvailableHours
{
    /// <summary>
    /// Available tables from 
    /// </summary>
    public TimeSpan From { get; set; }
    /// <summary>
    /// Available tables until
    /// </summary>
    public TimeSpan Until { get; set; }
}