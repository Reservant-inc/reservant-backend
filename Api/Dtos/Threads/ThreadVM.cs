using Reservant.Api.Dtos.Users;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Threads;

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

    /// <summary>
    /// Kind of the message thread
    /// </summary>
    public required MessageThreadType Type { get; init; }
}
