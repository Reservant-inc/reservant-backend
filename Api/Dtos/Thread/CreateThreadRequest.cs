namespace Reservant.Api.Dtos.Thread;

/// <summary>
/// Request to create a message thread
/// </summary>
public class CreateThreadRequest
{
    /// <summary>
    /// Title of the new thread
    /// </summary>
    /// <example>Test</example>
    public required string Title { get; init; }

    /// <summary>
    /// IDs of the participants
    /// </summary>
    /// <example>["a79631a0-a3bf-43fa-8fbe-46e5ee697eeb"]</example>
    public required List<Guid> ParticipantIds { get; set; }
}
