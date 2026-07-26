#nullable enable
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Runtime.Versioning;

using MessagePack;

using Serilog;
using Serilog.Context;

using SoundSwitch.IPC.Pipe.Messages;

namespace SoundSwitch.IPC.Pipe;

public static class NamedPipe
{
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard;
    private static NamedPipeClientStream? _clientStream;
    private static string? _clientPipeName;
    private const int CONNECTION_TIMEOUT = 5000; // 5 seconds
    internal static TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(15);
    // Idle wait before the server disconnects a silent client; internal/settable for tests.
    internal static TimeSpan ServerIdleTimeout { get; set; } = TimeSpan.FromSeconds(10);
    private const int MaxMessageSize = 1024 * 1024; // 1 MiB — control messages are a few KB at most

    private static readonly CancellationTokenSource CancellationTokenSource = new();
    private static readonly ConcurrentDictionary<Guid, Func<IPipeMessage, CancellationToken, Task<IPipeMessage>>> MessageHandlers = new();

    public static async Task<TResponse> SendRequestAsync<TResponse>(string pipeName, IPipeMessage request, CancellationToken cancellationToken = default) where TResponse : IPipeMessage
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationTokenSource.Token);
        linkedTokenSource.CancelAfter(ResponseTimeout);
        var token = linkedTokenSource.Token;
        try
        {
            if (_clientStream is not { IsConnected: true } || _clientPipeName != pipeName)
            {
                _clientStream?.Dispose();
                _clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                _clientPipeName = pipeName;
            }

            if (!_clientStream.IsConnected)
                await _clientStream.ConnectAsync(CONNECTION_TIMEOUT, token);

            // Write with length prefix
            var buffer = MessagePackSerializer.Serialize(request, SerializerOptions, token);
            var length = BitConverter.GetBytes(buffer.Length);
            await _clientStream.WriteAsync(length, token);
            await _clientStream.WriteAsync(buffer, token);
            await _clientStream.FlushAsync(token);

            // Read response with length prefix
            var lengthBuffer = new byte[4];
            await ReadExactAsync(_clientStream, lengthBuffer, 0, 4, token);
            var responseLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (responseLength <= 0 || responseLength > MaxMessageSize)
            {
                throw new InvalidDataException($"Server sent invalid response length {responseLength}");
            }

            var responseBuffer = new byte[responseLength];
            await ReadExactAsync(_clientStream, responseBuffer, 0, responseLength, token);

            var response = MessagePackSerializer.Deserialize<IPipeMessage>(responseBuffer, SerializerOptions, token);
            if (response == null)
            {
                throw new InvalidOperationException("Received null response from server");
            }

            if (response is ErrorResponse errorResponse)
            {
                throw new PipeRequestException(errorResponse.Error, errorResponse.NotReady);
            }

            return (TResponse)response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && !CancellationTokenSource.IsCancellationRequested)
        {
            // Cancellation came from our own response timeout, not from the caller or app shutdown
            DisposeClientStream();
            throw new TimeoutException($"The server did not respond within {ResponseTimeout}");
        }
        catch (Exception)
        {
            DisposeClientStream();
            throw;
        }
    }

    private static void DisposeClientStream()
    {
        _clientStream?.Dispose();
        _clientStream = null;
        _clientPipeName = null;
    }

    public static Guid RegisterMessageHandler(Func<IPipeMessage, CancellationToken, Task<IPipeMessage>> handler)
    {
        var id = Guid.NewGuid();
        MessageHandlers.TryAdd(id, handler);
        return id;
    }

    public static void UnregisterMessageHandler(Guid handlerId)
    {
        MessageHandlers.TryRemove(handlerId, out _);
    }

    public static void StartListening(string pipeName, CancellationToken cancellationToken = default)
    {
        // Not disposed here on purpose: the returned token is used by the listener task and the
        // per-connection handlers for the whole process lifetime. Disposing the linked source on
        // method return would make later token registrations throw ObjectDisposedException.
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationTokenSource.Token);
        var token = linkedTokenSource.Token;
        Task.Run(async () =>
        {
            using var logContext = LogContext.PushProperty("SourceContext", nameof(NamedPipe));
            Log.Information("Starting named pipe server");
            while (!token.IsCancellationRequested)
            {
                NamedPipeServerStream? serverStream = null;
                try
                {
                    var pipeId = Guid.NewGuid();
                    serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    Log.ForContext("PipeId", pipeId).Information("Waiting for connection");
                    await serverStream.WaitForConnectionAsync(token);

                    _ = ClientConnectedAsync(serverStream, pipeId, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    if (serverStream != null)
                    {
                        await serverStream.DisposeAsync();
                    }

                    break; // Listener is shutting down
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Pipe accept loop failed, retrying");
                    if (serverStream != null)
                    {
                        await serverStream.DisposeAsync();
                    }

                    try
                    {
                        // Retry creating the server stream after a brief delay
                        await Task.Delay(1000, token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break; // Listener is shutting down
                    }
                }
            }
        }, token);
    }

    private static async Task HandleCommunicationAsync(NamedPipeServerStream stream, Guid id, CancellationToken cancellationToken)
    {
        var logger = Log.ForContext("PipeId", id);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = cts.Token;
        cts.CancelAfter(ServerIdleTimeout);
        token.Register(() =>
        {
            try
            {
                logger.Warning("No message received in the last {Timeout}", ServerIdleTimeout);
                if (stream.IsConnected)
                {
                    logger.Information("Disconnecting client");
                    stream.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger.Verbose(ex, "Failed to disconnect idle client");
            }
        });
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Read request with length prefix (idle timeout applies while waiting)
                var lengthBuffer = new byte[4];
                await ReadExactAsync(stream, lengthBuffer, 0, 4, token);
                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (messageLength <= 0 || messageLength > MaxMessageSize)
                {
                    logger.Warning("Rejecting message with invalid length {MessageLength}", messageLength);
                    break;
                }

                var messageBuffer = new byte[messageLength];
                await ReadExactAsync(stream, messageBuffer, 0, messageLength, token);

                // Pause the idle timeout while the request is being processed
                cts.CancelAfter(Timeout.InfiniteTimeSpan);
                var request = MessagePackSerializer.Deserialize<IPipeMessage>(messageBuffer, SerializerOptions, token);
                logger.Verbose("Message {MessageType} received", request.GetType().Name);

                var response = await ProcessRequestAsync(request, token);

                // Write response with length prefix
                var responseBuffer = MessagePackSerializer.Serialize(response, SerializerOptions);
                var responseLength = BitConverter.GetBytes(responseBuffer.Length);
                await stream.WriteAsync(responseLength, token);
                await stream.WriteAsync(responseBuffer, token);
                await stream.FlushAsync(token);
                logger.Verbose("Response {ResponseType} sent", response.GetType().Name);
            }
            catch (IOException ex)
            {
                // The pipe is dead; spinning on it would never recover
                logger.Debug(ex, "Pipe connection broken, closing");
                break;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling communication");
            }
            finally
            {
                // Re-arm the idle timeout before waiting for the next request
                cts.CancelAfter(ServerIdleTimeout);
            }
        }
    }

    private static async Task<IPipeMessage> ProcessRequestAsync(IPipeMessage request, CancellationToken token)
    {
        if (MessageHandlers.IsEmpty)
        {
            Log.Warning("No message handler registered yet, server is still starting up");
            return new ErrorResponse { NotReady = true, Error = "Server is still starting up" };
        }

        Exception? lastException = null;
        foreach (var handler in MessageHandlers.Values)
        {
            try
            {
                var response = await handler(request, token);
                if (response != null)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Log.Error(ex, "Error executing message handler");
            }
        }

        return new ErrorResponse
        {
            Error = lastException != null
                ? $"All message handlers failed: {lastException.Message}"
                : "No message handler produced a response"
        };
    }

    private static async Task ClientConnectedAsync(NamedPipeServerStream stream, Guid id, CancellationToken token)
    {
        try
        {
            await HandleCommunicationAsync(stream, id, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.ForContext("PipeId", id).Debug(ex, "Unhandled exception while handling pipe communication");
        }

        // Always dispose: IsConnected stays true for a server stream until Disconnect/Dispose,
        // so it cannot tell us the pipe died via an IOException break.
        await DisposeStreamAsync(stream, id).ConfigureAwait(false);
    }

    private static async Task DisposeStreamAsync(NamedPipeServerStream stream, Guid id)
    {
        try
        {
            Log.ForContext("PipeId", id).Information("Disposing pipe");
            await stream.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.ForContext("PipeId", id).Debug(ex, "Failed to dispose pipe");
        }
    }

    private static async Task ReadExactAsync(PipeStream stream, byte[] buffer, int offset, int count, CancellationToken token)
    {
        var bytesRead = 0;
        var attempts = 0;
        const int maxAttempts = 3;
        while (bytesRead < count && attempts < maxAttempts)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset + bytesRead, count - bytesRead), token);
            if (read == 0)
            {
                attempts++;
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
                continue;
            }
            bytesRead += read;
        }
        if (bytesRead < count)
        {
            throw new EndOfStreamException();
        }

    }

    public static void Cleanup()
    {
        MessageHandlers.Clear();
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        _clientStream?.Dispose();
        _clientStream = null;
        _clientPipeName = null;
    }
}
