using Reservant.Api.Dtos.User;

namespace Reservant.Api.Dtos.Thread;

/// <summary>
/// Information about a message thread
/// </summary>
public class ThreadVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int ThreadId { get; init; }

    /// <summary>
    /// Title of the thread
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Participants
    /// </summary>
    public required List<UserSummaryVM> Participants { get; init; }
}
