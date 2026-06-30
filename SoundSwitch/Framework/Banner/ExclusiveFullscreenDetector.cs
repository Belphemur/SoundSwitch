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
/// exclusive fullscreen (FSE) mode where DWM composition is suspended
/// and no overlay window can be shown.
///
/// <b>Key insight:</b> In true exclusive fullscreen, the application
/// takes exclusive ownership of the display output via DXGI. DWM is
/// suspended for that monitor, and no Win32 window — not even a
/// WS_EX_TOPMOST one — can appear on screen. The only option is to
/// use a toast notification, which is handled by the OS notification
/// layer and does not require a visible window.
///
/// Modern games (e.g. Counter-Strike 2) use borderless fullscreen with
/// DXGI flip model, which provides equivalent performance without
/// suspending DWM. In this mode our banner overlay is safe because:
///   - BannerForm uses WS_EX_NOACTIVATE + ShowWithoutActivation,
///     which does NOT trigger WM_ACTIVATEAPP in the game.
///   - WS_EX_TOPMOST on the banner keeps it above the game window.
///   - DWM is still compositing, so the banner is rendered normally.
///
/// Detection strategy:
///   1. Find the monitor where the foreground window resides.
///   2. Compare the current display mode (resolution + refresh rate)
///      against the desktop default stored in the registry.
///   3. A mismatch means a process has changed the display mode,
///      which only happens in true exclusive fullscreen.
///   4. As a secondary signal, if the foreground window covers the
///      monitor and has WS_EX_TOPMOST, it may be an older-style FSE
///      game that runs at native resolution.
/// </summary>
internal static class ExclusiveFullscreenDetector
{
    #region Native Methods

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

    #endregion

    #region Constants

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    private const uint WS_POPUP = 0x80000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_EX_TOPMOST = 0x00000008;

    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int ENUM_REGISTRY_SETTINGS = -2;

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
    /// in true exclusive fullscreen mode, where DWM composition is suspended
    /// and no overlay window can be displayed.
    /// </summary>
    public static bool IsForegroundInExclusiveFullscreen()
    {
        try
        {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return false;

            // 1. Identify the monitor where the foreground window resides
            var screen = Screen.FromHandle(hWnd);

            // 2. Primary check: has the display mode been changed?
            //    True FSE takes exclusive control of the output and may change
            //    the resolution or refresh rate. This is the most reliable signal.
            if (IsDisplayModeChanged(screen.DeviceName))
                return true;

            // 3. Secondary check for older FSE games running at native resolution:
            //    If the foreground window covers the monitor, is borderless
            //    (WS_POPUP, no WS_CAPTION), and is WS_EX_TOPMOST, it is likely
            //    an older-style FSE application. Note: DXGI SetFullscreenState
            //    does not itself set WS_EX_TOPMOST, but some game engines do.
            if (!GetWindowRect(hWnd, out var rect))
                return false;

            var monitorBounds = screen.Bounds;

            bool coversMonitor = rect.Left <= monitorBounds.Left &&
                                 rect.Top <= monitorBounds.Top &&
                                 rect.Right >= monitorBounds.Right &&
                                 rect.Bottom >= monitorBounds.Bottom;
            if (!coversMonitor)
                return false;

            var style = (uint)GetWindowLong(hWnd, GWL_STYLE);
            bool isBorderless = (style & WS_POPUP) != 0 && (style & WS_CAPTION) == 0;
            if (!isBorderless)
                return false;

            var exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
            bool isTopmost = (exStyle & WS_EX_TOPMOST) != 0;

            return isTopmost;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error while executing exclusive fullscreen detection");
            return false;
        }
    }

    /// <summary>
    /// Checks if the current display mode on the given device differs from
    /// the desktop's registry (default) settings. A mismatch indicates that
    /// a process has taken exclusive control and changed the display mode —
    /// the hallmark of true exclusive fullscreen.
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
}
