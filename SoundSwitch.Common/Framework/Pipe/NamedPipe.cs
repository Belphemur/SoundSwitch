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

    public static event Action<PipeMessage>? OnMessageReceived;

    public static void StartListening(string pipeName, CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
                    await server.WaitForConnectionAsync(token);
                    var message = await MessagePackSerializer.DeserializeAsync<PipeMessage>(server, SerializerOptions, token);
                    if (message != null)
                    {
                        OnMessageReceived?.Invoke(message);
                    }
                }
                catch (Exception)
                {
                    // Ignored
                }
            }
        }, token);
    }

    public static void SendMessageToExistingInstance(string pipeName, PipeMessage message)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(TimeSpan.FromSeconds(5));
            MessagePackSerializer.Serialize(client, message, SerializerOptions);
        }
        catch (TimeoutException)
        {
            // Ignored
            // Can happen if the other instance is not ready to receive the message
        }
    }
}