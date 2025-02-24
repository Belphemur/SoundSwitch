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
    private const int CONNECTION_TIMEOUT = 5000; // 5 seconds

    private static readonly CancellationTokenSource CancellationTokenSource = new();
    private static readonly ConcurrentDictionary<Guid, Func<IPipeMessage, CancellationToken, Task<IPipeMessage>>> MessageHandlers = new();

    public static async Task<TResponse> SendRequestAsync<TResponse>(string pipeName, IPipeMessage request, CancellationToken cancellationToken = default) where TResponse : IPipeMessage
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationTokenSource.Token);
        var token = linkedTokenSource.Token;
        try
        {
            if (_clientStream is not { IsConnected: true })
            {
                _clientStream?.Dispose();
                _clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            }

            if (!_clientStream.IsConnected)
                await _clientStream.ConnectAsync(CONNECTION_TIMEOUT, token);

            // Write with length prefix
            var buffer = MessagePackSerializer.Serialize(request, SerializerOptions, cancellationToken);
            var length = BitConverter.GetBytes(buffer.Length);
            await _clientStream.WriteAsync(length, token);
            await _clientStream.WriteAsync(buffer, token);
            await _clientStream.FlushAsync(token);

            // Read response with length prefix
            var lengthBuffer = new byte[4];
            await ReadExactAsync(_clientStream, lengthBuffer, 0, 4, token);
            var responseLength = BitConverter.ToInt32(lengthBuffer, 0);
            var responseBuffer = new byte[responseLength];
            await ReadExactAsync(_clientStream, responseBuffer, 0, responseLength, token);

            var response = MessagePackSerializer.Deserialize<IPipeMessage>(responseBuffer, SerializerOptions, cancellationToken);
            if (response == null)
            {
                throw new InvalidOperationException("Received null response from server");
            }

            return (TResponse)response;
        }
        catch (Exception)
        {
            _clientStream?.Dispose();
            _clientStream = null;
            throw;
        }
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
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationTokenSource.Token);
        var token = linkedTokenSource.Token;
        Task.Run(async () =>
        {
            using var _ = LogContext.PushProperty("SourceContext", nameof(NamedPipe));
            Log.Information("Starting named pipe server");
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var pipeId = Guid.NewGuid();
                    var serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    Log.ForContext("PipeId", pipeId).Information("Waiting for connection");
                    await serverStream.WaitForConnectionAsync(token);

                    ClientConnectedAsync(serverStream, pipeId, token);
                }
                catch (Exception)
                {
                    // Retry creating the server stream after a brief delay
                    await Task.Delay(1000, token);
                }
            }
        }, linkedTokenSource.Token);
    }

    private static async Task HandleCommunicationAsync(NamedPipeServerStream stream, Guid id, CancellationToken cancellationToken)
    {
        var logger = Log.ForContext("PipeId", id);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = cts.Token;
        var timeout = TimeSpan.FromSeconds(10);
        cts.CancelAfter(timeout);
        token.Register(() =>
        {
            logger.Warning("No message received in the last {Timeout}", timeout);
            if (stream.IsConnected)
            {
                logger.Information("Disconnecting client");
                stream.Disconnect();
            }
        });
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Read request with length prefix
                var lengthBuffer = new byte[4];
                await ReadExactAsync(stream, lengthBuffer, 0, 4, token);
                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                var messageBuffer = new byte[messageLength];
                await ReadExactAsync(stream, messageBuffer, 0, messageLength, token);

                cts.CancelAfter(timeout);
                var request = MessagePackSerializer.Deserialize<IPipeMessage>(messageBuffer, SerializerOptions, token);
                logger.Verbose("Message {MessageType} received", request.GetType().Name);
                if (!MessageHandlers.IsEmpty)
                {
                    IPipeMessage? response = null;
                    foreach (var handler in MessageHandlers.Values)
                    {
                        try
                        {
                            response = await handler(request, token);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error executing message handler");
                        }
                    }

                    if (response != null)
                    {
                        // Write response with length prefix
                        var responseBuffer = MessagePackSerializer.Serialize(response, SerializerOptions);
                        var responseLength = BitConverter.GetBytes(responseBuffer.Length);
                        await stream.WriteAsync(responseLength, token);
                        await stream.WriteAsync(responseBuffer, token);
                        await stream.FlushAsync(token);
                        logger.Verbose("Response {ResponseType} sent", response.GetType().Name);
                    }
                }
            }
            catch (IOException)
            {
                await Task.Delay(500, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling communication");
            }
        }
    }

    private static async Task ClientConnectedAsync(NamedPipeServerStream stream, Guid id, CancellationToken token)
    {
        try
        {
            await HandleCommunicationAsync(stream, id, token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            Log.ForContext("PipeId", id).Information("Disposing pipe");
            await stream.DisposeAsync().ConfigureAwait(false);
            return;
        }

        if (!stream.IsConnected)
        {
            Log.ForContext("PipeId", id).Information("Disposing pipe");
            await stream.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task ReadExactAsync(PipeStream stream, byte[] buffer, int offset, int count, CancellationToken token)
    {
        var bytesRead = 0;
        while (bytesRead < count)
        {
            var read = await stream.ReadAsync(buffer, offset + bytesRead, count - bytesRead, token);
            if (read == 0)
                throw new EndOfStreamException();
            bytesRead += read;
        }
    }

    public static void Cleanup()
    {
        MessageHandlers.Clear();
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        _clientStream?.Dispose();
        _clientStream = null;
    }
}