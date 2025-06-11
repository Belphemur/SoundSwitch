using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SoundSwitch.Util.Url;

public static class BrowserUtil
{
    /// <summary>
    /// Open the given URL in user browser
    /// </summary>
    /// <param name="url"></param>
    public static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
            }
            catch (Exception)
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            throw new ArgumentException("Unknown platform");
        }
    }
}