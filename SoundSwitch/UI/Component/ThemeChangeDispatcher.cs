using System;
using System.Windows.Forms;

namespace SoundSwitch.UI.Component;

/// <summary>
/// Marshals a UI update onto a control's thread in a teardown-safe way.
/// <see cref="Control.BeginInvoke"/> is asynchronous, so between the
/// between the disposal/handle guards and the <see cref="Control.BeginInvoke"/> post
/// the control can be torn down; the helper re-checks on both sides and swallows the
/// resulting <see cref="InvalidOperationException"/> when teardown wins the race.
/// </summary>
internal static class ThemeChangeDispatcher
{
    public static void BeginThemeUpdate(Control control, Action update)
    {
        if (control.IsDisposed || control.Disposing || !control.IsHandleCreated)
        {
            return;
        }

        try
        {
            control.BeginInvoke(new Action(() =>
            {
                if (control.IsDisposed || control.Disposing)
                {
                    return;
                }

                update();
            }));
        }
        catch (InvalidOperationException)
        {
            // The handle was destroyed between the guard and the post; teardown won.
        }
    }
}
