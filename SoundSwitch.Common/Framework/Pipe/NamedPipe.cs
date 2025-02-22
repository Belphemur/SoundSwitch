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
    private static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.StandardResolver.Instance);
    private static NamedPipeServerStream? _serverStream;
    private static NamedPipeClientStream? _clientStream;

    public static event Action<IPipeMessage>? OnMessageReceived;

    public static void StartListening(string pipeName, CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _serverStream?.Dispose();
                    _serverStream = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            await _serverStream.WaitForConnectionAsync(token);
                            var message = await MessagePackSerializer.DeserializeAsync<IPipeMessage>(_serverStream, SerializerOptions, token);
                            if (message != null)
                            {
                                OnMessageReceived?.Invoke(message);
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

    public static void SendMessageToExistingInstance(string pipeName, IPipeMessage message)
    {
        try
        {
            if (_clientStream == null || !_clientStream.IsConnected)
            {
                _clientStream?.Dispose();
                _clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
            }

            _clientStream.Connect(TimeSpan.FromSeconds(5));
            MessagePackSerializer.Serialize(_clientStream, message, SerializerOptions);
            _clientStream.Flush();
        }
        catch (TimeoutException)
        {
            // Ignored
            // Can happen if the other instance is not ready to receive the message
        }
        catch (Exception)
        {
            // Reset the client stream on error
            _clientStream?.Dispose();
            _clientStream = null;
        }
    }

    public static void Cleanup()
    {
        _serverStream?.Dispose();
        _clientStream?.Dispose();
        _serverStream = null;
        _clientStream = null;
    }
}