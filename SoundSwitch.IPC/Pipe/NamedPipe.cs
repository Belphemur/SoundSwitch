#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Serilog;

namespace SoundSwitch.IPC.Pipe;

public static class NamedPipe
{
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance);
    private static NamedPipeServerStream? _serverStream;
    private static NamedPipeClientStream? _clientStream;
    private const int ConnectionTimeout = 5000; // 5 seconds

    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly ConcurrentDictionary<Guid, Func<IPipeMessage, CancellationToken, Task<IPipeMessage>>> _messageHandlers = new();

    public static async Task<TResponse> SendRequestAsync<TResponse>(string pipeName, IPipeMessage request, CancellationToken cancellationToken = default) where TResponse : IPipeMessage
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var token = linkedTokenSource.Token;
        try
        {
            if (_clientStream == null || !_clientStream.IsConnected)
            {
                _clientStream?.Dispose();
                _clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            }

            await _clientStream.ConnectAsync(ConnectionTimeout, token);
            await MessagePackSerializer.SerializeAsync(_clientStream, request, SerializerOptions, token);
            await _clientStream.FlushAsync(token);

            var response = await MessagePackSerializer.DeserializeAsync<TResponse>(_clientStream, SerializerOptions, token);
            if (response == null)
            {
                throw new InvalidOperationException("Received null response from server");
            }
            return response;
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
        _messageHandlers.TryAdd(id, handler);
        return id;
    }

    public static void UnregisterMessageHandler(Guid handlerId)
    {
        _messageHandlers.TryRemove(handlerId, out _);
    }

    public static void StartListening(string pipeName, CancellationToken cancellationToken = default)
    {
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
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
                    _serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            context.Information("Waiting for connection");
                            await _serverStream.WaitForConnectionAsync(token);

                            var request = await MessagePackSerializer.DeserializeAsync<IPipeMessage>(_serverStream, SerializerOptions, token);
                            context.Verbose("Received message: {Message}", request);
                            if (request != null && _messageHandlers.Count > 0)
                            {
                                IPipeMessage? response = null;
                                foreach (var handler in _messageHandlers.Values)
                                {
                                    try
                                    {
                                        response = await handler(request, token);
                                        if (response != null)
                                            break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex, "Error executing message handler");
                                    }
                                }

                                if (response != null)
                                {
                                    await MessagePackSerializer.SerializeAsync(_serverStream, response, SerializerOptions, token);
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

    public static void Cleanup()
    {
        _messageHandlers.Clear();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _serverStream?.Dispose();
        _clientStream?.Dispose();
        _serverStream = null;
        _clientStream = null;
    }
}