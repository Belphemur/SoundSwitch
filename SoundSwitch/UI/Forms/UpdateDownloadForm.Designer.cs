using SoundSwitch.UI.Controls;

namespace SoundSwitch.UI.Forms
{
    partial class UpdateDownloadForm
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
            this.downloadProgress = new SoundSwitch.UI.Controls.TextProgressBar();
            this.changeLog = new SoundSwitch.UI.Controls.ChangelogWebViewer();
            this.changeLogGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // changeLogGroup
            // 
            this.changeLogGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.changeLogGroup.Controls.Add(this.changeLog);
            this.changeLogGroup.Location = new System.Drawing.Point(12, 46);
            this.changeLogGroup.Name = "changeLogGroup";
            this.changeLogGroup.Size = new System.Drawing.Size(540, 229);
            this.changeLogGroup.TabIndex = 0;
            this.changeLogGroup.TabStop = false;
            this.changeLogGroup.Text = "Changelog";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(473, 304);
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
            this.installButton.Location = new System.Drawing.Point(12, 304);
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
            this.downloadProgress.DisplayStyle = SoundSwitch.UI.Controls.TextProgressBar.ProgressBarDisplayText.Percentage;
            this.downloadProgress.Location = new System.Drawing.Point(12, 17);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(540, 23);
            this.downloadProgress.TabIndex = 1;
            // 
            // changeLog
            // 
            this.changeLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changeLog.IsWebBrowserContextMenuEnabled = false;
            this.changeLog.Location = new System.Drawing.Point(3, 16);
            this.changeLog.MinimumSize = new System.Drawing.Size(20, 20);
            this.changeLog.Name = "changeLog";
            this.changeLog.Size = new System.Drawing.Size(534, 210);
            this.changeLog.TabIndex = 0;
            this.changeLog.WebBrowserShortcutsEnabled = false;
            // 
            // UpdateDownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(564, 339);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.changeLogGroup);
            this.Name = "UpdateDownloadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdateDownloadForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateDownloadForm_FormClosing);
            this.changeLogGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox changeLogGroup;
        private SoundSwitch.UI.Controls.ChangelogWebViewer changeLog;
        private SoundSwitch.UI.Controls.TextProgressBar downloadProgress;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button installButton;
    }
}