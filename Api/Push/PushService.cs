using System.Collections.Concurrent;

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
    private readonly ConcurrentDictionary<string, List<MessageSubscriber>> _subscribers = [];

    /// <summary>
    /// Add a message handler to handle sending messages
    /// </summary>
    /// <param name="userId">ID of the user that the handler should send the messages to</param>
    /// <param name="subscriber">The handler delegate</param>
    public void Subscribe(string userId, MessageSubscriber subscriber)
    {
        var userHandlers = _subscribers.GetOrAdd(userId, _ => []);
        lock (userHandlers)
        {
            userHandlers.Add(subscriber);
        }
    }

    /// <summary>
    /// Remove a message handler
    /// </summary>
    /// <param name="userId">ID of the user that the handler sent messages to</param>
    /// <param name="subscriber">The handler delegate</param>
    public void Unsubscribe(string userId, MessageSubscriber subscriber)
    {
        var userHandlers = _subscribers.GetOrAdd(userId, _ => []);
        lock (userHandlers)
        {
            userHandlers.Remove(subscriber);
            if (userHandlers.Count == 0)
            {
                _subscribers.Remove(userId, out _);
            }
        }
    }

    /// <summary>
    /// Broadcast a message to all subscribed clients
    /// </summary>
    public void Broadcast(byte[] message)
    {
        foreach (var userHandlers in _subscribers.Values)
        {
            lock (userHandlers)
            {
                foreach (var handler in userHandlers)
                {
                    handler.Invoke(message);
                }
            }
        }
    }

    /// <summary>
    /// Send a message to a subscribed user
    /// </summary>
    public void SendToUser(string userId, byte[] message)
    {
        if (!_subscribers.TryGetValue(userId, out var userHandlers))
        {
            return;
        }

        lock (userHandlers)
        {
            foreach (var handler in userHandlers)
            {
                handler.Invoke(message);
            }
        }
    }
}
