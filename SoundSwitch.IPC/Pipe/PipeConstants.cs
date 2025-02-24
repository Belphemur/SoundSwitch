namespace SoundSwitch.IPC.Pipe;

public static class PipeConstants
{
    private const string APP_NAME = "SoundSwitch";
    public static string GetUserPipeName() => $"{APP_NAME}{Environment.UserName}";
}