/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015 Antoine Aflalo
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

using System.Diagnostics;
using System.Windows.Forms;
using SoundSwitch.Properties;

namespace SoundSwitch.UI.Forms
{
    public partial class About : Form
    {

        public About()
        {
            InitializeComponent();
            Icon = Resources.HelpIcon;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://jeroenpelgrims.be");
        }

        private void About_Load(object sender, System.EventArgs e)
        {
            var version = Application.ProductVersion;
            txtVersion.Text = version;
            appNameLabel.Text = Application.ProductName;
        }

        private void Version_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Belphemur/SoundSwitch/releases");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://codefisher.org/pastel-svg/");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.liensberger.it/web/blog/?p=207");
        }

        private void maintainerLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.aaflalo.me");
        }

        private void eretikLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://eretik.omegahg.com");
        }
    }
}