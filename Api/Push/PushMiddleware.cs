using System.Collections.Concurrent;
using System.Net.WebSockets;
using Reservant.Api.Identity;

namespace Reservant.Api.Push;

/// <summary>
/// Implements the Web Socket server and sends the subscribed
/// clients messages from PushService
/// </summary>
public class PushMiddleware(
    PushService pushService,
    ILogger<PushMiddleware> logger,
    IHostApplicationLifetime appLifetime) : IMiddleware
{
    private const string EndpointPath = "/notifications/ws";
    private const int MessagePollDelayMs = 1000;

    private readonly CancellationToken _appStopping = appLifetime.ApplicationStopping;

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
            await context.Response.WriteAsync("Must be a WS connection", CancellationToken.None);
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User must be authenticated", CancellationToken.None);
            return;
        }

        await ServeWebSocket(context);
    }

    /// <summary>
    /// The main serving method. Accepts the connection and starts
    /// receiving and sending messages
    /// </summary>
    private async Task ServeWebSocket(HttpContext context)
    {
        try
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();

            using var clientClosingTokenSource = new CancellationTokenSource();

            List<CancellationToken> allTokens = [clientClosingTokenSource.Token, _appStopping];

            using var sessionTimeout =
                context.Items[HttpContextItems.AuthExpiresUtc] is DateTimeOffset authExpiresUtc
                    ? new CancellationTokenSource(authExpiresUtc - DateTime.UtcNow)
                    : null;
            if (sessionTimeout is not null)
            {
                allTokens.Add(sessionTimeout.Token);
            }

            using var stopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(allTokens.ToArray());
            var stopToken = stopTokenSource.Token;

            var receiveTask = Task.Run(async () =>
            {
                try
                {
                    // socket is disposed only after we await the result
                    // ReSharper disable once AccessToDisposedClosure
                    await ReceiveMessages(socket, CancellationToken.None);
                }
                finally
                {
                    // clientClosingTokenSource is disposed only after we await the result
                    // ReSharper disable once AccessToDisposedClosure
                    await clientClosingTokenSource.CancelAsync();
                }
            }, CancellationToken.None);

            await PushMessages(pushService, socket, stopToken);

            if (clientClosingTokenSource.IsCancellationRequested)
            {
                await receiveTask;
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Goodbye",
                    CancellationToken.None);
            }
            else if (sessionTimeout?.IsCancellationRequested ?? false)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Authenticated token expired",
                    CancellationToken.None);
                await receiveTask; // Receive the closing frame
            }
            else
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable,
                    "Server stopped",
                    CancellationToken.None);
                await receiveTask; // Receive the closing frame
            }
        }
        catch (WebSocketException ex)
        {
            if (ex.WebSocketErrorCode is WebSocketError.ConnectionClosedPrematurely)
            {
                logger.LogInformation(ex, "Client connection closed prematurely");
            }
            else
            {
                logger.LogError(ex, "Unexpected WebSocket error");
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
        catch (OperationCanceledException) { }
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
    /// Receive messages from the client in a loop, until the client sends a closing frame
    /// </summary>
    private static async Task ReceiveMessages(WebSocket socket, CancellationToken cancelToken)
    {
        try
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
        catch (OperationCanceledException) { }
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
