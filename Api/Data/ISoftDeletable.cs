namespace Reservant.Api.Data;

/// <summary>
/// Common interface for soft-deletable entities
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Property for showing effects of soft-delete
    /// </summary>
    public bool IsDeleted { get; set; }
}
