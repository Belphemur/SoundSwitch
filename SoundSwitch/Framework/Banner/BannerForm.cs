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
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Model;
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


        private WasapiOut _player;

        /// <summary>
        /// Constructor for the <see cref="BannerForm"/> class
        /// </summary>
        public BannerForm()
        {
            InitializeComponent();
            var screen = AppModel.Instance.NotifyUsingPrimaryScreen ? Screen.PrimaryScreen : Screen.FromPoint(Cursor.Position);
            StartPosition = FormStartPosition.Manual;
            Bounds = screen.Bounds;

            Location = new Point(screen.Bounds.X + 50, screen.Bounds.Y + 60);
        }

        protected override bool ShowWithoutActivation => true;

        /// <summary>
        /// Override the parameters used to create the window handle.
        /// Ensure that the window will be top-most and do not activate or take focus.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                p.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return p;
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
            if (_timerHide == null)
            {
                _timerHide = new Timer {Interval = 3000};
                _timerHide.Tick += TimerHide_Tick;
            }
            else
            {
                _timerHide.Enabled = false;
            }

            if (data.Image != null)
                pbxLogo.Image = data.Image;

            DestroySound();

            if (data.SoundFile != null)
            {
                PrepareSound(data);
            }

            _hiding = false;
            Opacity = .8;
            lblTop.Text = data.Title;
            lblTitle.Text = data.Text;
            _timerHide.Enabled = true;

            Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void PrepareSound(BannerData data)
        {
            _player = new WasapiOut();
            var task = new Task(() =>
            {
                using var waveStream = new CachedSoundWaveStream(data.SoundFile);
                _player.Init(waveStream);
                _player.Play();
                while (_player.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            });
            task.Start();
        }

        /// <summary>
        /// Destroy current sound player (if any)
        /// </summary>
        private void DestroySound()
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
            }
        }
      

        /// <summary>
        /// Event handler for the "hiding" timer.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void TimerHide_Tick(object sender, EventArgs e)
        {
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
