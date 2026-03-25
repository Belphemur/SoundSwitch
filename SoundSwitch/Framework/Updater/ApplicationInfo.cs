using System.Net.Http.Headers;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Updater;

internal static class ApplicationInfo
{
    public static readonly ProductInfoHeaderValue ProductValue = new(Application.ProductName, Application.ProductVersion);
    public static readonly ProductInfoHeaderValue CommentValue = new("(+https://soundwitch.aaflalo.me)");
}
