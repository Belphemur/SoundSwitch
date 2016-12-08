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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup(global::SoundSwitch.Properties.SettingsString.selected, System.Windows.Forms.HorizontalAlignment.Center);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup(global::SoundSwitch.Properties.SettingsString.selected, System.Windows.Forms.HorizontalAlignment.Center);
            this.RunAtStartup = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.communicationCheckbox = new System.Windows.Forms.CheckBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.playbackPage = new System.Windows.Forms.TabPage();
            this.playbackListView = new System.Windows.Forms.ListView();
            this.recordingPage = new System.Windows.Forms.TabPage();
            this.recordingListView = new System.Windows.Forms.ListView();
            this.appSettingTabPage = new System.Windows.Forms.TabPage();
            this.updateSettingsGroup = new System.Windows.Forms.GroupBox();
            this.stealthUpdateCheckbox = new System.Windows.Forms.CheckBox();
            this.betaVersionCheckbox = new System.Windows.Forms.CheckBox();
            this.audioSettingsGroup = new System.Windows.Forms.GroupBox();
            this.cyclerLabel = new System.Windows.Forms.Label();
            this.cyclerComboBox = new System.Windows.Forms.ComboBox();
            this.tooltipLabel = new System.Windows.Forms.Label();
            this.tooltipInfoComboBox = new System.Windows.Forms.ComboBox();
            this.selectSoundButton = new System.Windows.Forms.Button();
            this.notifLabel = new System.Windows.Forms.Label();
            this.notificationComboBox = new System.Windows.Forms.ComboBox();
            this.basicSettingsGroup = new System.Windows.Forms.GroupBox();
            this.hotkeyTextBox = new System.Windows.Forms.TextBox();
            this.hotkeysLabel = new System.Windows.Forms.Label();
            this.selectSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.hotkeysCheckbox = new System.Windows.Forms.CheckBox();
            this.checkboxSystrayIcon = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.playbackPage.SuspendLayout();
            this.recordingPage.SuspendLayout();
            this.appSettingTabPage.SuspendLayout();
            this.updateSettingsGroup.SuspendLayout();
            this.audioSettingsGroup.SuspendLayout();
            this.basicSettingsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // RunAtStartup
            // 
            this.RunAtStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RunAtStartup.AutoSize = true;
            this.RunAtStartup.Location = new System.Drawing.Point(6, 19);
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
            this.closeButton.Location = new System.Drawing.Point(478, 294);
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
            this.communicationCheckbox.Location = new System.Drawing.Point(6, 38);
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
            this.tabControl.Controls.Add(this.appSettingTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(541, 279);
            this.tabControl.TabIndex = 13;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // playbackPage
            // 
            this.playbackPage.Controls.Add(this.playbackListView);
            this.playbackPage.Location = new System.Drawing.Point(4, 22);
            this.playbackPage.Name = "playbackPage";
            this.playbackPage.Padding = new System.Windows.Forms.Padding(3);
            this.playbackPage.Size = new System.Drawing.Size(533, 253);
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
            listViewGroup1.Header = global::SoundSwitch.Properties.SettingsString.selected;
            listViewGroup1.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup1.Name = "selectedGroup";
            this.playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.playbackListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.playbackListView.Location = new System.Drawing.Point(-4, 0);
            this.playbackListView.Name = "playbackListView";
            this.playbackListView.Size = new System.Drawing.Size(585, 232);
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
            this.recordingPage.Size = new System.Drawing.Size(533, 253);
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
            listViewGroup2.Header = global::SoundSwitch.Properties.SettingsString.selected;
            listViewGroup2.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup2.Name = "selectedGroup";
            this.recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup2});
            this.recordingListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.recordingListView.Location = new System.Drawing.Point(-2, 0);
            this.recordingListView.Name = "recordingListView";
            this.recordingListView.Size = new System.Drawing.Size(583, 233);
            this.recordingListView.TabIndex = 17;
            this.recordingListView.UseCompatibleStateImageBehavior = false;
            this.recordingListView.View = System.Windows.Forms.View.Details;
            // 
            // appSettingTabPage
            // 
            this.appSettingTabPage.Controls.Add(this.updateSettingsGroup);
            this.appSettingTabPage.Controls.Add(this.audioSettingsGroup);
            this.appSettingTabPage.Controls.Add(this.basicSettingsGroup);
            this.appSettingTabPage.Location = new System.Drawing.Point(4, 22);
            this.appSettingTabPage.Name = "appSettingTabPage";
            this.appSettingTabPage.Size = new System.Drawing.Size(533, 253);
            this.appSettingTabPage.TabIndex = 2;
            this.appSettingTabPage.Text = global::SoundSwitch.Properties.SettingsString.settings;
            this.appSettingTabPage.UseVisualStyleBackColor = true;
            // 
            // updateSettingsGroup
            // 
            this.updateSettingsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.updateSettingsGroup.Controls.Add(this.stealthUpdateCheckbox);
            this.updateSettingsGroup.Controls.Add(this.betaVersionCheckbox);
            this.updateSettingsGroup.Location = new System.Drawing.Point(321, 3);
            this.updateSettingsGroup.Name = "updateSettingsGroup";
            this.updateSettingsGroup.Size = new System.Drawing.Size(209, 95);
            this.updateSettingsGroup.TabIndex = 14;
            this.updateSettingsGroup.TabStop = false;
            this.updateSettingsGroup.Text = "Update Settings";
            // 
            // stealthUpdateCheckbox
            // 
            this.stealthUpdateCheckbox.AutoSize = true;
            this.stealthUpdateCheckbox.Location = new System.Drawing.Point(6, 23);
            this.stealthUpdateCheckbox.Name = "stealthUpdateCheckbox";
            this.stealthUpdateCheckbox.Size = new System.Drawing.Size(111, 17);
            this.stealthUpdateCheckbox.TabIndex = 19;
            this.stealthUpdateCheckbox.Text = global::SoundSwitch.Properties.SettingsString.stealthUpdate;
            this.stealthUpdateCheckbox.UseVisualStyleBackColor = true;
            this.stealthUpdateCheckbox.CheckedChanged += new System.EventHandler(this.stealthUpdateCheckbox_CheckedChanged);
            // 
            // betaVersionCheckbox
            // 
            this.betaVersionCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.betaVersionCheckbox.AutoSize = true;
            this.betaVersionCheckbox.Location = new System.Drawing.Point(6, 52);
            this.betaVersionCheckbox.Name = "betaVersionCheckbox";
            this.betaVersionCheckbox.Size = new System.Drawing.Size(91, 17);
            this.betaVersionCheckbox.TabIndex = 18;
            this.betaVersionCheckbox.Text = global::SoundSwitch.Properties.SettingsString.beta;
            this.betaVersionCheckbox.UseVisualStyleBackColor = true;
            this.betaVersionCheckbox.CheckedChanged += new System.EventHandler(this.betaVersionCheckbox_CheckedChanged);
            // 
            // audioSettingsGroup
            // 
            this.audioSettingsGroup.Controls.Add(this.cyclerLabel);
            this.audioSettingsGroup.Controls.Add(this.cyclerComboBox);
            this.audioSettingsGroup.Controls.Add(this.tooltipLabel);
            this.audioSettingsGroup.Controls.Add(this.tooltipInfoComboBox);
            this.audioSettingsGroup.Controls.Add(this.communicationCheckbox);
            this.audioSettingsGroup.Controls.Add(this.selectSoundButton);
            this.audioSettingsGroup.Controls.Add(this.notifLabel);
            this.audioSettingsGroup.Controls.Add(this.notificationComboBox);
            this.audioSettingsGroup.Location = new System.Drawing.Point(3, 71);
            this.audioSettingsGroup.Name = "audioSettingsGroup";
            this.audioSettingsGroup.Size = new System.Drawing.Size(312, 179);
            this.audioSettingsGroup.TabIndex = 13;
            this.audioSettingsGroup.TabStop = false;
            this.audioSettingsGroup.Text = global::SoundSwitch.Properties.SettingsString.audioSettings;
            // 
            // cyclerLabel
            // 
            this.cyclerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cyclerLabel.AutoSize = true;
            this.cyclerLabel.Location = new System.Drawing.Point(6, 154);
            this.cyclerLabel.Name = "cyclerLabel";
            this.cyclerLabel.Size = new System.Drawing.Size(72, 13);
            this.cyclerLabel.TabIndex = 23;
            this.cyclerLabel.Text = "Cycle through";
            // 
            // cyclerComboBox
            // 
            this.cyclerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cyclerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cyclerComboBox.FormattingEnabled = true;
            this.cyclerComboBox.Location = new System.Drawing.Point(98, 150);
            this.cyclerComboBox.Name = "cyclerComboBox";
            this.cyclerComboBox.Size = new System.Drawing.Size(177, 21);
            this.cyclerComboBox.TabIndex = 22;
            this.cyclerComboBox.SelectedValueChanged += new System.EventHandler(this.cyclerComboBox_SelectedValueChanged);
            // 
            // tooltipLabel
            // 
            this.tooltipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tooltipLabel.AutoSize = true;
            this.tooltipLabel.Location = new System.Drawing.Point(6, 114);
            this.tooltipLabel.Name = "tooltipLabel";
            this.tooltipLabel.Size = new System.Drawing.Size(86, 13);
            this.tooltipLabel.TabIndex = 21;
            this.tooltipLabel.Text = "Tooltip on Hover";
            // 
            // tooltipInfoComboBox
            // 
            this.tooltipInfoComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tooltipInfoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tooltipInfoComboBox.FormattingEnabled = true;
            this.tooltipInfoComboBox.Location = new System.Drawing.Point(98, 110);
            this.tooltipInfoComboBox.Name = "tooltipInfoComboBox";
            this.tooltipInfoComboBox.Size = new System.Drawing.Size(177, 21);
            this.tooltipInfoComboBox.TabIndex = 20;
            this.tooltipInfoComboBox.SelectedValueChanged += new System.EventHandler(this.tooltipInfoComboBox_SelectedValueChanged);
            // 
            // selectSoundButton
            // 
            this.selectSoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectSoundButton.Location = new System.Drawing.Point(281, 70);
            this.selectSoundButton.Name = "selectSoundButton";
            this.selectSoundButton.Size = new System.Drawing.Size(24, 23);
            this.selectSoundButton.TabIndex = 19;
            this.selectSoundButton.Text = "...";
            this.selectSoundButton.UseVisualStyleBackColor = true;
            this.selectSoundButton.Visible = false;
            this.selectSoundButton.Click += new System.EventHandler(this.selectSoundButton_Click);
            // 
            // notifLabel
            // 
            this.notifLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.notifLabel.AutoSize = true;
            this.notifLabel.Location = new System.Drawing.Point(6, 76);
            this.notifLabel.Name = "notifLabel";
            this.notifLabel.Size = new System.Drawing.Size(60, 13);
            this.notifLabel.TabIndex = 17;
            this.notifLabel.Text = "Notification";
            // 
            // notificationComboBox
            // 
            this.notificationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.notificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationComboBox.FormattingEnabled = true;
            this.notificationComboBox.Location = new System.Drawing.Point(98, 71);
            this.notificationComboBox.Name = "notificationComboBox";
            this.notificationComboBox.Size = new System.Drawing.Size(177, 21);
            this.notificationComboBox.TabIndex = 16;
            this.notificationComboBox.SelectedValueChanged += new System.EventHandler(this.notificationComboBox_SelectedValueChanged);
            // 
            // basicSettingsGroup
            // 
            this.basicSettingsGroup.Controls.Add(this.checkboxSystrayIcon);
            this.basicSettingsGroup.Controls.Add(this.RunAtStartup);
            this.basicSettingsGroup.Location = new System.Drawing.Point(3, 3);
            this.basicSettingsGroup.Name = "basicSettingsGroup";
            this.basicSettingsGroup.Size = new System.Drawing.Size(312, 62);
            this.basicSettingsGroup.TabIndex = 0;
            this.basicSettingsGroup.TabStop = false;
            this.basicSettingsGroup.Text = global::SoundSwitch.Properties.SettingsString.basicSettings;
            // 
            // hotkeyTextBox
            // 
            this.hotkeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeyTextBox.Location = new System.Drawing.Point(63, 291);
            this.hotkeyTextBox.Name = "hotkeyTextBox";
            this.hotkeyTextBox.Size = new System.Drawing.Size(132, 20);
            this.hotkeyTextBox.TabIndex = 15;
            // 
            // hotkeysLabel
            // 
            this.hotkeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysLabel.AutoSize = true;
            this.hotkeysLabel.Location = new System.Drawing.Point(11, 294);
            this.hotkeysLabel.Name = "hotkeysLabel";
            this.hotkeysLabel.Size = new System.Drawing.Size(46, 13);
            this.hotkeysLabel.TabIndex = 14;
            this.hotkeysLabel.Text = "Hotkeys";
            // 
            // selectSoundFileDialog
            // 
            this.selectSoundFileDialog.FileName = "customSound";
            // 
            // hotkeysCheckbox
            // 
            this.hotkeysCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysCheckbox.AutoSize = true;
            this.hotkeysCheckbox.Location = new System.Drawing.Point(201, 294);
            this.hotkeysCheckbox.Name = "hotkeysCheckbox";
            this.hotkeysCheckbox.Size = new System.Drawing.Size(15, 14);
            this.hotkeysCheckbox.TabIndex = 20;
            this.hotkeysCheckbox.UseVisualStyleBackColor = true;
            this.hotkeysCheckbox.CheckedChanged += new System.EventHandler(this.hotkeysCheckbox_CheckedChanged);
            // 
            // checkboxSystrayIcon
            // 
            this.checkboxSystrayIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkboxSystrayIcon.AutoSize = true;
            this.checkboxSystrayIcon.Location = new System.Drawing.Point(6, 39);
            this.checkboxSystrayIcon.Name = "checkboxSystrayIcon";
            this.checkboxSystrayIcon.Size = new System.Drawing.Size(109, 17);
            this.checkboxSystrayIcon.TabIndex = 8;
            this.checkboxSystrayIcon.Text = global::SoundSwitch.Properties.SettingsString.keepSystrayIcon;
            this.checkboxSystrayIcon.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(565, 323);
            this.Controls.Add(this.hotkeysCheckbox);
            this.Controls.Add(this.hotkeyTextBox);
            this.Controls.Add(this.hotkeysLabel);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.closeButton);
            this.MinimumSize = new System.Drawing.Size(500, 325);
            this.Name = "Settings";
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.playbackPage.ResumeLayout(false);
            this.recordingPage.ResumeLayout(false);
            this.appSettingTabPage.ResumeLayout(false);
            this.updateSettingsGroup.ResumeLayout(false);
            this.updateSettingsGroup.PerformLayout();
            this.audioSettingsGroup.ResumeLayout(false);
            this.audioSettingsGroup.PerformLayout();
            this.basicSettingsGroup.ResumeLayout(false);
            this.basicSettingsGroup.PerformLayout();
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
        private System.Windows.Forms.Label hotkeysLabel;
        private System.Windows.Forms.ComboBox notificationComboBox;
        private System.Windows.Forms.Label notifLabel;
        private System.Windows.Forms.CheckBox betaVersionCheckbox;
        private System.Windows.Forms.OpenFileDialog selectSoundFileDialog;
        private System.Windows.Forms.Button selectSoundButton;
        private System.Windows.Forms.CheckBox hotkeysCheckbox;
        private System.Windows.Forms.TabPage appSettingTabPage;
        private System.Windows.Forms.GroupBox basicSettingsGroup;
        private System.Windows.Forms.GroupBox audioSettingsGroup;
        private System.Windows.Forms.GroupBox updateSettingsGroup;
        private System.Windows.Forms.CheckBox stealthUpdateCheckbox;
        private System.Windows.Forms.ComboBox tooltipInfoComboBox;
        private System.Windows.Forms.Label tooltipLabel;
        private System.Windows.Forms.Label cyclerLabel;
        private System.Windows.Forms.ComboBox cyclerComboBox;
        private System.Windows.Forms.CheckBox checkboxSystrayIcon;
    }
}