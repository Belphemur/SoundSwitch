﻿/********************************************************************
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
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio.Play;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Model;
using SoundSwitch.UI.Menu.Util;
using Timer = System.Windows.Forms.Timer;

namespace SoundSwitch.Framework.Banner
{
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
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Get the Screen object
        /// </summary>
        private static Screen GetScreen()
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
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        // /// <summary>
        // /// Override the parameters used to create the window handle.
        // /// Ensure that the window will be top-most and do not activate or take focus.
        // /// </summary>
        // protected override CreateParams CreateParams
        // {
        //     get
        //     {
        //         CreateParams p = base.CreateParams;
        //         p.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
        //         p.ExStyle |= 0x00000008; // WS_EX_TOPMOST
        //         return p;
        //     }
        // }

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
            if (_timerHide == null)
            {
                _timerHide = new Timer { Interval = (int)data.Ttl.TotalMilliseconds };
                _timerHide.Tick += TimerHide_Tick!;
            }
            else
            {
                _timerHide.Enabled = false;
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
            lblTop.Text = data.Title;
            lblTitle.Text = data.Text;
            Region = Region.FromHrgn(RoundedCorner.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            var screen = GetScreen();

            Location = data.Position.GetScreenPosition(screen, Height, Width, _currentOffset);

            _timerHide.Enabled = true;

            Show();
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
    }
}