#nullable enable
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Runtime.Versioning;
using MessagePack;
using Serilog;
using SoundSwitch.IPC.Pipe.Messages;

namespace SoundSwitch.IPC.Pipe;

public static class NamedPipe
{
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard;
    private static NamedPipeServerStream? _serverStream;
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
            if (_clientStream == null || !_clientStream.IsConnected)
            {
                _clientStream?.Dispose();
                _clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            }

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
            var context = Log.ForContext("SourceContext", nameof(NamedPipe));
            context.Information("Starting named pipe server");
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _serverStream?.Dispose();
                    _serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            context.Information("Waiting for connection");
                            await _serverStream.WaitForConnectionAsync(token);

                            // Read request with length prefix
                            var lengthBuffer = new byte[4];
                            await ReadExactAsync(_serverStream, lengthBuffer, 0, 4, token);
                            var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                            var messageBuffer = new byte[messageLength];
                            await ReadExactAsync(_serverStream, messageBuffer, 0, messageLength, token);

                            var request = MessagePackSerializer.Deserialize<IPipeMessage>(messageBuffer, SerializerOptions);
                            context.Verbose("Received message: {Message}", request);
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
                                    await _serverStream.WriteAsync(responseLength, token);
                                    await _serverStream.WriteAsync(responseBuffer, token);
                                    await _serverStream.FlushAsync(token);
                                }
                            }

                            _serverStream.Disconnect();
                        }
                        catch (IOException)
                        {
                            // Connection was broken, continue to wait for new connections
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    // Retry creating the server stream after a brief delay
                    await Task.Delay(1000, token);
                }
            }
        }, linkedTokenSource.Token);
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
        _serverStream?.Dispose();
        _clientStream?.Dispose();
        _serverStream = null;
        _clientStream = null;
    }
}