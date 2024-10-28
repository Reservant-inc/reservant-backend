namespace Reservant.Api.Models.Enums;

/// <summary>
/// Enum that allows filtering events by different type of users involvment in said event.
/// </summary>
public enum EventParticipationCategory
{
    /// <summary>
    /// Value for an event that was created by the user
    /// </summary>
    CreatedBy,
    /// <summary>
    /// Value for an event that the user participates in
    /// </summary>
    ParticipateIn,
    /// <summary>
    /// Value for an event that the user is interested in
    /// </summary>
    InterestedIn
}
