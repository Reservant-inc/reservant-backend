namespace Reservant.Api.Models.Dtos.Thread;

/// <summary>
/// Request to create a message thread
/// </summary>
public class CreateThreadRequest
{
    /// <summary>
    /// Title of the new thread
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// IDs of the participants
    /// </summary>
    public required List<string> ParticipantIds { get; set; }
}
