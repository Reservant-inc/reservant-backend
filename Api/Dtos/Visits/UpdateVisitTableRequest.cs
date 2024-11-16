namespace Reservant.Api.Dtos.Visits;

/// <summary>
/// Request to change a table in visit
/// </summary>
public class UpdateVisitTableRequest
{
    /// <summary>
    /// The ID of the new table to assign to the visit
    /// </summary>
    public int TableId { get; set; }
}