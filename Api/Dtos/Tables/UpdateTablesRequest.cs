namespace Reservant.Api.Dtos.Tables;

/// <summary>
/// Request to update the tables at a restaurant
/// </summary>
public class UpdateTablesRequest
{
    /// <summary>
    /// List of tables
    /// </summary>
    public required List<UpdateTableRequest> Tables { get; set; }
}

/// <summary>
/// Request to update a single tables at a restaurant
/// </summary>
public class UpdateTableRequest
{
    /// <summary>
    /// Unique ID within the restaurant
    /// </summary>
    public required int TableId { get; set; }

    /// <summary>
    /// How many people can sit at the table
    /// </summary>
    public required int Capacity { get; set; }
}
