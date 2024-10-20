namespace Reservant.Api.Models.Enums;
/// <summary>
/// Enum for discerning the status of the event.
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Status for an event that will happen in the future
    /// </summary>
    Future,
    /// <summary>
    /// Status for an event that cannot be joined
    /// </summary>
    NonJoinable,
    /// <summary>
    /// Status for an event that already happened
    /// </summary>
    Past
}
