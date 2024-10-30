namespace Reservant.Api.Dtos.Ingredients;

/// <summary>
/// Information about ingredient history
/// </summary>
public class IngredientHistoryRequest
{
    /// <summary>
    /// Date from ingredient history we want to see
    /// </summary>
    public DateTime? DateFrom { get; set; }
    
    /// <summary>
    /// Date to ingredient history we want to see
    /// </summary>
    public DateTime? DateUntil { get; set; }
    
    /// <summary>
    /// User that made ingredients changes
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Searched phrase in comments
    /// </summary>
    public string? Comment { get; set; }
}