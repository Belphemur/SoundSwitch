#nullable enable
namespace SoundSwitch.IPC.Pipe;

/// <summary>
/// Thrown when the pipe server explicitly reports a failure instead of a regular response.
/// </summary>
public sealed class PipeRequestException : Exception
{
    /// <summary>
    /// True when the server hadn't registered its message handlers yet (still starting up).
    /// Callers may retry instead of treating this as a hard failure.
    /// </summary>
    public bool NotReady { get; }

    public PipeRequestException(string message, bool notReady = false) : base(message)
    {
        NotReady = notReady;
    }
}
