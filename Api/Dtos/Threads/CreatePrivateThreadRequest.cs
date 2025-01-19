namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// Request to create a private thread between two users
/// </summary>
public class CreatePrivateThreadRequest
{
    /// <summary>
    /// ID of the other user
    /// </summary>
    public required Guid OtherUserId { get; set; }
}
