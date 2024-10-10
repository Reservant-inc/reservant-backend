namespace Reservant.Api.Dtos.Threads;

/// <summary>
/// DTO used for adding/removing a participant to/from a Thread
/// </summary>
public class AddRemoveParticipantDto
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public required Guid UserId { get; set; }
}
