/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
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

using System.Windows.Forms;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.Util.Url;

namespace SoundSwitch.UI.Forms
{
    public partial class About : Form
    {
        private static readonly System.Drawing.Icon helpIcon = Resources.HelpIcon;

        public About()
        {
            RightToLeft = new LanguageFactory().Get(AppModel.Instance.Language).IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            InitializeComponent();
            Icon = helpIcon;
        }

        private void About_Load(object sender, System.EventArgs e)
        {
            LocalizeForm();
            versionLinkLabel.Text = Application.ProductVersion;
            appNameLabel.Text = Application.ProductName;
        }

        private void LocalizeForm()
        {
            // Form itself
            Text = TrayIconStrings.about;

            // Author and Program Info
            authorAndProgramInfoGroupBox.Text = AboutStrings.author;
            maintainedByLabel.Text = AboutStrings.maintained;
            createdByLabel.Text = AboutStrings.created;
            versionLabel.Text = AboutStrings.version;
            logoMadeLabel.Text = AboutStrings.logo;

            // Credits and Attribution
            creditsAndAttributionGroupBox.Text = AboutStrings.credits;
            iconsLabel.Text = AboutStrings.icons;
            keyboardHotKeySystemLabel.Text = AboutStrings.keyboardHotkeys;
            defaultPlaybackDeviceChangeLabel.Text = AboutStrings.defaultPlaybackDeviceChange;
        }
        private void MaintainerLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("https://www.aaflalo.me");
        }


        private void CreatorLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("http://jeroenpelgrims.be");
        }

        private void Version_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("https://github.com/Belphemur/SoundSwitch/releases");
        }

        private void IconsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("https://codefisher.org/pastel-svg/");
        }

        private void LiensbergerLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("http://www.liensberger.it/web/blog/?p=207");
        }

        private void EretikLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("http://eretik.omegahg.com");
        }

        private void LogoCreatorLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowserUtil.OpenUrl("https://github.com/linadesteem");
        }
    }
}