using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Audio.Lister;

public record struct DefaultDevicePayload(DeviceFullInfo Device, Role Role);