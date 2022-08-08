using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.UI.Menu.Util;

public static class RoundedCorner
{
    private const int OS_WINDOWS_11 = 22000;

    // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
    // Copied from dwmapi.h
    private enum DWMWINDOWATTRIBUTE
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    // Copied from dwmapi.h
    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                     DWMWINDOWATTRIBUTE attribute,
                                                     ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                     uint cbAttribute);

    /// <summary>
    /// Round the corner of a menu/form (only for Windows 11)
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="preference"></param>
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
        int nLeftRect,     // x-coordinate of upper-left corner
        int nTopRect,      // y-coordinate of upper-left corner
        int nRightRect,    // x-coordinate of lower-right corner
        int nBottomRect,   // y-coordinate of lower-right corner
        int nWidthEllipse, // width of ellipse
        int nHeightEllipse // height of elli
    );
}