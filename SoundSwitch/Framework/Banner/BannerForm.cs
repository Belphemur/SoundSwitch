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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio.Play;
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
    private Timer _timerHide;
    private bool _hiding;
    private BannerData _currentData;
    private CancellationTokenSource _cancellationTokenSource = new();
    private int _currentOffset;
    private int _hide = 100;
    private float _defaultFontSize;
    private Size _defaultPictureSize;
    private Padding _defaultPadding;
    private bool _isCompact;
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Get the Screen object
    /// </summary>
    private Screen GetScreen()
    {
        return (AppModel.Instance.NotifyUsingPrimaryScreen ? Screen.PrimaryScreen : Screen.FromPoint(Cursor.Position))!;
    }

    /// <summary>
    /// Constructor for the <see cref="BannerForm"/> class
    /// </summary>
    public BannerForm()
    {
        InitializeComponent();
        var screen = GetScreen();
        StartPosition = FormStartPosition.Manual;
        Bounds = screen.Bounds;
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;

        // Store default sizes for compact mode calculations
        _defaultFontSize = Font.Size;
        _defaultPictureSize = pbxLogo.Size;
        _defaultPadding = Padding;

        // Add click event handlers
        Click += BannerForm_Click;
        lblTitle.Click += BannerForm_Click;
        lblTop.Click += BannerForm_Click;
        pbxLogo.Click += BannerForm_Click;
        // Enable double buffering to reduce flicker
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);

        // Also enable double buffering for container controls
        foreach (Control control in Controls)
        {
            EnableDoubleBuffering(control);
        }
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // turn on WS_EX_TOOLWINDOW style bit
            // Used to hide the banner from alt+tab
            // source: https://www.csharp411.com/hide-form-from-alttab/
            cp.ExStyle |= 0x80;  // WS_EX_TOOLWINDOW
            // Add WS_EX_NOACTIVATE to prevent stealing focus
            cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
            return cp;
        }
    }

    // Add this method to the BannerForm class

    /// <summary>
    /// Override the window procedure to handle paint messages and fix WS_EX_NOACTIVATE click issues
    /// </summary>
    /// <param name="m">The Windows message to process</param>
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        const int HTCLIENT = 1;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_ERASEBKGND = 0x0014;

        if (m.Msg == WM_NCHITTEST)
        {
            // Return HTCLIENT to handle mouse clicks properly with WS_EX_NOACTIVATE style
            m.Result = (IntPtr)HTCLIENT;
            return;
        }
        else if (m.Msg == WM_LBUTTONUP)
        {
            // Handle mouse clicks manually for WS_EX_NOACTIVATE windows
            Point location = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16));
            BannerForm_Click(this, new MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0));
            m.Result = IntPtr.Zero;
            return;
        }
        else if (m.Msg == WM_ERASEBKGND)
        {
            // Return non-zero to indicate we handled erasing the background
            // This prevents flickering during resize/update operations
            m.Result = (IntPtr)1;
            return;
        }

        base.WndProc(ref m);
    }

    /// <summary>
    /// Enables double buffering for a control to reduce flickering
    /// </summary>
    /// <param name="control">The control to enable double buffering on</param>
    private void EnableDoubleBuffering(Control control)
    {
        // Use reflection to enable double buffering on the control
        typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?
            .SetValue(control, true);

        // Apply to child controls recursively
        foreach (Control childControl in control.Controls)
        {
            EnableDoubleBuffering(childControl);
        }
    }


    /// <summary>
    /// Called internally to configure pass notification parameters
    /// </summary>
    /// <param name="data">The configuration data to setup the notification UI</param>
    internal void SetData(BannerData data)
    {
        if (_currentData != null && _currentData.Priority > data.Priority)
        {
            return;
        }

        _currentData = data;
        // No need for timer since there isn't any ttl
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
        Opacity = .9;
        lblTitle.Text = data.Text;
        lblTop.Text = data.Title;

        // Apply compact mode scaling if requested
        if (data.CompactMode)
            ApplyCompactMode();

        Region = Region.FromHrgn(RoundedCorner.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        var screen = GetScreen();

        Location = data.Position.GetScreenPosition(screen, Height, Width, _currentOffset);

        if (_timerHide != null)
            _timerHide.Enabled = true;

        Show();

    }

    /// <summary>
    /// Updates the banner's position using its configured position settings and the provided offset
    /// </summary>
    /// <param name="offset">Vertical offset to apply to the banner's position</param>
    public void UpdatePosition(int offset)
    {
        if (_currentData?.Position == null)
            return;

        var screen = GetScreen();
        Location = _currentData.Position.GetScreenPosition(screen, Height, Width, offset);
        _currentOffset = offset;
    }

    /// <summary>
    /// Update Location of banner depending of the position change
    /// </summary>
    /// <param name="positionChange"></param>
    /// <param name="opacityChange"></param>
    /// <param name="hideChange"></param>
    public void UpdateLocationOpacity(int positionChange, double opacityChange, int hideChange)
    {
        var screen = GetScreen();
        _currentOffset += positionChange;
        Location = _currentData.Position.GetScreenPosition(screen, Height, Width, _currentOffset);
        Opacity -= opacityChange;
        _hide -= hideChange;
        if (Opacity <= 0.0 || _hide <= 0)
        {
            _hiding = true;
            Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    private void PrepareSound(BannerData data)
    {
        JobScheduler.Instance.ScheduleJob(new PlaySoundJob(data.CurrentDeviceId, data.SoundFile), _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Destroy current sound player (if any)
    /// </summary>
    private void DestroySound()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    /// <summary>
    /// Applies or removes compact mode styling to the banner
    /// </summary>
    private void ApplyCompactMode()
    {
        // If already in compact mode, do nothing
        if (_isCompact)
            return;

        const float scaleFactorImage = 0.1f;
        const float scaleFactorFont = 0.8f;

        // Scale font
        Font = new Font(Font.FontFamily, _defaultFontSize * scaleFactorFont, Font.Style);
        lblTop.Font = new Font(lblTop.Font.FontFamily, lblTop.Font.Size * scaleFactorFont, lblTop.Font.Style);
        lblTitle.Font = new Font(lblTitle.Font.FontFamily, lblTitle.Font.Size * scaleFactorFont, lblTitle.Font.Style);

        // Scale image
        if (pbxLogo.Image != null && _defaultPictureSize.Width > 0 && _defaultPictureSize.Height > 0)
        {
            var newWidth = (int)(_defaultPictureSize.Width * scaleFactorImage);
            var newHeight = (int)(_defaultPictureSize.Height * scaleFactorImage);

            if (newWidth > 0 && newHeight > 0)
            {
                pbxLogo.Size = new Size(newWidth, newHeight);
            }
        }

        // Scale padding
        Padding = new Padding(
            (int)(_defaultPadding.Left * scaleFactorImage),
            (int)(_defaultPadding.Top * scaleFactorImage),
            (int)(_defaultPadding.Right * scaleFactorImage),
            (int)(_defaultPadding.Bottom * scaleFactorImage)
        );

        // Force the form to recalculate its size
        PerformLayout();
        _isCompact = true;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timerHide?.Dispose();
            _cancellationTokenSource?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Event handler for the "hiding" timer.
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">Arguments of the event</param>
    private void TimerHide_Tick(object sender, EventArgs e)
    {
        TriggerHidingDisposal();
    }

    /// <summary>
    /// Trigger hiding the banner and dispose when done fading out.
    /// </summary>
    private void TriggerHidingDisposal()
    {
        if (_hiding) return;

        _hiding = true;
        _timerHide.Enabled = false;
        DestroySound();
        FadeOut();
    }

    /// <summary>
    /// Implements an "fadeout" animation while hiding the window.
    /// In the end of the animation the form is self disposed.
    /// <remarks>The animation is canceled if the method <see cref="SetData"/> is called along the animation.</remarks>
    /// </summary>
    private async void FadeOut()
    {
        try
        {
            while (Opacity > 0.0)
            {
                await Task.Delay(50);

                if (!_hiding)
                    break;
                Opacity -= 0.05;
            }

            if (_hiding)
            {
                Dispose();
            }
        }
        catch (Win32Exception)
        {
            try
            {
                Dispose();
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }

    /// <summary>
    /// Event handler for clicks on any part of the banner
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">Arguments of the event</param>
    private void BannerForm_Click(object sender, EventArgs e)
    {
        if (_currentData?.OnClick == null)
        {
            TriggerHidingDisposal();
            return;
        }

        // Invoke the click callback if it's set
        _currentData?.OnClick?.Invoke(this, e);
    }
}