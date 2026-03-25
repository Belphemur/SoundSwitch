using System.Runtime.InteropServices;
using System;

namespace SoundSwitch.Common.Framework.UI;

/// <summary>
/// Utility class to handle rounded corners for forms.
/// </summary>
public static class RoundedCorner
{
    private const int OS_WINDOWS_11 = 22000;

    private enum DWMWINDOWATTRIBUTE
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                     DWMWINDOWATTRIBUTE attribute,
                                                     ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                     uint cbAttribute);

    /// <summary>
    /// Round the corner of a form (only for Windows 11)
    /// </summary>
    public static bool RoundCorner(IntPtr handle, DWM_WINDOW_CORNER_PREFERENCE preference)
    {
        if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build < OS_WINDOWS_11)
        {
            return false;
        }
        try {
            var pref = preference;
            DwmSetWindowAttribute(handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(uint));
            return true;
        } catch(Exception) {
            return false;
        }
    }

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn
    (
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse
    );
}
