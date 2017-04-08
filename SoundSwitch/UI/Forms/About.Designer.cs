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
            this.authorAndProgramInfoGroupBox.SuspendLayout();
            this.creditsAndAttributionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // appNameLabel
            // 
            this.appNameLabel.AutoSize = true;
            this.appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appNameLabel.Location = new System.Drawing.Point(108, 19);
            this.appNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.appNameLabel.Name = "appNameLabel";
            this.appNameLabel.Size = new System.Drawing.Size(210, 47);
            this.appNameLabel.TabIndex = 0;
            this.appNameLabel.Text = "AppName";
            // 
            // createdByLabel
            // 
            this.createdByLabel.AutoSize = true;
            this.createdByLabel.Location = new System.Drawing.Point(9, 71);
            this.createdByLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.createdByLabel.Name = "createdByLabel";
            this.createdByLabel.Size = new System.Drawing.Size(86, 20);
            this.createdByLabel.TabIndex = 1;
            this.createdByLabel.Text = "Created by";
            // 
            // creatorLinkLabel
            // 
            this.creatorLinkLabel.AutoSize = true;
            this.creatorLinkLabel.Location = new System.Drawing.Point(222, 71);
            this.creatorLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.creatorLinkLabel.Name = "creatorLinkLabel";
            this.creatorLinkLabel.Size = new System.Drawing.Size(122, 20);
            this.creatorLinkLabel.TabIndex = 2;
            this.creatorLinkLabel.TabStop = true;
            this.creatorLinkLabel.Text = "Jeroen Pelgrims";
            this.creatorLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(9, 106);
            this.versionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(63, 20);
            this.versionLabel.TabIndex = 3;
            this.versionLabel.Text = "Version";
            // 
            // versionLinkLabel
            // 
            this.versionLinkLabel.AutoSize = true;
            this.versionLinkLabel.Location = new System.Drawing.Point(222, 106);
            this.versionLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.versionLinkLabel.Name = "versionLinkLabel";
            this.versionLinkLabel.Size = new System.Drawing.Size(120, 20);
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
            this.authorAndProgramInfoGroupBox.Location = new System.Drawing.Point(12, 85);
            this.authorAndProgramInfoGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.authorAndProgramInfoGroupBox.Name = "authorAndProgramInfoGroupBox";
            this.authorAndProgramInfoGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.authorAndProgramInfoGroupBox.Size = new System.Drawing.Size(388, 145);
            this.authorAndProgramInfoGroupBox.TabIndex = 5;
            this.authorAndProgramInfoGroupBox.TabStop = false;
            this.authorAndProgramInfoGroupBox.Text = "Author and Program Info";
            // 
            // maintainerLinkLabel
            // 
            this.maintainerLinkLabel.AutoSize = true;
            this.maintainerLinkLabel.Location = new System.Drawing.Point(222, 32);
            this.maintainerLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.maintainerLinkLabel.Name = "maintainerLinkLabel";
            this.maintainerLinkLabel.Size = new System.Drawing.Size(108, 20);
            this.maintainerLinkLabel.TabIndex = 6;
            this.maintainerLinkLabel.TabStop = true;
            this.maintainerLinkLabel.Text = "Antoine Aflalo";
            this.maintainerLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.maintainerLinkLabel_LinkClicked);
            // 
            // maintainedByLabel
            // 
            this.maintainedByLabel.AutoSize = true;
            this.maintainedByLabel.Location = new System.Drawing.Point(9, 32);
            this.maintainedByLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.maintainedByLabel.Name = "maintainedByLabel";
            this.maintainedByLabel.Size = new System.Drawing.Size(107, 20);
            this.maintainedByLabel.TabIndex = 5;
            this.maintainedByLabel.Text = "Maintained by";
            // 
            // creditsAndAttributionGroupBox
            // 
            this.creditsAndAttributionGroupBox.Controls.Add(this.defaultPlaybackDeviceChangeLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.defaultPlaybackDeviceChangeLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.keyboardHotKeySystemLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.keyboardHotKeySystemLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.iconsLinkLabel);
            this.creditsAndAttributionGroupBox.Controls.Add(this.iconsLabel);
            this.creditsAndAttributionGroupBox.Location = new System.Drawing.Point(12, 254);
            this.creditsAndAttributionGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.creditsAndAttributionGroupBox.Name = "creditsAndAttributionGroupBox";
            this.creditsAndAttributionGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.creditsAndAttributionGroupBox.Size = new System.Drawing.Size(388, 161);
            this.creditsAndAttributionGroupBox.TabIndex = 6;
            this.creditsAndAttributionGroupBox.TabStop = false;
            this.creditsAndAttributionGroupBox.Text = "Credits and Attribution";
            // 
            // defaultPlaybackDeviceChangeLinkLabel
            // 
            this.defaultPlaybackDeviceChangeLinkLabel.AutoSize = true;
            this.defaultPlaybackDeviceChangeLinkLabel.Location = new System.Drawing.Point(222, 106);
            this.defaultPlaybackDeviceChangeLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.defaultPlaybackDeviceChangeLinkLabel.Name = "defaultPlaybackDeviceChangeLinkLabel";
            this.defaultPlaybackDeviceChangeLinkLabel.Size = new System.Drawing.Size(56, 20);
            this.defaultPlaybackDeviceChangeLinkLabel.TabIndex = 7;
            this.defaultPlaybackDeviceChangeLinkLabel.TabStop = true;
            this.defaultPlaybackDeviceChangeLinkLabel.Text = "EreTIk";
            this.defaultPlaybackDeviceChangeLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.eretikLabel_LinkClicked);
            // 
            // defaultPlaybackDeviceChangeLabel
            // 
            this.defaultPlaybackDeviceChangeLabel.Location = new System.Drawing.Point(9, 106);
            this.defaultPlaybackDeviceChangeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.defaultPlaybackDeviceChangeLabel.Name = "defaultPlaybackDeviceChangeLabel";
            this.defaultPlaybackDeviceChangeLabel.Size = new System.Drawing.Size(205, 42);
            this.defaultPlaybackDeviceChangeLabel.TabIndex = 4;
            this.defaultPlaybackDeviceChangeLabel.Text = "Default Playback device change";
            // 
            // keyboardHotKeySystemLinkLabel
            // 
            this.keyboardHotKeySystemLinkLabel.AutoSize = true;
            this.keyboardHotKeySystemLinkLabel.Location = new System.Drawing.Point(222, 69);
            this.keyboardHotKeySystemLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.keyboardHotKeySystemLinkLabel.Name = "keyboardHotKeySystemLinkLabel";
            this.keyboardHotKeySystemLinkLabel.Size = new System.Drawing.Size(159, 20);
            this.keyboardHotKeySystemLinkLabel.TabIndex = 3;
            this.keyboardHotKeySystemLinkLabel.TabStop = true;
            this.keyboardHotKeySystemLinkLabel.Text = "Christian Liensberger";
            this.keyboardHotKeySystemLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // keyboardHotKeySystemLabel
            // 
            this.keyboardHotKeySystemLabel.AutoSize = true;
            this.keyboardHotKeySystemLabel.Location = new System.Drawing.Point(9, 69);
            this.keyboardHotKeySystemLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.keyboardHotKeySystemLabel.Name = "keyboardHotKeySystemLabel";
            this.keyboardHotKeySystemLabel.Size = new System.Drawing.Size(186, 20);
            this.keyboardHotKeySystemLabel.TabIndex = 2;
            this.keyboardHotKeySystemLabel.Text = "Keyboard HotKey system";
            // 
            // iconsLinkLabel
            // 
            this.iconsLinkLabel.AutoSize = true;
            this.iconsLinkLabel.Location = new System.Drawing.Point(222, 35);
            this.iconsLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.iconsLinkLabel.Name = "iconsLinkLabel";
            this.iconsLinkLabel.Size = new System.Drawing.Size(92, 20);
            this.iconsLinkLabel.TabIndex = 1;
            this.iconsLinkLabel.TabStop = true;
            this.iconsLinkLabel.Text = "Pastel SVG";
            this.iconsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // iconsLabel
            // 
            this.iconsLabel.AutoSize = true;
            this.iconsLabel.Location = new System.Drawing.Point(9, 35);
            this.iconsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.iconsLabel.Name = "iconsLabel";
            this.iconsLabel.Size = new System.Drawing.Size(48, 20);
            this.iconsLabel.TabIndex = 0;
            this.iconsLabel.Text = "Icons";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SoundSwitch.Properties.Resources.SoundSwitch48;
            this.pictureBox1.Location = new System.Drawing.Point(32, 3);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(68, 77);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(418, 434);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.creditsAndAttributionGroupBox);
            this.Controls.Add(this.authorAndProgramInfoGroupBox);
            this.Controls.Add(this.appNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
    }
}