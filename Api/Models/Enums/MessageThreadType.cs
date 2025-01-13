namespace Reservant.Api.Models.Enums;

/// <summary>
/// Kind of a message thread
/// </summary>
public enum MessageThreadType
{
    /// <summary>
    /// Regular message thread created by users
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Message thread created automatically for an event
    /// </summary>
    Event,

    /// <summary>
    /// Message thread created automatically for a report
    /// </summary>
    Report,
}
