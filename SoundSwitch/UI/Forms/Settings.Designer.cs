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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Selected", System.Windows.Forms.HorizontalAlignment.Center);
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
            notificationsGroupBox = new System.Windows.Forms.GroupBox();
            bannerGroupBox = new System.Windows.Forms.GroupBox();
            positionLabel = new System.Windows.Forms.Label();
            secondsLabel = new System.Windows.Forms.Label();
            usePrimaryScreenCheckbox = new System.Windows.Forms.CheckBox();
            microphoneMuteLabel = new System.Windows.Forms.Label();
            positionComboBox = new System.Windows.Forms.ComboBox();
            microphoneMuteComboBox = new System.Windows.Forms.ComboBox();
            singleNotificationCheckbox = new System.Windows.Forms.CheckBox();
            onScreenTimeLabel = new System.Windows.Forms.Label();
            onScreenUpDown = new System.Windows.Forms.NumericUpDown();
            notificationComboBox = new System.Windows.Forms.ComboBox();
            selectSoundButton = new System.Windows.Forms.Button();
            deleteSoundButton = new System.Windows.Forms.Button();
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
            iconDoubleClickLabel = new System.Windows.Forms.Label();
            iconDoubleClickComboBox = new System.Windows.Forms.ComboBox();
            iconChangeLabel = new System.Windows.Forms.Label();
            iconChangeChoicesComboBox = new System.Windows.Forms.ComboBox();
            troubleshootingTabPage = new System.Windows.Forms.TabPage();
            exportConfigFileGroupBox = new System.Windows.Forms.GroupBox();
            exportConfigFileButton = new System.Windows.Forms.Button();
            exportConfigFileLabel = new System.Windows.Forms.Label();
            exportLogFilesGroupBox = new System.Windows.Forms.GroupBox();
            exportLogFilesButton = new System.Windows.Forms.Button();
            exportLogFilesLabel = new System.Windows.Forms.Label();
            resetAudioDevicesGroupBox = new System.Windows.Forms.GroupBox();
            resetAudioDevicesLabel = new System.Windows.Forms.Label();
            resetAudioDevicesButton = new System.Windows.Forms.Button();
            soundSwitchGroupBox = new System.Windows.Forms.GroupBox();
            troubleshootingLabel = new System.Windows.Forms.Label();
            soundSwitchPictureBox = new System.Windows.Forms.PictureBox();
            discordPictureBox = new System.Windows.Forms.PictureBox();
            donateLinkLabel = new System.Windows.Forms.LinkLabel();
            gitHubPictureBox = new System.Windows.Forms.PictureBox();
            gitHubHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            appNameLabel = new System.Windows.Forms.Label();
            discordCommunityLinkLabel = new System.Windows.Forms.LinkLabel();
            donatePictureBox = new System.Windows.Forms.PictureBox();
            selectSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
            hotkeysCheckBox = new System.Windows.Forms.CheckBox();
            hotKeyControl = new SoundSwitch.UI.Component.HotKeyTextBox();
            toggleMuteLabel = new System.Windows.Forms.Label();
            muteHotKey = new SoundSwitch.UI.Component.HotKeyTextBox();
            muteHotKeyCheckbox = new System.Windows.Forms.CheckBox();
            switchDeviceLabel = new System.Windows.Forms.Label();
            tabControl.SuspendLayout();
            playbackTabPage.SuspendLayout();
            recordingTabPage.SuspendLayout();
            profileTabPage.SuspendLayout();
            appSettingTabPage.SuspendLayout();
            notificationsGroupBox.SuspendLayout();
            bannerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)onScreenUpDown).BeginInit();
            languageGroupBox.SuspendLayout();
            updateSettingsGroupBox.SuspendLayout();
            audioSettingsGroupBox.SuspendLayout();
            basicSettingsGroupBox.SuspendLayout();
            troubleshootingTabPage.SuspendLayout();
            exportConfigFileGroupBox.SuspendLayout();
            exportLogFilesGroupBox.SuspendLayout();
            resetAudioDevicesGroupBox.SuspendLayout();
            soundSwitchGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)discordPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gitHubPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)donatePictureBox).BeginInit();
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
            closeButton.Location = new System.Drawing.Point(698, 517);
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
            tabControl.Size = new System.Drawing.Size(757, 485);
            tabControl.TabIndex = 13;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // playbackTabPage
            // 
            playbackTabPage.Controls.Add(playbackListView);
            playbackTabPage.Location = new System.Drawing.Point(4, 24);
            playbackTabPage.Name = "playbackTabPage";
            playbackTabPage.Padding = new System.Windows.Forms.Padding(3);
            playbackTabPage.Size = new System.Drawing.Size(749, 457);
            playbackTabPage.TabIndex = 0;
            playbackTabPage.Text = "Playback";
            playbackTabPage.UseVisualStyleBackColor = true;
            // 
            // playbackListView
            // 
            playbackListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            playbackListView.CheckBoxes = true;
            playbackListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Selected";
            listViewGroup1.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup1.Name = "selectedGroup";
            playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] { listViewGroup1 });
            playbackListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            playbackListView.Location = new System.Drawing.Point(3, 3);
            playbackListView.Name = "playbackListView";
            playbackListView.Size = new System.Drawing.Size(743, 451);
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
            recordingTabPage.Size = new System.Drawing.Size(749, 457);
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
            listViewGroup2.Header = "Selected";
            listViewGroup2.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup2.Name = "selectedGroup";
            recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] { listViewGroup2 });
            recordingListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            recordingListView.Location = new System.Drawing.Point(3, 3);
            recordingListView.Name = "recordingListView";
            recordingListView.Size = new System.Drawing.Size(743, 451);
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
            profileTabPage.Size = new System.Drawing.Size(749, 457);
            profileTabPage.TabIndex = 3;
            profileTabPage.Text = "Profiles";
            profileTabPage.UseVisualStyleBackColor = true;
            // 
            // editProfileButton
            // 
            editProfileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            editProfileButton.Enabled = false;
            editProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            editProfileButton.Location = new System.Drawing.Point(537, 425);
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
            deleteProfileButton.Location = new System.Drawing.Point(643, 425);
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
            profileExplanationLabel.Location = new System.Drawing.Point(6, 338);
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
            profilesListView.Size = new System.Drawing.Size(740, 328);
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
            addProfileButton.Location = new System.Drawing.Point(431, 425);
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
            appSettingTabPage.Controls.Add(notificationsGroupBox);
            appSettingTabPage.Controls.Add(languageGroupBox);
            appSettingTabPage.Controls.Add(updateSettingsGroupBox);
            appSettingTabPage.Controls.Add(audioSettingsGroupBox);
            appSettingTabPage.Controls.Add(basicSettingsGroupBox);
            appSettingTabPage.Location = new System.Drawing.Point(4, 24);
            appSettingTabPage.Name = "appSettingTabPage";
            appSettingTabPage.Size = new System.Drawing.Size(749, 457);
            appSettingTabPage.TabIndex = 2;
            appSettingTabPage.Text = "Settings";
            appSettingTabPage.UseVisualStyleBackColor = true;
            // 
            // notificationsGroupBox
            // 
            notificationsGroupBox.Controls.Add(bannerGroupBox);
            notificationsGroupBox.Controls.Add(notificationComboBox);
            notificationsGroupBox.Controls.Add(selectSoundButton);
            notificationsGroupBox.Controls.Add(deleteSoundButton);
            notificationsGroupBox.Location = new System.Drawing.Point(396, 232);
            notificationsGroupBox.Name = "notificationsGroupBox";
            notificationsGroupBox.Size = new System.Drawing.Size(347, 219);
            notificationsGroupBox.TabIndex = 16;
            notificationsGroupBox.TabStop = false;
            notificationsGroupBox.Text = "Notifications";
            // 
            // bannerGroupBox
            // 
            bannerGroupBox.Controls.Add(positionLabel);
            bannerGroupBox.Controls.Add(secondsLabel);
            bannerGroupBox.Controls.Add(usePrimaryScreenCheckbox);
            bannerGroupBox.Controls.Add(microphoneMuteLabel);
            bannerGroupBox.Controls.Add(positionComboBox);
            bannerGroupBox.Controls.Add(microphoneMuteComboBox);
            bannerGroupBox.Controls.Add(singleNotificationCheckbox);
            bannerGroupBox.Controls.Add(onScreenTimeLabel);
            bannerGroupBox.Controls.Add(onScreenUpDown);
            bannerGroupBox.Location = new System.Drawing.Point(6, 51);
            bannerGroupBox.Name = "bannerGroupBox";
            bannerGroupBox.Size = new System.Drawing.Size(335, 162);
            bannerGroupBox.TabIndex = 34;
            bannerGroupBox.TabStop = false;
            bannerGroupBox.Text = "Banner Options";
            // 
            // positionLabel
            // 
            positionLabel.Location = new System.Drawing.Point(6, 19);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new System.Drawing.Size(155, 23);
            positionLabel.TabIndex = 17;
            positionLabel.Text = "Position";
            positionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // secondsLabel
            // 
            secondsLabel.Location = new System.Drawing.Point(212, 48);
            secondsLabel.Margin = new System.Windows.Forms.Padding(3);
            secondsLabel.Name = "secondsLabel";
            secondsLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            secondsLabel.Size = new System.Drawing.Size(85, 23);
            secondsLabel.TabIndex = 33;
            secondsLabel.Text = "seconds";
            secondsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // usePrimaryScreenCheckbox
            // 
            usePrimaryScreenCheckbox.AutoSize = true;
            usePrimaryScreenCheckbox.Location = new System.Drawing.Point(6, 106);
            usePrimaryScreenCheckbox.Name = "usePrimaryScreenCheckbox";
            usePrimaryScreenCheckbox.Size = new System.Drawing.Size(165, 19);
            usePrimaryScreenCheckbox.TabIndex = 26;
            usePrimaryScreenCheckbox.Text = "Always use primary screen";
            usePrimaryScreenCheckbox.UseVisualStyleBackColor = true;
            usePrimaryScreenCheckbox.CheckedChanged += UsePrimaryScreenCheckbox_CheckedChanged;
            // 
            // microphoneMuteLabel
            // 
            microphoneMuteLabel.Location = new System.Drawing.Point(6, 77);
            microphoneMuteLabel.Name = "microphoneMuteLabel";
            microphoneMuteLabel.Size = new System.Drawing.Size(155, 23);
            microphoneMuteLabel.TabIndex = 32;
            microphoneMuteLabel.Text = "Microphone Muted";
            microphoneMuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // positionComboBox
            // 
            positionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            positionComboBox.FormattingEnabled = true;
            positionComboBox.Location = new System.Drawing.Point(167, 19);
            positionComboBox.Name = "positionComboBox";
            positionComboBox.Size = new System.Drawing.Size(130, 23);
            positionComboBox.TabIndex = 17;
            positionComboBox.SelectedValueChanged += PositionComboBox_SelectedValueChanged;
            // 
            // microphoneMuteComboBox
            // 
            microphoneMuteComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            microphoneMuteComboBox.FormattingEnabled = true;
            microphoneMuteComboBox.Location = new System.Drawing.Point(167, 77);
            microphoneMuteComboBox.Name = "microphoneMuteComboBox";
            microphoneMuteComboBox.Size = new System.Drawing.Size(130, 23);
            microphoneMuteComboBox.TabIndex = 31;
            microphoneMuteComboBox.SelectedValueChanged += MicrophoneMuteNotificationComboBox_SelectedValueChanged;
            // 
            // singleNotificationCheckbox
            // 
            singleNotificationCheckbox.AutoSize = true;
            singleNotificationCheckbox.Location = new System.Drawing.Point(6, 131);
            singleNotificationCheckbox.Name = "singleNotificationCheckbox";
            singleNotificationCheckbox.Size = new System.Drawing.Size(124, 19);
            singleNotificationCheckbox.TabIndex = 27;
            singleNotificationCheckbox.Text = "Single Notification";
            singleNotificationCheckbox.UseVisualStyleBackColor = true;
            // 
            // onScreenTimeLabel
            // 
            onScreenTimeLabel.Location = new System.Drawing.Point(6, 48);
            onScreenTimeLabel.Margin = new System.Windows.Forms.Padding(3);
            onScreenTimeLabel.Name = "onScreenTimeLabel";
            onScreenTimeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            onScreenTimeLabel.Size = new System.Drawing.Size(155, 23);
            onScreenTimeLabel.TabIndex = 29;
            onScreenTimeLabel.Text = "On-screen time";
            onScreenTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // onScreenUpDown
            // 
            onScreenUpDown.Location = new System.Drawing.Point(167, 48);
            onScreenUpDown.Name = "onScreenUpDown";
            onScreenUpDown.Size = new System.Drawing.Size(39, 23);
            onScreenUpDown.TabIndex = 28;
            onScreenUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
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
            selectSoundButton.Location = new System.Drawing.Point(249, 22);
            selectSoundButton.Name = "selectSoundButton";
            selectSoundButton.Size = new System.Drawing.Size(24, 24);
            selectSoundButton.TabIndex = 19;
            selectSoundButton.Text = "...";
            selectSoundButton.UseVisualStyleBackColor = true;
            selectSoundButton.Visible = false;
            selectSoundButton.Click += SelectSoundButton_Click;
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
            // languageGroupBox
            // 
            languageGroupBox.Controls.Add(languageComboBox);
            languageGroupBox.Location = new System.Drawing.Point(396, 165);
            languageGroupBox.Name = "languageGroupBox";
            languageGroupBox.Size = new System.Drawing.Size(347, 61);
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
            updateSettingsGroupBox.Location = new System.Drawing.Point(396, 3);
            updateSettingsGroupBox.Name = "updateSettingsGroupBox";
            updateSettingsGroupBox.Size = new System.Drawing.Size(347, 156);
            updateSettingsGroupBox.TabIndex = 14;
            updateSettingsGroupBox.TabStop = false;
            updateSettingsGroupBox.Text = "Update Settings";
            // 
            // telemetryCheckbox
            // 
            telemetryCheckbox.AutoSize = true;
            telemetryCheckbox.Location = new System.Drawing.Point(6, 130);
            telemetryCheckbox.Name = "telemetryCheckbox";
            telemetryCheckbox.Size = new System.Drawing.Size(77, 19);
            telemetryCheckbox.TabIndex = 22;
            telemetryCheckbox.Text = "Telemetry";
            telemetryCheckbox.UseVisualStyleBackColor = true;
            // 
            // updateNeverRadioButton
            // 
            updateNeverRadioButton.AutoSize = true;
            updateNeverRadioButton.Location = new System.Drawing.Point(6, 71);
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
            updateNotifyRadioButton.Location = new System.Drawing.Point(6, 46);
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
            updateSilentRadioButton.Location = new System.Drawing.Point(6, 21);
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
            includeBetaVersionsCheckBox.Location = new System.Drawing.Point(6, 103);
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
            audioSettingsGroupBox.Location = new System.Drawing.Point(3, 165);
            audioSettingsGroupBox.Name = "audioSettingsGroupBox";
            audioSettingsGroupBox.Size = new System.Drawing.Size(387, 286);
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
            cycleThroughLabel.Size = new System.Drawing.Size(155, 23);
            cycleThroughLabel.TabIndex = 23;
            cycleThroughLabel.Text = "Cycle through";
            cycleThroughLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cycleThroughComboBox
            // 
            cycleThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cycleThroughComboBox.FormattingEnabled = true;
            cycleThroughComboBox.Location = new System.Drawing.Point(167, 190);
            cycleThroughComboBox.Name = "cycleThroughComboBox";
            cycleThroughComboBox.Size = new System.Drawing.Size(182, 23);
            cycleThroughComboBox.TabIndex = 22;
            cycleThroughComboBox.SelectedValueChanged += CyclerComboBox_SelectedValueChanged;
            // 
            // tooltipOnHoverLabel
            // 
            tooltipOnHoverLabel.Location = new System.Drawing.Point(6, 154);
            tooltipOnHoverLabel.Name = "tooltipOnHoverLabel";
            tooltipOnHoverLabel.Size = new System.Drawing.Size(155, 23);
            tooltipOnHoverLabel.TabIndex = 21;
            tooltipOnHoverLabel.Text = "Tooltip on Hover";
            tooltipOnHoverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tooltipInfoComboBox
            // 
            tooltipInfoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tooltipInfoComboBox.FormattingEnabled = true;
            tooltipInfoComboBox.Location = new System.Drawing.Point(167, 154);
            tooltipInfoComboBox.Name = "tooltipInfoComboBox";
            tooltipInfoComboBox.Size = new System.Drawing.Size(182, 23);
            tooltipInfoComboBox.TabIndex = 20;
            tooltipInfoComboBox.SelectedValueChanged += TooltipInfoComboBox_SelectedValueChanged;
            // 
            // basicSettingsGroupBox
            // 
            basicSettingsGroupBox.Controls.Add(iconDoubleClickLabel);
            basicSettingsGroupBox.Controls.Add(iconDoubleClickComboBox);
            basicSettingsGroupBox.Controls.Add(iconChangeLabel);
            basicSettingsGroupBox.Controls.Add(startWithWindowsCheckBox);
            basicSettingsGroupBox.Controls.Add(iconChangeChoicesComboBox);
            basicSettingsGroupBox.Location = new System.Drawing.Point(3, 3);
            basicSettingsGroupBox.Name = "basicSettingsGroupBox";
            basicSettingsGroupBox.Size = new System.Drawing.Size(387, 156);
            basicSettingsGroupBox.TabIndex = 0;
            basicSettingsGroupBox.TabStop = false;
            basicSettingsGroupBox.Text = "Basic Settings";
            // 
            // iconDoubleClickLabel
            // 
            iconDoubleClickLabel.Location = new System.Drawing.Point(6, 80);
            iconDoubleClickLabel.Name = "iconDoubleClickLabel";
            iconDoubleClickLabel.Size = new System.Drawing.Size(155, 23);
            iconDoubleClickLabel.TabIndex = 29;
            iconDoubleClickLabel.Text = "Double Click Action";
            iconDoubleClickLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // iconDoubleClickComboBox
            // 
            iconDoubleClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            iconDoubleClickComboBox.FormattingEnabled = true;
            iconDoubleClickComboBox.Location = new System.Drawing.Point(167, 80);
            iconDoubleClickComboBox.Name = "iconDoubleClickComboBox";
            iconDoubleClickComboBox.Size = new System.Drawing.Size(182, 23);
            iconDoubleClickComboBox.TabIndex = 28;
            iconDoubleClickComboBox.SelectedIndexChanged += IconDoubleClickComboBox_SelectedIndexChanged;
            // 
            // iconChangeLabel
            // 
            iconChangeLabel.Location = new System.Drawing.Point(6, 51);
            iconChangeLabel.Name = "iconChangeLabel";
            iconChangeLabel.Size = new System.Drawing.Size(155, 23);
            iconChangeLabel.TabIndex = 27;
            iconChangeLabel.Text = "Systray Icon";
            iconChangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // iconChangeChoicesComboBox
            // 
            iconChangeChoicesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            iconChangeChoicesComboBox.FormattingEnabled = true;
            iconChangeChoicesComboBox.Location = new System.Drawing.Point(167, 51);
            iconChangeChoicesComboBox.Name = "iconChangeChoicesComboBox";
            iconChangeChoicesComboBox.Size = new System.Drawing.Size(182, 23);
            iconChangeChoicesComboBox.TabIndex = 26;
            iconChangeChoicesComboBox.SelectedIndexChanged += IconChangeChoicesComboBox_SelectedIndexChanged;
            // 
            // troubleshootingTabPage
            // 
            troubleshootingTabPage.Controls.Add(exportConfigFileGroupBox);
            troubleshootingTabPage.Controls.Add(exportLogFilesGroupBox);
            troubleshootingTabPage.Controls.Add(resetAudioDevicesGroupBox);
            troubleshootingTabPage.Controls.Add(soundSwitchGroupBox);
            troubleshootingTabPage.Location = new System.Drawing.Point(4, 24);
            troubleshootingTabPage.Name = "troubleshootingTabPage";
            troubleshootingTabPage.Padding = new System.Windows.Forms.Padding(3);
            troubleshootingTabPage.Size = new System.Drawing.Size(749, 457);
            troubleshootingTabPage.TabIndex = 4;
            troubleshootingTabPage.Text = "Troubleshooting";
            troubleshootingTabPage.UseVisualStyleBackColor = true;
            // 
            // exportConfigFileGroupBox
            // 
            exportConfigFileGroupBox.Controls.Add(exportConfigFileButton);
            exportConfigFileGroupBox.Controls.Add(exportConfigFileLabel);
            exportConfigFileGroupBox.Location = new System.Drawing.Point(3, 305);
            exportConfigFileGroupBox.Name = "exportConfigFileGroupBox";
            exportConfigFileGroupBox.Size = new System.Drawing.Size(290, 146);
            exportConfigFileGroupBox.TabIndex = 13;
            exportConfigFileGroupBox.TabStop = false;
            exportConfigFileGroupBox.Text = "Export Configuration File";
            // 
            // exportConfigFileButton
            // 
            exportConfigFileButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            exportConfigFileButton.Location = new System.Drawing.Point(209, 117);
            exportConfigFileButton.Name = "exportConfigFileButton";
            exportConfigFileButton.Size = new System.Drawing.Size(75, 23);
            exportConfigFileButton.TabIndex = 4;
            exportConfigFileButton.Text = "Export";
            exportConfigFileButton.UseVisualStyleBackColor = true;
            exportConfigFileButton.Click += ExportConfigFileButton_Click;
            // 
            // exportConfigFileLabel
            // 
            exportConfigFileLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            exportConfigFileLabel.Location = new System.Drawing.Point(6, 19);
            exportConfigFileLabel.Name = "exportConfigFileLabel";
            exportConfigFileLabel.Size = new System.Drawing.Size(278, 95);
            exportConfigFileLabel.TabIndex = 4;
            exportConfigFileLabel.Text = "description";
            // 
            // exportLogFilesGroupBox
            // 
            exportLogFilesGroupBox.Controls.Add(exportLogFilesButton);
            exportLogFilesGroupBox.Controls.Add(exportLogFilesLabel);
            exportLogFilesGroupBox.Location = new System.Drawing.Point(3, 154);
            exportLogFilesGroupBox.Name = "exportLogFilesGroupBox";
            exportLogFilesGroupBox.Size = new System.Drawing.Size(290, 145);
            exportLogFilesGroupBox.TabIndex = 10;
            exportLogFilesGroupBox.TabStop = false;
            exportLogFilesGroupBox.Text = "Export Log Files";
            // 
            // exportLogFilesButton
            // 
            exportLogFilesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            exportLogFilesButton.Location = new System.Drawing.Point(209, 116);
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
            exportLogFilesLabel.Size = new System.Drawing.Size(278, 94);
            exportLogFilesLabel.TabIndex = 2;
            exportLogFilesLabel.Text = "description";
            // 
            // resetAudioDevicesGroupBox
            // 
            resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesLabel);
            resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesButton);
            resetAudioDevicesGroupBox.Location = new System.Drawing.Point(3, 3);
            resetAudioDevicesGroupBox.Name = "resetAudioDevicesGroupBox";
            resetAudioDevicesGroupBox.Size = new System.Drawing.Size(290, 145);
            resetAudioDevicesGroupBox.TabIndex = 1;
            resetAudioDevicesGroupBox.TabStop = false;
            resetAudioDevicesGroupBox.Text = "Reset Per App Audio";
            // 
            // resetAudioDevicesLabel
            // 
            resetAudioDevicesLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            resetAudioDevicesLabel.Location = new System.Drawing.Point(6, 19);
            resetAudioDevicesLabel.Name = "resetAudioDevicesLabel";
            resetAudioDevicesLabel.Size = new System.Drawing.Size(278, 94);
            resetAudioDevicesLabel.TabIndex = 2;
            resetAudioDevicesLabel.Text = "description";
            // 
            // resetAudioDevicesButton
            // 
            resetAudioDevicesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            resetAudioDevicesButton.Location = new System.Drawing.Point(209, 116);
            resetAudioDevicesButton.Name = "resetAudioDevicesButton";
            resetAudioDevicesButton.Size = new System.Drawing.Size(75, 23);
            resetAudioDevicesButton.TabIndex = 0;
            resetAudioDevicesButton.Text = "Reset";
            resetAudioDevicesButton.UseVisualStyleBackColor = true;
            resetAudioDevicesButton.Click += ResetAudioDevicesButton_Click;
            // 
            // soundSwitchGroupBox
            // 
            soundSwitchGroupBox.Controls.Add(troubleshootingLabel);
            soundSwitchGroupBox.Controls.Add(soundSwitchPictureBox);
            soundSwitchGroupBox.Controls.Add(discordPictureBox);
            soundSwitchGroupBox.Controls.Add(donateLinkLabel);
            soundSwitchGroupBox.Controls.Add(gitHubPictureBox);
            soundSwitchGroupBox.Controls.Add(gitHubHelpLinkLabel);
            soundSwitchGroupBox.Controls.Add(appNameLabel);
            soundSwitchGroupBox.Controls.Add(discordCommunityLinkLabel);
            soundSwitchGroupBox.Controls.Add(donatePictureBox);
            soundSwitchGroupBox.Location = new System.Drawing.Point(299, 3);
            soundSwitchGroupBox.Name = "soundSwitchGroupBox";
            soundSwitchGroupBox.Size = new System.Drawing.Size(447, 448);
            soundSwitchGroupBox.TabIndex = 12;
            soundSwitchGroupBox.TabStop = false;
            // 
            // troubleshootingLabel
            // 
            troubleshootingLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            troubleshootingLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic);
            troubleshootingLabel.Location = new System.Drawing.Point(6, 94);
            troubleshootingLabel.Name = "troubleshootingLabel";
            troubleshootingLabel.Padding = new System.Windows.Forms.Padding(30);
            troubleshootingLabel.Size = new System.Drawing.Size(435, 175);
            troubleshootingLabel.TabIndex = 11;
            troubleshootingLabel.Text = "description";
            troubleshootingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // soundSwitchPictureBox
            // 
            soundSwitchPictureBox.Image = Properties.Resources.SoundSwitch48;
            soundSwitchPictureBox.Location = new System.Drawing.Point(102, 42);
            soundSwitchPictureBox.Name = "soundSwitchPictureBox";
            soundSwitchPictureBox.Size = new System.Drawing.Size(51, 49);
            soundSwitchPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            soundSwitchPictureBox.TabIndex = 9;
            soundSwitchPictureBox.TabStop = false;
            // 
            // discordPictureBox
            // 
            discordPictureBox.Image = Properties.Resources.DiscordMarkBlue32;
            discordPictureBox.Location = new System.Drawing.Point(126, 310);
            discordPictureBox.Name = "discordPictureBox";
            discordPictureBox.Size = new System.Drawing.Size(32, 32);
            discordPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            discordPictureBox.TabIndex = 4;
            discordPictureBox.TabStop = false;
            // 
            // donateLinkLabel
            // 
            donateLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            donateLinkLabel.AutoSize = true;
            donateLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            donateLinkLabel.Location = new System.Drawing.Point(164, 348);
            donateLinkLabel.Name = "donateLinkLabel";
            donateLinkLabel.Size = new System.Drawing.Size(60, 21);
            donateLinkLabel.TabIndex = 2;
            donateLinkLabel.TabStop = true;
            donateLinkLabel.Text = "Donate";
            donateLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            donateLinkLabel.LinkClicked += DonateLink;
            // 
            // gitHubPictureBox
            // 
            gitHubPictureBox.Image = Properties.Resources.GitHubMark32;
            gitHubPictureBox.Location = new System.Drawing.Point(126, 272);
            gitHubPictureBox.Name = "gitHubPictureBox";
            gitHubPictureBox.Size = new System.Drawing.Size(32, 32);
            gitHubPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            gitHubPictureBox.TabIndex = 3;
            gitHubPictureBox.TabStop = false;
            // 
            // gitHubHelpLinkLabel
            // 
            gitHubHelpLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            gitHubHelpLinkLabel.AutoSize = true;
            gitHubHelpLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gitHubHelpLinkLabel.Location = new System.Drawing.Point(164, 272);
            gitHubHelpLinkLabel.Name = "gitHubHelpLinkLabel";
            gitHubHelpLinkLabel.Size = new System.Drawing.Size(120, 21);
            gitHubHelpLinkLabel.TabIndex = 0;
            gitHubHelpLinkLabel.TabStop = true;
            gitHubHelpLinkLabel.Text = "Help Discussion";
            gitHubHelpLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            gitHubHelpLinkLabel.LinkClicked += GitHubHelpLink;
            // 
            // appNameLabel
            // 
            appNameLabel.AutoSize = true;
            appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            appNameLabel.Location = new System.Drawing.Point(153, 50);
            appNameLabel.Name = "appNameLabel";
            appNameLabel.Size = new System.Drawing.Size(184, 31);
            appNameLabel.TabIndex = 8;
            appNameLabel.Text = "SoundSwitch";
            // 
            // discordCommunityLinkLabel
            // 
            discordCommunityLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            discordCommunityLinkLabel.AutoSize = true;
            discordCommunityLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            discordCommunityLinkLabel.Location = new System.Drawing.Point(164, 310);
            discordCommunityLinkLabel.Name = "discordCommunityLinkLabel";
            discordCommunityLinkLabel.Size = new System.Drawing.Size(149, 21);
            discordCommunityLinkLabel.TabIndex = 1;
            discordCommunityLinkLabel.TabStop = true;
            discordCommunityLinkLabel.Text = "Community Discord";
            discordCommunityLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            discordCommunityLinkLabel.LinkClicked += DiscordCommunityLink;
            // 
            // donatePictureBox
            // 
            donatePictureBox.Image = Properties.Resources.Heart32;
            donatePictureBox.Location = new System.Drawing.Point(126, 348);
            donatePictureBox.Name = "donatePictureBox";
            donatePictureBox.Size = new System.Drawing.Size(32, 32);
            donatePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            donatePictureBox.TabIndex = 5;
            donatePictureBox.TabStop = false;
            // 
            // selectSoundFileDialog
            // 
            selectSoundFileDialog.FileName = "customSound";
            // 
            // hotkeysCheckBox
            // 
            hotkeysCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hotkeysCheckBox.AutoSize = true;
            hotkeysCheckBox.Location = new System.Drawing.Point(163, 522);
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
            hotKeyControl.Location = new System.Drawing.Point(19, 519);
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
            toggleMuteLabel.Location = new System.Drawing.Point(441, 500);
            toggleMuteLabel.Name = "toggleMuteLabel";
            toggleMuteLabel.Size = new System.Drawing.Size(73, 15);
            toggleMuteLabel.TabIndex = 22;
            toggleMuteLabel.Text = "Toggle mute";
            toggleMuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // muteHotKey
            // 
            muteHotKey.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            muteHotKey.Location = new System.Drawing.Point(441, 519);
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
            muteHotKeyCheckbox.Location = new System.Drawing.Point(585, 522);
            muteHotKeyCheckbox.Name = "muteHotKeyCheckbox";
            muteHotKeyCheckbox.Size = new System.Drawing.Size(100, 19);
            muteHotKeyCheckbox.TabIndex = 23;
            muteHotKeyCheckbox.Text = "Enable hotkey";
            muteHotKeyCheckbox.UseVisualStyleBackColor = true;
            muteHotKeyCheckbox.CheckedChanged += MuteHotKeyCheckbox_CheckedChanged;
            // 
            // switchDeviceLabel
            // 
            switchDeviceLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            switchDeviceLabel.AutoSize = true;
            switchDeviceLabel.Location = new System.Drawing.Point(19, 500);
            switchDeviceLabel.Name = "switchDeviceLabel";
            switchDeviceLabel.Size = new System.Drawing.Size(79, 15);
            switchDeviceLabel.TabIndex = 25;
            switchDeviceLabel.Text = "Switch device";
            switchDeviceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(779, 554);
            Controls.Add(switchDeviceLabel);
            Controls.Add(muteHotKey);
            Controls.Add(muteHotKeyCheckbox);
            Controls.Add(toggleMuteLabel);
            Controls.Add(hotKeyControl);
            Controls.Add(hotkeysCheckBox);
            Controls.Add(tabControl);
            Controls.Add(closeButton);
            MinimumSize = new System.Drawing.Size(795, 560);
            Name = "SettingsForm";
            Text = "Settings";
            tabControl.ResumeLayout(false);
            playbackTabPage.ResumeLayout(false);
            recordingTabPage.ResumeLayout(false);
            profileTabPage.ResumeLayout(false);
            profileTabPage.PerformLayout();
            appSettingTabPage.ResumeLayout(false);
            notificationsGroupBox.ResumeLayout(false);
            bannerGroupBox.ResumeLayout(false);
            bannerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)onScreenUpDown).EndInit();
            languageGroupBox.ResumeLayout(false);
            updateSettingsGroupBox.ResumeLayout(false);
            updateSettingsGroupBox.PerformLayout();
            audioSettingsGroupBox.ResumeLayout(false);
            audioSettingsGroupBox.PerformLayout();
            basicSettingsGroupBox.ResumeLayout(false);
            basicSettingsGroupBox.PerformLayout();
            troubleshootingTabPage.ResumeLayout(false);
            exportConfigFileGroupBox.ResumeLayout(false);
            exportLogFilesGroupBox.ResumeLayout(false);
            resetAudioDevicesGroupBox.ResumeLayout(false);
            soundSwitchGroupBox.ResumeLayout(false);
            soundSwitchGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)soundSwitchPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)discordPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)gitHubPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)donatePictureBox).EndInit();
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
        private System.Windows.Forms.GroupBox notificationsGroupBox;
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

        private System.Windows.Forms.Label onScreenTimeLabel;
        private System.Windows.Forms.NumericUpDown onScreenUpDown;
        private System.Windows.Forms.Label switchDeviceLabel;
        private System.Windows.Forms.Label microphoneMuteLabel;
        private System.Windows.Forms.ComboBox microphoneMuteComboBox;
        private System.Windows.Forms.Label secondsLabel;
        private System.Windows.Forms.GroupBox bannerGroupBox;
        private System.Windows.Forms.GroupBox soundSwitchGroupBox;
        private System.Windows.Forms.ComboBox iconDoubleClickComboBox;
        private System.Windows.Forms.Label iconDoubleClickLabel;
        private System.Windows.Forms.GroupBox exportConfigFileGroupBox;
        private System.Windows.Forms.Button exportConfigFileButton;
        private System.Windows.Forms.Label exportConfigFileLabel;
    }
}