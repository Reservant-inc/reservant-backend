﻿namespace Reservant.Api.Dtos.Messages;

/// <summary>
/// Request to create a message
/// </summary>
public class CreateMessageRequest
{
    /// <summary>
    /// Contents of the message
    /// </summary>
    public required string Contents { get; set; }
}
