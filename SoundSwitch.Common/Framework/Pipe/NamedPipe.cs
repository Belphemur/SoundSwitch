#nullable enable
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using SoundSwitch.Common.Framework.EnumParser;

namespace SoundSwitch.Common.Framework.Pipe;

public static class NamedPipe
{
    public static event Action<MessageEnum>? OnMessageReceived;

    public enum MessageEnum
    {
        OpenSettings
    }

    public static void StartListening(string pipeName, CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
                await server.WaitForConnectionAsync(token);
                using var reader = new StreamReader(server);
                var message = (await reader.ReadLineAsync(token)).TryParseEnum<MessageEnum>();
                if (message != null)
                {
                    OnMessageReceived?.Invoke(message.Value);
                }
            }
        }, token);
    }
    
    
    public static void SendMessageToExistingInstance(string pipeName, MessageEnum messageEnum)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(TimeSpan.FromSeconds(5));
            using var writer = new StreamWriter(client);
            writer.WriteLine(messageEnum.ToString());
        }
        catch (TimeoutException)
        {
            // Ignored
            // Can happen if the other instance is not ready to receive the message
        }
    }
}