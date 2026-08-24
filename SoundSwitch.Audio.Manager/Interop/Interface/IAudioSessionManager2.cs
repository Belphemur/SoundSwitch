using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IAudioSessionManager2 (audiopolicy.h). Only the members SoundSwitch uses are declared, in
    /// strict Windows SDK vtable order: the two IAudioSessionManager base slots, then
    /// GetSessionEnumerator. Slots after the last used member are omitted (a COM interface
    /// dispatches by slot position; undeclared trailing slots are never called).
    /// </summary>
    [ComImport]
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        [PreserveSig]
        HRESULT GetAudioSessionControl([In] ref Guid sessionId, [In] uint streamFlags, [Out] out IntPtr sessionControl);

        [PreserveSig]
        HRESULT GetSimpleAudioVolume([In] ref Guid sessionId, [In] uint streamFlags, [Out] out IntPtr audioVolume);

        [PreserveSig]
        HRESULT GetSessionEnumerator([Out] out IAudioSessionEnumerator sessionEnum);
    }

    /// <summary>
    /// IAudioSessionEnumerator (audiopolicy.h). Complete vtable (both methods are used).
    /// </summary>
    [ComImport]
    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig]
        HRESULT GetCount([Out] out int sessionCount);

        [PreserveSig]
        HRESULT GetSession([In] int sessionNumber, [Out] out IAudioSessionControl2 session);
    }

    /// <summary>
    /// IAudioSessionControl2 (audiopolicy.h). The vtable is declared in strict Windows SDK order:
    /// the nine IAudioSessionControl base slots, then the IAudioSessionControl2 slots up to
    /// GetProcessId (the last member SoundSwitch uses). IsSystemSoundsSession and
    /// SetDuckingPreference trail the last used member and are intentionally omitted.
    /// GetProcessId sits at slot 13, so all preceding slots must be present and ordered.
    /// </summary>
    [ComImport]
    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2
    {
        // IAudioSessionControl base slots
        [PreserveSig]
        HRESULT GetState([Out] out AudioSessionState state);

        [PreserveSig]
        HRESULT GetDisplayName([Out][MarshalAs(UnmanagedType.LPWStr)] out string displayName);

        [PreserveSig]
        HRESULT SetDisplayName([In][MarshalAs(UnmanagedType.LPWStr)] string displayName, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT GetIconPath([Out][MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

        [PreserveSig]
        HRESULT SetIconPath([In][MarshalAs(UnmanagedType.LPWStr)] string iconPath, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT GetGroupingParam([Out] out Guid groupingId);

        [PreserveSig]
        HRESULT SetGroupingParam([In] ref Guid groupingId, [In] ref Guid eventContext);

        [PreserveSig]
        HRESULT RegisterAudioSessionNotification([In] IntPtr client);

        [PreserveSig]
        HRESULT UnregisterAudioSessionNotification([In] IntPtr client);

        // IAudioSessionControl2 slots
        [PreserveSig]
        HRESULT GetSessionIdentifier([Out][MarshalAs(UnmanagedType.LPWStr)] out string sessionIdentifier);

        [PreserveSig]
        HRESULT GetSessionInstanceIdentifier([Out][MarshalAs(UnmanagedType.LPWStr)] out string sessionInstanceIdentifier);

        [PreserveSig]
        HRESULT GetProcessId([Out] out uint processId);
    }
}
