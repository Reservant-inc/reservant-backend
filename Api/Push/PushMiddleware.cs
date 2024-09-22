using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Reservant.Api.Push;

/// <summary>
/// Implements the Web Socket server and sends the subscribed
/// clients messages from PushService
/// </summary>
public class PushMiddleware(
    PushService pushService,
    ILogger<PushMiddleware> logger) : IMiddleware
{
    private const string EndpointPath = "/notifications/ws";
    private const int MessagePollDelayMs = 1000;

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path != EndpointPath)
        {
            await next.Invoke(context);
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Must be a WS connection");
            return;
        }

        try
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();

            var requestCancel = new CancellationTokenSource();
            var stopToken = CancellationTokenSource.CreateLinkedTokenSource(
                requestCancel.Token, context.RequestAborted).Token;

            var pushTask = Task.Run(async () =>
            {
                try
                {
                    // socket is disposed only after we await the result
                    // ReSharper disable once AccessToDisposedClosure
                    await PushMessages(pushService, socket, stopToken);
                }
                finally
                {
                    await requestCancel.CancelAsync();
                }
            });

            try
            {
                await ReceiveMessages(socket, stopToken);
            }
            finally
            {
                await requestCancel.CancelAsync();
            }

            await pushTask; // Wait for any pending send operations to complete

            while (!stopToken.IsCancellationRequested)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable,
                    "Server stopped",
                    CancellationToken.None);
            }
        }
        catch (WebSocketException ex)
        {
            switch (ex.WebSocketErrorCode)
            {
                case WebSocketError.ConnectionClosedPrematurely:
                    logger.LogInformation(ex, "Client connection closed prematurely");
                    break;

                default:
                    logger.LogError(ex, "Unexpected WebSocket error");
                    break;
            }
        }
    }

    /// <summary>
    /// Send messages from PushService in a loop
    /// </summary>
    private static async Task PushMessages(PushService source, WebSocket socket, CancellationToken cancelToken)
    {
        var messages = new ConcurrentQueue<byte[]>();

        source.OnMessage += EnqueueAuthorizedMessages;
        try
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (!messages.TryDequeue(out var message))
                {
                    await Task.Delay(MessagePollDelayMs, cancelToken);
                    continue;
                }

                await socket.SendAsync(
                    message,
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancelToken);
            }
        }
        finally
        {
            source.OnMessage -= EnqueueAuthorizedMessages;
        }

        return;

        void EnqueueAuthorizedMessages(byte[] message)
        {
            messages.Enqueue(message);
        }
    }

    /// <summary>
    /// Receive message from the client in a loop, until the client closes connection
    /// </summary>
    private static async Task ReceiveMessages(WebSocket socket, CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            var (response, message) = await ReceiveFullMessage(socket, cancelToken);
            if (response.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Receive message from a client to the end
    /// </summary>
    /// <returns>Last WebSocketReceiveResult and the message</returns>
    private static async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(
        WebSocket socket, CancellationToken cancelToken)
    {
        WebSocketReceiveResult response;
        var message = new List<byte>();

        var buffer = new byte[4096];
        do
        {
            response = await socket.ReceiveAsync(buffer, cancelToken);
            message.AddRange(buffer[..response.Count]);
        }
        while (!response.EndOfMessage);

        return (response, message);
    }
}
