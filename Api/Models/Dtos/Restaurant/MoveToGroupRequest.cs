namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Request to move a restaurant to another group
/// </summary>
public class MoveToGroupRequest
{
    /// <summary>
    /// ID of the group to move the restaurant to
    /// </summary>
    public int GroupId { get; init; }
}