using SoundSwitch.Localization;
using SoundSwitch.UI.Component.ListView;
using SoundSwitch.Util.Url;
using System;

namespace SoundSwitch.UI.Forms;

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
        playbackListView = new SoundSwitch.UI.Component.ListView.ListViewExtended();
        recordingTabPage = new System.Windows.Forms.TabPage();
        recordingListView = new SoundSwitch.UI.Component.ListView.ListViewExtended();
        profileTabPage = new System.Windows.Forms.TabPage();
        profilesPanel = new System.Windows.Forms.Panel();
        profileExplanationLabel = new System.Windows.Forms.Label();
        editProfileButton = new System.Windows.Forms.Button();
        addProfileButton = new System.Windows.Forms.Button();
        deleteProfileButton = new System.Windows.Forms.Button();
        profilesListView = new SoundSwitch.UI.Component.ListView.IconListView();
        appSettingTabPage = new System.Windows.Forms.TabPage();
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
        notificationsTabPage = new System.Windows.Forms.TabPage();
        customSoundFileGroupBox = new System.Windows.Forms.GroupBox();
        deleteSoundButton = new System.Windows.Forms.Button();
        selectSoundButton = new System.Windows.Forms.Button();
        notificationsGroupBox = new System.Windows.Forms.GroupBox();
        switchProfileNotificationLabel = new System.Windows.Forms.Label();
        microphoneMuteNotificationLabel = new System.Windows.Forms.Label();
        switchProfileNotificationComboBox = new System.Windows.Forms.ComboBox();
        microphoneMuteNotificationComboBox = new System.Windows.Forms.ComboBox();
        advancedModeCheckBox = new System.Windows.Forms.CheckBox();
        switchDeviceNotificationLabel = new System.Windows.Forms.Label();
        switchDeviceNotificationComboBox = new System.Windows.Forms.ComboBox();
        bannerOptionsGroupBox = new System.Windows.Forms.GroupBox();
        microphoneMuteGroupBox = new System.Windows.Forms.GroupBox();
        microphoneMuteLabel = new System.Windows.Forms.Label();
        microphoneMuteBannerComboBox = new System.Windows.Forms.ComboBox();
        microphoneUnmuteLabel = new System.Windows.Forms.Label();
        microphoneUnmuteBannerComboBox = new System.Windows.Forms.ComboBox();
        bannerShowOnComboBox = new System.Windows.Forms.ComboBox();
        bannerShowOnLabel = new System.Windows.Forms.Label();
        onScreenTimeLabel = new System.Windows.Forms.Label();

        singleNotificationCheckbox = new System.Windows.Forms.CheckBox();

        onScreenUpDown = new SoundSwitch.UI.Component.NumericUpDownWithUnits();
        opacityLabel = new System.Windows.Forms.Label();
        opacityUpDown = new SoundSwitch.UI.Component.NumericUpDownWithUnits();
        positionGroupBox = new System.Windows.Forms.GroupBox();
        selectCustomPositionButton = new System.Windows.Forms.Button();
        positionCustomRadioButton = new System.Windows.Forms.RadioButton();
        positionBottomRightRadioButton = new System.Windows.Forms.RadioButton();
        positionBottomCenterRadioButton = new System.Windows.Forms.RadioButton();
        positionBottomLeftRadioButton = new System.Windows.Forms.RadioButton();
        positionCenterRightRadioButton = new System.Windows.Forms.RadioButton();
        positionCenterRadioButton = new System.Windows.Forms.RadioButton();
        positionCenterLeftRadioButton = new System.Windows.Forms.RadioButton();
        positionTopRightRadioButton = new System.Windows.Forms.RadioButton();
        positionTopCenterRadioButton = new System.Windows.Forms.RadioButton();
        positionTopLeftRadioButton = new System.Windows.Forms.RadioButton();
        appSoundLockTabPage = new System.Windows.Forms.TabPage();
        appSoundLockPanel = new System.Windows.Forms.Panel();
        addAppSoundRuleButton = new System.Windows.Forms.Button();
        editAppSoundRuleButton = new System.Windows.Forms.Button();
        deleteAppSoundRuleButton = new System.Windows.Forms.Button();
        appSoundLockListView = new SoundSwitch.UI.Component.ListView.IconListView();
        troubleshootingTabPage = new System.Windows.Forms.TabPage();
        troubleshootingLabel = new System.Windows.Forms.Label();
        appNameLabel = new System.Windows.Forms.Label();
        configFileGroupBox = new System.Windows.Forms.GroupBox();
        importConfigFileButton = new System.Windows.Forms.Button();
        exportConfigFileButton = new System.Windows.Forms.Button();
        configFileLabel = new System.Windows.Forms.Label();
        donateLinkLabel = new System.Windows.Forms.LinkLabel();
        exportLogFilesGroupBox = new System.Windows.Forms.GroupBox();
        exportLogFilesButton = new System.Windows.Forms.Button();
        exportLogFilesLabel = new System.Windows.Forms.Label();
        discordCommunityLinkLabel = new System.Windows.Forms.LinkLabel();
        resetAudioDevicesGroupBox = new System.Windows.Forms.GroupBox();
        resetAudioDevicesLabel = new System.Windows.Forms.Label();
        resetAudioDevicesButton = new System.Windows.Forms.Button();
        gitHubHelpLinkLabel = new System.Windows.Forms.LinkLabel();
        selectSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
        hotKeyCheckBox = new System.Windows.Forms.CheckBox();
        hotKeyControl = new SoundSwitch.UI.Component.HotKeyTextBox();
        toggleMuteLabel = new System.Windows.Forms.Label();
        muteHotKey = new SoundSwitch.UI.Component.HotKeyTextBox();
        muteHotKeyCheckBox = new System.Windows.Forms.CheckBox();
        switchDeviceLabel = new System.Windows.Forms.Label();
        tabControl.SuspendLayout();
        playbackTabPage.SuspendLayout();
        recordingTabPage.SuspendLayout();
        profileTabPage.SuspendLayout();
        profilesPanel.SuspendLayout();
        appSettingTabPage.SuspendLayout();
        languageGroupBox.SuspendLayout();
        updateSettingsGroupBox.SuspendLayout();
        audioSettingsGroupBox.SuspendLayout();
        basicSettingsGroupBox.SuspendLayout();
        notificationsTabPage.SuspendLayout();
        customSoundFileGroupBox.SuspendLayout();
        notificationsGroupBox.SuspendLayout();
        bannerOptionsGroupBox.SuspendLayout();
        microphoneMuteGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)onScreenUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)opacityUpDown).BeginInit();
        positionGroupBox.SuspendLayout();
        troubleshootingTabPage.SuspendLayout();
        configFileGroupBox.SuspendLayout();
        exportLogFilesGroupBox.SuspendLayout();
        resetAudioDevicesGroupBox.SuspendLayout();
        SuspendLayout();
        // 
        // startWithWindowsCheckBox
        // 
        startWithWindowsCheckBox.AutoSize = true;
        startWithWindowsCheckBox.Location = new System.Drawing.Point(6, 22);
        startWithWindowsCheckBox.Name = "startWithWindowsCheckBox";
        startWithWindowsCheckBox.Size = new System.Drawing.Size(203, 19);
        startWithWindowsCheckBox.TabIndex = 7;
        startWithWindowsCheckBox.Text = "Start automatically with Windows";
        startWithWindowsCheckBox.UseVisualStyleBackColor = true;
        startWithWindowsCheckBox.CheckedChanged += RunAtStartup_CheckedChanged;
        // 
        // closeButton
        // 
        closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        closeButton.AutoSize = true;
        closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        closeButton.Location = new System.Drawing.Point(586, 421);
        closeButton.Name = "closeButton";
        closeButton.Size = new System.Drawing.Size(72, 25);
        closeButton.TabIndex = 11;
        closeButton.Text = "Close";
        closeButton.UseVisualStyleBackColor = true;
        closeButton.Click += CloseButton_Click;
        // 
        // switchCommunicationDeviceCheckBox
        // 
        switchCommunicationDeviceCheckBox.AutoSize = true;
        switchCommunicationDeviceCheckBox.Location = new System.Drawing.Point(6, 22);
        switchCommunicationDeviceCheckBox.Name = "switchCommunicationDeviceCheckBox";
        switchCommunicationDeviceCheckBox.Size = new System.Drawing.Size(230, 19);
        switchCommunicationDeviceCheckBox.TabIndex = 12;
        switchCommunicationDeviceCheckBox.Text = "Switch Default Communication Device";
        switchCommunicationDeviceCheckBox.UseVisualStyleBackColor = true;
        switchCommunicationDeviceCheckBox.CheckedChanged += CommunicationCheckbox_CheckedChanged;
        // 
        // tabControl
        // 
        tabControl.Controls.Add(playbackTabPage);
        tabControl.Controls.Add(recordingTabPage);
        tabControl.Controls.Add(profileTabPage);
        tabControl.Controls.Add(appSoundLockTabPage);
        tabControl.Controls.Add(appSettingTabPage);
        tabControl.Controls.Add(notificationsTabPage);
        tabControl.Controls.Add(troubleshootingTabPage);
        tabControl.Dock = System.Windows.Forms.DockStyle.Top;
        tabControl.Location = new System.Drawing.Point(0, 0);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new System.Drawing.Size(670, 391);
        tabControl.TabIndex = 13;
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        // 
        // playbackTabPage
        // 
        playbackTabPage.Controls.Add(playbackListView);
        playbackTabPage.Location = new System.Drawing.Point(4, 24);
        playbackTabPage.Name = "playbackTabPage";
        playbackTabPage.Padding = new System.Windows.Forms.Padding(3);
        playbackTabPage.Size = new System.Drawing.Size(662, 363);
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
        playbackListView.Size = new System.Drawing.Size(656, 357);
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
        recordingTabPage.Size = new System.Drawing.Size(662, 363);
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
        recordingListView.Size = new System.Drawing.Size(656, 357);
        recordingListView.TabIndex = 17;
        recordingListView.UseCompatibleStateImageBehavior = false;
        recordingListView.View = System.Windows.Forms.View.Details;
        // 
        // profileTabPage
        // 
        profileTabPage.Controls.Add(profilesPanel);
        profileTabPage.Controls.Add(profilesListView);
        profileTabPage.Location = new System.Drawing.Point(4, 24);
        profileTabPage.Name = "profileTabPage";
        profileTabPage.Padding = new System.Windows.Forms.Padding(3);
        profileTabPage.Size = new System.Drawing.Size(662, 363);
        profileTabPage.TabIndex = 3;
        profileTabPage.Text = "Profiles";
        profileTabPage.UseVisualStyleBackColor = true;
        // 
        // profilesPanel
        // 
        profilesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        profilesPanel.Controls.Add(profileExplanationLabel);
        profilesPanel.Controls.Add(editProfileButton);
        profilesPanel.Controls.Add(addProfileButton);
        profilesPanel.Controls.Add(deleteProfileButton);
        profilesPanel.Location = new System.Drawing.Point(3, 256);
        profilesPanel.Name = "profilesPanel";
        profilesPanel.Size = new System.Drawing.Size(656, 104);
        profilesPanel.TabIndex = 6;
        // 
        // profileExplanationLabel
        // 
        profileExplanationLabel.AutoSize = true;
        profileExplanationLabel.Location = new System.Drawing.Point(0, 0);
        profileExplanationLabel.MaximumSize = new System.Drawing.Size(500, 0);
        profileExplanationLabel.Name = "profileExplanationLabel";
        profileExplanationLabel.Size = new System.Drawing.Size(100, 30);
        profileExplanationLabel.TabIndex = 3;
        profileExplanationLabel.Text = "Explanation line 1\r\nOptional line 2";
        // 
        // editProfileButton
        // 
        editProfileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        editProfileButton.AutoSize = true;
        editProfileButton.Enabled = false;
        editProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        editProfileButton.Location = new System.Drawing.Point(447, 76);
        editProfileButton.Name = "editProfileButton";
        editProfileButton.Size = new System.Drawing.Size(100, 25);
        editProfileButton.TabIndex = 5;
        editProfileButton.Text = "Edit";
        editProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        editProfileButton.UseVisualStyleBackColor = true;
        editProfileButton.Click += EditProfileButton_Click;
        // 
        // addProfileButton
        // 
        addProfileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        addProfileButton.AutoSize = true;
        addProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        addProfileButton.Location = new System.Drawing.Point(341, 76);
        addProfileButton.Name = "addProfileButton";
        addProfileButton.Size = new System.Drawing.Size(100, 25);
        addProfileButton.TabIndex = 1;
        addProfileButton.Text = "Add";
        addProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        addProfileButton.UseVisualStyleBackColor = true;
        addProfileButton.Click += AddProfileButton_Click;
        // 
        // deleteProfileButton
        // 
        deleteProfileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        deleteProfileButton.AutoSize = true;
        deleteProfileButton.Enabled = false;
        deleteProfileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        deleteProfileButton.Location = new System.Drawing.Point(553, 76);
        deleteProfileButton.Name = "deleteProfileButton";
        deleteProfileButton.Size = new System.Drawing.Size(100, 25);
        deleteProfileButton.TabIndex = 4;
        deleteProfileButton.Text = "Delete";
        deleteProfileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        deleteProfileButton.UseVisualStyleBackColor = true;
        deleteProfileButton.Click += DeleteProfileButton_Click;
        // 
        // profilesListView
        // 
        profilesListView.Dock = System.Windows.Forms.DockStyle.Top;
        profilesListView.FullRowSelect = true;
        profilesListView.Location = new System.Drawing.Point(3, 3);
        profilesListView.Name = "profilesListView";
        profilesListView.OwnerDraw = true;
        profilesListView.ShowGroups = false;
        profilesListView.Size = new System.Drawing.Size(656, 247);
        profilesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
        profilesListView.TabIndex = 2;
        profilesListView.UseCompatibleStateImageBehavior = false;
        profilesListView.View = System.Windows.Forms.View.Details;
        profilesListView.SelectedIndexChanged += ProfilesListView_SelectedIndexChanged;
        profilesListView.DoubleClick += ProfilesListView_DoubleClick;
        // 
        // appSettingTabPage
        // 
        appSettingTabPage.Controls.Add(languageGroupBox);
        appSettingTabPage.Controls.Add(updateSettingsGroupBox);
        appSettingTabPage.Controls.Add(audioSettingsGroupBox);
        appSettingTabPage.Controls.Add(basicSettingsGroupBox);
        appSettingTabPage.Location = new System.Drawing.Point(4, 24);
        appSettingTabPage.Name = "appSettingTabPage";
        appSettingTabPage.Size = new System.Drawing.Size(662, 363);
        appSettingTabPage.TabIndex = 2;
        appSettingTabPage.Text = "General";
        appSettingTabPage.UseVisualStyleBackColor = true;
        // 
        // languageGroupBox
        // 
        languageGroupBox.Controls.Add(languageComboBox);
        languageGroupBox.Location = new System.Drawing.Point(359, 186);
        languageGroupBox.Name = "languageGroupBox";
        languageGroupBox.Size = new System.Drawing.Size(300, 55);
        languageGroupBox.TabIndex = 15;
        languageGroupBox.TabStop = false;
        languageGroupBox.Text = "Language";
        // 
        // languageComboBox
        // 
        languageComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
        languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        languageComboBox.FormattingEnabled = true;
        languageComboBox.Location = new System.Drawing.Point(3, 19);
        languageComboBox.Name = "languageComboBox";
        languageComboBox.Size = new System.Drawing.Size(294, 23);
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
        updateSettingsGroupBox.Location = new System.Drawing.Point(359, 3);
        updateSettingsGroupBox.Name = "updateSettingsGroupBox";
        updateSettingsGroupBox.Size = new System.Drawing.Size(300, 177);
        updateSettingsGroupBox.TabIndex = 14;
        updateSettingsGroupBox.TabStop = false;
        updateSettingsGroupBox.Text = "Update Settings";
        // 
        // telemetryCheckbox
        // 
        telemetryCheckbox.AutoSize = true;
        telemetryCheckbox.Location = new System.Drawing.Point(6, 122);
        telemetryCheckbox.Name = "telemetryCheckbox";
        telemetryCheckbox.Size = new System.Drawing.Size(77, 19);
        telemetryCheckbox.TabIndex = 22;
        telemetryCheckbox.Text = "Telemetry";
        telemetryCheckbox.UseVisualStyleBackColor = true;
        // 
        // updateNeverRadioButton
        // 
        updateNeverRadioButton.AutoSize = true;
        updateNeverRadioButton.Location = new System.Drawing.Point(6, 72);
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
        updateNotifyRadioButton.Location = new System.Drawing.Point(6, 47);
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
        updateSilentRadioButton.Location = new System.Drawing.Point(6, 22);
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
        includeBetaVersionsCheckBox.Location = new System.Drawing.Point(6, 97);
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
        audioSettingsGroupBox.Location = new System.Drawing.Point(3, 144);
        audioSettingsGroupBox.Name = "audioSettingsGroupBox";
        audioSettingsGroupBox.Size = new System.Drawing.Size(350, 216);
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
        cycleThroughLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        cycleThroughLabel.Location = new System.Drawing.Point(6, 157);
        cycleThroughLabel.Name = "cycleThroughLabel";
        cycleThroughLabel.Size = new System.Drawing.Size(150, 23);
        cycleThroughLabel.TabIndex = 23;
        cycleThroughLabel.Text = "Cycle through";
        cycleThroughLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // cycleThroughComboBox
        // 
        cycleThroughComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        cycleThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cycleThroughComboBox.FormattingEnabled = true;
        cycleThroughComboBox.Location = new System.Drawing.Point(162, 157);
        cycleThroughComboBox.Name = "cycleThroughComboBox";
        cycleThroughComboBox.Size = new System.Drawing.Size(182, 23);
        cycleThroughComboBox.TabIndex = 22;
        cycleThroughComboBox.SelectedValueChanged += CyclerComboBox_SelectedValueChanged;
        // 
        // tooltipOnHoverLabel
        // 
        tooltipOnHoverLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        tooltipOnHoverLabel.Location = new System.Drawing.Point(6, 128);
        tooltipOnHoverLabel.Name = "tooltipOnHoverLabel";
        tooltipOnHoverLabel.Size = new System.Drawing.Size(150, 23);
        tooltipOnHoverLabel.TabIndex = 21;
        tooltipOnHoverLabel.Text = "Tooltip on Hover";
        tooltipOnHoverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // tooltipInfoComboBox
        // 
        tooltipInfoComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        tooltipInfoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        tooltipInfoComboBox.FormattingEnabled = true;
        tooltipInfoComboBox.Location = new System.Drawing.Point(162, 128);
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
        basicSettingsGroupBox.Size = new System.Drawing.Size(350, 135);
        basicSettingsGroupBox.TabIndex = 0;
        basicSettingsGroupBox.TabStop = false;
        basicSettingsGroupBox.Text = "Basic Settings";
        // 
        // iconDoubleClickLabel
        // 
        iconDoubleClickLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        iconDoubleClickLabel.Location = new System.Drawing.Point(6, 76);
        iconDoubleClickLabel.Name = "iconDoubleClickLabel";
        iconDoubleClickLabel.Size = new System.Drawing.Size(150, 23);
        iconDoubleClickLabel.TabIndex = 29;
        iconDoubleClickLabel.Text = "Double Click Action";
        iconDoubleClickLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // iconDoubleClickComboBox
        // 
        iconDoubleClickComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        iconDoubleClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        iconDoubleClickComboBox.FormattingEnabled = true;
        iconDoubleClickComboBox.Location = new System.Drawing.Point(162, 76);
        iconDoubleClickComboBox.Name = "iconDoubleClickComboBox";
        iconDoubleClickComboBox.Size = new System.Drawing.Size(182, 23);
        iconDoubleClickComboBox.TabIndex = 28;
        iconDoubleClickComboBox.SelectedIndexChanged += IconDoubleClickComboBox_SelectedIndexChanged;
        // 
        // iconChangeLabel
        // 
        iconChangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        iconChangeLabel.Location = new System.Drawing.Point(6, 47);
        iconChangeLabel.Name = "iconChangeLabel";
        iconChangeLabel.Size = new System.Drawing.Size(150, 23);
        iconChangeLabel.TabIndex = 27;
        iconChangeLabel.Text = "Systray Icon";
        iconChangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // iconChangeChoicesComboBox
        // 
        iconChangeChoicesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        iconChangeChoicesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        iconChangeChoicesComboBox.FormattingEnabled = true;
        iconChangeChoicesComboBox.Location = new System.Drawing.Point(162, 47);
        iconChangeChoicesComboBox.Name = "iconChangeChoicesComboBox";
        iconChangeChoicesComboBox.Size = new System.Drawing.Size(182, 23);
        iconChangeChoicesComboBox.TabIndex = 26;
        iconChangeChoicesComboBox.SelectedIndexChanged += IconChangeChoicesComboBox_SelectedIndexChanged;
        // 
        // notificationsTabPage
        // 
        notificationsTabPage.Controls.Add(customSoundFileGroupBox);
        notificationsTabPage.Controls.Add(notificationsGroupBox);
        notificationsTabPage.Controls.Add(bannerOptionsGroupBox);
        notificationsTabPage.Location = new System.Drawing.Point(4, 24);
        notificationsTabPage.Name = "notificationsTabPage";
        notificationsTabPage.Size = new System.Drawing.Size(662, 363);
        notificationsTabPage.TabIndex = 5;
        notificationsTabPage.Text = "Notifications";
        notificationsTabPage.UseVisualStyleBackColor = true;
        // 
        // customSoundFileGroupBox
        // 
        customSoundFileGroupBox.Controls.Add(deleteSoundButton);
        customSoundFileGroupBox.Controls.Add(selectSoundButton);
        customSoundFileGroupBox.Location = new System.Drawing.Point(359, 3);
        customSoundFileGroupBox.Name = "customSoundFileGroupBox";
        customSoundFileGroupBox.Size = new System.Drawing.Size(300, 53);
        customSoundFileGroupBox.TabIndex = 35;
        customSoundFileGroupBox.TabStop = false;
        customSoundFileGroupBox.Text = "Custom Sound File";
        // 
        // deleteSoundButton
        // 
        deleteSoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        deleteSoundButton.AutoSize = true;
        deleteSoundButton.Image = global::SoundSwitch.Properties.Resources.delete;
        deleteSoundButton.Location = new System.Drawing.Point(269, 19);
        deleteSoundButton.Name = "deleteSoundButton";
        deleteSoundButton.Size = new System.Drawing.Size(25, 25);
        deleteSoundButton.TabIndex = 24;
        deleteSoundButton.UseVisualStyleBackColor = true;
        deleteSoundButton.Click += DeleteSoundButton_Click;
        // 
        // selectSoundButton
        // 
        selectSoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        selectSoundButton.Location = new System.Drawing.Point(6, 19);
        selectSoundButton.Name = "selectSoundButton";
        selectSoundButton.Size = new System.Drawing.Size(257, 25);
        selectSoundButton.TabIndex = 19;
        selectSoundButton.Text = "Select…";
        selectSoundButton.UseVisualStyleBackColor = true;
        selectSoundButton.Click += SelectSoundButton_Click;
        // 
        // notificationsGroupBox
        // 
        notificationsGroupBox.BackColor = System.Drawing.Color.White;
        notificationsGroupBox.Controls.Add(switchProfileNotificationLabel);
        notificationsGroupBox.Controls.Add(microphoneMuteNotificationLabel);
        notificationsGroupBox.Controls.Add(switchProfileNotificationComboBox);
        notificationsGroupBox.Controls.Add(microphoneMuteNotificationComboBox);
        notificationsGroupBox.Controls.Add(advancedModeCheckBox);
        notificationsGroupBox.Controls.Add(switchDeviceNotificationLabel);
        notificationsGroupBox.Controls.Add(switchDeviceNotificationComboBox);
        notificationsGroupBox.Location = new System.Drawing.Point(3, 3);
        notificationsGroupBox.Name = "notificationsGroupBox";
        notificationsGroupBox.Size = new System.Drawing.Size(350, 166);
        notificationsGroupBox.TabIndex = 17;
        notificationsGroupBox.TabStop = false;
        notificationsGroupBox.Text = "Notification Type";
        // 
        // switchProfileNotificationLabel
        // 
        switchProfileNotificationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        switchProfileNotificationLabel.Location = new System.Drawing.Point(6, 93);
        switchProfileNotificationLabel.Name = "switchProfileNotificationLabel";
        switchProfileNotificationLabel.Size = new System.Drawing.Size(150, 23);
        switchProfileNotificationLabel.TabIndex = 44;
        switchProfileNotificationLabel.Text = "Switch profile";
        switchProfileNotificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // microphoneMuteNotificationLabel
        // 
        microphoneMuteNotificationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneMuteNotificationLabel.Location = new System.Drawing.Point(6, 122);
        microphoneMuteNotificationLabel.Name = "microphoneMuteNotificationLabel";
        microphoneMuteNotificationLabel.Size = new System.Drawing.Size(150, 23);
        microphoneMuteNotificationLabel.TabIndex = 46;
        microphoneMuteNotificationLabel.Text = "Microphone mute";
        microphoneMuteNotificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // switchProfileNotificationComboBox
        // 
        switchProfileNotificationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        switchProfileNotificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        switchProfileNotificationComboBox.FormattingEnabled = true;
        switchProfileNotificationComboBox.Location = new System.Drawing.Point(162, 93);
        switchProfileNotificationComboBox.Name = "switchProfileNotificationComboBox";
        switchProfileNotificationComboBox.Size = new System.Drawing.Size(182, 23);
        switchProfileNotificationComboBox.TabIndex = 43;
        switchProfileNotificationComboBox.SelectedValueChanged += SwitchProfileNotificationComboBox_SelectedValueChanged;
        // 
        // microphoneMuteNotificationComboBox
        // 
        microphoneMuteNotificationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneMuteNotificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        microphoneMuteNotificationComboBox.FormattingEnabled = true;
        microphoneMuteNotificationComboBox.Location = new System.Drawing.Point(162, 122);
        microphoneMuteNotificationComboBox.Name = "microphoneMuteNotificationComboBox";
        microphoneMuteNotificationComboBox.Size = new System.Drawing.Size(182, 23);
        microphoneMuteNotificationComboBox.TabIndex = 45;
        microphoneMuteNotificationComboBox.SelectedValueChanged += MicrophoneMuteNotificationComboBox_SelectedValueChanged;
        // 
        // advancedModeCheckBox
        // 
        advancedModeCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
        advancedModeCheckBox.AutoSize = true;
        advancedModeCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
        advancedModeCheckBox.Location = new System.Drawing.Point(3, 19);
        advancedModeCheckBox.Name = "advancedModeCheckBox";
        advancedModeCheckBox.Size = new System.Drawing.Size(344, 25);
        advancedModeCheckBox.TabIndex = 40;
        advancedModeCheckBox.Text = "Advanced...";
        advancedModeCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        advancedModeCheckBox.UseVisualStyleBackColor = true;
        advancedModeCheckBox.CheckedChanged += AdvancedModeCheckBox_CheckedChanged;
        // 
        // switchDeviceNotificationLabel
        // 
        switchDeviceNotificationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        switchDeviceNotificationLabel.Location = new System.Drawing.Point(6, 50);
        switchDeviceNotificationLabel.Name = "switchDeviceNotificationLabel";
        switchDeviceNotificationLabel.Size = new System.Drawing.Size(150, 23);
        switchDeviceNotificationLabel.TabIndex = 36;
        switchDeviceNotificationLabel.Text = "Switch device";
        switchDeviceNotificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // switchDeviceNotificationComboBox
        // 
        switchDeviceNotificationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        switchDeviceNotificationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        switchDeviceNotificationComboBox.FormattingEnabled = true;
        switchDeviceNotificationComboBox.Location = new System.Drawing.Point(162, 50);
        switchDeviceNotificationComboBox.Name = "switchDeviceNotificationComboBox";
        switchDeviceNotificationComboBox.Size = new System.Drawing.Size(182, 23);
        switchDeviceNotificationComboBox.TabIndex = 16;
        switchDeviceNotificationComboBox.SelectedValueChanged += SwitchDeviceNotificationComboBox_SelectedValueChanged;
        // 
        // bannerOptionsGroupBox
        // 
        bannerOptionsGroupBox.Controls.Add(microphoneMuteGroupBox);
        bannerOptionsGroupBox.Controls.Add(bannerShowOnComboBox);
        bannerOptionsGroupBox.Controls.Add(bannerShowOnLabel);
        bannerOptionsGroupBox.Controls.Add(onScreenTimeLabel);
        bannerOptionsGroupBox.Controls.Add(singleNotificationCheckbox);
        bannerOptionsGroupBox.Controls.Add(onScreenUpDown);
        bannerOptionsGroupBox.Controls.Add(opacityLabel);
        bannerOptionsGroupBox.Controls.Add(opacityUpDown);
        bannerOptionsGroupBox.Controls.Add(positionGroupBox);
        bannerOptionsGroupBox.Location = new System.Drawing.Point(3, 175);
        bannerOptionsGroupBox.Name = "bannerOptionsGroupBox";
        bannerOptionsGroupBox.Size = new System.Drawing.Size(656, 185);
        bannerOptionsGroupBox.TabIndex = 57;
        bannerOptionsGroupBox.TabStop = false;
        bannerOptionsGroupBox.Text = "Banner Options";
        // 
        // microphoneMuteGroupBox
        // 
        microphoneMuteGroupBox.Controls.Add(microphoneMuteLabel);
        microphoneMuteGroupBox.Controls.Add(microphoneMuteBannerComboBox);
        microphoneMuteGroupBox.Controls.Add(microphoneUnmuteLabel);
        microphoneMuteGroupBox.Controls.Add(microphoneUnmuteBannerComboBox);
        microphoneMuteGroupBox.Location = new System.Drawing.Point(269, 12);
        microphoneMuteGroupBox.Name = "microphoneMuteGroupBox";
        microphoneMuteGroupBox.Size = new System.Drawing.Size(180, 167);
        microphoneMuteGroupBox.TabIndex = 52;
        microphoneMuteGroupBox.TabStop = false;
        microphoneMuteGroupBox.Text = "Microphone Mute";
        // 
        // microphoneMuteLabel
        // 
        microphoneMuteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneMuteLabel.Location = new System.Drawing.Point(5, 92);
        microphoneMuteLabel.Name = "microphoneMuteLabel";
        microphoneMuteLabel.Size = new System.Drawing.Size(169, 23);
        microphoneMuteLabel.TabIndex = 43;
        microphoneMuteLabel.Text = "Microphone Off";
        microphoneMuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // microphoneMuteBannerComboBox
        // 
        microphoneMuteBannerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneMuteBannerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        microphoneMuteBannerComboBox.FormattingEnabled = true;
        microphoneMuteBannerComboBox.Location = new System.Drawing.Point(30, 118);
        // 
        // appSoundLockTabPage
        // 
        appSoundLockTabPage.Controls.Add(appSoundLockPanel);
        appSoundLockTabPage.Controls.Add(appSoundLockListView);
        appSoundLockTabPage.Location = new System.Drawing.Point(4, 24);
        appSoundLockTabPage.Name = "appSoundLockTabPage";
        appSoundLockTabPage.Padding = new System.Windows.Forms.Padding(3);
        appSoundLockTabPage.Size = new System.Drawing.Size(662, 363);
        appSoundLockTabPage.TabIndex = 6;
        appSoundLockTabPage.Text = "App Sound Lock";
        appSoundLockTabPage.UseVisualStyleBackColor = true;
        // 
        // appSoundLockPanel
        // 
        appSoundLockPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        appSoundLockPanel.Controls.Add(addAppSoundRuleButton);
        appSoundLockPanel.Controls.Add(editAppSoundRuleButton);
        appSoundLockPanel.Controls.Add(deleteAppSoundRuleButton);
        appSoundLockPanel.Location = new System.Drawing.Point(3, 256);
        appSoundLockPanel.Name = "appSoundLockPanel";
        appSoundLockPanel.Size = new System.Drawing.Size(656, 104);
        appSoundLockPanel.TabIndex = 7;
        // 
        // addAppSoundRuleButton
        // 
        addAppSoundRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        addAppSoundRuleButton.AutoSize = true;
        addAppSoundRuleButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        addAppSoundRuleButton.Location = new System.Drawing.Point(341, 76);
        addAppSoundRuleButton.Name = "addAppSoundRuleButton";
        addAppSoundRuleButton.Size = new System.Drawing.Size(100, 25);
        addAppSoundRuleButton.TabIndex = 8;
        addAppSoundRuleButton.Text = "Add";
        addAppSoundRuleButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        addAppSoundRuleButton.UseVisualStyleBackColor = true;
        addAppSoundRuleButton.Click += AddAppSoundRuleButton_Click;
        // 
        // editAppSoundRuleButton
        // 
        editAppSoundRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        editAppSoundRuleButton.AutoSize = true;
        editAppSoundRuleButton.Enabled = false;
        editAppSoundRuleButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        editAppSoundRuleButton.Location = new System.Drawing.Point(447, 76);
        editAppSoundRuleButton.Name = "editAppSoundRuleButton";
        editAppSoundRuleButton.Size = new System.Drawing.Size(100, 25);
        editAppSoundRuleButton.TabIndex = 9;
        editAppSoundRuleButton.Text = "Edit";
        editAppSoundRuleButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        editAppSoundRuleButton.UseVisualStyleBackColor = true;
        editAppSoundRuleButton.Click += EditAppSoundRuleButton_Click;
        // 
        // deleteAppSoundRuleButton
        // 
        deleteAppSoundRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        deleteAppSoundRuleButton.AutoSize = true;
        deleteAppSoundRuleButton.Enabled = false;
        deleteAppSoundRuleButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        deleteAppSoundRuleButton.Location = new System.Drawing.Point(553, 76);
        deleteAppSoundRuleButton.Name = "deleteAppSoundRuleButton";
        deleteAppSoundRuleButton.Size = new System.Drawing.Size(100, 25);
        deleteAppSoundRuleButton.TabIndex = 10;
        deleteAppSoundRuleButton.Text = "Delete";
        deleteAppSoundRuleButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        deleteAppSoundRuleButton.UseVisualStyleBackColor = true;
        deleteAppSoundRuleButton.Click += DeleteAppSoundRuleButton_Click;
        // 
        // appSoundLockListView
        // 
        appSoundLockListView.Dock = System.Windows.Forms.DockStyle.Top;
        appSoundLockListView.FullRowSelect = true;
        appSoundLockListView.Location = new System.Drawing.Point(3, 3);
        appSoundLockListView.Name = "appSoundLockListView";
        appSoundLockListView.OwnerDraw = true;
        appSoundLockListView.ShowGroups = false;
        appSoundLockListView.Size = new System.Drawing.Size(656, 247);
        appSoundLockListView.TabIndex = 11;
        appSoundLockListView.UseCompatibleStateImageBehavior = false;
        appSoundLockListView.View = System.Windows.Forms.View.Details;
        appSoundLockListView.SelectedIndexChanged += AppSoundLockListView_SelectedIndexChanged;
        appSoundLockListView.DoubleClick += AppSoundLockListView_DoubleClick;
        // 
        // troubleshootingTabPage
        // 
        microphoneMuteBannerComboBox.Name = "microphoneMuteBannerComboBox";
        microphoneMuteBannerComboBox.Size = new System.Drawing.Size(120, 23);
        microphoneMuteBannerComboBox.TabIndex = 31;
        microphoneMuteBannerComboBox.SelectedValueChanged += MicrophoneMuteBannerComboBox_SelectedValueChanged;
        // 
        // microphoneUnmuteLabel
        // 
        microphoneUnmuteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneUnmuteLabel.Location = new System.Drawing.Point(6, 40);
        microphoneUnmuteLabel.Name = "microphoneUnmuteLabel";
        microphoneUnmuteLabel.Size = new System.Drawing.Size(168, 23);
        microphoneUnmuteLabel.TabIndex = 42;
        microphoneUnmuteLabel.Text = "Microphone On";
        microphoneUnmuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // microphoneUnmuteBannerComboBox
        // 
        microphoneUnmuteBannerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        microphoneUnmuteBannerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        microphoneUnmuteBannerComboBox.FormattingEnabled = true;
        microphoneUnmuteBannerComboBox.Location = new System.Drawing.Point(30, 66);
        microphoneUnmuteBannerComboBox.Name = "microphoneUnmuteBannerComboBox";
        microphoneUnmuteBannerComboBox.Size = new System.Drawing.Size(120, 23);
        microphoneUnmuteBannerComboBox.TabIndex = 41;
        microphoneUnmuteBannerComboBox.SelectedValueChanged += MicrophoneUnmuteBannerComboBox_SelectedValueChanged;
        // 
        // 
        // bannerShowOnLabel
        // 
        bannerShowOnLabel.Location = new System.Drawing.Point(6, 22);
        bannerShowOnLabel.Name = "bannerShowOnLabel";
        bannerShowOnLabel.Size = new System.Drawing.Size(100, 23);
        bannerShowOnLabel.TabIndex = 26;
        bannerShowOnLabel.Text = "Show on";
        bannerShowOnLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // bannerShowOnComboBox
        // 
        bannerShowOnComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        bannerShowOnComboBox.FormattingEnabled = true;
        bannerShowOnComboBox.Location = new System.Drawing.Point(112, 22);
        bannerShowOnComboBox.Name = "bannerShowOnComboBox";
        bannerShowOnComboBox.Size = new System.Drawing.Size(151, 23);
        bannerShowOnComboBox.TabIndex = 27;
        bannerShowOnComboBox.SelectedValueChanged += BannerShowOnComboBox_SelectedValueChanged;
        // 
        // onScreenTimeLabel
        // 
        onScreenTimeLabel.Location = new System.Drawing.Point(7, 72);
        onScreenTimeLabel.Margin = new System.Windows.Forms.Padding(3);
        onScreenTimeLabel.Name = "onScreenTimeLabel";
        onScreenTimeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
        onScreenTimeLabel.Size = new System.Drawing.Size(130, 23);
        onScreenTimeLabel.TabIndex = 29;
        onScreenTimeLabel.Text = "On-screen time";
        onScreenTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        // 
        // singleNotificationCheckbox
        // 
        singleNotificationCheckbox.AutoSize = true;
        singleNotificationCheckbox.Location = new System.Drawing.Point(6, 47);
        singleNotificationCheckbox.Name = "singleNotificationCheckbox";
        singleNotificationCheckbox.Size = new System.Drawing.Size(124, 19);
        singleNotificationCheckbox.TabIndex = 27;
        singleNotificationCheckbox.Text = "Single Notification";
        singleNotificationCheckbox.UseVisualStyleBackColor = true;

        // 
        // onScreenUpDown
        // 
        onScreenUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        onScreenUpDown.Location = new System.Drawing.Point(143, 72);
        onScreenUpDown.Name = "onScreenUpDown";
        onScreenUpDown.Size = new System.Drawing.Size(55, 23);
        onScreenUpDown.TabIndex = 28;
        onScreenUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // opacityLabel
        // 
        opacityLabel.Location = new System.Drawing.Point(7, 101);
        opacityLabel.Margin = new System.Windows.Forms.Padding(3);
        opacityLabel.Name = "opacityLabel";
        opacityLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
        opacityLabel.Size = new System.Drawing.Size(130, 23);
        opacityLabel.TabIndex = 49;
        opacityLabel.Text = "Opacity";
        opacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // opacityUpDown
        // 
        opacityUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        opacityUpDown.Location = new System.Drawing.Point(143, 101);
        opacityUpDown.Name = "opacityUpDown";
        opacityUpDown.Size = new System.Drawing.Size(55, 23);
        opacityUpDown.TabIndex = 51;
        opacityUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // positionGroupBox
        // 
        positionGroupBox.Controls.Add(selectCustomPositionButton);
        positionGroupBox.Controls.Add(positionCustomRadioButton);
        positionGroupBox.Controls.Add(positionBottomRightRadioButton);
        positionGroupBox.Controls.Add(positionBottomCenterRadioButton);
        positionGroupBox.Controls.Add(positionBottomLeftRadioButton);
        positionGroupBox.Controls.Add(positionCenterRightRadioButton);
        positionGroupBox.Controls.Add(positionCenterRadioButton);
        positionGroupBox.Controls.Add(positionCenterLeftRadioButton);
        positionGroupBox.Controls.Add(positionTopRightRadioButton);
        positionGroupBox.Controls.Add(positionTopCenterRadioButton);
        positionGroupBox.Controls.Add(positionTopLeftRadioButton);
        positionGroupBox.Location = new System.Drawing.Point(455, 12);
        positionGroupBox.Name = "positionGroupBox";
        positionGroupBox.Size = new System.Drawing.Size(196, 167);
        positionGroupBox.TabIndex = 35;
        positionGroupBox.TabStop = false;
        positionGroupBox.Text = "Position";
        positionGroupBox.Paint += PositionGroupBox_Paint;
        // 
        // selectCustomPositionButton
        // 
        selectCustomPositionButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        selectCustomPositionButton.Location = new System.Drawing.Point(93, 136);
        selectCustomPositionButton.Name = "selectCustomPositionButton";
        selectCustomPositionButton.Size = new System.Drawing.Size(76, 25);
        selectCustomPositionButton.TabIndex = 36;
        selectCustomPositionButton.Text = "Custom…";
        selectCustomPositionButton.UseVisualStyleBackColor = true;
        selectCustomPositionButton.Click += SelectCustomPositionButton_Click;
        // 
        // positionCustomRadioButton
        // 
        positionCustomRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
        positionCustomRadioButton.Location = new System.Drawing.Point(27, 136);
        positionCustomRadioButton.Name = "positionCustomRadioButton";
        positionCustomRadioButton.Size = new System.Drawing.Size(40, 25);
        positionCustomRadioButton.TabIndex = 9;
        positionCustomRadioButton.UseVisualStyleBackColor = true;
        positionCustomRadioButton.CheckedChanged += PositionCustomRadioButton_CheckedChanged;
        // 
        // positionBottomRightRadioButton
        // 
        positionBottomRightRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionBottomRightRadioButton.Location = new System.Drawing.Point(129, 94);
        positionBottomRightRadioButton.Margin = new System.Windows.Forms.Padding(10, 5, 21, 12);
        positionBottomRightRadioButton.Name = "positionBottomRightRadioButton";
        positionBottomRightRadioButton.Size = new System.Drawing.Size(40, 20);
        positionBottomRightRadioButton.TabIndex = 8;
        positionBottomRightRadioButton.UseVisualStyleBackColor = true;
        positionBottomRightRadioButton.CheckedChanged += PositionBottomRightRadioButton_CheckedChanged;
        // 
        // positionBottomCenterRadioButton
        // 
        positionBottomCenterRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionBottomCenterRadioButton.Location = new System.Drawing.Point(79, 94);
        positionBottomCenterRadioButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 12);
        positionBottomCenterRadioButton.Name = "positionBottomCenterRadioButton";
        positionBottomCenterRadioButton.Size = new System.Drawing.Size(40, 20);
        positionBottomCenterRadioButton.TabIndex = 7;
        positionBottomCenterRadioButton.UseVisualStyleBackColor = true;
        positionBottomCenterRadioButton.CheckedChanged += PositionBottomCenterRadioButton_CheckedChanged;
        // 
        // positionBottomLeftRadioButton
        // 
        positionBottomLeftRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionBottomLeftRadioButton.Location = new System.Drawing.Point(27, 94);
        positionBottomLeftRadioButton.Margin = new System.Windows.Forms.Padding(21, 5, 10, 12);
        positionBottomLeftRadioButton.Name = "positionBottomLeftRadioButton";
        positionBottomLeftRadioButton.Size = new System.Drawing.Size(40, 20);
        positionBottomLeftRadioButton.TabIndex = 6;
        positionBottomLeftRadioButton.UseVisualStyleBackColor = true;
        positionBottomLeftRadioButton.CheckedChanged += PositionBottomLeftRadioButton_CheckedChanged;
        // 
        // positionCenterRightRadioButton
        // 
        positionCenterRightRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionCenterRightRadioButton.Location = new System.Drawing.Point(129, 64);
        positionCenterRightRadioButton.Margin = new System.Windows.Forms.Padding(10, 5, 21, 5);
        positionCenterRightRadioButton.Name = "positionCenterRightRadioButton";
        positionCenterRightRadioButton.Size = new System.Drawing.Size(40, 20);
        positionCenterRightRadioButton.TabIndex = 5;
        positionCenterRightRadioButton.UseVisualStyleBackColor = true;
        positionCenterRightRadioButton.CheckedChanged += PositionCenterRightRadioButton_CheckedChanged;
        // 
        // positionCenterRadioButton
        // 
        positionCenterRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionCenterRadioButton.Location = new System.Drawing.Point(79, 64);
        positionCenterRadioButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
        positionCenterRadioButton.Name = "positionCenterRadioButton";
        positionCenterRadioButton.Size = new System.Drawing.Size(40, 20);
        positionCenterRadioButton.TabIndex = 4;
        positionCenterRadioButton.UseVisualStyleBackColor = true;
        positionCenterRadioButton.CheckedChanged += PositionCenterRadioButton_CheckedChanged;
        // 
        // positionCenterLeftRadioButton
        // 
        positionCenterLeftRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionCenterLeftRadioButton.Location = new System.Drawing.Point(27, 64);
        positionCenterLeftRadioButton.Margin = new System.Windows.Forms.Padding(21, 5, 10, 5);
        positionCenterLeftRadioButton.Name = "positionCenterLeftRadioButton";
        positionCenterLeftRadioButton.Size = new System.Drawing.Size(40, 20);
        positionCenterLeftRadioButton.TabIndex = 3;
        positionCenterLeftRadioButton.UseVisualStyleBackColor = true;
        positionCenterLeftRadioButton.CheckedChanged += PositionCenterLeftRadioButton_CheckedChanged;
        // 
        // positionTopRightRadioButton
        // 
        positionTopRightRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionTopRightRadioButton.Location = new System.Drawing.Point(129, 34);
        positionTopRightRadioButton.Margin = new System.Windows.Forms.Padding(10, 12, 21, 5);
        positionTopRightRadioButton.Name = "positionTopRightRadioButton";
        positionTopRightRadioButton.Size = new System.Drawing.Size(40, 20);
        positionTopRightRadioButton.TabIndex = 2;
        positionTopRightRadioButton.UseVisualStyleBackColor = true;
        positionTopRightRadioButton.CheckedChanged += PositionTopRightRadioButton_CheckedChanged;
        // 
        // positionTopCenterRadioButton
        // 
        positionTopCenterRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionTopCenterRadioButton.Location = new System.Drawing.Point(79, 34);
        positionTopCenterRadioButton.Margin = new System.Windows.Forms.Padding(10, 12, 10, 5);
        positionTopCenterRadioButton.Name = "positionTopCenterRadioButton";
        positionTopCenterRadioButton.Size = new System.Drawing.Size(40, 20);
        positionTopCenterRadioButton.TabIndex = 1;
        positionTopCenterRadioButton.UseVisualStyleBackColor = true;
        positionTopCenterRadioButton.CheckedChanged += PositionTopCenterRadioButton_CheckedChanged;
        // 
        // positionTopLeftRadioButton
        // 
        positionTopLeftRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
        positionTopLeftRadioButton.Location = new System.Drawing.Point(27, 34);
        positionTopLeftRadioButton.Margin = new System.Windows.Forms.Padding(21, 12, 10, 5);
        positionTopLeftRadioButton.Name = "positionTopLeftRadioButton";
        positionTopLeftRadioButton.Size = new System.Drawing.Size(40, 20);
        positionTopLeftRadioButton.TabIndex = 0;
        positionTopLeftRadioButton.UseVisualStyleBackColor = true;
        positionTopLeftRadioButton.CheckedChanged += PositionTopLeftRadioButton_CheckedChanged;
        // 
        // troubleshootingTabPage
        // 
        troubleshootingTabPage.Controls.Add(troubleshootingLabel);
        troubleshootingTabPage.Controls.Add(appNameLabel);
        troubleshootingTabPage.Controls.Add(configFileGroupBox);
        troubleshootingTabPage.Controls.Add(donateLinkLabel);
        troubleshootingTabPage.Controls.Add(exportLogFilesGroupBox);
        troubleshootingTabPage.Controls.Add(discordCommunityLinkLabel);
        troubleshootingTabPage.Controls.Add(resetAudioDevicesGroupBox);
        troubleshootingTabPage.Controls.Add(gitHubHelpLinkLabel);
        troubleshootingTabPage.Location = new System.Drawing.Point(4, 24);
        troubleshootingTabPage.Name = "troubleshootingTabPage";
        troubleshootingTabPage.Padding = new System.Windows.Forms.Padding(3);
        troubleshootingTabPage.Size = new System.Drawing.Size(662, 363);
        troubleshootingTabPage.TabIndex = 4;
        troubleshootingTabPage.Text = "Troubleshooting";
        troubleshootingTabPage.UseVisualStyleBackColor = true;
        // 
        // troubleshootingLabel
        // 
        troubleshootingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        troubleshootingLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic);
        troubleshootingLabel.Location = new System.Drawing.Point(3, 166);
        troubleshootingLabel.Name = "troubleshootingLabel";
        troubleshootingLabel.Padding = new System.Windows.Forms.Padding(30);
        troubleshootingLabel.Size = new System.Drawing.Size(656, 134);
        troubleshootingLabel.TabIndex = 11;
        troubleshootingLabel.Text = "description";
        troubleshootingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // appNameLabel
        // 
        appNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
        appNameLabel.Image = global::SoundSwitch.Properties.Resources.SoundSwitch48;
        appNameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        appNameLabel.Location = new System.Drawing.Point(3, 300);
        appNameLabel.Name = "appNameLabel";
        appNameLabel.Size = new System.Drawing.Size(236, 60);
        appNameLabel.TabIndex = 8;
        appNameLabel.Text = "SoundSwitch";
        appNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // configFileGroupBox
        // 
        configFileGroupBox.Controls.Add(importConfigFileButton);
        configFileGroupBox.Controls.Add(exportConfigFileButton);
        configFileGroupBox.Controls.Add(configFileLabel);
        configFileGroupBox.Location = new System.Drawing.Point(409, 3);
        configFileGroupBox.Name = "configFileGroupBox";
        configFileGroupBox.Size = new System.Drawing.Size(250, 160);
        configFileGroupBox.TabIndex = 13;
        configFileGroupBox.TabStop = false;
        configFileGroupBox.Text = "Configuration File";
        // 
        // importConfigFileButton
        // 
        importConfigFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        importConfigFileButton.Location = new System.Drawing.Point(88, 131);
        importConfigFileButton.Name = "importConfigFileButton";
        importConfigFileButton.Size = new System.Drawing.Size(75, 23);
        importConfigFileButton.TabIndex = 5;
        importConfigFileButton.Text = "Import";
        importConfigFileButton.UseVisualStyleBackColor = true;
        importConfigFileButton.Click += ImportConfigFileButton_Click;
        // 
        // exportConfigFileButton
        // 
        exportConfigFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        exportConfigFileButton.Location = new System.Drawing.Point(169, 131);
        exportConfigFileButton.Name = "exportConfigFileButton";
        exportConfigFileButton.Size = new System.Drawing.Size(75, 23);
        exportConfigFileButton.TabIndex = 4;
        exportConfigFileButton.Text = "Export";
        exportConfigFileButton.UseVisualStyleBackColor = true;
        exportConfigFileButton.Click += ExportConfigFileButton_Click;
        // 
        // configFileLabel
        // 
        configFileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        configFileLabel.Location = new System.Drawing.Point(6, 19);
        configFileLabel.Name = "configFileLabel";
        configFileLabel.Size = new System.Drawing.Size(238, 109);
        configFileLabel.TabIndex = 4;
        configFileLabel.Text = "description";
        // 
        // donateLinkLabel
        // 
        donateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        donateLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        donateLinkLabel.Image = global::SoundSwitch.Properties.Resources.Heart32;
        donateLinkLabel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        donateLinkLabel.Location = new System.Drawing.Point(557, 300);
        donateLinkLabel.Name = "donateLinkLabel";
        donateLinkLabel.Size = new System.Drawing.Size(102, 60);
        donateLinkLabel.TabIndex = 2;
        donateLinkLabel.TabStop = true;
        donateLinkLabel.Text = "Donate";
        donateLinkLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        donateLinkLabel.LinkClicked += DonateLink_LinkClicked;
        // 
        // exportLogFilesGroupBox
        // 
        exportLogFilesGroupBox.Controls.Add(exportLogFilesButton);
        exportLogFilesGroupBox.Controls.Add(exportLogFilesLabel);
        exportLogFilesGroupBox.Location = new System.Drawing.Point(206, 3);
        exportLogFilesGroupBox.Name = "exportLogFilesGroupBox";
        exportLogFilesGroupBox.Size = new System.Drawing.Size(197, 160);
        exportLogFilesGroupBox.TabIndex = 10;
        exportLogFilesGroupBox.TabStop = false;
        exportLogFilesGroupBox.Text = "Export Log Files";
        // 
        // exportLogFilesButton
        // 
        exportLogFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        exportLogFilesButton.Location = new System.Drawing.Point(116, 131);
        exportLogFilesButton.Name = "exportLogFilesButton";
        exportLogFilesButton.Size = new System.Drawing.Size(75, 23);
        exportLogFilesButton.TabIndex = 3;
        exportLogFilesButton.Text = "Export";
        exportLogFilesButton.UseVisualStyleBackColor = true;
        exportLogFilesButton.Click += ExportLogFilesButton_Click;
        // 
        // exportLogFilesLabel
        // 
        exportLogFilesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        exportLogFilesLabel.Location = new System.Drawing.Point(6, 19);
        exportLogFilesLabel.Name = "exportLogFilesLabel";
        exportLogFilesLabel.Size = new System.Drawing.Size(185, 109);
        exportLogFilesLabel.TabIndex = 2;
        exportLogFilesLabel.Text = "description";
        // 
        // discordCommunityLinkLabel
        // 
        discordCommunityLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        discordCommunityLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        discordCommunityLinkLabel.Image = global::SoundSwitch.Properties.Resources.DiscordMarkBlue32;
        discordCommunityLinkLabel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        discordCommunityLinkLabel.Location = new System.Drawing.Point(401, 300);
        discordCommunityLinkLabel.Name = "discordCommunityLinkLabel";
        discordCommunityLinkLabel.Size = new System.Drawing.Size(150, 60);
        discordCommunityLinkLabel.TabIndex = 1;
        discordCommunityLinkLabel.TabStop = true;
        discordCommunityLinkLabel.Text = "Community Discord";
        discordCommunityLinkLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        discordCommunityLinkLabel.LinkClicked += DiscordCommunityLink_LinkClicked;
        // 
        // resetAudioDevicesGroupBox
        // 
        resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesLabel);
        resetAudioDevicesGroupBox.Controls.Add(resetAudioDevicesButton);
        resetAudioDevicesGroupBox.Location = new System.Drawing.Point(3, 3);
        resetAudioDevicesGroupBox.Name = "resetAudioDevicesGroupBox";
        resetAudioDevicesGroupBox.Size = new System.Drawing.Size(197, 160);
        resetAudioDevicesGroupBox.TabIndex = 1;
        resetAudioDevicesGroupBox.TabStop = false;
        resetAudioDevicesGroupBox.Text = "Reset Per App Audio";
        // 
        // resetAudioDevicesLabel
        // 
        resetAudioDevicesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        resetAudioDevicesLabel.Location = new System.Drawing.Point(6, 19);
        resetAudioDevicesLabel.Name = "resetAudioDevicesLabel";
        resetAudioDevicesLabel.Size = new System.Drawing.Size(185, 109);
        resetAudioDevicesLabel.TabIndex = 2;
        resetAudioDevicesLabel.Text = "description";
        // 
        // resetAudioDevicesButton
        // 
        resetAudioDevicesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        resetAudioDevicesButton.Location = new System.Drawing.Point(116, 131);
        resetAudioDevicesButton.Name = "resetAudioDevicesButton";
        resetAudioDevicesButton.Size = new System.Drawing.Size(75, 23);
        resetAudioDevicesButton.TabIndex = 0;
        resetAudioDevicesButton.Text = "Reset";
        resetAudioDevicesButton.UseVisualStyleBackColor = true;
        resetAudioDevicesButton.Click += ResetAudioDevicesButton_Click;
        // 
        // gitHubHelpLinkLabel
        // 
        gitHubHelpLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        gitHubHelpLinkLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        gitHubHelpLinkLabel.Image = global::SoundSwitch.Properties.Resources.GitHubMark32;
        gitHubHelpLinkLabel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        gitHubHelpLinkLabel.Location = new System.Drawing.Point(245, 300);
        gitHubHelpLinkLabel.Name = "gitHubHelpLinkLabel";
        gitHubHelpLinkLabel.Size = new System.Drawing.Size(150, 60);
        gitHubHelpLinkLabel.TabIndex = 0;
        gitHubHelpLinkLabel.TabStop = true;
        gitHubHelpLinkLabel.Text = "Help Discussion";
        gitHubHelpLinkLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        gitHubHelpLinkLabel.LinkClicked += GitHubHelpLink_LinkClicked;
        // 
        // selectSoundFileDialog
        // 
        selectSoundFileDialog.FileName = "customSound";
        // 
        // hotKeyCheckBox
        // 
        hotKeyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        hotKeyCheckBox.AutoSize = true;
        hotKeyCheckBox.Location = new System.Drawing.Point(156, 430);
        hotKeyCheckBox.Name = "hotKeyCheckBox";
        hotKeyCheckBox.Size = new System.Drawing.Size(100, 19);
        hotKeyCheckBox.TabIndex = 20;
        hotKeyCheckBox.Text = "Enable hotkey";
        hotKeyCheckBox.UseVisualStyleBackColor = true;
        hotKeyCheckBox.CheckedChanged += HotKeyCheckBox_CheckedChanged;
        // 
        // hotKeyControl
        // 
        hotKeyControl.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        hotKeyControl.Location = new System.Drawing.Point(12, 426);
        hotKeyControl.Name = "hotKeyControl";
        hotKeyControl.Size = new System.Drawing.Size(138, 23);
        hotKeyControl.TabIndex = 21;
        hotKeyControl.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        hotKeyControl.HotKeyChanged += HotKeyControl_HotKeyChanged;
        // 
        // toggleMuteLabel
        // 
        toggleMuteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        toggleMuteLabel.AutoSize = true;
        toggleMuteLabel.Location = new System.Drawing.Point(300, 407);
        toggleMuteLabel.Name = "toggleMuteLabel";
        toggleMuteLabel.Size = new System.Drawing.Size(73, 15);
        toggleMuteLabel.TabIndex = 22;
        toggleMuteLabel.Text = "Toggle mute";
        toggleMuteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // muteHotKey
        // 
        muteHotKey.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        muteHotKey.Location = new System.Drawing.Point(300, 426);
        muteHotKey.Name = "muteHotKey";
        muteHotKey.Size = new System.Drawing.Size(138, 23);
        muteHotKey.TabIndex = 24;
        muteHotKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        muteHotKey.HotKeyChanged += HotKeyControl_HotKeyChanged;
        // 
        // muteHotKeyCheckBox
        // 
        muteHotKeyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        muteHotKeyCheckBox.AutoSize = true;
        muteHotKeyCheckBox.Location = new System.Drawing.Point(444, 429);
        muteHotKeyCheckBox.Name = "muteHotKeyCheckBox";
        muteHotKeyCheckBox.Size = new System.Drawing.Size(100, 19);
        muteHotKeyCheckBox.TabIndex = 23;
        muteHotKeyCheckBox.Text = "Enable hotkey";
        muteHotKeyCheckBox.UseVisualStyleBackColor = true;
        muteHotKeyCheckBox.CheckedChanged += MuteHotKeyCheckBox_CheckedChanged;
        // 
        // switchDeviceLabel
        // 
        switchDeviceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        switchDeviceLabel.AutoSize = true;
        switchDeviceLabel.Location = new System.Drawing.Point(12, 408);
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
        ClientSize = new System.Drawing.Size(670, 461);
        Controls.Add(switchDeviceLabel);
        Controls.Add(muteHotKey);
        Controls.Add(muteHotKeyCheckBox);
        Controls.Add(toggleMuteLabel);
        Controls.Add(hotKeyControl);
        Controls.Add(hotKeyCheckBox);
        Controls.Add(tabControl);
        Controls.Add(closeButton);
        MinimumSize = new System.Drawing.Size(686, 500);
        Text = "Settings";
        tabControl.ResumeLayout(false);
        playbackTabPage.ResumeLayout(false);
        recordingTabPage.ResumeLayout(false);
        profileTabPage.ResumeLayout(false);
        profilesPanel.ResumeLayout(false);
        profilesPanel.PerformLayout();
        appSettingTabPage.ResumeLayout(false);
        languageGroupBox.ResumeLayout(false);
        updateSettingsGroupBox.ResumeLayout(false);
        updateSettingsGroupBox.PerformLayout();
        audioSettingsGroupBox.ResumeLayout(false);
        audioSettingsGroupBox.PerformLayout();
        basicSettingsGroupBox.ResumeLayout(false);
        basicSettingsGroupBox.PerformLayout();
        notificationsTabPage.ResumeLayout(false);
        customSoundFileGroupBox.ResumeLayout(false);
        customSoundFileGroupBox.PerformLayout();
        notificationsGroupBox.ResumeLayout(false);
        notificationsGroupBox.PerformLayout();
        bannerOptionsGroupBox.ResumeLayout(false);
        bannerOptionsGroupBox.PerformLayout();
        microphoneMuteGroupBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)onScreenUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)opacityUpDown).EndInit();
        positionGroupBox.ResumeLayout(false);
        troubleshootingTabPage.ResumeLayout(false);
        configFileGroupBox.ResumeLayout(false);
        exportLogFilesGroupBox.ResumeLayout(false);
        resetAudioDevicesGroupBox.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.GroupBox bannerOptionsGroupBox;



    private System.Windows.Forms.Label microphoneUnmuteLabel;
    private System.Windows.Forms.Label microphoneMuteLabel;

    private System.Windows.Forms.GroupBox microphoneMuteGroupBox;

    private System.Windows.Forms.Label opacityLabel;

    private SoundSwitch.UI.Component.NumericUpDownWithUnits opacityUpDown;

    private System.Windows.Forms.Button selectCustomPositionButton;

    private System.Windows.Forms.RadioButton positionCustomRadioButton;


    private System.Windows.Forms.ComboBox microphoneUnmuteBannerComboBox;

    private System.Windows.Forms.GroupBox customSoundFileGroupBox;

    private System.Windows.Forms.CheckBox advancedModeCheckBox;

    #endregion

    private System.Windows.Forms.RadioButton positionTopLeftRadioButton;
    private System.Windows.Forms.RadioButton positionTopCenterRadioButton;
    private System.Windows.Forms.RadioButton positionTopRightRadioButton;
    private System.Windows.Forms.RadioButton positionCenterRightRadioButton;
    private System.Windows.Forms.RadioButton positionCenterRadioButton;
    private System.Windows.Forms.RadioButton positionCenterLeftRadioButton;
    private System.Windows.Forms.RadioButton positionBottomRightRadioButton;
    private System.Windows.Forms.RadioButton positionBottomCenterRadioButton;
    private System.Windows.Forms.RadioButton positionBottomLeftRadioButton;
    private System.Windows.Forms.GroupBox positionGroupBox;
    private System.Windows.Forms.Label switchProfileNotificationLabel;
    private System.Windows.Forms.ComboBox microphoneMuteNotificationComboBox;
    private System.Windows.Forms.Label switchDeviceNotificationLabel;
    private System.Windows.Forms.Label microphoneMuteNotificationLabel;
    private System.Windows.Forms.ComboBox switchProfileNotificationComboBox;
    private System.Windows.Forms.Panel profilesPanel;
    private System.Windows.Forms.TabPage notificationsTabPage;
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
    private System.Windows.Forms.CheckBox hotKeyCheckBox;
    private System.Windows.Forms.ComboBox iconChangeChoicesComboBox;
    private System.Windows.Forms.Label iconChangeLabel;
    private System.Windows.Forms.CheckBox includeBetaVersionsCheckBox;
    private System.Windows.Forms.ComboBox languageComboBox;
    private System.Windows.Forms.GroupBox languageGroupBox;
    private System.Windows.Forms.ComboBox switchDeviceNotificationComboBox;
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
    private System.Windows.Forms.TabPage appSoundLockTabPage;
    private System.Windows.Forms.Panel appSoundLockPanel;
    private System.Windows.Forms.Button addAppSoundRuleButton;
    private System.Windows.Forms.Button editAppSoundRuleButton;
    private System.Windows.Forms.Button deleteAppSoundRuleButton;
    private SoundSwitch.UI.Component.ListView.IconListView appSoundLockListView;
    private System.Windows.Forms.ComboBox tooltipInfoComboBox;
    private System.Windows.Forms.Label tooltipOnHoverLabel;
    private System.Windows.Forms.RadioButton updateNeverRadioButton;
    private System.Windows.Forms.RadioButton updateNotifyRadioButton;
    private System.Windows.Forms.GroupBox updateSettingsGroupBox;
    private System.Windows.Forms.RadioButton updateSilentRadioButton;
    private System.Windows.Forms.Label bannerShowOnLabel;
    private System.Windows.Forms.ComboBox bannerShowOnComboBox;
    private System.Windows.Forms.Label toggleMuteLabel;
    private SoundSwitch.UI.Component.HotKeyTextBox muteHotKey;
    private System.Windows.Forms.CheckBox muteHotKeyCheckBox;
    private System.Windows.Forms.CheckBox telemetryCheckbox;
    private System.Windows.Forms.CheckBox quickMenuCheckbox;
    private System.Windows.Forms.CheckBox keepVolumeCheckbox;
    private System.Windows.Forms.GroupBox notificationsGroupBox;
    private System.Windows.Forms.CheckBox singleNotificationCheckbox;
    private System.Windows.Forms.TabPage troubleshootingTabPage;
    private System.Windows.Forms.GroupBox resetAudioDevicesGroupBox;
    private System.Windows.Forms.Button resetAudioDevicesButton;
    private System.Windows.Forms.Label resetAudioDevicesLabel;
    private System.Windows.Forms.GroupBox exportLogFilesGroupBox;
    private System.Windows.Forms.Label exportLogFilesLabel;
    private System.Windows.Forms.Button exportLogFilesButton;
    private System.Windows.Forms.LinkLabel discordCommunityLinkLabel;
    private System.Windows.Forms.LinkLabel gitHubHelpLinkLabel;
    private System.Windows.Forms.Label appNameLabel;
    private System.Windows.Forms.LinkLabel donateLinkLabel;
    private System.Windows.Forms.Label troubleshootingLabel;
    private System.Windows.Forms.Label onScreenTimeLabel;
    private SoundSwitch.UI.Component.NumericUpDownWithUnits onScreenUpDown;
    private System.Windows.Forms.Label switchDeviceLabel;
    private System.Windows.Forms.ComboBox microphoneMuteBannerComboBox;
    private System.Windows.Forms.ComboBox iconDoubleClickComboBox;
    private System.Windows.Forms.Label iconDoubleClickLabel;
    private System.Windows.Forms.GroupBox configFileGroupBox;
    private System.Windows.Forms.Label configFileLabel;
    private System.Windows.Forms.Button exportConfigFileButton;
    private System.Windows.Forms.Button importConfigFileButton;
}
