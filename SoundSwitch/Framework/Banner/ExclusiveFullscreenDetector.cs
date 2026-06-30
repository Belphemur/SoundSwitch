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
/// Detects whether the current foreground window is running in
/// exclusive fullscreen (FSE) mode — or in a fullscreen state where
/// showing a Win32 overlay would disrupt the application.
///
/// Detection strategy (relaxed compared to previous implementation):
///   1. The foreground window covers at least the entire monitor work area.
///   2. It uses a borderless style (WS_POPUP without WS_CAPTION, or no
///      WS_THICKFRAME/WS_CAPTION combination).
///   3. The process is NOT a known desktop shell (explorer.exe).
///   4. Optionally, if WS_EX_TOPMOST is set, that's a strong FSE signal.
///   5. If the display mode differs from the desktop default, it's
///      almost certainly FSE.
///
/// The goal is to avoid false negatives (missing real FSE) at the cost
/// of occasionally treating borderless-windowed games as FSE — which
/// only means using a toast notification instead of a banner overlay.
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

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

    // DWM window attribute: DWMWA_CLOAKED
    private const int DWMWA_CLOAKED = 14;

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
    /// in exclusive fullscreen mode (or a fullscreen state that would be
    /// disrupted by showing a Win32 overlay).
    /// </summary>
    public static bool IsForegroundInExclusiveFullscreen()
    {
        try
        {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return false;

            // 1. Get the window bounds
            if (!GetWindowRect(hWnd, out var rect))
                return false;

            // 2. Get the monitor this window lives on
            var screen = Screen.FromHandle(hWnd);
            var monitorBounds = screen.Bounds;

            // 3. Does the window cover at least the entire monitor?
            bool coversMonitor = rect.Left <= monitorBounds.Left &&
                                 rect.Top <= monitorBounds.Top &&
                                 rect.Right >= monitorBounds.Right &&
                                 rect.Bottom >= monitorBounds.Bottom;
            if (!coversMonitor)
                return false;

            // 4. Check window style — must be borderless
            var style = (uint)GetWindowLong(hWnd, GWL_STYLE);
            bool isBorderless = IsBorderlessStyle(style);
            if (!isBorderless)
                return false;

            // 5. Exclude desktop shell windows (explorer.exe desktop, taskbar, etc.)
            if (IsDesktopShellWindow(hWnd))
                return false;

            // 6. Strong signal: WS_EX_TOPMOST is set — very likely FSE
            var exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_TOPMOST) != 0)
                return true;

            // 7. Check if display mode differs from desktop default
            //    This catches FSE games that change resolution/refresh rate
            if (IsDisplayModeChanged(screen.DeviceName))
                return true;

            // 8. If the window is fullscreen + borderless but not topmost and
            //    no display mode change, it could still be FSE (e.g. CS2 running
            //    at native resolution). Check if this is likely a game/media app
            //    by verifying it's not cloaked (hidden by DWM).
            if (IsCloakedByDwm(hWnd))
                return false;

            // A fullscreen borderless window that is not the desktop shell
            // is very likely a game or media application. Treat it as FSE to
            // avoid disrupting it with an overlay. The worst case is using
            // toast for a borderless-windowed game, which is acceptable.
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error while executing exclusive fullscreen detection");
            return false;
        }
    }

    /// <summary>
    /// Determines if the window style indicates a borderless window.
    /// FSE and borderless-windowed games both use WS_POPUP without WS_CAPTION,
    /// or sometimes just lack both WS_CAPTION and WS_THICKFRAME.
    /// </summary>
    private static bool IsBorderlessStyle(uint style)
    {
        // Classic FSE/borderless: WS_POPUP set, no caption
        if ((style & WS_POPUP) != 0 && (style & WS_CAPTION) == 0)
            return true;

        // Some games use a child/overlapped window without caption or thick frame
        if ((style & WS_CAPTION) == 0 && (style & WS_THICKFRAME) == 0)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if the window belongs to explorer.exe (the Windows shell).
    /// This prevents false positives from the desktop or taskbar.
    /// </summary>
    private static bool IsDesktopShellWindow(IntPtr hWnd)
    {
        try
        {
            GetWindowThreadProcessId(hWnd, out var pid);
            if (pid == 0)
                return false;

            using var process = System.Diagnostics.Process.GetProcessById((int)pid);
            var processName = process?.ProcessName;
            if (string.IsNullOrEmpty(processName))
                return false;

            return string.Equals(processName, "explorer", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // If we can't determine, assume it's not the shell
            return false;
        }
    }

    /// <summary>
    /// Checks if the current display mode on the given device differs from
    /// the desktop's registry (default) settings. A mismatch strongly indicates
    /// that a game has taken exclusive control and changed the display mode.
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

            // Compare resolution and refresh rate
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
    /// Checks if a window is cloaked (hidden) by DWM. Cloaked windows are
    /// not visible to the user and should not be considered FSE.
    /// </summary>
    private static bool IsCloakedByDwm(IntPtr hWnd)
    {
        try
        {
            var hr = DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out var cloaked, sizeof(int));
            return hr == 0 && cloaked != 0;
        }
        catch
        {
            return false;
        }
    }
}
