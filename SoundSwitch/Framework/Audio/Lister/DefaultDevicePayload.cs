using SoundSwitch.Audio.Manager.Interop.Enum;

using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Audio.Lister;

public record struct DefaultDevicePayload(DeviceFullInfo Device, ERole Role);
