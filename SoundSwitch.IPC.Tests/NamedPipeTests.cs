using System;
using System.IO.Pipes;
using System.Threading.Tasks;

using FluentAssertions;

using MessagePack;

using NUnit.Framework;

using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages;
using SoundSwitch.IPC.Pipe.Messages.OpenSettings;
using SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;

namespace SoundSwitch.IPC.Tests;

/// <summary>
/// Covers the request/response guarantees of <see cref="NamedPipe"/>.
/// NamedPipe is static with process-global state (handler registry, client stream, listener CTS),
/// so the fixture must not run in parallel and no test may call <see cref="NamedPipe.Cleanup"/>.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class NamedPipeTests
{
    private static string StartServer()
    {
        var pipeName = $"SoundSwitch_Test_{Guid.NewGuid():N}";
        NamedPipe.StartListening(pipeName);
        return pipeName;
    }

    private static Task WaitForServerReadyAsync() => Task.Delay(TimeSpan.FromMilliseconds(200));

    [Test]
    public async Task SendRequestAsync_WhenHandlerResponds_ReturnsResponse()
    {
        var pipeName = StartServer();
        var handlerId = NamedPipe.RegisterMessageHandler((_, _) => Task.FromResult<IPipeMessage>(new OpenSettingsResponse { Success = true }));
        try
        {
            await WaitForServerReadyAsync();

            var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(pipeName, new OpenSettingsRequest());

            response.Success.Should().BeTrue();
        }
        finally
        {
            NamedPipe.UnregisterMessageHandler(handlerId);
        }
    }

    [Test]
    public async Task SendRequestAsync_WhenNoHandlerRegistered_ThrowsNotReadyPipeRequestException()
    {
        var pipeName = StartServer();
        await WaitForServerReadyAsync();

        var act = () => NamedPipe.SendRequestAsync<OpenSettingsResponse>(pipeName, new OpenSettingsRequest());

        var assertion = await act.Should().ThrowAsync<PipeRequestException>();
        assertion.Which.NotReady.Should().BeTrue("the server reports itself as still starting up when no handler is registered");
    }

    [Test]
    public async Task SendRequestAsync_WhenHandlerThrows_ThrowsPipeRequestExceptionWithHandlerError()
    {
        var pipeName = StartServer();
        var handlerId = NamedPipe.RegisterMessageHandler((_, _) => throw new InvalidOperationException("boom"));
        try
        {
            await WaitForServerReadyAsync();

            var act = () => NamedPipe.SendRequestAsync<OpenSettingsResponse>(pipeName, new OpenSettingsRequest());

            var assertion = await act.Should().ThrowAsync<PipeRequestException>();
            assertion.Which.Message.Should().Contain("boom");
            assertion.Which.NotReady.Should().BeFalse("a handler was registered, it just failed");
        }
        finally
        {
            NamedPipe.UnregisterMessageHandler(handlerId);
        }
    }

    [Test]
    public async Task Server_WhenClientDisconnectsAbruptly_KeepsServingNewConnections()
    {
        var pipeName = StartServer();
        var handlerId = NamedPipe.RegisterMessageHandler((_, _) => Task.FromResult<IPipeMessage>(new OpenSettingsResponse { Success = true }));
        try
        {
            await WaitForServerReadyAsync();

            // Connect raw, write a partial length prefix, then vanish mid-request
            await using (var rawClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await rawClient.ConnectAsync(5000);
                await rawClient.WriteAsync(new byte[] { 0, 0 });
            }

            var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(pipeName, new OpenSettingsRequest());

            response.Success.Should().BeTrue("a broken connection must kill only that connection, not the listener");
        }
        finally
        {
            NamedPipe.UnregisterMessageHandler(handlerId);
        }
    }

    [Test]
    public async Task SendRequestAsync_WhenServerDoesNotRespondInTime_ThrowsTimeoutException()
    {
        var pipeName = StartServer();
        var originalTimeout = NamedPipe.ResponseTimeout;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerId = NamedPipe.RegisterMessageHandler(async (_, _) =>
        {
            await gate.Task;
            return (IPipeMessage)new OpenSettingsResponse { Success = true };
        });
        try
        {
            NamedPipe.ResponseTimeout = TimeSpan.FromMilliseconds(500);
            await WaitForServerReadyAsync();

            var act = () => NamedPipe.SendRequestAsync<OpenSettingsResponse>(pipeName, new OpenSettingsRequest());

            await act.Should().ThrowAsync<TimeoutException>();
        }
        finally
        {
            NamedPipe.ResponseTimeout = originalTimeout;
            // Release the server-side handler so no slow request lingers past this test
            gate.TrySetResult();
            NamedPipe.UnregisterMessageHandler(handlerId);
        }
    }

    [Test]
    public async Task SendRequestAsync_WhenPipeNameChanges_ConnectsToTheNamedPipe()
    {
        var firstPipeName = StartServer();
        var secondPipeName = StartServer();
        var handlerId = NamedPipe.RegisterMessageHandler((request, _) => Task.FromResult<IPipeMessage>(request switch
        {
            OpenSettingsRequest => new OpenSettingsResponse { Success = true },
            TriggerSwitchRequest => new TriggerSwitchResponse { Success = true },
            _ => new ErrorResponse { Error = $"Unexpected request {request.GetType().Name}" }
        }));
        var originalIdleTimeout = NamedPipe.ServerIdleTimeout;
        try
        {
            await WaitForServerReadyAsync();

            // Shrink the idle timeout so the first connection is reaped fast enough to make the
            // stale-connection reuse bug observable without a ~12s wait.
            NamedPipe.ServerIdleTimeout = TimeSpan.FromMilliseconds(500);

            var firstResponse = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(firstPipeName, new OpenSettingsRequest());
            firstResponse.Success.Should().BeTrue();

            // Regression setup for the client-stream reuse bug: without pipe-name tracking the
            // second call silently reuses the first pipe's connection. That is only observable
            // once the first connection is dead, so wait for the server to reap it as idle.
            // 2s is a bounded 4x margin over the 500ms idle timeout.
            await Task.Delay(TimeSpan.FromSeconds(2));

            var secondResponse = await NamedPipe.SendRequestAsync<TriggerSwitchResponse>(secondPipeName, new TriggerSwitchRequest());
            secondResponse.Success.Should().BeTrue("the request must reach the server listening on the requested pipe name");
        }
        finally
        {
            NamedPipe.ServerIdleTimeout = originalIdleTimeout;
            NamedPipe.UnregisterMessageHandler(handlerId);
        }
    }

    [Test]
    public void ErrorResponse_WhenSerializedAsPipeMessage_RoundTripsThroughUnion()
    {
        IPipeMessage original = new ErrorResponse { NotReady = true, Error = "x" };

        var bytes = MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard);
        var result = MessagePackSerializer.Deserialize<IPipeMessage>(bytes, MessagePackSerializerOptions.Standard);

        var errorResponse = result.Should().BeOfType<ErrorResponse>("union key 15 must map back to ErrorResponse").Which;
        errorResponse.NotReady.Should().BeTrue();
        errorResponse.Error.Should().Be("x");
    }
}
