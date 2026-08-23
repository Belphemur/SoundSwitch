using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IAudioEndpointVolume (endpointvolume.h). The vtable is declared in strict Windows SDK
    /// order up to the members SoundSwitch uses (Register/UnregisterControlChangeNotify,
    /// GetChannelCount, master scalar get/set, per-channel scalar get/set, Mute get/set);
    /// VolumeStepInfo and later slots are unused and intentionally omitted — a COM interface
    /// dispatches by slot position, so nothing past the last used member may be declared early.
    /// </summary>
    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig]
        HRESULT RegisterControlChangeNotify([In] IAudioEndpointVolumeCallback notify);

        [PreserveSig]
        HRESULT UnregisterControlChangeNotify([In] IAudioEndpointVolumeCallback notify);

        [PreserveSig]
        HRESULT GetChannelCount([Out] out uint channelCount);

        [PreserveSig]
        HRESULT SetMasterVolumeLevel([In] float levelDb, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT SetMasterVolumeLevelScalar([In] float level, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT GetMasterVolumeLevel([Out] out float levelDb);

        [PreserveSig]
        HRESULT GetMasterVolumeLevelScalar([Out] out float level);

        [PreserveSig]
        HRESULT SetChannelVolumeLevel([In] uint channel, [In] float levelDb, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT SetChannelVolumeLevelScalar([In] uint channel, [In] float level, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT GetChannelVolumeLevel([In] uint channel, [Out] out float levelDb);

        [PreserveSig]
        HRESULT GetChannelVolumeLevelScalar([In] uint channel, [Out] out float level);

        [PreserveSig]
        HRESULT SetMute([In][MarshalAs(UnmanagedType.Bool)] bool mute, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT GetMute([Out][MarshalAs(UnmanagedType.Bool)] out bool mute);
    }

    /// <summary>
    /// IAudioEndpointVolumeCallback (endpointvolume.h), implemented in managed code.
    /// OnNotify is invoked by the audio service on one of its own threads.
    /// </summary>
    [ComImport]
    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeCallback
    {
        void OnNotify([In] IntPtr notifyData);
    }

    /// <summary>
    /// Fixed header of the native AUDIO_VOLUME_NOTIFICATION_DATA (endpointvolume.h).
    /// The variable-length afChannelVolumes tail is not read.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioVolumeNotificationDataNative
    {
        public Guid guidEventContext;
        /// <summary>Native BOOL (4 bytes).</summary>
        public int bMuted;
        public float fMasterVolume;
        public uint nChannels;
    }
}
