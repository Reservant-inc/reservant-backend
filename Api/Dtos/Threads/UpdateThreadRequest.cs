namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// Request to update a message thread
/// </summary>
public class UpdateThreadRequest
{
    /// <summary>
    /// Title of the new thread
    /// </summary>
    /// <example>Watek Testowy</example>
    public required string Title { get; init; }
}
