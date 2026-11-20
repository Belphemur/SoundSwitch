using System.Text.Json;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;

// Test-only endpoint control. Unlike the app CLI this does not need a running
// SoundSwitch instance, and explicit endpoint operations never fall back to the default.
try
{
    var snapshot = args.Length == 1 && args[0] == "snapshot";
    var get = args.Length == 2 && args[0] == "get";
    var set = args.Length == 3 && args[0] == "set";
    if (!snapshot && !get && !set)
        throw new ArgumentException("Usage: snapshot | get <endpoint-id> | set <endpoint-id> <true|false>");
    if (!snapshot && string.IsNullOrWhiteSpace(args[1]))
        throw new ArgumentException("An explicit endpoint ID is required.");
    var muted = set && bool.Parse(args[2]);
    var switcher = AudioSwitcher.Instance;
    using var microphone = snapshot
        ? switcher.GetDefaultAudioDevice(EDataFlow.eCapture, ERole.eCommunications)
        : switcher.GetDevice(args[1]);
    if (microphone == null || microphone.DataFlow != EDataFlow.eCapture || microphone.State != EDeviceState.Active)
        throw new InvalidOperationException("The requested microphone is unavailable; no other endpoint will be changed.");

    var result = switcher.InteractWithDevice(microphone, device =>
    {
        var volume = device.EndpointVolume ?? throw new InvalidOperationException("Microphone volume control is unavailable.");
        if (set) volume.Mute = muted;
        return new { id = device.Id, name = device.FriendlyName, isUsb = device.IsUsb, isMuted = volume.Mute };
    });
    Console.WriteLine(JsonSerializer.Serialize(result));
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
