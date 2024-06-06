using Reservant.Api.Models.Dtos.User;

namespace Reservant.Api.Models.Dtos.Thread;

/// <summary>
/// Information about a message thread
/// </summary>
public class ThreadVM
{
    /// <summary>
    /// Title of the thread
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Participants
    /// </summary>
    public required List<UserSummaryVM> Participants { get; init; }
}
