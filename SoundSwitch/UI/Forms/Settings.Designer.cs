namespace SoundSwitch.UI.Forms
{
    sealed partial class SettingsForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
            this.startWithWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.switchCommunicationDeviceCheckBox = new System.Windows.Forms.CheckBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.playbackTabPage = new System.Windows.Forms.TabPage();
            this.playbackListView = new System.Windows.Forms.ListView();
            this.recordingTabPage = new System.Windows.Forms.TabPage();
            this.recordingListView = new System.Windows.Forms.ListView();
            this.appSettingTabPage = new System.Windows.Forms.TabPage();
            this.languageGroupBox = new System.Windows.Forms.GroupBox();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.updateSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.updateNeverRadioButton = new System.Windows.Forms.RadioButton();
            this.updateNotifyRadioButton = new System.Windows.Forms.RadioButton();
            this.updateSilentRadioButton = new System.Windows.Forms.RadioButton();
            this.includeBetaVersionsCheckBox = new System.Windows.Forms.CheckBox();
            this.audioSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.cycleThroughLabel = new System.Windows.Forms.Label();
            this.cycleThroughComboBox = new System.Windows.Forms.ComboBox();
            this.tooltipOnHoverLabel = new System.Windows.Forms.Label();
            this.tooltipInfoComboBox = new System.Windows.Forms.ComboBox();
            this.selectSoundButton = new System.Windows.Forms.Button();
            this.notificationLabel = new System.Windows.Forms.Label();
            this.notificationComboBox = new System.Windows.Forms.ComboBox();
            this.basicSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.keepSystemTrayIconCheckBox = new System.Windows.Forms.CheckBox();
            this.hotkeysTextBox = new System.Windows.Forms.TextBox();
            this.hotkeysLabel = new System.Windows.Forms.Label();
            this.selectSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.hotkeysCheckBox = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.playbackTabPage.SuspendLayout();
            this.recordingTabPage.SuspendLayout();
            this.appSettingTabPage.SuspendLayout();
            this.languageGroupBox.SuspendLayout();
            this.updateSettingsGroupBox.SuspendLayout();
            this.audioSettingsGroupBox.SuspendLayout();
            this.basicSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // startWithWindowsCheckBox
            // 
            this.startWithWindowsCheckBox.AutoSize = true;
            this.startWithWindowsCheckBox.Location = new System.Drawing.Point(9, 35);
            this.startWithWindowsCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.startWithWindowsCheckBox.Name = "startWithWindowsCheckBox";
            this.startWithWindowsCheckBox.Size = new System.Drawing.Size(266, 24);
            this.startWithWindowsCheckBox.TabIndex = 7;
            this.startWithWindowsCheckBox.Text = "Start automatically with Windows";
            this.startWithWindowsCheckBox.UseVisualStyleBackColor = true;
            this.startWithWindowsCheckBox.CheckedChanged += new System.EventHandler(this.RunAtStartup_CheckedChanged);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(732, 452);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(112, 35);
            this.closeButton.TabIndex = 11;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // switchCommunicationDeviceCheckBox
            // 
            this.switchCommunicationDeviceCheckBox.AutoSize = true;
            this.switchCommunicationDeviceCheckBox.Location = new System.Drawing.Point(9, 35);
            this.switchCommunicationDeviceCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.switchCommunicationDeviceCheckBox.Name = "switchCommunicationDeviceCheckBox";
            this.switchCommunicationDeviceCheckBox.Size = new System.Drawing.Size(304, 24);
            this.switchCommunicationDeviceCheckBox.TabIndex = 12;
            this.switchCommunicationDeviceCheckBox.Text = "Switch Default Communication Device";
            this.switchCommunicationDeviceCheckBox.UseVisualStyleBackColor = true;
            this.switchCommunicationDeviceCheckBox.CheckedChanged += new System.EventHandler(this.communicationCheckbox_CheckedChanged);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.playbackTabPage);
            this.tabControl.Controls.Add(this.recordingTabPage);
            this.tabControl.Controls.Add(this.appSettingTabPage);
            this.tabControl.Location = new System.Drawing.Point(18, 9);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(826, 429);
            this.tabControl.TabIndex = 13;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // playbackTabPage
            // 
            this.playbackTabPage.Controls.Add(this.playbackListView);
            this.playbackTabPage.Location = new System.Drawing.Point(4, 29);
            this.playbackTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.playbackTabPage.Name = "playbackTabPage";
            this.playbackTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.playbackTabPage.Size = new System.Drawing.Size(818, 396);
            this.playbackTabPage.TabIndex = 0;
            this.playbackTabPage.Text = "Playback";
            this.playbackTabPage.UseVisualStyleBackColor = true;
            // 
            // playbackListView
            // 
            this.playbackListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.playbackListView.CheckBoxes = true;
            this.playbackListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Selected";
            listViewGroup1.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup1.Name = "selectedGroup";
            this.playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.playbackListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.playbackListView.Location = new System.Drawing.Point(4, 5);
            this.playbackListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.playbackListView.Name = "playbackListView";
            this.playbackListView.Size = new System.Drawing.Size(810, 386);
            this.playbackListView.TabIndex = 14;
            this.playbackListView.UseCompatibleStateImageBehavior = false;
            this.playbackListView.View = System.Windows.Forms.View.Details;
            // 
            // recordingTabPage
            // 
            this.recordingTabPage.Controls.Add(this.recordingListView);
            this.recordingTabPage.Location = new System.Drawing.Point(4, 29);
            this.recordingTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.recordingTabPage.Name = "recordingTabPage";
            this.recordingTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.recordingTabPage.Size = new System.Drawing.Size(818, 396);
            this.recordingTabPage.TabIndex = 1;
            this.recordingTabPage.Text = "Recording";
            this.recordingTabPage.UseVisualStyleBackColor = true;
            // 
            // recordingListView
            // 
            this.recordingListView.AccessibleName = "recordingListView";
            this.recordingListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.recordingListView.CheckBoxes = true;
            this.recordingListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup2.Header = "Selected";
            listViewGroup2.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup2.Name = "selectedGroup";
            this.recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup2});
            this.recordingListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.recordingListView.Location = new System.Drawing.Point(4, 5);
            this.recordingListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.recordingListView.Name = "recordingListView";
            this.recordingListView.Size = new System.Drawing.Size(810, 386);
            this.recordingListView.TabIndex = 17;
            this.recordingListView.UseCompatibleStateImageBehavior = false;
            this.recordingListView.View = System.Windows.Forms.View.Details;
            // 
            // appSettingTabPage
            // 
            this.appSettingTabPage.Controls.Add(this.languageGroupBox);
            this.appSettingTabPage.Controls.Add(this.updateSettingsGroupBox);
            this.appSettingTabPage.Controls.Add(this.audioSettingsGroupBox);
            this.appSettingTabPage.Controls.Add(this.basicSettingsGroupBox);
            this.appSettingTabPage.Location = new System.Drawing.Point(4, 29);
            this.appSettingTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.appSettingTabPage.Name = "appSettingTabPage";
            this.appSettingTabPage.Size = new System.Drawing.Size(818, 396);
            this.appSettingTabPage.TabIndex = 2;
            this.appSettingTabPage.Text = "Settings";
            this.appSettingTabPage.UseVisualStyleBackColor = true;
            // 
            // languageGroupBox
            // 
            this.languageGroupBox.Controls.Add(this.languageComboBox);
            this.languageGroupBox.Location = new System.Drawing.Point(482, 212);
            this.languageGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.languageGroupBox.Name = "languageGroupBox";
            this.languageGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.languageGroupBox.Size = new System.Drawing.Size(328, 92);
            this.languageGroupBox.TabIndex = 15;
            this.languageGroupBox.TabStop = false;
            this.languageGroupBox.Text = "Language";
            // 
            // languageComboBox
            // 
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Items.AddRange(new object[] {
            "English (en)",
            "French (fr)",
            "German (de)"});
            this.languageComboBox.Location = new System.Drawing.Point(12, 35);
            this.languageComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(302, 28);
            this.languageComboBox.TabIndex = 17;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // updateSettingsGroupBox
            // 
            this.updateSettingsGroupBox.Controls.Add(this.updateNeverRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.updateNotifyRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.updateSilentRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.includeBetaVersionsCheckBox);
            this.updateSettingsGroupBox.Location = new System.Drawing.Point(482, 5);
            this.updateSettingsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.updateSettingsGroupBox.Name = "updateSettingsGroupBox";
            this.updateSettingsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.updateSettingsGroupBox.Size = new System.Drawing.Size(328, 198);
            this.updateSettingsGroupBox.TabIndex = 14;
            this.updateSettingsGroupBox.TabStop = false;
            this.updateSettingsGroupBox.Text = "Update Settings";
            // 
            // updateNeverRadioButton
            // 
            this.updateNeverRadioButton.AutoSize = true;
            this.updateNeverRadioButton.Location = new System.Drawing.Point(10, 106);
            this.updateNeverRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.updateNeverRadioButton.Name = "updateNeverRadioButton";
            this.updateNeverRadioButton.Size = new System.Drawing.Size(206, 24);
            this.updateNeverRadioButton.TabIndex = 21;
            this.updateNeverRadioButton.TabStop = true;
            this.updateNeverRadioButton.Text = "Never check for updates";
            this.updateNeverRadioButton.UseVisualStyleBackColor = true;
            this.updateNeverRadioButton.CheckedChanged += new System.EventHandler(this.updateNeverRadioButton_CheckedChanged);
            // 
            // updateNotifyRadioButton
            // 
            this.updateNotifyRadioButton.AutoSize = true;
            this.updateNotifyRadioButton.Location = new System.Drawing.Point(10, 69);
            this.updateNotifyRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.updateNotifyRadioButton.Name = "updateNotifyRadioButton";
            this.updateNotifyRadioButton.Size = new System.Drawing.Size(296, 24);
            this.updateNotifyRadioButton.TabIndex = 20;
            this.updateNotifyRadioButton.TabStop = true;
            this.updateNotifyRadioButton.Text = "Notify me when updates are available";
            this.updateNotifyRadioButton.UseVisualStyleBackColor = true;
            this.updateNotifyRadioButton.CheckedChanged += new System.EventHandler(this.updateNotifyRadioButton_CheckedChanged);
            // 
            // updateSilentRadioButton
            // 
            this.updateSilentRadioButton.AutoSize = true;
            this.updateSilentRadioButton.Location = new System.Drawing.Point(10, 32);
            this.updateSilentRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.updateSilentRadioButton.Name = "updateSilentRadioButton";
            this.updateSilentRadioButton.Size = new System.Drawing.Size(234, 24);
            this.updateSilentRadioButton.TabIndex = 19;
            this.updateSilentRadioButton.TabStop = true;
            this.updateSilentRadioButton.Text = "Install updates automatically";
            this.updateSilentRadioButton.UseVisualStyleBackColor = true;
            this.updateSilentRadioButton.CheckedChanged += new System.EventHandler(this.updateSilentRadioButton_CheckedChanged);
            // 
            // includeBetaVersionsCheckBox
            // 
            this.includeBetaVersionsCheckBox.AutoSize = true;
            this.includeBetaVersionsCheckBox.Location = new System.Drawing.Point(10, 155);
            this.includeBetaVersionsCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.includeBetaVersionsCheckBox.Name = "includeBetaVersionsCheckBox";
            this.includeBetaVersionsCheckBox.Size = new System.Drawing.Size(187, 24);
            this.includeBetaVersionsCheckBox.TabIndex = 18;
            this.includeBetaVersionsCheckBox.Text = "Include Beta versions";
            this.includeBetaVersionsCheckBox.UseVisualStyleBackColor = true;
            this.includeBetaVersionsCheckBox.CheckedChanged += new System.EventHandler(this.betaVersionCheckbox_CheckedChanged);
            // 
            // audioSettingsGroupBox
            // 
            this.audioSettingsGroupBox.Controls.Add(this.cycleThroughLabel);
            this.audioSettingsGroupBox.Controls.Add(this.cycleThroughComboBox);
            this.audioSettingsGroupBox.Controls.Add(this.tooltipOnHoverLabel);
            this.audioSettingsGroupBox.Controls.Add(this.tooltipInfoComboBox);
            this.audioSettingsGroupBox.Controls.Add(this.switchCommunicationDeviceCheckBox);
            this.audioSettingsGroupBox.Controls.Add(this.selectSoundButton);
            this.audioSettingsGroupBox.Controls.Add(this.notificationLabel);
            this.audioSettingsGroupBox.Controls.Add(this.notificationComboBox);
            this.audioSettingsGroupBox.Location = new System.Drawing.Point(4, 135);
            this.audioSettingsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.audioSettingsGroupBox.Name = "audioSettingsGroupBox";
            this.audioSettingsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.audioSettingsGroupBox.Size = new System.Drawing.Size(468, 249);
            this.audioSettingsGroupBox.TabIndex = 13;
            this.audioSettingsGroupBox.TabStop = false;
            this.audioSettingsGroupBox.Text = "Audio Settings";
            // 
            // cycleThroughLabel
            // 
            this.cycleThroughLabel.Location = new System.Drawing.Point(-12, 199);
            this.cycleThroughLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.cycleThroughLabel.Name = "cycleThroughLabel";
            this.cycleThroughLabel.Size = new System.Drawing.Size(150, 20);
            this.cycleThroughLabel.TabIndex = 23;
            this.cycleThroughLabel.Text = "Cycle through";
            this.cycleThroughLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cycleThroughComboBox
            // 
            this.cycleThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cycleThroughComboBox.FormattingEnabled = true;
            this.cycleThroughComboBox.Location = new System.Drawing.Point(147, 196);
            this.cycleThroughComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cycleThroughComboBox.Name = "cycleThroughComboBox";
            this.cycleThroughComboBox.Size = new System.Drawing.Size(264, 28);
            this.cycleThroughComboBox.TabIndex = 22;
            this.cycleThroughComboBox.SelectedValueChanged += new System.EventHandler(this.cyclerComboBox_SelectedValueChanged);
            // 
            // tooltipOnHoverLabel
            // 
            this.tooltipOnHoverLabel.Location = new System.Drawing.Point(-12, 146);
            this.tooltipOnHoverLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tooltipOnHoverLabel.Name = "tooltipOnHoverLabel";
            this.tooltipOnHoverLabel.Size = new System.Drawing.Size(150, 20);
            this.tooltipOnHoverLabel.TabIndex = 21;
            this.tooltipOnHoverLabel.Text = "Tooltip on Hover";
            this.tooltipOnHoverLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tooltipInfoComboBox
            // 
            this.tooltipInfoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tooltipInfoComboBox.FormattingEnabled = true;
            this.tooltipInfoComboBox.Location = new System.Drawing.Point(147, 142);
            this.tooltipInfoComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tooltipInfoComboBox.Name = "tooltipInfoComboBox";
            this.tooltipInfoComboBox.Size = new System.Drawing.Size(264, 28);
            this.tooltipInfoComboBox.TabIndex = 20;
            this.tooltipInfoComboBox.SelectedValueChanged += new System.EventHandler(this.tooltipInfoComboBox_SelectedValueChanged);
            // 
            // selectSoundButton
            // 
            this.selectSoundButton.Location = new System.Drawing.Point(422, 86);
            this.selectSoundButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.selectSoundButton.Name = "selectSoundButton";
            this.selectSoundButton.Size = new System.Drawing.Size(36, 35);
            this.selectSoundButton.TabIndex = 19;
            this.selectSoundButton.Text = "...";
            this.selectSoundButton.UseVisualStyleBackColor = true;
            this.selectSoundButton.Visible = false;
            this.selectSoundButton.Click += new System.EventHandler(this.selectSoundButton_Click);
            // 
            // notificationLabel
            // 
            this.notificationLabel.Location = new System.Drawing.Point(-12, 92);
            this.notificationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.notificationLabel.Name = "notificationLabel";
            this.notificationLabel.Size = new System.Drawing.Size(150, 20);
            this.notificationLabel.TabIndex = 17;
            this.notificationLabel.Text = "Notification";
            this.notificationLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // notificationComboBox
            // 
            this.notificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationComboBox.FormattingEnabled = true;
            this.notificationComboBox.Location = new System.Drawing.Point(147, 88);
            this.notificationComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.notificationComboBox.Name = "notificationComboBox";
            this.notificationComboBox.Size = new System.Drawing.Size(264, 28);
            this.notificationComboBox.TabIndex = 16;
            this.notificationComboBox.SelectedValueChanged += new System.EventHandler(this.notificationComboBox_SelectedValueChanged);
            // 
            // basicSettingsGroupBox
            // 
            this.basicSettingsGroupBox.Controls.Add(this.keepSystemTrayIconCheckBox);
            this.basicSettingsGroupBox.Controls.Add(this.startWithWindowsCheckBox);
            this.basicSettingsGroupBox.Location = new System.Drawing.Point(4, 5);
            this.basicSettingsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.basicSettingsGroupBox.Name = "basicSettingsGroupBox";
            this.basicSettingsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.basicSettingsGroupBox.Size = new System.Drawing.Size(468, 122);
            this.basicSettingsGroupBox.TabIndex = 0;
            this.basicSettingsGroupBox.TabStop = false;
            this.basicSettingsGroupBox.Text = "Basic Settings";
            // 
            // keepSystemTrayIconCheckBox
            // 
            this.keepSystemTrayIconCheckBox.AutoSize = true;
            this.keepSystemTrayIconCheckBox.Location = new System.Drawing.Point(9, 71);
            this.keepSystemTrayIconCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.keepSystemTrayIconCheckBox.Name = "keepSystemTrayIconCheckBox";
            this.keepSystemTrayIconCheckBox.Size = new System.Drawing.Size(307, 24);
            this.keepSystemTrayIconCheckBox.TabIndex = 8;
            this.keepSystemTrayIconCheckBox.Text = "Keep SoundSwitch\'s System Tray Icon";
            this.keepSystemTrayIconCheckBox.UseVisualStyleBackColor = true;
            this.keepSystemTrayIconCheckBox.CheckedChanged += new System.EventHandler(this.checkboxSystrayIcon_CheckedChanged);
            // 
            // hotkeysTextBox
            // 
            this.hotkeysTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysTextBox.Location = new System.Drawing.Point(135, 451);
            this.hotkeysTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hotkeysTextBox.Name = "hotkeysTextBox";
            this.hotkeysTextBox.Size = new System.Drawing.Size(196, 26);
            this.hotkeysTextBox.TabIndex = 15;
            // 
            // hotkeysLabel
            // 
            this.hotkeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysLabel.Location = new System.Drawing.Point(-24, 455);
            this.hotkeysLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.hotkeysLabel.Name = "hotkeysLabel";
            this.hotkeysLabel.Size = new System.Drawing.Size(150, 20);
            this.hotkeysLabel.TabIndex = 14;
            this.hotkeysLabel.Text = "Hotkeys";
            this.hotkeysLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // selectSoundFileDialog
            // 
            this.selectSoundFileDialog.FileName = "customSound";
            // 
            // hotkeysCheckBox
            // 
            this.hotkeysCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysCheckBox.AutoSize = true;
            this.hotkeysCheckBox.Location = new System.Drawing.Point(345, 454);
            this.hotkeysCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hotkeysCheckBox.Name = "hotkeysCheckBox";
            this.hotkeysCheckBox.Size = new System.Drawing.Size(22, 21);
            this.hotkeysCheckBox.TabIndex = 20;
            this.hotkeysCheckBox.UseVisualStyleBackColor = true;
            this.hotkeysCheckBox.CheckedChanged += new System.EventHandler(this.hotkeysCheckbox_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(862, 497);
            this.Controls.Add(this.hotkeysCheckBox);
            this.Controls.Add(this.hotkeysTextBox);
            this.Controls.Add(this.hotkeysLabel);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.closeButton);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(884, 553);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.playbackTabPage.ResumeLayout(false);
            this.recordingTabPage.ResumeLayout(false);
            this.appSettingTabPage.ResumeLayout(false);
            this.languageGroupBox.ResumeLayout(false);
            this.updateSettingsGroupBox.ResumeLayout(false);
            this.updateSettingsGroupBox.PerformLayout();
            this.audioSettingsGroupBox.ResumeLayout(false);
            this.audioSettingsGroupBox.PerformLayout();
            this.basicSettingsGroupBox.ResumeLayout(false);
            this.basicSettingsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox startWithWindowsCheckBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox switchCommunicationDeviceCheckBox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage playbackTabPage;
        private System.Windows.Forms.ListView playbackListView;
        private System.Windows.Forms.TabPage recordingTabPage;
        private System.Windows.Forms.ListView recordingListView;
        private System.Windows.Forms.TextBox hotkeysTextBox;
        private System.Windows.Forms.Label hotkeysLabel;
        private System.Windows.Forms.ComboBox notificationComboBox;
        private System.Windows.Forms.Label notificationLabel;
        private System.Windows.Forms.CheckBox includeBetaVersionsCheckBox;
        private System.Windows.Forms.OpenFileDialog selectSoundFileDialog;
        private System.Windows.Forms.Button selectSoundButton;
        private System.Windows.Forms.CheckBox hotkeysCheckBox;
        private System.Windows.Forms.TabPage appSettingTabPage;
        private System.Windows.Forms.GroupBox basicSettingsGroupBox;
        private System.Windows.Forms.GroupBox audioSettingsGroupBox;
        private System.Windows.Forms.GroupBox updateSettingsGroupBox;
        private System.Windows.Forms.ComboBox tooltipInfoComboBox;
        private System.Windows.Forms.Label tooltipOnHoverLabel;
        private System.Windows.Forms.Label cycleThroughLabel;
        private System.Windows.Forms.ComboBox cycleThroughComboBox;
        private System.Windows.Forms.CheckBox keepSystemTrayIconCheckBox;
        private System.Windows.Forms.RadioButton updateNotifyRadioButton;
        private System.Windows.Forms.RadioButton updateSilentRadioButton;
        private System.Windows.Forms.RadioButton updateNeverRadioButton;
        private System.Windows.Forms.GroupBox languageGroupBox;
        private System.Windows.Forms.ComboBox languageComboBox;
    }
}