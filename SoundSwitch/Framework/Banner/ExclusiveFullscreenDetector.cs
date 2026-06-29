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
/// exclusive fullscreen (FSE) mode.
///
/// In FSE the application holds the DXGI flip chain exclusively.
/// Any new top-level Win32 window — even with WS_EX_NOACTIVATE — causes
/// Windows to send WM_ACTIVATEAPP(FALSE) to the game, which makes it
/// minimize itself and release the exclusive mode.
///
/// We detect this by combining three signals:
///   1. The foreground window covers the entire monitor exactly.
///   2. It has WS_POPUP style (no title bar / border — typical for FSE).
///   3. It has WS_EX_TOPMOST (FSE windows are always topmost).
///
/// Borderless-windowed games also satisfy (1) and (2) but NOT (3), because
/// modern games running in borderless windowed mode are NOT topmost — the
/// Desktop Window Manager composites them normally. FSE windows are topmost
/// because they bypass DWM entirely.
/// </summary>
internal static class ExclusiveFullscreenDetector
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private const int GWL_STYLE   = -16;
    private const int GWL_EXSTYLE = -20;
    private const uint WS_POPUP    = 0x80000000;
    private const uint WS_CAPTION  = 0x00C00000;
    private const uint WS_EX_TOPMOST = 0x00000008;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    /// <summary>
    /// Returns <c>true</c> when the foreground window appears to be running
    /// in true exclusive fullscreen mode on its monitor.
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
            var b = screen.Bounds;

            // 3. Does the window cover the entire monitor exactly?
            bool coversMonitor = rect.Left   <= b.Left  &&
                                 rect.Top    <= b.Top   &&
                                 rect.Right  >= b.Right &&
                                 rect.Bottom >= b.Bottom;
            if (!coversMonitor)
                return false;

            // 4. WS_POPUP + no WS_CAPTION → borderless, no title bar
            //    (both FSE and borderless-windowed pass this)
            var style = (uint)GetWindowLong(hWnd, GWL_STYLE);
            bool isPopupStyle = (style & WS_POPUP) != 0 && (style & WS_CAPTION) == 0;
            if (!isPopupStyle)
                return false;

            // 5. WS_EX_TOPMOST is the key differentiator:
            //    FSE windows are always topmost; borderless-windowed are NOT.
            var exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
            bool isTopmost = (exStyle & WS_EX_TOPMOST) != 0;

            if (!isTopmost)
                return false;

            // 6. Direct3D / Graphics loaded modules detection
            // If the foreground window meets style/bounds criteria, check if it uses D3D / graphics APIs.
            // If it is indeed a Direct3D/graphics process, or if we cannot confidently check (e.g. access denied),
            // we return true because the style/bounds check already passed.
            if (IsGraphicsProcess(hWnd))
            {
                return true;
            }

            // Fallback: If we couldn't confidently identify graphics modules, but the window size/style is an exact match,
            // we still treat it as a potential exclusive fullscreen window.
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error while executing exclusive fullscreen detection");
            return false;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private static bool IsGraphicsProcess(IntPtr hWnd)
    {
        try
        {
            GetWindowThreadProcessId(hWnd, out var pid);
            if (pid == 0)
                return false;

            using var process = System.Diagnostics.Process.GetProcessById((int)pid);
            if (process == null)
                return false;

            // Enumerate modules to check for common Direct3D/Graphics DLLs
            foreach (System.Diagnostics.ProcessModule module in process.Modules)
            {
                if (module?.ModuleName == null)
                    continue;

                var moduleName = module.ModuleName.ToLowerInvariant();
                if (moduleName.StartsWith("d3d") && moduleName.EndsWith(".dll"))
                {
                    return true;
                }

                if (moduleName == "dxgi.dll" || moduleName == "opengl32.dll" || moduleName.Contains("vulkan-1"))
                {
                    return true;
                }
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Access denied on elevated processes, we log a verbose/debug line
            Serilog.Log.Debug("Access denied when trying to read modules for foreground process.");
        }
        catch (Exception ex)
        {
            Serilog.Log.Debug(ex, "Could not determine if process uses Direct3D.");
        }

        return false;
    }
}
