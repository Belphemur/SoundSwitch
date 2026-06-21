/********************************************************************
 * Copyright (C) 2015-2017 Antoine Aflalo
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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Serilog;

using SoundSwitch.Framework.Audio.Play;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Model;
using SoundSwitch.UI.Menu.Util;

using Timer = System.Windows.Forms.Timer;

namespace SoundSwitch.Framework.Banner;

/// <summary>
/// This class implements the UI form used to show a Banner notification.
/// </summary>
public partial class BannerForm : Form
{
    private static readonly IntPtr HWND_TOPMOST = new(-1);

    [Flags]
    private enum SetWindowPosFlags : uint
    {
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOACTIVATE = 0x0010,
    }

    /// <summary>
    /// Mirrors the native WINDOWPOS struct received via WM_WINDOWPOSCHANGING /
    /// WM_WINDOWPOSCHANGED.
    /// Used to inject <c>SWP_NOACTIVATE</c> into every position change so the
    /// banner never steals focus.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    private sealed class CustomPositionMessageFilter(BannerForm owner) : IMessageFilter
    {
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WmKeyDown && m.Msg != WmSysKeyDown)
                return false;

            return owner.HandleCustomPositionKey((Keys)m.WParam.ToInt32());
        }
    }

    private Timer _timerHide;
    private bool _hiding;
    private BannerData _currentData;
    private BannerPositionFactory _bannerPositionFactory = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private int _currentOffset;
    private int _hide = 100;
    private float _defaultFontSize;
    private Size _defaultPictureSize;
    private Padding _defaultPadding;
    private bool _isCompact;
    private Point _lastMousePosition;
    private CustomPositionMessageFilter _customPositionMessageFilter;

    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Get the Screen object
    /// </summary>
    internal static Screen GetScreen() =>
        (AppModel.Instance.NotifyUsingPrimaryScreen ? Screen.PrimaryScreen : Screen.FromPoint(Cursor.Position))!;

    /// <summary>
    /// Constructor for the <see cref="BannerForm"/> class
    /// </summary>
    public BannerForm()
    {
        InitializeComponent();
        StartPosition = FormStartPosition.Manual;
        Bounds = GetScreen().Bounds;
        TopLevel = true;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;

        // Store default sizes for compact mode calculations
        _defaultFontSize = Font.Size;
        _defaultPictureSize = pbxLogo.Size;
        _defaultPadding = Padding;

        // Register event handlers
        RegisterHandlers(this);
        RegisterHandlers(lblTitle);
        RegisterHandlers(lblTop);
        RegisterHandlers(pbxLogo);
        RegisterHandlers(tableLayoutPanel);

        // Enable double buffering to reduce flicker
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);

        // Also enable double buffering for container controls
        foreach (Control control in Controls)
            EnableDoubleBuffering(control);
    }

    private void RegisterHandlers(Control control)
    {
        control.Click += BannerForm_Click;
        control.MouseDown += BannerForm_MouseDown;
        control.MouseUp += BannerForm_MouseUp;
        control.MouseMove += BannerForm_MouseMove;
        control.KeyDown += BannerForm_KeyDown;
    }

    protected override bool ShowWithoutActivation => true;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        SetWindowPosFlags uFlags);

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // WS_EX_TOOLWINDOW — hide the banner from Alt+Tab
            cp.ExStyle |= 0x80;
            // WS_EX_NOACTIVATE — prevent the window from being activated
            cp.ExStyle |= 0x08000000;
            // WS_EX_TOPMOST — stay on top even in exclusive fullscreen
            cp.ExStyle |= 0x00000008;
            // WS_EX_LAYERED — prevent handle recreation when Opacity changes
            cp.ExStyle |= 0x00080000;
            return cp;
        }
    }

    /// <summary>
    /// Override the window procedure to handle paint messages and fix
    /// WS_EX_NOACTIVATE click issues.
    ///
    /// Focus-steal prevention uses three layers:
    /// 1. WM_MOUSEACTIVATE  → MA_NOACTIVATE   (blocks click-triggered activation)
    /// 2. WM_WINDOWPOSCHANGING → SWP_NOACTIVATE (blocks position-change activation)
    /// 3. WM_ACTIVATE (WA_ACTIVE/WA_CLICKACTIVE) → swallowed (last-resort backstop)
    /// </summary>
    /// <param name="m">The Windows message to process</param>
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        const int HTCLIENT = 1;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_ERASEBKGND = 0x0014;
        const int WM_ACTIVATE = 0x0006;
        const int WA_INACTIVE = 0;
        const int WA_ACTIVE = 1;
        const int WA_CLICKACTIVE = 2;
        const int WM_MOUSEACTIVATE = 0x0021;
        const int MA_NOACTIVATE = 3;
        const int WM_WINDOWPOSCHANGING = 0x0046;
        const uint SWP_NOACTIVATE = 0x0010;
        const int MK_LBUTTON = 0x0001;

        switch (m.Msg)
        {
            case WM_WINDOWPOSCHANGING:
                // Intercept every SetWindowPos call (including internal WinForms
                // and system calls) and inject SWP_NOACTIVATE so the banner can
                // never accidentally steal focus from a fullscreen game or any
                // other app.
                var windowPos = Marshal.PtrToStructure<WINDOWPOS>(m.LParam);
                windowPos.flags |= SWP_NOACTIVATE;
                Marshal.StructureToPtr(windowPos, m.LParam, false);
                base.WndProc(ref m);
                return;

            case WM_ACTIVATE:
                int activationState = m.WParam.ToInt32() & 0xFFFF;

                if (activationState == WA_INACTIVE)
                {
                    // Safe to pass to base: WA_INACTIVE means the window is
                    // LOSING focus. DefWindowProc does NOT call SetFocus here,
                    // so there is no risk of stealing focus. Passing it through
                    // lets the Form.Deactivate event fire normally.
                    base.WndProc(ref m);
                }
                // WA_ACTIVE and WA_CLICKACTIVE: swallow entirely.
                // DefWindowProc would call SetFocus() for these states, which
                // would steal focus. This is the final backstop for any
                // OS/programmatic activation that slips past the other layers.
                return;

            case WM_MOUSEACTIVATE:
                // Prevent mouse clicks from activating the window while still
                // allowing the click message to be delivered (MA_NOACTIVATE,
                // NOT MA_NOACTIVATEEAT, so the click still reaches us).
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;

            case WM_NCHITTEST:
                // Borderless form: force all hit-testing to the client area so
                // mouse messages are delivered normally.
                m.Result = HTCLIENT;
                return;

            case WM_LBUTTONDOWN:
                // Forward left-button press manually so dragging works in
                // CustomPositionMode. The (short) casts correctly handle
                // negative screen coordinates on multi-monitor setups.
                var downPoint = PointToClient(new Point(
                    (short)(m.LParam.ToInt32() & 0xFFFF),
                    (short)(m.LParam.ToInt32() >> 16)));
                BannerForm_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, downPoint.X, downPoint.Y, 0));
                m.Result = IntPtr.Zero;
                return;

            case WM_LBUTTONUP:
                // For WS_EX_NOACTIVATE windows we forward the release manually.
                // IMPORTANT: forward BOTH MouseUp and Click. MouseUp is what
                // CustomPositionMode uses to persist the dragged position and
                // restart the hide timer; Click is what normal-mode banners use
                // to dismiss / invoke their OnClick callback. Each handler
                // checks the mode internally, so calling both is safe.
                var upPoint = PointToClient(new Point(
                    (short)(m.LParam.ToInt32() & 0xFFFF),
                    (short)(m.LParam.ToInt32() >> 16)));
                BannerForm_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 1, upPoint.X, upPoint.Y, 0));
                BannerForm_Click(this, new MouseEventArgs(MouseButtons.Left, 1, upPoint.X, upPoint.Y, 0));
                m.Result = IntPtr.Zero;
                return;

            case WM_MOUSEMOVE:
                // Forward mouse moves while the left button is held so the
                // CustomPositionMode drag logic receives them.
                if ((m.WParam.ToInt32() & MK_LBUTTON) != 0)
                {
                    var movePoint = PointToClient(new Point(
                        (short)(m.LParam.ToInt32() & 0xFFFF),
                        (short)(m.LParam.ToInt32() >> 16)));
                    BannerForm_MouseMove(this, new MouseEventArgs(MouseButtons.Left, 0, movePoint.X, movePoint.Y, 0));
                }
                m.Result = IntPtr.Zero;
                return;

            case WM_ERASEBKGND:
                // Return non-zero to indicate we handled erasing the background.
                // This prevents flickering during resize/update operations.
                m.Result = (IntPtr)1;
                return;

            default:
                base.WndProc(ref m);
                break;
        }
    }

    /// <summary>
    /// Enables double buffering for a control to reduce flickering
    /// </summary>
    /// <param name="control">The control to enable double buffering on</param>
    private void EnableDoubleBuffering(Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?
            .SetValue(control, true);

        foreach (Control childControl in control.Controls)
            EnableDoubleBuffering(childControl);
    }

    /// <summary>
    /// Called internally to configure and pass notification parameters
    /// </summary>
    /// <param name="data">The configuration data to setup the notification UI</param>
    internal void SetData(BannerData data)
    {
        if (_currentData != null && _currentData.Priority > data.Priority) return;

        _currentData = data;

        if (data.Ttl != TimeSpan.MaxValue)
        {
            if (_timerHide == null)
            {
                _timerHide = new Timer { Interval = (int)data.Ttl.TotalMilliseconds };
                _timerHide.Tick += TimerHide_Tick!;
            }
            else
            {
                _timerHide.Enabled = false;
            }
        }

        if (data.Image != null)
            pbxLogo.Image = data.Image;

        if (data.SoundFile != null)
        {
            DestroySound();
            PrepareSound(data);
        }

        _hiding = false;
        Opacity = (double)data.Opacity / 100;
        lblTitle.Text = data.Text;
        lblTop.Text = data.Title;

        if (data.CustomPositionMode)
        {
            _customPositionMessageFilter ??= new CustomPositionMessageFilter(this);
            Application.AddMessageFilter(_customPositionMessageFilter);
        }
        else if (_customPositionMessageFilter != null)
        {
            Application.RemoveMessageFilter(_customPositionMessageFilter);
            _customPositionMessageFilter = null;
        }

        if (data.CompactMode)
            ApplyCompactMode();

        Region = Region.FromHrgn(RoundedCorner.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        Location = data.Position.GetScreenPosition(GetScreen(), Height, Width, _currentOffset);

        // C# 14 null-conditional assignment: only assigns when a timer exists
        // (none is created when Ttl == TimeSpan.MaxValue). RHS is evaluated
        // only if _timerHide is non-null.
        _timerHide?.Enabled = true;

        Show();
        EnsureTopMostWithoutActivation();
    }

    /// <summary>
    /// Updates the banner's position using its configured position settings and the provided offset
    /// </summary>
    /// <param name="offset">Vertical offset to apply to the banner's position</param>
    public void UpdatePosition(int offset)
    {
        if (_currentData?.Position == null) return;

        Location = _currentData.Position.GetScreenPosition(GetScreen(), Height, Width, offset);
        _currentOffset = offset;
    }

    /// <summary>
    /// Update Location of banner depending on the position change
    /// </summary>
    public void UpdateLocationOpacity(int positionChange, double opacityChange, int hideChange)
    {
        _currentOffset += positionChange;
        Location = _currentData.Position.GetScreenPosition(GetScreen(), Height, Width, _currentOffset);
        Opacity -= opacityChange;
        _hide -= hideChange;

        if (Opacity <= 0.0 || _hide <= 0)
        {
            _hiding = true;
            Dispose();
        }
    }

    private void PrepareSound(BannerData data)
    {
        JobScheduler.Instance.ScheduleJob(new PlaySoundJob(data.CurrentDeviceId, data.SoundFile), _cancellationTokenSource.Token);
    }

    private void EnsureTopMostWithoutActivation()
    {
        if (!IsHandleCreated) return;

        var flags = SetWindowPosFlags.SWP_NOMOVE |
                    SetWindowPosFlags.SWP_NOSIZE |
                    SetWindowPosFlags.SWP_NOACTIVATE;

        if (SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, flags))
            return;

        var lastError = Marshal.GetLastWin32Error();
        Log.Warning("SetWindowPos failed while refreshing the banner topmost state with Win32Error={error}", lastError);
    }

    private void DestroySound()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    private void ApplyCompactMode()
    {
        if (_isCompact) return;

        const float scaleFactorImage = 0.1f;
        const float scaleFactorFont = 0.8f;

        Font = new Font(Font.FontFamily, _defaultFontSize * scaleFactorFont, Font.Style);
        lblTop.Font = new Font(lblTop.Font.FontFamily, lblTop.Font.Size * scaleFactorFont, lblTop.Font.Style);
        lblTitle.Font = new Font(lblTitle.Font.FontFamily, lblTitle.Font.Size * scaleFactorFont, lblTitle.Font.Style);

        if (pbxLogo.Image != null && _defaultPictureSize.Width > 0 && _defaultPictureSize.Height > 0)
        {
            var newWidth = (int)(_defaultPictureSize.Width * scaleFactorImage);
            var newHeight = (int)(_defaultPictureSize.Height * scaleFactorImage);

            if (newWidth > 0 && newHeight > 0)
                pbxLogo.Size = new Size(newWidth, newHeight);
        }

        Padding = new Padding(
            (int)(_defaultPadding.Left * scaleFactorImage),
            (int)(_defaultPadding.Top * scaleFactorImage),
            (int)(_defaultPadding.Right * scaleFactorImage),
            (int)(_defaultPadding.Bottom * scaleFactorImage));

        PerformLayout();
        _isCompact = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_customPositionMessageFilter != null)
            {
                Application.RemoveMessageFilter(_customPositionMessageFilter);
                _customPositionMessageFilter = null;
            }

            _timerHide?.Dispose();
            _cancellationTokenSource?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void TimerHide_Tick(object sender, EventArgs e) => TriggerHidingDisposal();

    private void TriggerHidingDisposal()
    {
        if (_hiding) return;

        _hiding = true;
        // A persistent banner (Ttl == TimeSpan.MaxValue) has no timer, yet can
        // still be dismissed by a click. C# 14 null-conditional assignment makes
        // this a no-op when _timerHide is null — no NRE, no explicit guard.
        _timerHide?.Enabled = false;
        DestroySound();
        FadeOut();
    }

    private async void FadeOut()
    {
        try
        {
            while (Opacity > 0.0)
            {
                await Task.Delay(50);
                if (!_hiding) break;
                Opacity -= 0.05;
            }

            if (_hiding) Dispose();
        }
        catch (Win32Exception)
        {
            try { Dispose(); }
            catch { /* ignored */ }
        }
    }

    private void BannerForm_Click(object sender, EventArgs e)
    {
        if (_currentData == null || _currentData.CustomPositionMode) return;

        if (_currentData.OnClick == null)
        {
            TriggerHidingDisposal();
            return;
        }

        _currentData.OnClick?.Invoke(this, e);
    }

    private void BannerForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (_currentData == null || !_currentData.CustomPositionMode) return;
        if (e.Button == MouseButtons.Left)
        {
            _lastMousePosition = new Point(e.X, e.Y);
            _timerHide?.Stop();
        }
    }

    private void BannerForm_MouseUp(object sender, MouseEventArgs e)
    {
        if (_currentData == null || !_currentData.CustomPositionMode) return;
        if (e.Button == MouseButtons.Left)
        {
            AppModel.Instance.CustomBannerPosition = Location;
            _timerHide?.Start();
        }
    }

    private void BannerForm_MouseMove(object sender, MouseEventArgs e)
    {
        if (_currentData == null || !_currentData.CustomPositionMode) return;
        if (e.Button == MouseButtons.Left)
        {
            var screen = GetScreen().Bounds;

            Point newLocation = new(
                Left + e.X - _lastMousePosition.X,
                Top + e.Y - _lastMousePosition.Y);

            newLocation.X = Math.Max(0, Math.Min(newLocation.X, screen.Width - Width));
            newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, screen.Height - Height));

            Location = newLocation;
        }
    }

    private void BannerForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (_currentData == null || !_currentData.CustomPositionMode) return;
        if (HandleCustomPositionKey(e.KeyCode))
            e.Handled = true;
    }

    private bool HandleCustomPositionKey(Keys key)
    {
        if (_currentData == null || !_currentData.CustomPositionMode)
            return false;

        switch (key)
        {
            case Keys.Escape:
                Dispose();
                return true;
            case Keys.R:
                Location = Point.Empty;
                AppModel.Instance.CustomBannerPosition = Location;
                return true;
            default:
                return false;
        }
    }
}
