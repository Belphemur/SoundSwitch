using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Framework.WinApi;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System;

namespace SoundSwitch.UI.Component;

public class HotKeyTextBox : TextBox
{
    public class Event : EventArgs;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [DefaultValue(null)]
    public HotKey HotKey
    {
        get => _hotKey;
        set
        {
            _hotKey = value;
            Text = value?.Display() ?? "";
        }
    }

    private HotKey _hotKey;
    private bool _listenToHotkey;

    [Browsable(true)] public event EventHandler<Event> HotKeyChanged;

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ListenToHotkey
    {
        get => _listenToHotkey;
        set
        {
            _listenToHotkey = value;
            if (value)
            {
                WindowsAPIAdapter.HotKeyPressed += WindowsAPIAdapterOnHotKeyPressed;
            }
            else
            {
                WindowsAPIAdapter.HotKeyPressed -= WindowsAPIAdapterOnHotKeyPressed;
            }
        }
    }

    public void CleanHotKeyChangedHandler()
    {
        HotKeyChanged = null;
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing && ListenToHotkey)
        {
            WindowsAPIAdapter.HotKeyPressed -= WindowsAPIAdapterOnHotKeyPressed;
        }

        base.Dispose(disposing);
    }

    private void WindowsAPIAdapterOnHotKeyPressed(object? sender, WindowsAPIAdapter.KeyPressedEventArgs e)
    {
        Invoke(new Action(() =>
        {
            if (!Visible)
            {
                return;
            }

            HotKey = e.HotKey;
            ForeColor = Color.Green;
            HotKeyChanged?.Invoke(this, new Event());
        }), null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        HotKey.ModifierKeys modifierKeys = 0;
        foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
        {
            {
                switch (pressedModifier)
                {
                    case Keys.Control:
                        modifierKeys |= HotKey.ModifierKeys.Control;
                        break;
                    case Keys.Alt:
                        modifierKeys |= HotKey.ModifierKeys.Alt;
                        break;
                    case Keys.Shift:
                        modifierKeys |= HotKey.ModifierKeys.Shift;
                        break;
                    case Keys.LWin:
                    case Keys.RWin:
                        modifierKeys |= HotKey.ModifierKeys.Win;
                        break;
                }
            }
        }

        var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
        var key = normalPressedKeys.FirstOrDefault();

        // Case 1: No key pressed - show error state
        if (key == Keys.None)
        {
            SetInvalidState(key, modifierKeys);
            e.Handled = true;
            base.OnKeyUp(e);
            return;
        }

        // Case 2: No modifiers - only accept special keys
        if (modifierKeys == 0 && !IsSpecialKey(key))
        {
            SetInvalidState(key, modifierKeys);
            e.Handled = true;
            base.OnKeyUp(e);
            return;
        }

        // Valid hotkey - set and trigger change event
        SetValidHotKey(key, modifierKeys);

        e.Handled = true;
        base.OnKeyUp(e);
    }

    private void SetValidHotKey(Keys key, HotKey.ModifierKeys modifierKeys)
    {
        HotKey = new HotKey(key, modifierKeys);
        ForeColor = Color.Green;
        HotKeyChanged?.Invoke(this, new Event());
    }

    private void SetInvalidState(Keys key, HotKey.ModifierKeys modifierKeys)
    {
        Text = new HotKey(key, modifierKeys).Display();
        ForeColor = Color.Crimson;
    }

    private static bool IsSpecialKey(Keys key)
    {
        // Function keys
        if (key >= Keys.F1 && key <= Keys.F24)
            return true;

        // Media and special keys
        return key switch
        {
            Keys.PrintScreen or Keys.Scroll or Keys.Pause or Keys.Insert or Keys.Home or Keys.PageUp or Keys.Delete or Keys.End or Keys.PageDown or Keys.Left or Keys.Up or Keys.Right or Keys.Down or Keys.NumLock or Keys.Play or Keys.Zoom or Keys.SelectMedia or Keys.MediaStop or Keys.MediaPlayPause or Keys.MediaNextTrack or Keys.MediaPreviousTrack or Keys.VolumeDown or Keys.VolumeUp or Keys.VolumeMute or Keys.BrowserBack or Keys.BrowserFavorites or Keys.BrowserForward or Keys.BrowserHome or Keys.BrowserRefresh or Keys.BrowserSearch or Keys.BrowserStop or Keys.LaunchApplication1 or Keys.LaunchApplication2 or Keys.LaunchMail or Keys.Multiply or Keys.Divide or Keys.Add or Keys.Subtract => true,
            _ => false,
        };
    }
}
