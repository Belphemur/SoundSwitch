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
            this.appNameLabel = new System.Windows.Forms.Label();
            this.createdByLabel = new System.Windows.Forms.Label();
            this.creatorLinkLabel = new System.Windows.Forms.LinkLabel();
            this.versionLabel = new System.Windows.Forms.Label();
            this.versionLinkLabel = new System.Windows.Forms.LinkLabel();
            this.authorAndProgramInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.maintainerLinkLabel = new System.Windows.Forms.LinkLabel();
            this.maintainedByLabel = new System.Windows.Forms.Label();
            this.creditsAndAttributionGroupBox = new System.Windows.Forms.GroupBox();
            this.defaultPlaybackDeviceChangeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.defaultPlaybackDeviceChangeLabel = new System.Windows.Forms.Label();
            this.keyboardHotKeySystemLinkLabel = new System.Windows.Forms.LinkLabel();
            this.keyboardHotKeySystemLabel = new System.Windows.Forms.Label();
            this.iconsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.iconsLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.logoCreatorLabel = new System.Windows.Forms.LinkLabel();
            this.logoMadeLabel = new System.Windows.Forms.Label();
            this.authorAndProgramInfoGroupBox.SuspendLayout();
            this.creditsAndAttributionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // appNameLabel
            // 
            this.appNameLabel.AutoSize = true;
            this.appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appNameLabel.Location = new System.Drawing.Point(72, 13);
            this.appNameLabel.Name = "appNameLabel";
            this.appNameLabel.Size = new System.Drawing.Size(141, 31);
            this.appNameLabel.TabIndex = 0;
            this.appNameLabel.Text = "AppName";
            // 
            // createdByLabel
            // 
            this.createdByLabel.AutoSize = true;
            this.createdByLabel.Location = new System.Drawing.Point(6, 47);
            this.createdByLabel.Name = "createdByLabel";
            this.createdByLabel.Size = new System.Drawing.Size(58, 13);
            this.createdByLabel.TabIndex = 1;
            this.createdByLabel.Text = "Created by";
            // 
            // creatorLinkLabel
            // 
            this.creatorLinkLabel.AutoSize = true;
            this.creatorLinkLabel.Location = new System.Drawing.Point(148, 47);
            this.creatorLinkLabel.Name = "creatorLinkLabel";
            this.creatorLinkLabel.Size = new System.Drawing.Size(81, 13);
            this.creatorLinkLabel.TabIndex = 2;
            this.creatorLinkLabel.TabStop = true;
            this.creatorLinkLabel.Text = "Jeroen Pelgrims";
            this.creatorLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(6, 71);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(42, 13);
            this.versionLabel.TabIndex = 3;
            this.versionLabel.Text = "Version";
            // 
            // versionLinkLabel
            // 
            this.versionLinkLabel.AutoSize = true;
            this.versionLinkLabel.Location = new System.Drawing.Point(148, 71);
            this.versionLinkLabel.Name = "versionLinkLabel";
            this.versionLinkLabel.Size = new System.Drawing.Size(79, 13);
            this.versionLinkLabel.TabIndex = 4;
            this.versionLinkLabel.TabStop = true;
            this.versionLinkLabel.Text = "X.XX.X.XXXXX";
            this.versionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Version_LinkClicked);
            // 
            // authorAndProgramInfoGroupBox
            // 
            this.authorAndProgramInfoGroupBox.Controls.Add(this.maintainerLinkLabel);
            this.authorAndProgramInfoGroupBox.Controls.Add(this.maintainedByLabel);
            this.authorAndProgramInfoGroupBox.Controls.Add(this.creatorLinkLabel);
            this.authorAndProgramInfoGroupBox.Controls.Add(this.versionLinkLabel);
            this.authorAndProgramInfoGroupBox.Controls.Add(this.createdByLabel);
            this.authorAndProgramInfoGroupBox.Controls.Add(this.versionLabel);
            this.authorAndProgramInfoGroupBox.Location = new System.Drawing.Point(8, 57);
            this.authorAndProgramInfoGroupBox.Name = "authorAndProgramInfoGroupBox";
            this.authorAndProgramInfoGroupBox.Size = new System.Drawing.Size(259, 97);
            this.authorAndProgramInfoGroupBox.TabIndex = 5;
            this.authorAndProgramInfoGroupBox.TabStop = false;
            this.authorAndProgramInfoGroupBox.Text = "Author and Program Info";
            // 
            // maintainerLinkLabel
            // 
            this.maintainerLinkLabel.AutoSize = true;
            this.maintainerLinkLabel.Location = new System.Drawing.Point(148, 21);
            this.maintainerLinkLabel.Name = "maintainerLinkLabel";
            this.maintainerLinkLabel.Size = new System.Drawing.Size(72, 13);
            this.maintainerLinkLabel.TabIndex = 6;
            this.maintainerLinkLabel.TabStop = true;
            this.maintainerLinkLabel.Text = "Antoine Aflalo";
            this.maintainerLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.maintainerLinkLabel_LinkClicked);
            // 
            // maintainedByLabel
            // 
            this.maintainedByLabel.AutoSize = true;
            this.maintainedByLabel.Location = new System.Drawing.Point(6, 21);
            this.maintainedByLabel.Name = "maintainedByLabel";
            this.maintainedByLabel.Size = new System.Drawing.Size(73, 13);
            this.maintainedByLabel.TabIndex = 5;
            this.maintainedByLabel.Text = "Maintained by";
            // 
            // creditsAndAttributionGroupBox
            // 
            this.creditsAndAttributionGroupBox.Controls.Add(this.logoCreatorLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.logoMadeLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.defaultPlaybackDeviceChangeLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.defaultPlaybackDeviceChangeLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.keyboardHotKeySystemLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.keyboardHotKeySystemLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.iconsLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.iconsLabel);
            this.creditsAndAttributionGroupBox.Location = new System.Drawing.Point(8, 169);
            this.creditsAndAttributionGroupBox.Name = "creditsAndAttributionGroupBox";
            this.creditsAndAttributionGroupBox.Size = new System.Drawing.Size(259, 146);
            this.creditsAndAttributionGroupBox.TabIndex = 6;
            this.creditsAndAttributionGroupBox.TabStop = false;
            this.creditsAndAttributionGroupBox.Text = "Credits and Attribution";
            // 
            // defaultPlaybackDeviceChangeLinkLabel
            // 
            this.defaultPlaybackDeviceChangeLinkLabel.AutoSize = true;
            this.defaultPlaybackDeviceChangeLinkLabel.Location = new System.Drawing.Point(148, 71);
            this.defaultPlaybackDeviceChangeLinkLabel.Name = "defaultPlaybackDeviceChangeLinkLabel";
            this.defaultPlaybackDeviceChangeLinkLabel.Size = new System.Drawing.Size(39, 13);
            this.defaultPlaybackDeviceChangeLinkLabel.TabIndex = 7;
            this.defaultPlaybackDeviceChangeLinkLabel.TabStop = true;
            this.defaultPlaybackDeviceChangeLinkLabel.Text = "EreTIk";
            this.defaultPlaybackDeviceChangeLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.eretikLabel_LinkClicked);
            // 
            // defaultPlaybackDeviceChangeLabel
            // 
            this.defaultPlaybackDeviceChangeLabel.Location = new System.Drawing.Point(6, 71);
            this.defaultPlaybackDeviceChangeLabel.Name = "defaultPlaybackDeviceChangeLabel";
            this.defaultPlaybackDeviceChangeLabel.Size = new System.Drawing.Size(137, 28);
            this.defaultPlaybackDeviceChangeLabel.TabIndex = 4;
            this.defaultPlaybackDeviceChangeLabel.Text = "Default Playback device change";
            // 
            // keyboardHotKeySystemLinkLabel
            // 
            this.keyboardHotKeySystemLinkLabel.AutoSize = true;
            this.keyboardHotKeySystemLinkLabel.Location = new System.Drawing.Point(148, 46);
            this.keyboardHotKeySystemLinkLabel.Name = "keyboardHotKeySystemLinkLabel";
            this.keyboardHotKeySystemLinkLabel.Size = new System.Drawing.Size(105, 13);
            this.keyboardHotKeySystemLinkLabel.TabIndex = 3;
            this.keyboardHotKeySystemLinkLabel.TabStop = true;
            this.keyboardHotKeySystemLinkLabel.Text = "Christian Liensberger";
            this.keyboardHotKeySystemLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // keyboardHotKeySystemLabel
            // 
            this.keyboardHotKeySystemLabel.AutoSize = true;
            this.keyboardHotKeySystemLabel.Location = new System.Drawing.Point(6, 46);
            this.keyboardHotKeySystemLabel.Name = "keyboardHotKeySystemLabel";
            this.keyboardHotKeySystemLabel.Size = new System.Drawing.Size(125, 13);
            this.keyboardHotKeySystemLabel.TabIndex = 2;
            this.keyboardHotKeySystemLabel.Text = "Keyboard HotKey system";
            // 
            // iconsLinkLabel
            // 
            this.iconsLinkLabel.AutoSize = true;
            this.iconsLinkLabel.Location = new System.Drawing.Point(148, 23);
            this.iconsLinkLabel.Name = "iconsLinkLabel";
            this.iconsLinkLabel.Size = new System.Drawing.Size(61, 13);
            this.iconsLinkLabel.TabIndex = 1;
            this.iconsLinkLabel.TabStop = true;
            this.iconsLinkLabel.Text = "Pastel SVG";
            this.iconsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // iconsLabel
            // 
            this.iconsLabel.AutoSize = true;
            this.iconsLabel.Location = new System.Drawing.Point(6, 23);
            this.iconsLabel.Name = "iconsLabel";
            this.iconsLabel.Size = new System.Drawing.Size(33, 13);
            this.iconsLabel.TabIndex = 0;
            this.iconsLabel.Text = "Icons";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SoundSwitch.Properties.Resources.SoundSwitch48;
            this.pictureBox1.Location = new System.Drawing.Point(21, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(51, 49);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // logoCreatorLabel
            // 
            this.logoCreatorLabel.AutoSize = true;
            this.logoCreatorLabel.Location = new System.Drawing.Point(148, 105);
            this.logoCreatorLabel.Name = "logoCreatorLabel";
            this.logoCreatorLabel.Size = new System.Drawing.Size(74, 13);
            this.logoCreatorLabel.TabIndex = 9;
            this.logoCreatorLabel.TabStop = true;
            this.logoCreatorLabel.Text = "@linadesteem";
            this.logoCreatorLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.logoCreatorLabel_LinkClicked);
            // 
            // logoMadeLabel
            // 
            this.logoMadeLabel.Location = new System.Drawing.Point(6, 105);
            this.logoMadeLabel.Name = "logoMadeLabel";
            this.logoMadeLabel.Size = new System.Drawing.Size(137, 28);
            this.logoMadeLabel.TabIndex = 8;
            this.logoMadeLabel.Text = "Logo made by";
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(279, 320);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.creditsAndAttributionGroupBox);
            this.Controls.Add(this.authorAndProgramInfoGroupBox);
            this.Controls.Add(this.appNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.Text = "About SoundSwitch";
            this.Load += new System.EventHandler(this.About_Load);
            this.authorAndProgramInfoGroupBox.ResumeLayout(false);
            this.authorAndProgramInfoGroupBox.PerformLayout();
            this.creditsAndAttributionGroupBox.ResumeLayout(false);
            this.creditsAndAttributionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.Label createdByLabel;
        private System.Windows.Forms.LinkLabel creatorLinkLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.LinkLabel versionLinkLabel;
        private System.Windows.Forms.GroupBox authorAndProgramInfoGroupBox;
        private System.Windows.Forms.GroupBox creditsAndAttributionGroupBox;
        private System.Windows.Forms.LinkLabel iconsLinkLabel;
        private System.Windows.Forms.Label iconsLabel;
        private System.Windows.Forms.LinkLabel keyboardHotKeySystemLinkLabel;
        private System.Windows.Forms.Label keyboardHotKeySystemLabel;
        private System.Windows.Forms.Label defaultPlaybackDeviceChangeLabel;
        private System.Windows.Forms.LinkLabel maintainerLinkLabel;
        private System.Windows.Forms.Label maintainedByLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel defaultPlaybackDeviceChangeLinkLabel;
        private System.Windows.Forms.LinkLabel logoCreatorLabel;
        private System.Windows.Forms.Label logoMadeLabel;
    }
}