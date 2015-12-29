using SoundSwitch.Properties;

namespace SoundSwitch.UI.Forms
{
    partial class Settings
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
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup(global::SoundSwitch.Properties.SettingsString.selected, System.Windows.Forms.HorizontalAlignment.Center);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup(global::SoundSwitch.Properties.SettingsString.selected, System.Windows.Forms.HorizontalAlignment.Center);
            this.RunAtStartup = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.communicationCheckbox = new System.Windows.Forms.CheckBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.playbackPage = new System.Windows.Forms.TabPage();
            this.playbackListView = new System.Windows.Forms.ListView();
            this.recordingPage = new System.Windows.Forms.TabPage();
            this.recordingListView = new System.Windows.Forms.ListView();
            this.hotkeyTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.notificationComboBox = new System.Windows.Forms.ComboBox();
            this.notifLabel = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.playbackPage.SuspendLayout();
            this.recordingPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // RunAtStartup
            // 
            this.RunAtStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RunAtStartup.AutoSize = true;
            this.RunAtStartup.Location = new System.Drawing.Point(12, 265);
            this.RunAtStartup.Name = "RunAtStartup";
            this.RunAtStartup.Size = new System.Drawing.Size(95, 17);
            this.RunAtStartup.TabIndex = 7;
            this.RunAtStartup.Text = global::SoundSwitch.Properties.SettingsString.runStartup;
            this.RunAtStartup.UseVisualStyleBackColor = true;
            this.RunAtStartup.CheckedChanged += new System.EventHandler(this.RunAtStartup_CheckedChanged);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(428, 251);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 11;
            this.closeButton.Text = global::SoundSwitch.Properties.SettingsString.close;
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // communicationCheckbox
            // 
            this.communicationCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.communicationCheckbox.AutoSize = true;
            this.communicationCheckbox.Location = new System.Drawing.Point(113, 265);
            this.communicationCheckbox.Name = "communicationCheckbox";
            this.communicationCheckbox.Size = new System.Drawing.Size(140, 17);
            this.communicationCheckbox.TabIndex = 12;
            this.communicationCheckbox.Text = global::SoundSwitch.Properties.SettingsString.setComm;
            this.communicationCheckbox.UseVisualStyleBackColor = true;
            this.communicationCheckbox.CheckedChanged += new System.EventHandler(this.communicationCheckbox_CheckedChanged);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.playbackPage);
            this.tabControl.Controls.Add(this.recordingPage);
            this.tabControl.Location = new System.Drawing.Point(12, 6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(491, 218);
            this.tabControl.TabIndex = 13;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // playbackPage
            // 
            this.playbackPage.Controls.Add(this.playbackListView);
            this.playbackPage.Location = new System.Drawing.Point(4, 22);
            this.playbackPage.Name = "playbackPage";
            this.playbackPage.Padding = new System.Windows.Forms.Padding(3);
            this.playbackPage.Size = new System.Drawing.Size(483, 192);
            this.playbackPage.TabIndex = 0;
            this.playbackPage.Text = global::SoundSwitch.Properties.SettingsString.playback;
            this.playbackPage.UseVisualStyleBackColor = true;
            // 
            // playbackListView
            // 
            this.playbackListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackListView.CheckBoxes = true;
            listViewGroup3.Header = global::SoundSwitch.Properties.SettingsString.selected;
            listViewGroup3.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup3.Name = "selectedGroup";
            this.playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3});
            this.playbackListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.playbackListView.Location = new System.Drawing.Point(-4, 0);
            this.playbackListView.Name = "playbackListView";
            this.playbackListView.Size = new System.Drawing.Size(493, 194);
            this.playbackListView.TabIndex = 14;
            this.playbackListView.UseCompatibleStateImageBehavior = false;
            this.playbackListView.View = System.Windows.Forms.View.Details;
            // 
            // recordingPage
            // 
            this.recordingPage.Controls.Add(this.recordingListView);
            this.recordingPage.Location = new System.Drawing.Point(4, 22);
            this.recordingPage.Name = "recordingPage";
            this.recordingPage.Padding = new System.Windows.Forms.Padding(3);
            this.recordingPage.Size = new System.Drawing.Size(483, 192);
            this.recordingPage.TabIndex = 1;
            this.recordingPage.Text = global::SoundSwitch.Properties.SettingsString.recording;
            this.recordingPage.UseVisualStyleBackColor = true;
            // 
            // recordingListView
            // 
            this.recordingListView.AccessibleName = "recordingListView";
            this.recordingListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingListView.CheckBoxes = true;
            listViewGroup4.Header = global::SoundSwitch.Properties.SettingsString.selected;
            listViewGroup4.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup4.Name = "selectedGroup";
            this.recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4});
            this.recordingListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.recordingListView.Location = new System.Drawing.Point(-2, 0);
            this.recordingListView.Name = "recordingListView";
            this.recordingListView.Size = new System.Drawing.Size(477, 192);
            this.recordingListView.TabIndex = 17;
            this.recordingListView.UseCompatibleStateImageBehavior = false;
            this.recordingListView.View = System.Windows.Forms.View.Details;
            // 
            // hotkeyTextBox
            // 
            this.hotkeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeyTextBox.Location = new System.Drawing.Point(65, 230);
            this.hotkeyTextBox.Name = "hotkeyTextBox";
            this.hotkeyTextBox.Size = new System.Drawing.Size(132, 20);
            this.hotkeyTextBox.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 233);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Hotkeys";
            // 
            // notificationComboBox
            // 
            this.notificationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.notificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationComboBox.FormattingEnabled = true;
            this.notificationComboBox.Location = new System.Drawing.Point(301, 230);
            this.notificationComboBox.Name = "notificationComboBox";
            this.notificationComboBox.Size = new System.Drawing.Size(121, 21);
            this.notificationComboBox.TabIndex = 16;
            this.notificationComboBox.SelectedValueChanged += new System.EventHandler(this.notificationComboBox_SelectedValueChanged);
            // 
            // notifLabel
            // 
            this.notifLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.notifLabel.AutoSize = true;
            this.notifLabel.Location = new System.Drawing.Point(235, 233);
            this.notifLabel.Name = "notifLabel";
            this.notifLabel.Size = new System.Drawing.Size(60, 13);
            this.notifLabel.TabIndex = 17;
            this.notifLabel.Text = "Notification";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(515, 286);
            this.Controls.Add(this.notifLabel);
            this.Controls.Add(this.notificationComboBox);
            this.Controls.Add(this.hotkeyTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.communicationCheckbox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.RunAtStartup);
            this.MinimumSize = new System.Drawing.Size(500, 325);
            this.Name = "Settings";
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.playbackPage.ResumeLayout(false);
            this.recordingPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox RunAtStartup;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox communicationCheckbox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage playbackPage;
        private System.Windows.Forms.ListView playbackListView;
        private System.Windows.Forms.TabPage recordingPage;
        private System.Windows.Forms.ListView recordingListView;
        private System.Windows.Forms.TextBox hotkeyTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox notificationComboBox;
        private System.Windows.Forms.Label notifLabel;
    }
}