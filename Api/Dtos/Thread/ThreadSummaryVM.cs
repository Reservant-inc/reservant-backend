namespace Reservant.Api.Dtos.Thread;

/// <summary>
/// Basic information about a message thread
/// </summary>
public class ThreadSummaryVM
{
    /// <summary>
    /// Title of the thread
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Number of participants
    /// </summary>
    public required int NumberOfParticipants { get; init; }
}
