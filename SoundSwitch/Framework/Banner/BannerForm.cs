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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Banner
{
    /// <summary>
    /// This class implements the UI form used to show a Banner notification.
    /// </summary>
    public partial class BannerForm : Form
    {
        private Timer timerHide;
        private bool hiding;
        private System.Media.SoundPlayer player;

        /// <summary>
        /// Constructor for the <see cref="BannerForm"/> class
        /// </summary>
        public BannerForm()
        {
            InitializeComponent();

            this.Location = new Point(50, 60);
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
            if (this.timerHide == null)
            {
                this.timerHide = new Timer();
                this.timerHide.Interval = 3000;
                this.timerHide.Tick += TimerHide_Tick;
            }
            else
            {
                this.timerHide.Enabled = false;
            }

            if (data.Image != null)
                this.pbxLogo.Image = data.Image;

            DestroySound();

            if (!string.IsNullOrEmpty(data.SoundFilePath))
            {
                this.player = new System.Media.SoundPlayer();
                player.SoundLocation = data.SoundFilePath;
                player.Play();
            }

            this.hiding = false;
            this.Opacity = .8;
            this.lblTop.Text = data.Title;
            this.lblTitle.Text = data.Text;
            this.timerHide.Enabled = true;

            this.Show();
        }

        /// <summary>
        /// Destroy current sound player (if any)
        /// </summary>
        private void DestroySound()
        {
            if (this.player != null)
            {
                this.player.Dispose();
                this.player = null;
            }
        }

        /// <summary>
        /// Event handler for the "hiding" timer.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void TimerHide_Tick(object sender, EventArgs e)
        {
            this.hiding = true;
            this.timerHide.Enabled = false;
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
            while (this.Opacity > 0.0)
            {
                await Task.Delay(50);

                if (!this.hiding)
                    break;
                this.Opacity -= 0.05;
            }

            if (this.hiding)
            {
                this.Dispose();
            }
        }
    }
}
