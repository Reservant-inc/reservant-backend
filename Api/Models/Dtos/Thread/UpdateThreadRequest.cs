namespace Reservant.Api.Models.Dtos.Thread;

/// <summary>
/// Request to update a message thread
/// </summary>
public class UpdateThreadRequest
{
    /// <summary>
    /// Title of the new thread
    /// </summary>
    public required string Title { get; init; }
}
