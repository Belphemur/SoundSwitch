using SoundSwitch.UI.UserControls;

namespace SoundSwitch.UI.Forms
{
    sealed partial class UpdateDownloadForm
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
            this.changeLogGroup = new System.Windows.Forms.GroupBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.installButton = new System.Windows.Forms.Button();
            this.downloadProgress = new SoundSwitch.UI.UserControls.TextProgressBar();
            this.changeLog = new SoundSwitch.UI.UserControls.ChangelogWebViewer();
            this.changeLogGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // changeLogGroup
            // 
            this.changeLogGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.changeLogGroup.Controls.Add(this.changeLog);
            this.changeLogGroup.Location = new System.Drawing.Point(8, 35);
            this.changeLogGroup.Margin = new System.Windows.Forms.Padding(2);
            this.changeLogGroup.Name = "changeLogGroup";
            this.changeLogGroup.Padding = new System.Windows.Forms.Padding(2);
            this.changeLogGroup.Size = new System.Drawing.Size(698, 349);
            this.changeLogGroup.TabIndex = 0;
            this.changeLogGroup.TabStop = false;
            this.changeLogGroup.Text = "Changelog";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(628, 392);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // installButton
            // 
            this.installButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.installButton.Enabled = false;
            this.installButton.Location = new System.Drawing.Point(11, 392);
            this.installButton.Margin = new System.Windows.Forms.Padding(2);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(75, 23);
            this.installButton.TabIndex = 3;
            this.installButton.Text = "Install";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // downloadProgress
            // 
            this.downloadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadProgress.CustomText = null;
            this.downloadProgress.DisplayStyle = SoundSwitch.UI.UserControls.TextProgressBar.ProgressBarDisplayText.Percentage;
            this.downloadProgress.Location = new System.Drawing.Point(8, 11);
            this.downloadProgress.Margin = new System.Windows.Forms.Padding(2);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(698, 20);
            this.downloadProgress.TabIndex = 1;
            // 
            // changeLog
            // 
            this.changeLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changeLog.IsWebBrowserContextMenuEnabled = false;
            this.changeLog.Location = new System.Drawing.Point(2, 15);
            this.changeLog.Margin = new System.Windows.Forms.Padding(2);
            this.changeLog.MinimumSize = new System.Drawing.Size(13, 13);
            this.changeLog.Name = "changeLog";
            this.changeLog.Size = new System.Drawing.Size(694, 332);
            this.changeLog.TabIndex = 0;
            this.changeLog.WebBrowserShortcutsEnabled = false;
            this.changeLog.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.changeLog_Navigating);
            // 
            // UpdateDownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(714, 426);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.changeLogGroup);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "UpdateDownloadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdateDownloadForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateDownloadForm_FormClosing);
            this.changeLogGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox changeLogGroup;
        private ChangelogWebViewer changeLog;
        private TextProgressBar downloadProgress;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button installButton;
    }
}