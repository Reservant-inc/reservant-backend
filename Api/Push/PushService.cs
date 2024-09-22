namespace Reservant.Api.Push;

/// <summary>
/// Delegate that handles sending a message to the client
/// </summary>
public delegate void MessageSubscriber(byte[] message);

/// <summary>
/// Service responsible for sending push messages to clients
/// </summary>
public class PushService
{
    /// <summary>
    /// Invoked when a new message is sent
    /// </summary>
    public event MessageSubscriber OnMessage = delegate { };

    /// <summary>
    /// Broadcast a message to all subscribed clients
    /// </summary>
    public void Broadcast(byte[] message)
    {
        OnMessage.Invoke(message);
    }
}
