using System;
using SoundSwitch.Localization;
using SoundSwitch.UI.Component.ListView;
using SoundSwitch.Util.Url;

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
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
            startWithWindowsCheckBox = new System.Windows.Forms.CheckBox();
            closeButton = new System.Windows.Forms.Button();
            switchCommunicationDeviceCheckBox = new System.Windows.Forms.CheckBox();
            tabControl = new System.Windows.Forms.TabControl();
            playbackTabPage = new System.Windows.Forms.TabPage();
            playbackListView = new ListViewExtended();
            recordingTabPage = new System.Windows.Forms.TabPage();
            recordingListView = new ListViewExtended();
            profileTabPage = new System.Windows.Forms.TabPage();
            editProfileButton = new System.Windows.Forms.Button();
            deleteProfileButton = new System.Windows.Forms.Button();
            profileExplanationLabel = new System.Windows.Forms.Label();
            profilesListView = new IconListView();
            addProfileButton = new System.Windows.Forms.Button();
            appSettingTabPage = new System.Windows.Forms.TabPage();
            notificationGroupBox = new System.Windows.Forms.GroupBox();
            singleNotificationCheckbox = new System.Windows.Forms.CheckBox();
            positionComboBox = new System.Windows.Forms.ComboBox();
            notificationComboBox = new System.Windows.Forms.ComboBox();
            selectSoundButton = new System.Windows.Forms.Button();
            usePrimaryScreenCheckbox = new System.Windows.Forms.CheckBox();
            deleteSoundButton = new System.Windows.Forms.Button();
            positionLabel = new System.Windows.Forms.Label();
            languageGroupBox = new System.Windows.Forms.GroupBox();
            languageComboBox = new System.Windows.Forms.ComboBox();
            updateSettingsGroupBox = new System.Windows.Forms.GroupBox();
            telemetryCheckbox = new System.Windows.Forms.CheckBox();
            updateNeverRadioButton = new System.Windows.Forms.RadioButton();
            updateNotifyRadioButton = new System.Windows.Forms.RadioButton();
            updateSilentRadioButton = new System.Windows.Forms.RadioButton();
            includeBetaVersionsCheckBox = new System.Windows.Forms.CheckBox();
            audioSettingsGroupBox = new System.Windows.Forms.GroupBox();
            quickMenuCheckbox = new System.Windows.Forms.CheckBox();
            keepVolumeCheckbox = new System.Windows.Forms.CheckBox();
            foregroundAppCheckbox = new System.Windows.Forms.CheckBox();
            cycleThroughLabel = new System.Windows.Forms.Label();
            cycleThroughComboBox = new System.Windows.Forms.ComboBox();
            tooltipOnHoverLabel = new System.Windows.Forms.Label();
            tooltipInfoComboBox = new System.Windows.Forms.ComboBox();
            basicSettingsGroupBox = new System.Windows.Forms.GroupBox();
            iconChangeLabel = new System.Windows.Forms.Label();
            iconChangeChoicesComboBox = new System.Windows.Forms.ComboBox();
            troubleshootingTabPage = new System.Windows.Forms.TabPage();
            troubleshootingLabel = new System.Windows.Forms.Label();
            soundSwitchPictureBox = new System.Windows.Forms.PictureBox();
            discordCommunityLinkLabel = new System.Windows.Forms.LinkLabel();
            gitHubHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            exportLogFilesGroupBox = new System.Windows.Forms.GroupBox();
            exportLogFilesButton = new System.Windows.Forms.Button();
            exportLogFilesLabel = new System.Windows.Forms.Label();
            appNameLabel = new System.Windows.Forms.Label();
            resetAudioDevicesGroupBox = new System.Windows.Forms.GroupBox();
            resetAudioDevicesLabel = new System.Windows.Forms.Label();
            resetAudioDevicesButton = new System.Windows.Forms.Button();
            donateLinkLabel = new System.Windows.Forms.LinkLabel();
            discordPictureBox = new System.Windows.Forms.PictureBox();
            donatePictureBox = new System.Windows.Forms.PictureBox();
            gitHubPictureBox = new System.Windows.Forms.PictureBox();
            selectSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
            hotkeysCheckBox = new System.Windows.Forms.CheckBox();
            hotKeyControl = new Component.HotKeyTextBox();
            toggleMuteLabel = new System.Windows.Forms.Label();
            muteHotKey = new Component.HotKeyTextBox();
            muteHotKeyCheckbox = new System.Windows.Forms.CheckBox();
            tabControl.SuspendLayout();
            playbackTabPage.SuspendLayout();
            recordingTabPage.SuspendLayout();
            profileTabPage.SuspendLayout();
            appSettingTabPage.SuspendLayout();
            notificationGroupBox.SuspendLayout();
            languageGroupBox.SuspendLayout();
            updateSettingsGroupBox.SuspendLayout();
            audioSettingsGroupBox.SuspendLayout();
            basicSettingsGroupBox.SuspendLayout();
            troubleshootingTabPage.SuspendLayout();
            exportLogFilesGroupBox.SuspendLayout();
            resetAudioDevicesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)discordPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)donatePictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gitHubPictureBox).BeginInit();
            SuspendLayout();
            // 
            // startWithWindowsCheckBox
            // 
            startWithWindowsCheckBox.AutoSize = true;
            startWithWindowsCheckBox.Location = new System.Drawing.Point(6, 23);
            startWithWindowsCheckBox.Name = "startWithWindowsCheckBox";
            startWithWindowsCheckBox.Size = new System.Drawing.Size(203, 19);
            startWithWindowsCheckBox.TabIndex = 7;
            startWithWindowsCheckBox.Text = "Start automatically with Windows";
            startWithWindowsCheckBox.UseVisualStyleBackColor = true;
            startWithWindowsCheckBox.CheckedChanged += RunAtStartup_CheckedChanged;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            closeButton.Location = new System.Drawing.Point(703, 436);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(72, 26);
            closeButton.TabIndex = 11;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // switchCommunicationDeviceCheckBox
            // 
            switchCommunicationDeviceCheckBox.AutoSize = true;
            switchCommunicationDeviceCheckBox.Location = new System.Drawing.Point(6, 23);
            switchCommunicationDeviceCheckBox.Name = "switchCommunicationDeviceCheckBox";
            switchCommunicationDeviceCheckBox.Size = new System.Drawing.Size(230, 19);
            switchCommunicationDeviceCheckBox.TabIndex = 12;
            switchCommunicationDeviceCheckBox.Text = "Switch Default Communication Device";
            switchCommunicationDeviceCheckBox.UseVisualStyleBackColor = true;
            switchCommunicationDeviceCheckBox.CheckedChanged += CommunicationCheckbox_CheckedChanged;
            // 
            // tabControl
            // 
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl.Controls.Add(playbackTabPage);
            tabControl.Controls.Add(recordingTabPage);
            tabControl.Controls.Add(profileTabPage);
            tabControl.Controls.Add(appSettingTabPage);
            tabControl.Controls.Add(troubleshootingTabPage);
            tabControl.Location = new System.Drawing.Point(12, 6);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(762, 404);
            tabControl.TabIndex = 13;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // playbackTabPage
            // 
            playbackTabPage.Controls.Add(playbackListView);
            playbackTabPage.Location = new System.Drawing.Point(4, 24);
            playbackTabPage.Name = "playbackTabPage";
            playbackTabPage.Padding = new System.Windows.Forms.Padding(3);
            playbackTabPage.Size = new System.Drawing.Size(754, 376);
            playbackTabPage.TabIndex = 0;
            playbackTabPage.Text = "Playback";
            playbackTabPage.UseVisualStyleBackColor = true;
            // 
            // playbackListView
            // 
            playbackListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            playbackListView.CheckBoxes = true;
            playbackListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup3.Header = "Selected";
            listViewGroup3.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup3.Name = "selectedGroup";
            playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] { listViewGroup3 });
            playbackListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            playbackListView.Location = new System.Drawing.Point(3, 3);
            playbackListView.Name = "playbackListView";
            playbackListView.Size = new System.Drawing.Size(748, 370);
            playbackListView.TabIndex = 14;
            playbackListView.UseCompatibleStateImageBehavior = false;
            playbackListView.View = System.Windows.Forms.View.Details;
            // 
            // recordingTabPage
            // 
            recordingTabPage.Controls.Add(recordingListView);
            recordingTabPage.Location = new System.Drawing.Point(4, 24);
            recordingTabPage.Name = "recordingTabPage";
            recordingTabPage.Padding = new System.Windows.Forms.Padding(3);
            recordingTabPage.Size = new System.Drawing.Size(754, 376);
            recordingTabPage.TabIndex = 1;
            recordingTabPage.Text = "Recording";
            recordingTabPage.UseVisualStyleBackColor = true;
            // 
            // recordingListView
            // 
            recordingListView.AccessibleName = "recordingListView";
            recordingListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            recordingListView.CheckBoxes = true;
            recordingListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup4.Header = "Selected";
            listViewGroup4.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup4.Name = "selectedGroup";
            recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] { listViewGroup4 });
            recordingListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            recordingListView.Location = new System.Drawing.Point(3, 3);
            recordingListView.Name = "recordingListView";
            recordingListView.Size = new System.Drawing.Size(748, 370);
            recordingListView.TabIndex = 17;
            recordingListView.UseCompatibleStateImageBehavior = false;
            recordingListView.View = System.Windows.Forms.View.Details;
            // 
            // profileTabPage
            // 
            profileTabPage.Controls.Add(editProfileButton);
            profileTabPage.Controls.Add(deleteProfileButton);
            profileTabPage.Controls.Add(profileExplanationLabel);
            profileTabPage.Controls.Add(profilesListView);
            profileTabPage.Controls.Add(addProfileButton);
            profileTabPage.Location = new System.Drawing.Point(4, 24);
            profileTabPage.Name = "profileTabPage";
            profileTabPage.Padding = new System.Windows.Forms.Padding(3);
            profileTabPage.Size = new System.Drawing.Size(754, 376);
            profileTabPage.TabIndex = 3;
            profileTabPage.Text = "Profiles";
            profileTabPage.UseVisualStyleBackColor = true;
            // 
            // editProfileButton
            // 
            editProfileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            editProfileButton.Enabled = false;
            editProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            editProfileButton.Location = new System.Drawing.Point(542, 344);
            editProfileButton.Name = "editProfileButton";
            editProfileButton.Size = new System.Drawing.Size(100, 26);
            editProfileButton.TabIndex = 5;
            editProfileButton.Text = "Edit";
            editProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            editProfileButton.UseVisualStyleBackColor = true;
            editProfileButton.Click += EditProfileButton_Click;
            // 
            // deleteProfileButton
            // 
            deleteProfileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            deleteProfileButton.Enabled = false;
            deleteProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            deleteProfileButton.Location = new System.Drawing.Point(648, 344);
            deleteProfileButton.Name = "deleteProfileButton";
            deleteProfileButton.Size = new System.Drawing.Size(100, 26);
            deleteProfileButton.TabIndex = 4;
            deleteProfileButton.Text = "Delete";
            deleteProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            deleteProfileButton.UseVisualStyleBackColor = true;
            deleteProfileButton.Click += DeleteProfileButton_Click;
            // 
            // profileExplanationLabel
            // 
            profileExplanationLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            profileExplanationLabel.AutoSize = true;
            profileExplanationLabel.Location = new System.Drawing.Point(6, 257);
            profileExplanationLabel.MaximumSize = new System.Drawing.Size(500, 0);
            profileExplanationLabel.Name = "profileExplanationLabel";
            profileExplanationLabel.Size = new System.Drawing.Size(100, 30);
            profileExplanationLabel.TabIndex = 3;
            profileExplanationLabel.Text = "Explanation line 1\r\nOptional line 2";
            // 
            // profilesListView
            // 
            profilesListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            profilesListView.FullRowSelect = true;
            profilesListView.Location = new System.Drawing.Point(3, 3);
            profilesListView.Name = "profilesListView";
            profilesListView.OwnerDraw = true;
            profilesListView.ShowGroups = false;
            profilesListView.Size = new System.Drawing.Size(748, 247);
            profilesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            profilesListView.TabIndex = 2;
            profilesListView.UseCompatibleStateImageBehavior = false;
            profilesListView.View = System.Windows.Forms.View.Details;
            profilesListView.SelectedIndexChanged += ProfilesListView_SelectedIndexChanged;
            profilesListView.DoubleClick += ProfilesListView_DoubleClick;
            // 
            // addProfileButton
            // 
            addProfileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            addProfileButton.Location = new System.Drawing.Point(436, 344);
            addProfileButton.Name = "addProfileButton";
            addProfileButton.Size = new System.Drawing.Size(100, 26);
            addProfileButton.TabIndex = 1;
            addProfileButton.Text = "Add";
            addProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            addProfileButton.UseVisualStyleBackColor = true;
            addProfileButton.Click += AddProfileButton_Click;
            // 
            // appSettingTabPage
            // 
            appSettingTabPage.Controls.Add(notificationGroupBox);
            appSettingTabPage.Controls.Add(languageGroupBox);
            appSettingTabPage.Controls.Add(updateSettingsGroupBox);
            appSettingTabPage.Controls.Add(audioSettingsGroupBox);
            appSettingTabPage.Controls.Add(basicSettingsGroupBox);
            appSettingTabPage.Location = new System.Drawing.Point(4, 24);
            appSettingTabPage.Name = "appSettingTabPage";
            appSettingTabPage.Size = new System.Drawing.Size(754, 376);
            appSettingTabPage.TabIndex = 2;
            appSettingTabPage.Text = "Settings";
            appSettingTabPage.UseVisualStyleBackColor = true;
            // 
            // notificationGroupBox
            // 
            notificationGroupBox.Controls.Add(singleNotificationCheckbox);
            notificationGroupBox.Controls.Add(positionComboBox);
            notificationGroupBox.Controls.Add(notificationComboBox);
            notificationGroupBox.Controls.Add(selectSoundButton);
            notificationGroupBox.Controls.Add(usePrimaryScreenCheckbox);
            notificationGroupBox.Controls.Add(deleteSoundButton);
            notificationGroupBox.Controls.Add(positionLabel);
            notificationGroupBox.Location = new System.Drawing.Point(434, 232);
            notificationGroupBox.Name = "notificationGroupBox";
            notificationGroupBox.Size = new System.Drawing.Size(315, 138);
            notificationGroupBox.TabIndex = 16;
            notificationGroupBox.TabStop = false;
            notificationGroupBox.Text = "Notification";
            // 
            // singleNotificationCheckbox
            // 
            singleNotificationCheckbox.AutoSize = true;
            singleNotificationCheckbox.Location = new System.Drawing.Point(78, 106);
            singleNotificationCheckbox.Name = "singleNotificationCheckbox";
            singleNotificationCheckbox.Size = new System.Drawing.Size(124, 19);
            singleNotificationCheckbox.TabIndex = 27;
            singleNotificationCheckbox.Text = "Single Notification";
            singleNotificationCheckbox.UseVisualStyleBackColor = true;
            // 
            // positionComboBox
            // 
            positionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            positionComboBox.FormattingEnabled = true;
            positionComboBox.Location = new System.Drawing.Point(122, 51);
            positionComboBox.Name = "positionComboBox";
            positionComboBox.Size = new System.Drawing.Size(121, 23);
            positionComboBox.TabIndex = 17;
            positionComboBox.SelectedValueChanged += PositionComboBox_SelectedValueChanged;
            // 
            // notificationComboBox
            // 
            notificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            notificationComboBox.FormattingEnabled = true;
            notificationComboBox.Location = new System.Drawing.Point(6, 22);
            notificationComboBox.Name = "notificationComboBox";
            notificationComboBox.Size = new System.Drawing.Size(237, 23);
            notificationComboBox.TabIndex = 16;
            notificationComboBox.SelectedValueChanged += NotificationComboBox_SelectedValueChanged;
            // 
            // selectSoundButton
            // 
            selectSoundButton.Location = new System.Drawing.Point(249, 21);
            selectSoundButton.Name = "selectSoundButton";
            selectSoundButton.Size = new System.Drawing.Size(24, 24);
            selectSoundButton.TabIndex = 19;
            selectSoundButton.Text = "...";
            selectSoundButton.UseVisualStyleBackColor = true;
            selectSoundButton.Visible = false;
            selectSoundButton.Click += SelectSoundButton_Click;
            // 
            // usePrimaryScreenCheckbox
            // 
            usePrimaryScreenCheckbox.AutoSize = true;
            usePrimaryScreenCheckbox.Location = new System.Drawing.Point(78, 80);
            usePrimaryScreenCheckbox.Name = "usePrimaryScreenCheckbox";
            usePrimaryScreenCheckbox.Size = new System.Drawing.Size(165, 19);
            usePrimaryScreenCheckbox.TabIndex = 26;
            usePrimaryScreenCheckbox.Text = "Always use primary screen";
            usePrimaryScreenCheckbox.UseVisualStyleBackColor = true;
            usePrimaryScreenCheckbox.CheckedChanged += UsePrimaryScreenCheckbox_CheckedChanged;
            // 
            // deleteSoundButton
            // 
            deleteSoundButton.Image = Properties.Resources.delete;
            deleteSoundButton.Location = new System.Drawing.Point(279, 21);
            deleteSoundButton.Name = "deleteSoundButton";
            deleteSoundButton.Size = new System.Drawing.Size(24, 24);
            deleteSoundButton.TabIndex = 24;
            deleteSoundButton.UseVisualStyleBackColor = true;
            deleteSoundButton.Visible = false;
            deleteSoundButton.Click += DeleteSoundButton_Click;
            // 
            // positionLabel
            // 
            positionLabel.Location = new System.Drawing.Point(6, 51);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new System.Drawing.Size(110, 23);
            positionLabel.TabIndex = 17;
            positionLabel.Text = "Position";
            positionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // languageGroupBox
            // 
            languageGroupBox.Controls.Add(languageComboBox);
            languageGroupBox.Location = new System.Drawing.Point(434, 165);
            languageGroupBox.Name = "languageGroupBox";
            languageGroupBox.Size = new System.Drawing.Size(315, 61);
            languageGroupBox.TabIndex = 15;
            languageGroupBox.TabStop = false;
            languageGroupBox.Text = "Language";
            // 
            // languageComboBox
            // 
            languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Location = new System.Drawing.Point(6, 22);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new System.Drawing.Size(237, 23);
            languageComboBox.TabIndex = 17;
            languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
            // 
            // updateSettingsGroupBox
            // 
            updateSettingsGroupBox.Controls.Add(telemetryCheckbox);
            updateSettingsGroupBox.Controls.Add(updateNeverRadioButton);
            updateSettingsGroupBox.Controls.Add(updateNotifyRadioButton);
            updateSettingsGroupBox.Controls.Add(updateSilentRadioButton);
            updateSettingsGroupBox.Controls.Add(includeBetaVersionsCheckBox);
            updateSettingsGroupBox.Location = new System.Drawing.Point(434, 3);
            updateSettingsGroupBox.Name = "updateSettingsGroupBox";
            updateSettingsGroupBox.Size = new System.Drawing.Size(315, 156);
            updateSettingsGroupBox.TabIndex = 14;
            updateSettingsGroupBox.TabStop = false;
            updateSettingsGroupBox.Text = "Update Settings";
            // 
            // telemetryCheckbox
            // 
            telemetryCheckbox.AutoSize = true;
            telemetryCheckbox.Location = new System.Drawing.Point(7, 130);
            telemetryCheckbox.Name = "telemetryCheckbox";
            telemetryCheckbox.Size = new System.Drawing.Size(77, 19);
            telemetryCheckbox.TabIndex = 22;
            telemetryCheckbox.Text = "Telemetry";
            telemetryCheckbox.UseVisualStyleBackColor = true;
            // 
            // updateNeverRadioButton
            // 
            updateNeverRadioButton.AutoSize = true;
            updateNeverRadioButton.Location = new System.Drawing.Point(7, 71);
            updateNeverRadioButton.Name = "updateNeverRadioButton";
            updateNeverRadioButton.Size = new System.Drawing.Size(153, 19);
            updateNeverRadioButton.TabIndex = 21;
            updateNeverRadioButton.TabStop = true;
            updateNeverRadioButton.Text = "Never check for updates";
            updateNeverRadioButton.UseVisualStyleBackColor = true;
            updateNeverRadioButton.CheckedChanged += UpdateNeverRadioButton_CheckedChanged;
            // 
            // updateNotifyRadioButton
            // 
            updateNotifyRadioButton.AutoSize = true;
            updateNotifyRadioButton.Location = new System.Drawing.Point(7, 46);
            updateNotifyRadioButton.Name = "updateNotifyRadioButton";
            updateNotifyRadioButton.Size = new System.Drawing.Size(223, 19);
            updateNotifyRadioButton.TabIndex = 20;
            updateNotifyRadioButton.TabStop = true;
            updateNotifyRadioButton.Text = "Notify me when updates are available";
            updateNotifyRadioButton.UseVisualStyleBackColor = true;
            updateNotifyRadioButton.CheckedChanged += UpdateNotifyRadioButton_CheckedChanged;
            // 
            // updateSilentRadioButton
            // 
            updateSilentRadioButton.AutoSize = true;
            updateSilentRadioButton.Location = new System.Drawing.Point(7, 21);
            updateSilentRadioButton.Name = "updateSilentRadioButton";
            updateSilentRadioButton.Size = new System.Drawing.Size(176, 19);
            updateSilentRadioButton.TabIndex = 19;
            updateSilentRadioButton.TabStop = true;
            updateSilentRadioButton.Text = "Install updates automatically";
            updateSilentRadioButton.UseVisualStyleBackColor = true;
            updateSilentRadioButton.CheckedChanged += UpdateSilentRadioButton_CheckedChanged;
            // 
            // includeBetaVersionsCheckBox
            // 
            includeBetaVersionsCheckBox.AutoSize = true;
            includeBetaVersionsCheckBox.Location = new System.Drawing.Point(7, 103);
            includeBetaVersionsCheckBox.Name = "includeBetaVersionsCheckBox";
            includeBetaVersionsCheckBox.Size = new System.Drawing.Size(137, 19);
            includeBetaVersionsCheckBox.TabIndex = 18;
            includeBetaVersionsCheckBox.Text = "Include Beta versions";
            includeBetaVersionsCheckBox.UseVisualStyleBackColor = true;
            includeBetaVersionsCheckBox.CheckedChanged += BetaVersionCheckbox_CheckedChanged;
            // 
            // audioSettingsGroupBox
            // 
            audioSettingsGroupBox.Controls.Add(quickMenuCheckbox);
            audioSettingsGroupBox.Controls.Add(keepVolumeCheckbox);
            audioSettingsGroupBox.Controls.Add(foregroundAppCheckbox);
            audioSettingsGroupBox.Controls.Add(cycleThroughLabel);
            audioSettingsGroupBox.Controls.Add(cycleThroughComboBox);
            audioSettingsGroupBox.Controls.Add(tooltipOnHoverLabel);
            audioSettingsGroupBox.Controls.Add(tooltipInfoComboBox);
            audioSettingsGroupBox.Controls.Add(switchCommunicationDeviceCheckBox);
            audioSettingsGroupBox.Location = new System.Drawing.Point(3, 102);
            audioSettingsGroupBox.Name = "audioSettingsGroupBox";
            audioSettingsGroupBox.Size = new System.Drawing.Size(425, 268);
            audioSettingsGroupBox.TabIndex = 13;
            audioSettingsGroupBox.TabStop = false;
            audioSettingsGroupBox.Text = "Audio Settings";
            // 
            // quickMenuCheckbox
            // 
            quickMenuCheckbox.AutoSize = true;
            quickMenuCheckbox.Location = new System.Drawing.Point(6, 103);
            quickMenuCheckbox.Name = "quickMenuCheckbox";
            quickMenuCheckbox.Size = new System.Drawing.Size(144, 19);
            quickMenuCheckbox.TabIndex = 27;
            quickMenuCheckbox.Text = "QuickMenu on hotkey";
            quickMenuCheckbox.UseVisualStyleBackColor = true;
            // 
            // keepVolumeCheckbox
            // 
            keepVolumeCheckbox.Location = new System.Drawing.Point(6, 73);
            keepVolumeCheckbox.Name = "keepVolumeCheckbox";
            keepVolumeCheckbox.Size = new System.Drawing.Size(277, 24);
            keepVolumeCheckbox.TabIndex = 29;
            keepVolumeCheckbox.Text = "Keep volume levels across playback devices";
            keepVolumeCheckbox.CheckedChanged += KeepVolumeCheckbox_CheckedChanged;
            // 
            // foregroundAppCheckbox
            // 
            foregroundAppCheckbox.AutoSize = true;
            foregroundAppCheckbox.Location = new System.Drawing.Point(6, 48);
            foregroundAppCheckbox.Name = "foregroundAppCheckbox";
            foregroundAppCheckbox.Size = new System.Drawing.Size(149, 19);
            foregroundAppCheckbox.TabIndex = 25;
            foregroundAppCheckbox.Text = "Switch Foreground app";
            foregroundAppCheckbox.UseVisualStyleBackColor = true;
            foregroundAppCheckbox.CheckedChanged += ForegroundAppCheckbox_CheckedChanged;
            // 
            // cycleThroughLabel
            // 
            cycleThroughLabel.Location = new System.Drawing.Point(6, 190);
            cycleThroughLabel.Name = "cycleThroughLabel";
            cycleThroughLabel.Size = new System.Drawing.Size(106, 23);
            cycleThroughLabel.TabIndex = 23;
            cycleThroughLabel.Text = "Cycle through";
            cycleThroughLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cycleThroughComboBox
            // 
            cycleThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cycleThroughComboBox.FormattingEnabled = true;
            cycleThroughComboBox.Location = new System.Drawing.Point(118, 190);
            cycleThroughComboBox.Name = "cycleThroughComboBox";
            cycleThroughComboBox.Size = new System.Drawing.Size(237, 23);
            cycleThroughComboBox.TabIndex = 22;
            cycleThroughComboBox.SelectedValueChanged += CyclerComboBox_SelectedValueChanged;
            // 
            // tooltipOnHoverLabel
            // 
            tooltipOnHoverLabel.Location = new System.Drawing.Point(6, 154);
            tooltipOnHoverLabel.Name = "tooltipOnHoverLabel";
            tooltipOnHoverLabel.Size = new System.Drawing.Size(106, 23);
            tooltipOnHoverLabel.TabIndex = 21;
            tooltipOnHoverLabel.Text = "Tooltip on Hover";
            tooltipOnHoverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tooltipInfoComboBox
            // 
            tooltipInfoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tooltipInfoComboBox.FormattingEnabled = true;
            tooltipInfoComboBox.Location = new System.Drawing.Point(118, 154);
            tooltipInfoComboBox.Name = "tooltipInfoComboBox";
            tooltipInfoComboBox.Size = new System.Drawing.Size(237, 23);
            tooltipInfoComboBox.TabIndex = 20;
            tooltipInfoComboBox.SelectedValueChanged += TooltipInfoComboBox_SelectedValueChanged;
            // 
            // basicSettingsGroupBox
            // 
            basicSettingsGroupBox.Controls.Add(iconChangeLabel);
            basicSettingsGroupBox.Controls.Add(startWithWindowsCheckBox);
            basicSettingsGroupBox.Controls.Add(iconChangeChoicesComboBox);
            basicSettingsGroupBox.Location = new System.Drawing.Point(3, 3);
            basicSettingsGroupBox.Name = "basicSettingsGroupBox";
            basicSettingsGroupBox.Size = new System.Drawing.Size(425, 93);
            basicSettingsGroupBox.TabIndex = 0;
            basicSettingsGroupBox.TabStop = false;
            basicSettingsGroupBox.Text = "Basic Settings";
            // 
            // iconChangeLabel
            // 
            iconChangeLabel.Location = new System.Drawing.Point(6, 51);
            iconChangeLabel.Name = "iconChangeLabel";
            iconChangeLabel.Size = new System.Drawing.Size(106, 23);
            iconChangeLabel.TabIndex = 27;
            iconChangeLabel.Text = "Systray Icon";
            iconChangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // iconChangeChoicesComboBox
            // 
            iconChangeChoicesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            iconChangeChoicesComboBox.FormattingEnabled = true;
            iconChangeChoicesComboBox.Location = new System.Drawing.Point(118, 51);
            iconChangeChoicesComboBox.Name = "iconChangeChoicesComboBox";
            iconChangeChoicesComboBox.Size = new System.Drawing.Size(237, 23);
            iconChangeChoicesComboBox.TabIndex = 26;
            iconChangeChoicesComboBox.SelectedIndexChanged += IconChangeChoicesComboBox_SelectedIndexChanged;
            // 
            // troubleshootingTabPage
            // 
            troubleshootingTabPage.Controls.Add(troubleshootingLabel);
            troubleshootingTabPage.Controls.Add(soundSwitchPictureBox);
            troubleshootingTabPage.Controls.Add(discordCommunityLinkLabel);
            troubleshootingTabPage.Controls.Add(gitHubHelpLinkLabel);
            troubleshootingTabPage.Controls.Add(exportLogFilesGroupBox);
            troubleshootingTabPage.Controls.Add(appNameLabel);
            troubleshootingTabPage.Controls.Add(resetAudioDevicesGroupBox);
            troubleshootingTabPage.Controls.Add(donateLinkLabel);
            troubleshootingTabPage.Controls.Add(discordPictureBox);
            troubleshootingTabPage.Controls.Add(donatePictureBox);
            troubleshootingTabPage.Controls.Add(gitHubPictureBox);
            troubleshootingTabPage.Location = new System.Drawing.Point(4, 24);
            troubleshootingTabPage.Name = "troubleshootingTabPage";
            troubleshootingTabPage.Padding = new System.Windows.Forms.Padding(3);
            troubleshootingTabPage.Size = new System.Drawing.Size(754, 376);
            troubleshootingTabPage.TabIndex = 4;
            troubleshootingTabPage.Text = "Troubleshooting";
            troubleshootingTabPage.UseVisualStyleBackColor = true;
            // 
            // troubleshootingLabel
            // 
            troubleshootingLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            troubleshootingLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic);
            troubleshootingLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            troubleshootingLabel.Location = new System.Drawing.Point(299, 72);
            troubleshootingLabel.Name = "troubleshootingLabel";
            troubleshootingLabel.Padding = new System.Windows.Forms.Padding(60, 30, 30, 30);
            troubleshootingLabel.Size = new System.Drawing.Size(452, 162);
            troubleshootingLabel.TabIndex = 11;
            troubleshootingLabel.Text = "If you need any help or further troubleshooting with SoundSwitch, you can visit the Help Discussion or Community Discord link, or help support us by visiting the Donate link below.";
            // 
            // soundSwitchPictureBox
            // 
            soundSwitchPictureBox.Image = Properties.Resources.SoundSwitch48;
            soundSwitchPictureBox.Location = new System.Drawing.Point(414, 20);
            soundSwitchPictureBox.Name = "soundSwitchPictureBox";
            soundSwitchPictureBox.Size = new System.Drawing.Size(51, 49);
            soundSwitchPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            soundSwitchPictureBox.TabIndex = 9;
            soundSwitchPictureBox.TabStop = false;
            // 
            // discordCommunityLinkLabel
            // 
            discordCommunityLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            discordCommunityLinkLabel.AutoSize = true;
            discordCommunityLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            discordCommunityLinkLabel.Location = new System.Drawing.Point(480, 280);
            discordCommunityLinkLabel.Name = "discordCommunityLinkLabel";
            discordCommunityLinkLabel.Size = new System.Drawing.Size(149, 21);
            discordCommunityLinkLabel.TabIndex = 1;
            discordCommunityLinkLabel.TabStop = true;
            discordCommunityLinkLabel.Text = "Community Discord";
            discordCommunityLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            discordCommunityLinkLabel.LinkClicked += DiscordCommunityLinkLabel_LinkClicked;
            // 
            // gitHubHelpLinkLabel
            // 
            gitHubHelpLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            gitHubHelpLinkLabel.AutoSize = true;
            gitHubHelpLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gitHubHelpLinkLabel.Location = new System.Drawing.Point(480, 242);
            gitHubHelpLinkLabel.Name = "gitHubHelpLinkLabel";
            gitHubHelpLinkLabel.Size = new System.Drawing.Size(120, 21);
            gitHubHelpLinkLabel.TabIndex = 0;
            gitHubHelpLinkLabel.TabStop = true;
            gitHubHelpLinkLabel.Text = "Help Discussion";
            gitHubHelpLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            gitHubHelpLinkLabel.LinkClicked += GitHubHelpLinkLabel_LinkClicked;
            // 
            // exportLogFilesGroupBox
            // 
            exportLogFilesGroupBox.Controls.Add(exportLogFilesButton);
            exportLogFilesGroupBox.Controls.Add(exportLogFilesLabel);
            exportLogFilesGroupBox.Location = new System.Drawing.Point(3, 129);
            exportLogFilesGroupBox.Name = "exportLogFilesGroupBox";
            exportLogFilesGroupBox.Size = new System.Drawing.Size(290, 110);
            exportLogFilesGroupBox.TabIndex = 10;
            exportLogFilesGroupBox.TabStop = false;
            exportLogFilesGroupBox.Text = "Export Log Files";
            // 
            // exportLogFilesButton
            // 
            exportLogFilesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            exportLogFilesButton.Location = new System.Drawing.Point(209, 81);
            exportLogFilesButton.Name = "exportLogFilesButton";
            exportLogFilesButton.Size = new System.Drawing.Size(75, 23);
            exportLogFilesButton.TabIndex = 3;
            exportLogFilesButton.Text = "Export";
            exportLogFilesButton.UseVisualStyleBackColor = true;
            exportLogFilesButton.Click += ExportLogFilesButton_Click;
            // 
            // exportLogFilesLabel
            // 
            exportLogFilesLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            exportLogFilesLabel.Location = new System.Drawing.Point(6, 19);
            exportLogFilesLabel.Name = "exportLogFilesLabel";
            exportLogFilesLabel.Size = new System.Drawing.Size(278, 59);
            exportLogFilesLabel.TabIndex = 2;
            exportLogFilesLabel.Text = "Export all log files into a zip archive for further troubleshooting in either the Help Discussions or Community Discord.\r\n";
            // 
            // appNameLabel
            // 
            appNameLabel.AutoSize = true;
            appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            appNameLabel.Location = new System.Drawing.Point(465, 31);
            appNameLabel.Name = "appNameLabel";
            appNameLabel.Size = new System.Drawing.Size(184, 31);
            appNameLabel.TabIndex = 8;
            appNameLabel.Text = "SoundSwitch";
            // 
            // resetAudioDevicesGroupBox
            // 
            resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesLabel);
            resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesButton);
            resetAudioDevicesGroupBox.Location = new System.Drawing.Point(3, 3);
            resetAudioDevicesGroupBox.Name = "resetAudioDevicesGroupBox";
            resetAudioDevicesGroupBox.Size = new System.Drawing.Size(290, 120);
            resetAudioDevicesGroupBox.TabIndex = 1;
            resetAudioDevicesGroupBox.TabStop = false;
            resetAudioDevicesGroupBox.Text = "Reset Per App Audio";
            // 
            // resetAudioDevicesLabel
            // 
            resetAudioDevicesLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            resetAudioDevicesLabel.Location = new System.Drawing.Point(6, 19);
            resetAudioDevicesLabel.Name = "resetAudioDevicesLabel";
            resetAudioDevicesLabel.Size = new System.Drawing.Size(278, 69);
            resetAudioDevicesLabel.TabIndex = 2;
            resetAudioDevicesLabel.Text = "Resets the audio device assignment to specified programs in Windows in case audio devices are not reassigned to programs when switching devices.";
            // 
            // resetAudioDevicesButton
            // 
            resetAudioDevicesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            resetAudioDevicesButton.Location = new System.Drawing.Point(209, 91);
            resetAudioDevicesButton.Name = "resetAudioDevicesButton";
            resetAudioDevicesButton.Size = new System.Drawing.Size(75, 23);
            resetAudioDevicesButton.TabIndex = 0;
            resetAudioDevicesButton.Text = "Reset";
            resetAudioDevicesButton.UseVisualStyleBackColor = true;
            resetAudioDevicesButton.Click += ResetAudioDevicesButton_Click;
            // 
            // donateLinkLabel
            // 
            donateLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            donateLinkLabel.AutoSize = true;
            donateLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            donateLinkLabel.Location = new System.Drawing.Point(480, 318);
            donateLinkLabel.Name = "donateLinkLabel";
            donateLinkLabel.Size = new System.Drawing.Size(60, 21);
            donateLinkLabel.TabIndex = 2;
            donateLinkLabel.TabStop = true;
            donateLinkLabel.Text = "Donate";
            donateLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            donateLinkLabel.LinkClicked += DonateLinkLabel_LinkClicked;
            // 
            // discordPictureBox
            // 
            discordPictureBox.Image = Properties.Resources.DiscordMarkBlue32;
            discordPictureBox.Location = new System.Drawing.Point(442, 275);
            discordPictureBox.Name = "discordPictureBox";
            discordPictureBox.Size = new System.Drawing.Size(32, 32);
            discordPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            discordPictureBox.TabIndex = 4;
            discordPictureBox.TabStop = false;
            // 
            // donatePictureBox
            // 
            donatePictureBox.Image = Properties.Resources.Heart32;
            donatePictureBox.Location = new System.Drawing.Point(442, 313);
            donatePictureBox.Name = "donatePictureBox";
            donatePictureBox.Size = new System.Drawing.Size(32, 32);
            donatePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            donatePictureBox.TabIndex = 5;
            donatePictureBox.TabStop = false;
            // 
            // gitHubPictureBox
            // 
            gitHubPictureBox.Image = Properties.Resources.GitHubMark32;
            gitHubPictureBox.Location = new System.Drawing.Point(442, 237);
            gitHubPictureBox.Name = "gitHubPictureBox";
            gitHubPictureBox.Size = new System.Drawing.Size(32, 32);
            gitHubPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            gitHubPictureBox.TabIndex = 3;
            gitHubPictureBox.TabStop = false;
            // 
            // selectSoundFileDialog
            // 
            selectSoundFileDialog.FileName = "customSound";
            // 
            // hotkeysCheckBox
            // 
            hotkeysCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hotkeysCheckBox.AutoSize = true;
            hotkeysCheckBox.Location = new System.Drawing.Point(163, 441);
            hotkeysCheckBox.Name = "hotkeysCheckBox";
            hotkeysCheckBox.Size = new System.Drawing.Size(100, 19);
            hotkeysCheckBox.TabIndex = 20;
            hotkeysCheckBox.Text = "Enable hotkey";
            hotkeysCheckBox.UseVisualStyleBackColor = true;
            hotkeysCheckBox.CheckedChanged += HotkeysCheckbox_CheckedChanged;
            // 
            // hotKeyControl
            // 
            hotKeyControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hotKeyControl.ListenToHotkey = false;
            hotKeyControl.Location = new System.Drawing.Point(19, 438);
            hotKeyControl.Name = "hotKeyControl";
            hotKeyControl.Size = new System.Drawing.Size(138, 23);
            hotKeyControl.TabIndex = 21;
            hotKeyControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            hotKeyControl.HotKeyChanged += HotKeyControl_HotKeyChanged;
            // 
            // toggleMuteLabel
            // 
            toggleMuteLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            toggleMuteLabel.AutoSize = true;
            toggleMuteLabel.Location = new System.Drawing.Point(441, 419);
            toggleMuteLabel.Name = "toggleMuteLabel";
            toggleMuteLabel.Size = new System.Drawing.Size(73, 15);
            toggleMuteLabel.TabIndex = 22;
            toggleMuteLabel.Text = "Toggle mute";
            toggleMuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // muteHotKey
            // 
            muteHotKey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            muteHotKey.ListenToHotkey = false;
            muteHotKey.Location = new System.Drawing.Point(441, 438);
            muteHotKey.Name = "muteHotKey";
            muteHotKey.Size = new System.Drawing.Size(138, 23);
            muteHotKey.TabIndex = 24;
            muteHotKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            muteHotKey.HotKeyChanged += HotKeyControl_HotKeyChanged;
            // 
            // muteHotKeyCheckbox
            // 
            muteHotKeyCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            muteHotKeyCheckbox.AutoSize = true;
            muteHotKeyCheckbox.Location = new System.Drawing.Point(585, 441);
            muteHotKeyCheckbox.Name = "muteHotKeyCheckbox";
            muteHotKeyCheckbox.Size = new System.Drawing.Size(100, 19);
            muteHotKeyCheckbox.TabIndex = 23;
            muteHotKeyCheckbox.Text = "Enable hotkey";
            muteHotKeyCheckbox.UseVisualStyleBackColor = true;
            muteHotKeyCheckbox.CheckedChanged += MuteHotKeyCheckbox_CheckedChanged;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(784, 473);
            Controls.Add(muteHotKey);
            Controls.Add(muteHotKeyCheckbox);
            Controls.Add(toggleMuteLabel);
            Controls.Add(hotKeyControl);
            Controls.Add(hotkeysCheckBox);
            Controls.Add(tabControl);
            Controls.Add(closeButton);
            MinimumSize = new System.Drawing.Size(800, 512);
            Name = "SettingsForm";
            Text = "Settings";
            tabControl.ResumeLayout(false);
            playbackTabPage.ResumeLayout(false);
            recordingTabPage.ResumeLayout(false);
            profileTabPage.ResumeLayout(false);
            profileTabPage.PerformLayout();
            appSettingTabPage.ResumeLayout(false);
            appSettingTabPage.PerformLayout();
            troubleshootingTabPage.ResumeLayout(false);
            troubleshootingTabPage.PerformLayout();
            notificationGroupBox.ResumeLayout(false);
            notificationGroupBox.PerformLayout();
            languageGroupBox.ResumeLayout(false);
            updateSettingsGroupBox.ResumeLayout(false);
            updateSettingsGroupBox.PerformLayout();
            audioSettingsGroupBox.ResumeLayout(false);
            audioSettingsGroupBox.PerformLayout();
            basicSettingsGroupBox.ResumeLayout(false);
            basicSettingsGroupBox.PerformLayout();
            exportLogFilesGroupBox.ResumeLayout(false);
            resetAudioDevicesGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)discordPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)donatePictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)gitHubPictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button addProfileButton;
        private System.Windows.Forms.TabPage appSettingTabPage;
        private System.Windows.Forms.GroupBox audioSettingsGroupBox;
        private System.Windows.Forms.GroupBox basicSettingsGroupBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ComboBox cycleThroughComboBox;
        private System.Windows.Forms.Label cycleThroughLabel;
        private System.Windows.Forms.Button deleteProfileButton;
        private System.Windows.Forms.Button deleteSoundButton;
        private System.Windows.Forms.Button editProfileButton;
        private System.Windows.Forms.CheckBox foregroundAppCheckbox;
        private SoundSwitch.UI.Component.HotKeyTextBox hotKeyControl;
        private System.Windows.Forms.CheckBox hotkeysCheckBox;
        private System.Windows.Forms.ComboBox iconChangeChoicesComboBox;
        private System.Windows.Forms.Label iconChangeLabel;
        private System.Windows.Forms.CheckBox includeBetaVersionsCheckBox;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.GroupBox languageGroupBox;
        private System.Windows.Forms.ComboBox notificationComboBox;
        private System.Windows.Forms.Label positionLabel;
        private SoundSwitch.UI.Component.ListView.ListViewExtended playbackListView;
        private System.Windows.Forms.TabPage playbackTabPage;
        private System.Windows.Forms.Label profileExplanationLabel;
        private SoundSwitch.UI.Component.ListView.IconListView profilesListView;
        private SoundSwitch.UI.Component.ListView.ListViewExtended recordingListView;
        private System.Windows.Forms.TabPage recordingTabPage;
        private System.Windows.Forms.Button selectSoundButton;
        private System.Windows.Forms.OpenFileDialog selectSoundFileDialog;
        private System.Windows.Forms.CheckBox startWithWindowsCheckBox;
        private System.Windows.Forms.CheckBox switchCommunicationDeviceCheckBox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage profileTabPage;
        private System.Windows.Forms.ComboBox tooltipInfoComboBox;
        private System.Windows.Forms.Label tooltipOnHoverLabel;
        private System.Windows.Forms.RadioButton updateNeverRadioButton;
        private System.Windows.Forms.RadioButton updateNotifyRadioButton;
        private System.Windows.Forms.GroupBox updateSettingsGroupBox;
        private System.Windows.Forms.RadioButton updateSilentRadioButton;
        private System.Windows.Forms.CheckBox usePrimaryScreenCheckbox;
        private System.Windows.Forms.Label toggleMuteLabel;
        private Component.HotKeyTextBox muteHotKey;
        private System.Windows.Forms.CheckBox muteHotKeyCheckbox;
        private System.Windows.Forms.CheckBox telemetryCheckbox;
        private System.Windows.Forms.CheckBox quickMenuCheckbox;
        private System.Windows.Forms.CheckBox keepVolumeCheckbox;
        private System.Windows.Forms.GroupBox notificationGroupBox;
        private System.Windows.Forms.ComboBox positionComboBox;
        private System.Windows.Forms.CheckBox singleNotificationCheckbox;
        private System.Windows.Forms.TabPage troubleshootingTabPage;
        private System.Windows.Forms.GroupBox resetAudioDevicesGroupBox;
        private System.Windows.Forms.Button resetAudioDevicesButton;
        private System.Windows.Forms.Label resetAudioDevicesLabel;
        private System.Windows.Forms.GroupBox exportLogFilesGroupBox;
        private System.Windows.Forms.Label exportLogFilesLabel;
        private System.Windows.Forms.Button exportLogFilesButton;
        private System.Windows.Forms.PictureBox soundSwitchPictureBox;
        private System.Windows.Forms.LinkLabel discordCommunityLinkLabel;
        private System.Windows.Forms.LinkLabel gitHubHelpLinkLabel;
        private System.Windows.Forms.Label appNameLabel;
        private System.Windows.Forms.LinkLabel donateLinkLabel;
        private System.Windows.Forms.PictureBox donatePictureBox;
        private System.Windows.Forms.PictureBox gitHubPictureBox;
        private System.Windows.Forms.PictureBox discordPictureBox;
        private System.Windows.Forms.Label troubleshootingLabel;

        #endregion

    }
}