using Reservant.Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Class for handling payments
/// </summary>
public class Transaction : ISoftDeletable
{

    /// <summary>
    /// id of the transaction
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// amount of money in the transaction
    /// </summary>
    public required double Amount { get; set; }

    /// <summary>
    /// date of the transaction
    /// </summary>
    public required DateOnly Date {  get; set; }

    /// <summary>
    /// user who made the transaction
    /// </summary>
    public User User { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
