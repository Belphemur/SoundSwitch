using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IMMNotificationClient (mmdeviceapi.h), implemented in managed code by
    /// <see cref="SoundSwitch.Audio.Manager.AudioDeviceNotificationClient"/>.
    /// The vtable is exactly the five notification callbacks (plus the three implicit IUnknown
    /// slots the runtime provides) — no extra stubs. SoundSwitch implements all five callbacks and
    /// nothing more.
    /// </summary>
    [ComImport]
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, EDeviceState newState);

        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, PROPERTYKEY key);
    }
}
