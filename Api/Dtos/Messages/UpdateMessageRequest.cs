namespace Reservant.Api.Dtos.Messages;

/// <summary>
/// Request to update 
/// </summary>
public class UpdateMessageRequest
{
    /// <summary>
    /// Contents of the message
    /// </summary>
    public required string Contents { get; set; }
}
