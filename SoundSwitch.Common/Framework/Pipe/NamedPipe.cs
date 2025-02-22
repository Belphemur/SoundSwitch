#nullable enable
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace SoundSwitch.Common.Framework.Pipe;

public static class NamedPipe
{
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance);
    private static NamedPipeServerStream? _serverStream;
    private static NamedPipeClientStream? _clientStream;
    private const int ConnectionTimeout = 5000; // 5 seconds

    public static async Task<TResponse> SendRequestAsync<TResponse>(string pipeName, IPipeMessage request, CancellationToken token = default) where TResponse : IPipeMessage
    {
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

    public delegate Task<IPipeMessage> MessageHandler(IPipeMessage request, CancellationToken token);
    public static event MessageHandler? OnMessageReceived;

    public static void StartListening(string pipeName, CancellationToken token)
    {
        Task.Run(async () =>
        {
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
                            await _serverStream.WaitForConnectionAsync(token);

                            var request = await MessagePackSerializer.DeserializeAsync<IPipeMessage>(_serverStream, SerializerOptions, token);
                            if (request != null && OnMessageReceived != null)
                            {
                                var response = await OnMessageReceived(request, token);
                                await MessagePackSerializer.SerializeAsync(_serverStream, response, SerializerOptions, token);
                                await _serverStream.FlushAsync(token);
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
        }, token);
    }

    public static void Cleanup()
    {
        _serverStream?.Dispose();
        _clientStream?.Dispose();
        _serverStream = null;
        _clientStream = null;
    }
}