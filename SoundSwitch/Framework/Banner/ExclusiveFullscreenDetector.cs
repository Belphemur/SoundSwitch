/********************************************************************
 * Copyright (C) 2015-2024 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Banner;

/// <summary>
/// Detects whether the current foreground window is running in true
/// exclusive fullscreen (FSE) mode, as opposed to borderless windowed.
///
/// Why this matters:
///   In FSE, the application owns the display flip chain. When any new
///   top-level Win32 window appears, Windows sends WM_ACTIVATEAPP(FALSE) to
///   the game, which typically minimizes itself and releases the exclusive
///   mode. Borderless windowed is composited by DWM and does not have this
///   problem.
///
/// Detection strategy (layered):
///
///   1. PRIMARY: SHQueryUserNotificationState() returns
///      QUNS_RUNNING_D3D_FULL_SCREEN. This is the only Windows API that
///      explicitly says "a D3D app is running in exclusive fullscreen".
///      It is global, not per-window, but it is the strongest signal.
///
///   2. SECONDARY: Display mode change on the foreground window's monitor.
///      Compares ENUM_CURRENT_SETTINGS vs ENUM_REGISTRY_SETTINGS. Only true
///      FSE changes resolution or refresh rate. Gated behind the foreground
///      window covering the monitor to avoid false positives from unrelated
///      mode changes.
///
///   3. SHELL EXCLUSION: The Windows shell (explorer.exe) is excluded to
///      prevent false positives from the desktop or taskbar.
///
/// Known limitations:
///   - Windows Fullscreen Optimizations (default on Win10/11) make many
///     "Fullscreen" games actually run as borderless windowed. Those will
///     correctly NOT be detected as FSE.
///   - SHQueryUserNotificationState is global; if a FSE game is on a
///     secondary monitor while the user is interacting with a windowed
///     app on the primary monitor, we may still toast. This is acceptable
///     because toast is non-disruptive.
/// </summary>
internal static class ExclusiveFullscreenDetector
{
    #region Native Methods

    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out QueryUserNotificationState pquns);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [DllImport("psapi.dll", CharSet = CharSet.Auto)]
    private static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, System.Text.StringBuilder lpBaseName, int nSize);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    #endregion

    #region Constants

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const uint WS_POPUP = 0x80000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WS_EX_TOPMOST = 0x00000008;

    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int ENUM_REGISTRY_SETTINGS = -2;

    private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    #endregion

    #region Native Enums

    private enum QueryUserNotificationState
    {
        NotPresent = 1,
        Busy = 2,
        RunningD3dFullScreen = 3,
        PresentationMode = 4,
        AcceptsNotifications = 5,
        QuietTime = 6,
        App = 7
    }

    #endregion

    #region Native Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;

        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    #endregion

    /// <summary>
    /// Returns <c>true</c> when the foreground window appears to be running
    /// in true exclusive fullscreen mode. In that case, the banner cannot
    /// be displayed and a toast notification should be used instead.
    /// </summary>
    public static bool IsForegroundInExclusiveFullscreen()
    {
        try
        {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return false;

            // Exclude the Windows shell to prevent false positives.
            if (IsShellWindow(hWnd))
                return false;

            // Layer 1: SHQueryUserNotificationState — the official Windows signal.
            // QUNS_RUNNING_D3D_FULL_SCREEN is the only API that explicitly says
            // "a D3D app is running in exclusive fullscreen".
            if (QueryNotificationStateSaysFullscreen())
                return true;

            // Layer 2: Display mode change on the foreground window's monitor.
            // Gated behind the foreground window covering the monitor to avoid
            // false positives from unrelated mode changes (fixes CodeRabbit #158).
            if (!GetWindowRect(hWnd, out var rect))
                return false;

            var screen = Screen.FromHandle(hWnd);
            var monitorBounds = screen.Bounds;

            bool coversMonitor = rect.Left <= monitorBounds.Left &&
                                 rect.Top <= monitorBounds.Top &&
                                 rect.Right >= monitorBounds.Right &&
                                 rect.Bottom >= monitorBounds.Bottom;

            if (!coversMonitor)
                return false;

            // Only if the foreground window covers the monitor AND the display
            // mode has changed do we consider this true exclusive fullscreen.
            return IsDisplayModeChanged(screen.DeviceName);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error while executing exclusive fullscreen detection");
            return false;
        }
    }

    /// <summary>
    /// Queries Windows notification state to determine if a D3D application
    /// is running in exclusive fullscreen mode.
    /// </summary>
    private static bool QueryNotificationStateSaysFullscreen()
    {
        int hr = SHQueryUserNotificationState(out var state);
        if (hr != 0) // S_OK == 0
            return false;

        return state == QueryUserNotificationState.RunningD3dFullScreen;
    }

    /// <summary>
    /// Checks if the current display mode on the given device differs from
    /// the desktop's registry (default) settings. A mismatch indicates that
    /// a process has taken exclusive control and changed the display mode.
    /// </summary>
    private static bool IsDisplayModeChanged(string deviceName)
    {
        try
        {
            var currentMode = new DEVMODE();
            currentMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();

            var registryMode = new DEVMODE();
            registryMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();

            if (!EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref currentMode))
                return false;

            if (!EnumDisplaySettings(deviceName, ENUM_REGISTRY_SETTINGS, ref registryMode))
                return false;

            return currentMode.dmPelsWidth != registryMode.dmPelsWidth ||
                   currentMode.dmPelsHeight != registryMode.dmPelsHeight ||
                   currentMode.dmDisplayFrequency != registryMode.dmDisplayFrequency;
        }
        catch (Exception ex)
        {
            Serilog.Log.Debug(ex, "Could not compare display modes for FSE detection");
            return false;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the given window belongs to the Windows shell
    /// (explorer.exe or ShellExperienceHost.exe), which should never be
    /// treated as exclusive fullscreen.
    /// </summary>
    private static bool IsShellWindow(IntPtr hWnd)
    {
        try
        {
            GetWindowThreadProcessId(hWnd, out var pid);

            var hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProcess == IntPtr.Zero)
                return false;

            try
            {
                var name = new System.Text.StringBuilder(260);
                uint len = GetModuleBaseName(hProcess, IntPtr.Zero, name, name.Capacity);
                if (len == 0)
                    return false;

                string processName = name.ToString();
                return processName.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase) ||
                       processName.Equals("ShellExperienceHost.exe", StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
        catch
        {
            return false;
        }
    }
}
