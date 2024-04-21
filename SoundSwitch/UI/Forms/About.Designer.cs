namespace SoundSwitch.UI.Forms
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            appNameLabel = new System.Windows.Forms.Label();
            createdByLabel = new System.Windows.Forms.Label();
            creatorLinkLabel = new System.Windows.Forms.LinkLabel();
            versionLabel = new System.Windows.Forms.Label();
            versionLinkLabel = new System.Windows.Forms.LinkLabel();
            authorAndProgramInfoGroupBox = new System.Windows.Forms.GroupBox();
            maintainerLinkLabel = new System.Windows.Forms.LinkLabel();
            maintainedByLabel = new System.Windows.Forms.Label();
            creditsAndAttributionGroupBox = new System.Windows.Forms.GroupBox();
            logoCreatorLabel = new System.Windows.Forms.LinkLabel();
            logoMadeLabel = new System.Windows.Forms.Label();
            defaultPlaybackDeviceChangeLinkLabel = new System.Windows.Forms.LinkLabel();
            defaultPlaybackDeviceChangeLabel = new System.Windows.Forms.Label();
            keyboardHotKeySystemLinkLabel = new System.Windows.Forms.LinkLabel();
            keyboardHotKeySystemLabel = new System.Windows.Forms.Label();
            iconsLinkLabel = new System.Windows.Forms.LinkLabel();
            iconsLabel = new System.Windows.Forms.Label();
            soundSwitchPictureBox = new System.Windows.Forms.PictureBox();
            authorAndProgramInfoGroupBox.SuspendLayout();
            creditsAndAttributionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).BeginInit();
            SuspendLayout();
            // 
            // appNameLabel
            // 
            appNameLabel.AutoSize = true;
            appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            appNameLabel.Location = new System.Drawing.Point(72, 13);
            appNameLabel.Name = "appNameLabel";
            appNameLabel.Size = new System.Drawing.Size(184, 31);
            appNameLabel.TabIndex = 0;
            appNameLabel.Text = "SoundSwitch";
            // 
            // createdByLabel
            // 
            createdByLabel.AutoSize = true;
            createdByLabel.Location = new System.Drawing.Point(6, 47);
            createdByLabel.Name = "createdByLabel";
            createdByLabel.Size = new System.Drawing.Size(64, 15);
            createdByLabel.TabIndex = 1;
            createdByLabel.Text = "Created by";
            // 
            // creatorLinkLabel
            // 
            creatorLinkLabel.AutoSize = true;
            creatorLinkLabel.Location = new System.Drawing.Point(148, 47);
            creatorLinkLabel.Name = "creatorLinkLabel";
            creatorLinkLabel.Size = new System.Drawing.Size(90, 15);
            creatorLinkLabel.TabIndex = 2;
            creatorLinkLabel.TabStop = true;
            creatorLinkLabel.Text = "Jeroen Pelgrims";
            creatorLinkLabel.LinkClicked += CreatorLabel_LinkClicked;
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Location = new System.Drawing.Point(6, 71);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(45, 15);
            versionLabel.TabIndex = 3;
            versionLabel.Text = "Version";
            // 
            // versionLinkLabel
            // 
            versionLinkLabel.AutoSize = true;
            versionLinkLabel.Location = new System.Drawing.Point(148, 71);
            versionLinkLabel.Name = "versionLinkLabel";
            versionLinkLabel.Size = new System.Drawing.Size(79, 15);
            versionLinkLabel.TabIndex = 4;
            versionLinkLabel.TabStop = true;
            versionLinkLabel.Text = "X.XX.X.XXXXX";
            versionLinkLabel.LinkClicked += Version_LinkClicked;
            // 
            // authorAndProgramInfoGroupBox
            // 
            authorAndProgramInfoGroupBox.Controls.Add(maintainerLinkLabel);
            authorAndProgramInfoGroupBox.Controls.Add(maintainedByLabel);
            authorAndProgramInfoGroupBox.Controls.Add(creatorLinkLabel);
            authorAndProgramInfoGroupBox.Controls.Add(versionLinkLabel);
            authorAndProgramInfoGroupBox.Controls.Add(createdByLabel);
            authorAndProgramInfoGroupBox.Controls.Add(versionLabel);
            authorAndProgramInfoGroupBox.Location = new System.Drawing.Point(8, 57);
            authorAndProgramInfoGroupBox.Name = "authorAndProgramInfoGroupBox";
            authorAndProgramInfoGroupBox.Size = new System.Drawing.Size(270, 97);
            authorAndProgramInfoGroupBox.TabIndex = 5;
            authorAndProgramInfoGroupBox.TabStop = false;
            authorAndProgramInfoGroupBox.Text = "Author and Program Info";
            // 
            // maintainerLinkLabel
            // 
            maintainerLinkLabel.AutoSize = true;
            maintainerLinkLabel.Location = new System.Drawing.Point(148, 21);
            maintainerLinkLabel.Name = "maintainerLinkLabel";
            maintainerLinkLabel.Size = new System.Drawing.Size(83, 15);
            maintainerLinkLabel.TabIndex = 6;
            maintainerLinkLabel.TabStop = true;
            maintainerLinkLabel.Text = "Antoine Aflalo";
            maintainerLinkLabel.LinkClicked += MaintainerLinkLabel_LinkClicked;
            // 
            // maintainedByLabel
            // 
            maintainedByLabel.AutoSize = true;
            maintainedByLabel.Location = new System.Drawing.Point(6, 21);
            maintainedByLabel.Name = "maintainedByLabel";
            maintainedByLabel.Size = new System.Drawing.Size(83, 15);
            maintainedByLabel.TabIndex = 5;
            maintainedByLabel.Text = "Maintained by";
            // 
            // creditsAndAttributionGroupBox
            // 
            creditsAndAttributionGroupBox.Controls.Add(logoCreatorLabel);
            creditsAndAttributionGroupBox.Controls.Add(logoMadeLabel);
            creditsAndAttributionGroupBox.Controls.Add(defaultPlaybackDeviceChangeLinkLabel);
            creditsAndAttributionGroupBox.Controls.Add(defaultPlaybackDeviceChangeLabel);
            creditsAndAttributionGroupBox.Controls.Add(keyboardHotKeySystemLinkLabel);
            creditsAndAttributionGroupBox.Controls.Add(keyboardHotKeySystemLabel);
            creditsAndAttributionGroupBox.Controls.Add(iconsLinkLabel);
            creditsAndAttributionGroupBox.Controls.Add(iconsLabel);
            creditsAndAttributionGroupBox.Location = new System.Drawing.Point(8, 169);
            creditsAndAttributionGroupBox.Name = "creditsAndAttributionGroupBox";
            creditsAndAttributionGroupBox.Size = new System.Drawing.Size(270, 146);
            creditsAndAttributionGroupBox.TabIndex = 6;
            creditsAndAttributionGroupBox.TabStop = false;
            creditsAndAttributionGroupBox.Text = "Credits and Attribution";
            // 
            // logoCreatorLabel
            // 
            logoCreatorLabel.AutoSize = true;
            logoCreatorLabel.Location = new System.Drawing.Point(148, 105);
            logoCreatorLabel.Name = "logoCreatorLabel";
            logoCreatorLabel.Size = new System.Drawing.Size(82, 15);
            logoCreatorLabel.TabIndex = 9;
            logoCreatorLabel.TabStop = true;
            logoCreatorLabel.Text = "@linadesteem";
            logoCreatorLabel.LinkClicked += LogoCreatorLinkLabel_LinkClicked;
            // 
            // logoMadeLabel
            // 
            logoMadeLabel.Location = new System.Drawing.Point(6, 105);
            logoMadeLabel.Name = "logoMadeLabel";
            logoMadeLabel.Size = new System.Drawing.Size(137, 28);
            logoMadeLabel.TabIndex = 8;
            logoMadeLabel.Text = "Logo made by";
            // 
            // defaultPlaybackDeviceChangeLinkLabel
            // 
            defaultPlaybackDeviceChangeLinkLabel.AutoSize = true;
            defaultPlaybackDeviceChangeLinkLabel.Location = new System.Drawing.Point(148, 71);
            defaultPlaybackDeviceChangeLinkLabel.Name = "defaultPlaybackDeviceChangeLinkLabel";
            defaultPlaybackDeviceChangeLinkLabel.Size = new System.Drawing.Size(38, 15);
            defaultPlaybackDeviceChangeLinkLabel.TabIndex = 7;
            defaultPlaybackDeviceChangeLinkLabel.TabStop = true;
            defaultPlaybackDeviceChangeLinkLabel.Text = "EreTIk";
            defaultPlaybackDeviceChangeLinkLabel.LinkClicked += EretikLinkLabel_LinkClicked;
            // 
            // defaultPlaybackDeviceChangeLabel
            // 
            defaultPlaybackDeviceChangeLabel.Location = new System.Drawing.Point(6, 71);
            defaultPlaybackDeviceChangeLabel.Name = "defaultPlaybackDeviceChangeLabel";
            defaultPlaybackDeviceChangeLabel.Size = new System.Drawing.Size(137, 28);
            defaultPlaybackDeviceChangeLabel.TabIndex = 4;
            defaultPlaybackDeviceChangeLabel.Text = "Default Playback device change";
            // 
            // keyboardHotKeySystemLinkLabel
            // 
            keyboardHotKeySystemLinkLabel.AutoSize = true;
            keyboardHotKeySystemLinkLabel.Location = new System.Drawing.Point(148, 46);
            keyboardHotKeySystemLinkLabel.Name = "keyboardHotKeySystemLinkLabel";
            keyboardHotKeySystemLinkLabel.Size = new System.Drawing.Size(118, 15);
            keyboardHotKeySystemLinkLabel.TabIndex = 3;
            keyboardHotKeySystemLinkLabel.TabStop = true;
            keyboardHotKeySystemLinkLabel.Text = "Christian Liensberger";
            keyboardHotKeySystemLinkLabel.LinkClicked += LiensbergerLinkLabel_LinkClicked;
            // 
            // keyboardHotKeySystemLabel
            // 
            keyboardHotKeySystemLabel.AutoSize = true;
            keyboardHotKeySystemLabel.Location = new System.Drawing.Point(6, 46);
            keyboardHotKeySystemLabel.Name = "keyboardHotKeySystemLabel";
            keyboardHotKeySystemLabel.Size = new System.Drawing.Size(139, 15);
            keyboardHotKeySystemLabel.TabIndex = 2;
            keyboardHotKeySystemLabel.Text = "Keyboard HotKey system";
            // 
            // iconsLinkLabel
            // 
            iconsLinkLabel.AutoSize = true;
            iconsLinkLabel.Location = new System.Drawing.Point(148, 23);
            iconsLinkLabel.Name = "iconsLinkLabel";
            iconsLinkLabel.Size = new System.Drawing.Size(62, 15);
            iconsLinkLabel.TabIndex = 1;
            iconsLinkLabel.TabStop = true;
            iconsLinkLabel.Text = "Pastel SVG";
            iconsLinkLabel.LinkClicked += IconsLinkLabel_LinkClicked;
            // 
            // iconsLabel
            // 
            iconsLabel.AutoSize = true;
            iconsLabel.Location = new System.Drawing.Point(6, 23);
            iconsLabel.Name = "iconsLabel";
            iconsLabel.Size = new System.Drawing.Size(35, 15);
            iconsLabel.TabIndex = 0;
            iconsLabel.Text = "Icons";
            // 
            // soundSwitchPictureBox
            // 
            soundSwitchPictureBox.Image = Properties.Resources.SoundSwitch48;
            soundSwitchPictureBox.Location = new System.Drawing.Point(21, 2);
            soundSwitchPictureBox.Name = "soundSwitchPictureBox";
            soundSwitchPictureBox.Size = new System.Drawing.Size(51, 49);
            soundSwitchPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            soundSwitchPictureBox.TabIndex = 7;
            soundSwitchPictureBox.TabStop = false;
            // 
            // About
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(284, 320);
            Controls.Add(soundSwitchPictureBox);
            Controls.Add(creditsAndAttributionGroupBox);
            Controls.Add(authorAndProgramInfoGroupBox);
            Controls.Add(appNameLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "About";
            Text = "About SoundSwitch";
            Load += About_Load;
            authorAndProgramInfoGroupBox.ResumeLayout(false);
            authorAndProgramInfoGroupBox.PerformLayout();
            creditsAndAttributionGroupBox.ResumeLayout(false);
            creditsAndAttributionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.GroupBox authorAndProgramInfoGroupBox;
        private System.Windows.Forms.Label createdByLabel;
        private System.Windows.Forms.LinkLabel creatorLinkLabel;
        private System.Windows.Forms.GroupBox creditsAndAttributionGroupBox;
        private System.Windows.Forms.Label defaultPlaybackDeviceChangeLabel;
        private System.Windows.Forms.LinkLabel defaultPlaybackDeviceChangeLinkLabel;
        private System.Windows.Forms.Label iconsLabel;
        private System.Windows.Forms.LinkLabel iconsLinkLabel;
        private System.Windows.Forms.Label keyboardHotKeySystemLabel;
        private System.Windows.Forms.LinkLabel keyboardHotKeySystemLinkLabel;
        private System.Windows.Forms.LinkLabel logoCreatorLabel;
        private System.Windows.Forms.Label logoMadeLabel;
        private System.Windows.Forms.Label maintainedByLabel;
        private System.Windows.Forms.LinkLabel maintainerLinkLabel;
        private System.Windows.Forms.PictureBox soundSwitchPictureBox;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.LinkLabel versionLinkLabel;

        #endregion
    }
}