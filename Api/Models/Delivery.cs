using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.MenuItem;

namespace Reservant.Api.Models;

/// <summary>
/// Delivery object
/// </summary>
public class Delivery : ISoftDeletable
{
    
    /// <summary>
    /// Unique identifier for the delivery record.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Positions
    /// </summary>
    [Required] 
    public Tuple<MenuItemVM, int> positions { get; set; }
    
    // navigation?
    
    
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}