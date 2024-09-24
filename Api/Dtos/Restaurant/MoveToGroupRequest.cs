namespace Reservant.Api.Dtos.Restaurant;

/// <summary>
/// Request to move a restaurant to another group
/// </summary>
public class MoveToGroupRequest
{
    /// <summary>
    /// ID of the group to move the restaurant to
    /// </summary>
    public required int GroupId { get; init; }
}
