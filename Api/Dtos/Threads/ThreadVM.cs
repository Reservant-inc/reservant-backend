using AutoMapper;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// Information about a message thread
/// </summary>
[AutoMap(typeof(Thread))]
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
