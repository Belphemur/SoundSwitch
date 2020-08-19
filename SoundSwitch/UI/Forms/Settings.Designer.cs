using System;
using SoundSwitch.Localization;
using SoundSwitch.UI.Component.ListView;

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
            this.startWithWindowsCheckBox          = new System.Windows.Forms.CheckBox();
            this.closeButton                       = new System.Windows.Forms.Button();
            this.switchCommunicationDeviceCheckBox = new System.Windows.Forms.CheckBox();
            this.tabControl                        = new System.Windows.Forms.TabControl();
            this.playbackTabPage                   = new System.Windows.Forms.TabPage();
            this.playbackListView                  = new SoundSwitch.UI.Component.ListView.ListViewExtended();
            this.recordingTabPage                  = new System.Windows.Forms.TabPage();
            this.recordingListView                 = new SoundSwitch.UI.Component.ListView.ListViewExtended();
            this.tabProfile                        = new System.Windows.Forms.TabPage();
            this.editProfileButton                 = new System.Windows.Forms.Button();
            this.deleteProfileButton               = new System.Windows.Forms.Button();
            this.profileExplanationLabel           = new System.Windows.Forms.Label();
            this.profilesListView                  = new SoundSwitch.UI.Component.ListView.IconListView();
            this.addProfileButton                  = new System.Windows.Forms.Button();
            this.appSettingTabPage                 = new System.Windows.Forms.TabPage();
            this.languageGroupBox                  = new System.Windows.Forms.GroupBox();
            this.languageComboBox                  = new System.Windows.Forms.ComboBox();
            this.updateSettingsGroupBox            = new System.Windows.Forms.GroupBox();
            this.updateNeverRadioButton            = new System.Windows.Forms.RadioButton();
            this.updateNotifyRadioButton           = new System.Windows.Forms.RadioButton();
            this.updateSilentRadioButton           = new System.Windows.Forms.RadioButton();
            this.includeBetaVersionsCheckBox       = new System.Windows.Forms.CheckBox();
            this.audioSettingsGroupBox             = new System.Windows.Forms.GroupBox();
            this.foregroundAppCheckbox             = new System.Windows.Forms.CheckBox();
            this.deleteSoundButton                 = new System.Windows.Forms.Button();
            this.cycleThroughLabel                 = new System.Windows.Forms.Label();
            this.cycleThroughComboBox              = new System.Windows.Forms.ComboBox();
            this.tooltipOnHoverLabel               = new System.Windows.Forms.Label();
            this.tooltipInfoComboBox               = new System.Windows.Forms.ComboBox();
            this.selectSoundButton                 = new System.Windows.Forms.Button();
            this.notificationLabel                 = new System.Windows.Forms.Label();
            this.notificationComboBox              = new System.Windows.Forms.ComboBox();
            this.basicSettingsGroupBox             = new System.Windows.Forms.GroupBox();
            this.iconChangeLabel                   = new System.Windows.Forms.Label();
            this.iconChangeChoicesComboBox         = new System.Windows.Forms.ComboBox();
            this.selectSoundFileDialog             = new System.Windows.Forms.OpenFileDialog();
            this.hotkeysCheckBox                   = new System.Windows.Forms.CheckBox();
            this.hotKeyControl                     = new SoundSwitch.UI.Component.HotKeyTextBox();
            this.tabControl.SuspendLayout();
            this.playbackTabPage.SuspendLayout();
            this.recordingTabPage.SuspendLayout();
            this.tabProfile.SuspendLayout();
            this.appSettingTabPage.SuspendLayout();
            this.languageGroupBox.SuspendLayout();
            this.updateSettingsGroupBox.SuspendLayout();
            this.audioSettingsGroupBox.SuspendLayout();
            this.basicSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // startWithWindowsCheckBox
            // 
            this.startWithWindowsCheckBox.AutoSize                =  true;
            this.startWithWindowsCheckBox.Location                =  new System.Drawing.Point(6, 23);
            this.startWithWindowsCheckBox.Name                    =  "startWithWindowsCheckBox";
            this.startWithWindowsCheckBox.Size                    =  new System.Drawing.Size(181, 17);
            this.startWithWindowsCheckBox.TabIndex                =  7;
            this.startWithWindowsCheckBox.Text                    =  "Start automatically with Windows";
            this.startWithWindowsCheckBox.UseVisualStyleBackColor =  true;
            this.startWithWindowsCheckBox.CheckedChanged          += new System.EventHandler(this.RunAtStartup_CheckedChanged);
            // 
            // closeButton
            // 
            this.closeButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult            =  System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location                =  new System.Drawing.Point(673, 367);
            this.closeButton.Name                    =  "closeButton";
            this.closeButton.Size                    =  new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex                =  11;
            this.closeButton.Text                    =  "Close";
            this.closeButton.UseVisualStyleBackColor =  true;
            this.closeButton.Click                   += new System.EventHandler(this.closeButton_Click);
            // 
            // switchCommunicationDeviceCheckBox
            // 
            this.switchCommunicationDeviceCheckBox.AutoSize                =  true;
            this.switchCommunicationDeviceCheckBox.Location                =  new System.Drawing.Point(6, 23);
            this.switchCommunicationDeviceCheckBox.Name                    =  "switchCommunicationDeviceCheckBox";
            this.switchCommunicationDeviceCheckBox.Size                    =  new System.Drawing.Size(207, 17);
            this.switchCommunicationDeviceCheckBox.TabIndex                =  12;
            this.switchCommunicationDeviceCheckBox.Text                    =  "Switch Default Communication Device";
            this.switchCommunicationDeviceCheckBox.UseVisualStyleBackColor =  true;
            this.switchCommunicationDeviceCheckBox.CheckedChanged          += new System.EventHandler(this.communicationCheckbox_CheckedChanged);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.playbackTabPage);
            this.tabControl.Controls.Add(this.recordingTabPage);
            this.tabControl.Controls.Add(this.tabProfile);
            this.tabControl.Controls.Add(this.appSettingTabPage);
            this.tabControl.Location             =  new System.Drawing.Point(12, 6);
            this.tabControl.Name                 =  "tabControl";
            this.tabControl.SelectedIndex        =  0;
            this.tabControl.Size                 =  new System.Drawing.Size(743, 353);
            this.tabControl.TabIndex             =  13;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // playbackTabPage
            // 
            this.playbackTabPage.Controls.Add(this.playbackListView);
            this.playbackTabPage.Location                = new System.Drawing.Point(4, 22);
            this.playbackTabPage.Name                    = "playbackTabPage";
            this.playbackTabPage.Padding                 = new System.Windows.Forms.Padding(3);
            this.playbackTabPage.Size                    = new System.Drawing.Size(735, 327);
            this.playbackTabPage.TabIndex                = 0;
            this.playbackTabPage.Text                    = "Playback";
            this.playbackTabPage.UseVisualStyleBackColor = true;
            // 
            // playbackListView
            // 
            this.playbackListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.playbackListView.CheckBoxes  = true;
            this.playbackListView.Dock        = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header             = "Selected";
            listViewGroup1.HeaderAlignment    = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup1.Name               = "selectedGroup";
            this.playbackListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {listViewGroup1});
            this.playbackListView.HeaderStyle                     = System.Windows.Forms.ColumnHeaderStyle.None;
            this.playbackListView.HideSelection                   = false;
            this.playbackListView.Location                        = new System.Drawing.Point(3, 3);
            this.playbackListView.Name                            = "playbackListView";
            this.playbackListView.Size                            = new System.Drawing.Size(729, 321);
            this.playbackListView.TabIndex                        = 14;
            this.playbackListView.UseCompatibleStateImageBehavior = false;
            this.playbackListView.View                            = System.Windows.Forms.View.Details;
            // 
            // recordingTabPage
            // 
            this.recordingTabPage.Controls.Add(this.recordingListView);
            this.recordingTabPage.Location                = new System.Drawing.Point(4, 22);
            this.recordingTabPage.Name                    = "recordingTabPage";
            this.recordingTabPage.Padding                 = new System.Windows.Forms.Padding(3);
            this.recordingTabPage.Size                    = new System.Drawing.Size(735, 327);
            this.recordingTabPage.TabIndex                = 1;
            this.recordingTabPage.Text                    = "Recording";
            this.recordingTabPage.UseVisualStyleBackColor = true;
            // 
            // recordingListView
            // 
            this.recordingListView.AccessibleName = "recordingListView";
            this.recordingListView.BorderStyle    = System.Windows.Forms.BorderStyle.None;
            this.recordingListView.CheckBoxes     = true;
            this.recordingListView.Dock           = System.Windows.Forms.DockStyle.Fill;
            listViewGroup2.Header                 = "Selected";
            listViewGroup2.HeaderAlignment        = System.Windows.Forms.HorizontalAlignment.Center;
            listViewGroup2.Name                   = "selectedGroup";
            this.recordingListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {listViewGroup2});
            this.recordingListView.HeaderStyle                     = System.Windows.Forms.ColumnHeaderStyle.None;
            this.recordingListView.HideSelection                   = false;
            this.recordingListView.Location                        = new System.Drawing.Point(3, 3);
            this.recordingListView.Name                            = "recordingListView";
            this.recordingListView.Size                            = new System.Drawing.Size(729, 321);
            this.recordingListView.TabIndex                        = 17;
            this.recordingListView.UseCompatibleStateImageBehavior = false;
            this.recordingListView.View                            = System.Windows.Forms.View.Details;
            // 
            // tabProfile
            // 
            this.tabProfile.Controls.Add(this.editProfileButton);
            this.tabProfile.Controls.Add(this.deleteProfileButton);
            this.tabProfile.Controls.Add(this.profileExplanationLabel);
            this.tabProfile.Controls.Add(this.profilesListView);
            this.tabProfile.Controls.Add(this.addProfileButton);
            this.tabProfile.Location                = new System.Drawing.Point(4, 22);
            this.tabProfile.Name                    = "tabProfile";
            this.tabProfile.Padding                 = new System.Windows.Forms.Padding(3);
            this.tabProfile.Size                    = new System.Drawing.Size(735, 327);
            this.tabProfile.TabIndex                = 3;
            this.tabProfile.Text                    = "Profiles";
            this.tabProfile.UseVisualStyleBackColor = true;
            // 
            // editProfileButton
            // 
            this.editProfileButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.editProfileButton.Enabled                 =  false;
            this.editProfileButton.ImageAlign              =  System.Drawing.ContentAlignment.MiddleLeft;
            this.editProfileButton.Location                =  new System.Drawing.Point(518, 296);
            this.editProfileButton.Name                    =  "editProfileButton";
            this.editProfileButton.Size                    =  new System.Drawing.Size(100, 26);
            this.editProfileButton.TabIndex                =  5;
            this.editProfileButton.Text                    =  "Edit";
            this.editProfileButton.TextImageRelation       =  System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.editProfileButton.UseVisualStyleBackColor =  true;
            this.editProfileButton.Click                   += new System.EventHandler(this.editProfileButton_Click);
            // 
            // deleteProfileButton
            // 
            this.deleteProfileButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteProfileButton.Enabled                 =  false;
            this.deleteProfileButton.ImageAlign              =  System.Drawing.ContentAlignment.MiddleLeft;
            this.deleteProfileButton.Location                =  new System.Drawing.Point(624, 296);
            this.deleteProfileButton.Name                    =  "deleteProfileButton";
            this.deleteProfileButton.Size                    =  new System.Drawing.Size(100, 26);
            this.deleteProfileButton.TabIndex                =  4;
            this.deleteProfileButton.Text                    =  "Delete";
            this.deleteProfileButton.TextImageRelation       =  System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.deleteProfileButton.UseVisualStyleBackColor =  true;
            this.deleteProfileButton.Click                   += new System.EventHandler(this.deleteProfileButton_Click);
            // 
            // profileExplanationLabel
            // 
            this.profileExplanationLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.profileExplanationLabel.Location = new System.Drawing.Point(6, 264);
            this.profileExplanationLabel.Name     = "profileExplanationLabel";
            this.profileExplanationLabel.Size     = new System.Drawing.Size(718, 30);
            this.profileExplanationLabel.TabIndex = 3;
            this.profileExplanationLabel.Text     = "Explanation line 1\r\nOptional line 2";
            // 
            // profilesListView
            // 
            this.profilesListView.Anchor                          =  ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.profilesListView.FullRowSelect                   =  true;
            this.profilesListView.HideSelection                   =  false;
            this.profilesListView.Location                        =  new System.Drawing.Point(6, 3);
            this.profilesListView.Name                            =  "profilesListView";
            this.profilesListView.OwnerDraw                       =  true;
            this.profilesListView.ShowGroups                      =  false;
            this.profilesListView.Size                            =  new System.Drawing.Size(721, 254);
            this.profilesListView.TabIndex                        =  2;
            this.profilesListView.UseCompatibleStateImageBehavior =  false;
            this.profilesListView.View                            =  System.Windows.Forms.View.Details;
            this.profilesListView.SelectedIndexChanged            += new System.EventHandler(this.profilesListView_SelectedIndexChanged);
            this.profilesListView.DoubleClick                     += new System.EventHandler(this.profilesListView_DoubleClick);
            // 
            // addProfileButton
            // 
            this.addProfileButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addProfileButton.Location                =  new System.Drawing.Point(412, 296);
            this.addProfileButton.Name                    =  "addProfileButton";
            this.addProfileButton.Size                    =  new System.Drawing.Size(100, 26);
            this.addProfileButton.TabIndex                =  1;
            this.addProfileButton.Text                    =  "Add";
            this.addProfileButton.TextImageRelation       =  System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.addProfileButton.UseVisualStyleBackColor =  true;
            this.addProfileButton.Click                   += new System.EventHandler(this.addProfileButton_Click);
            // 
            // appSettingTabPage
            // 
            this.appSettingTabPage.Controls.Add(this.languageGroupBox);
            this.appSettingTabPage.Controls.Add(this.updateSettingsGroupBox);
            this.appSettingTabPage.Controls.Add(this.audioSettingsGroupBox);
            this.appSettingTabPage.Controls.Add(this.basicSettingsGroupBox);
            this.appSettingTabPage.Location                = new System.Drawing.Point(4, 22);
            this.appSettingTabPage.Name                    = "appSettingTabPage";
            this.appSettingTabPage.Size                    = new System.Drawing.Size(735, 327);
            this.appSettingTabPage.TabIndex                = 2;
            this.appSettingTabPage.Text                    = "Settings";
            this.appSettingTabPage.UseVisualStyleBackColor = true;
            // 
            // languageGroupBox
            // 
            this.languageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.languageGroupBox.Controls.Add(this.languageComboBox);
            this.languageGroupBox.Location = new System.Drawing.Point(434, 156);
            this.languageGroupBox.Name     = "languageGroupBox";
            this.languageGroupBox.Size     = new System.Drawing.Size(298, 61);
            this.languageGroupBox.TabIndex = 15;
            this.languageGroupBox.TabStop  = false;
            this.languageGroupBox.Text     = "Language";
            // 
            // languageComboBox
            // 
            this.languageComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled    =  true;
            this.languageComboBox.Location             =  new System.Drawing.Point(8, 23);
            this.languageComboBox.Name                 =  "languageComboBox";
            this.languageComboBox.Size                 =  new System.Drawing.Size(247, 21);
            this.languageComboBox.TabIndex             =  17;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // updateSettingsGroupBox
            // 
            this.updateSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.updateSettingsGroupBox.Controls.Add(this.updateNeverRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.updateNotifyRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.updateSilentRadioButton);
            this.updateSettingsGroupBox.Controls.Add(this.includeBetaVersionsCheckBox);
            this.updateSettingsGroupBox.Location = new System.Drawing.Point(434, 12);
            this.updateSettingsGroupBox.Name     = "updateSettingsGroupBox";
            this.updateSettingsGroupBox.Size     = new System.Drawing.Size(298, 132);
            this.updateSettingsGroupBox.TabIndex = 14;
            this.updateSettingsGroupBox.TabStop  = false;
            this.updateSettingsGroupBox.Text     = "Update Settings";
            // 
            // updateNeverRadioButton
            // 
            this.updateNeverRadioButton.AutoSize                =  true;
            this.updateNeverRadioButton.Location                =  new System.Drawing.Point(7, 71);
            this.updateNeverRadioButton.Name                    =  "updateNeverRadioButton";
            this.updateNeverRadioButton.Size                    =  new System.Drawing.Size(143, 17);
            this.updateNeverRadioButton.TabIndex                =  21;
            this.updateNeverRadioButton.TabStop                 =  true;
            this.updateNeverRadioButton.Text                    =  "Never check for updates";
            this.updateNeverRadioButton.UseVisualStyleBackColor =  true;
            this.updateNeverRadioButton.CheckedChanged          += new System.EventHandler(this.updateNeverRadioButton_CheckedChanged);
            // 
            // updateNotifyRadioButton
            // 
            this.updateNotifyRadioButton.AutoSize                =  true;
            this.updateNotifyRadioButton.Location                =  new System.Drawing.Point(7, 46);
            this.updateNotifyRadioButton.Name                    =  "updateNotifyRadioButton";
            this.updateNotifyRadioButton.Size                    =  new System.Drawing.Size(202, 17);
            this.updateNotifyRadioButton.TabIndex                =  20;
            this.updateNotifyRadioButton.TabStop                 =  true;
            this.updateNotifyRadioButton.Text                    =  "Notify me when updates are available";
            this.updateNotifyRadioButton.UseVisualStyleBackColor =  true;
            this.updateNotifyRadioButton.CheckedChanged          += new System.EventHandler(this.updateNotifyRadioButton_CheckedChanged);
            // 
            // updateSilentRadioButton
            // 
            this.updateSilentRadioButton.AutoSize                =  true;
            this.updateSilentRadioButton.Location                =  new System.Drawing.Point(7, 21);
            this.updateSilentRadioButton.Name                    =  "updateSilentRadioButton";
            this.updateSilentRadioButton.Size                    =  new System.Drawing.Size(157, 17);
            this.updateSilentRadioButton.TabIndex                =  19;
            this.updateSilentRadioButton.TabStop                 =  true;
            this.updateSilentRadioButton.Text                    =  "Install updates automatically";
            this.updateSilentRadioButton.UseVisualStyleBackColor =  true;
            this.updateSilentRadioButton.CheckedChanged          += new System.EventHandler(this.updateSilentRadioButton_CheckedChanged);
            // 
            // includeBetaVersionsCheckBox
            // 
            this.includeBetaVersionsCheckBox.AutoSize                =  true;
            this.includeBetaVersionsCheckBox.Location                =  new System.Drawing.Point(7, 103);
            this.includeBetaVersionsCheckBox.Name                    =  "includeBetaVersionsCheckBox";
            this.includeBetaVersionsCheckBox.Size                    =  new System.Drawing.Size(128, 17);
            this.includeBetaVersionsCheckBox.TabIndex                =  18;
            this.includeBetaVersionsCheckBox.Text                    =  "Include Beta versions";
            this.includeBetaVersionsCheckBox.UseVisualStyleBackColor =  true;
            this.includeBetaVersionsCheckBox.CheckedChanged          += new System.EventHandler(this.betaVersionCheckbox_CheckedChanged);
            // 
            // audioSettingsGroupBox
            // 
            this.audioSettingsGroupBox.Controls.Add(this.foregroundAppCheckbox);
            this.audioSettingsGroupBox.Controls.Add(this.deleteSoundButton);
            this.audioSettingsGroupBox.Controls.Add(this.cycleThroughLabel);
            this.audioSettingsGroupBox.Controls.Add(this.cycleThroughComboBox);
            this.audioSettingsGroupBox.Controls.Add(this.tooltipOnHoverLabel);
            this.audioSettingsGroupBox.Controls.Add(this.tooltipInfoComboBox);
            this.audioSettingsGroupBox.Controls.Add(this.switchCommunicationDeviceCheckBox);
            this.audioSettingsGroupBox.Controls.Add(this.selectSoundButton);
            this.audioSettingsGroupBox.Controls.Add(this.notificationLabel);
            this.audioSettingsGroupBox.Controls.Add(this.notificationComboBox);
            this.audioSettingsGroupBox.Location = new System.Drawing.Point(3, 117);
            this.audioSettingsGroupBox.Name     = "audioSettingsGroupBox";
            this.audioSettingsGroupBox.Size     = new System.Drawing.Size(425, 193);
            this.audioSettingsGroupBox.TabIndex = 13;
            this.audioSettingsGroupBox.TabStop  = false;
            this.audioSettingsGroupBox.Text     = "Audio Settings";
            // 
            // foregroundAppCheckbox
            // 
            this.foregroundAppCheckbox.AutoSize                =  true;
            this.foregroundAppCheckbox.Location                =  new System.Drawing.Point(6, 48);
            this.foregroundAppCheckbox.Name                    =  "foregroundAppCheckbox";
            this.foregroundAppCheckbox.Size                    =  new System.Drawing.Size(136, 17);
            this.foregroundAppCheckbox.TabIndex                =  25;
            this.foregroundAppCheckbox.Text                    =  "Switch Foreground app";
            this.foregroundAppCheckbox.UseVisualStyleBackColor =  true;
            this.foregroundAppCheckbox.CheckedChanged          += new System.EventHandler(this.ForegroundAppCheckbox_CheckedChanged);
            // 
            // deleteSoundButton
            // 
            this.deleteSoundButton.Image                   =  global::SoundSwitch.Properties.Resources.delete;
            this.deleteSoundButton.Location                =  new System.Drawing.Point(391, 77);
            this.deleteSoundButton.Name                    =  "deleteSoundButton";
            this.deleteSoundButton.Size                    =  new System.Drawing.Size(23, 23);
            this.deleteSoundButton.TabIndex                =  24;
            this.deleteSoundButton.UseVisualStyleBackColor =  true;
            this.deleteSoundButton.Visible                 =  false;
            this.deleteSoundButton.Click                   += new System.EventHandler(this.deleteSoundButton_Click);
            // 
            // cycleThroughLabel
            // 
            this.cycleThroughLabel.Location  = new System.Drawing.Point(2, 153);
            this.cycleThroughLabel.Name      = "cycleThroughLabel";
            this.cycleThroughLabel.Size      = new System.Drawing.Size(100, 15);
            this.cycleThroughLabel.TabIndex  = 23;
            this.cycleThroughLabel.Text      = "Cycle through";
            this.cycleThroughLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cycleThroughComboBox
            // 
            this.cycleThroughComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cycleThroughComboBox.FormattingEnabled    =  true;
            this.cycleThroughComboBox.Location             =  new System.Drawing.Point(108, 151);
            this.cycleThroughComboBox.Name                 =  "cycleThroughComboBox";
            this.cycleThroughComboBox.Size                 =  new System.Drawing.Size(247, 21);
            this.cycleThroughComboBox.TabIndex             =  22;
            this.cycleThroughComboBox.SelectedValueChanged += new System.EventHandler(this.cyclerComboBox_SelectedValueChanged);
            // 
            // tooltipOnHoverLabel
            // 
            this.tooltipOnHoverLabel.Location  = new System.Drawing.Point(2, 117);
            this.tooltipOnHoverLabel.Name      = "tooltipOnHoverLabel";
            this.tooltipOnHoverLabel.Size      = new System.Drawing.Size(100, 15);
            this.tooltipOnHoverLabel.TabIndex  = 21;
            this.tooltipOnHoverLabel.Text      = "Tooltip on Hover";
            this.tooltipOnHoverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tooltipInfoComboBox
            // 
            this.tooltipInfoComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tooltipInfoComboBox.FormattingEnabled    =  true;
            this.tooltipInfoComboBox.Location             =  new System.Drawing.Point(108, 115);
            this.tooltipInfoComboBox.Name                 =  "tooltipInfoComboBox";
            this.tooltipInfoComboBox.Size                 =  new System.Drawing.Size(247, 21);
            this.tooltipInfoComboBox.TabIndex             =  20;
            this.tooltipInfoComboBox.SelectedValueChanged += new System.EventHandler(this.tooltipInfoComboBox_SelectedValueChanged);
            // 
            // selectSoundButton
            // 
            this.selectSoundButton.Location                =  new System.Drawing.Point(361, 77);
            this.selectSoundButton.Name                    =  "selectSoundButton";
            this.selectSoundButton.Size                    =  new System.Drawing.Size(24, 23);
            this.selectSoundButton.TabIndex                =  19;
            this.selectSoundButton.Text                    =  "...";
            this.selectSoundButton.UseVisualStyleBackColor =  true;
            this.selectSoundButton.Visible                 =  false;
            this.selectSoundButton.Click                   += new System.EventHandler(this.selectSoundButton_Click);
            // 
            // notificationLabel
            // 
            this.notificationLabel.Location  = new System.Drawing.Point(2, 81);
            this.notificationLabel.Name      = "notificationLabel";
            this.notificationLabel.Size      = new System.Drawing.Size(100, 15);
            this.notificationLabel.TabIndex  = 17;
            this.notificationLabel.Text      = "Notification";
            this.notificationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // notificationComboBox
            // 
            this.notificationComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationComboBox.FormattingEnabled    =  true;
            this.notificationComboBox.Location             =  new System.Drawing.Point(108, 79);
            this.notificationComboBox.Name                 =  "notificationComboBox";
            this.notificationComboBox.Size                 =  new System.Drawing.Size(247, 21);
            this.notificationComboBox.TabIndex             =  16;
            this.notificationComboBox.SelectedValueChanged += new System.EventHandler(this.notificationComboBox_SelectedValueChanged);
            // 
            // basicSettingsGroupBox
            // 
            this.basicSettingsGroupBox.Controls.Add(this.iconChangeLabel);
            this.basicSettingsGroupBox.Controls.Add(this.startWithWindowsCheckBox);
            this.basicSettingsGroupBox.Controls.Add(this.iconChangeChoicesComboBox);
            this.basicSettingsGroupBox.Location = new System.Drawing.Point(3, 12);
            this.basicSettingsGroupBox.Name     = "basicSettingsGroupBox";
            this.basicSettingsGroupBox.Size     = new System.Drawing.Size(425, 93);
            this.basicSettingsGroupBox.TabIndex = 0;
            this.basicSettingsGroupBox.TabStop  = false;
            this.basicSettingsGroupBox.Text     = "Basic Settings";
            // 
            // iconChangeLabel
            // 
            this.iconChangeLabel.Location  = new System.Drawing.Point(2, 53);
            this.iconChangeLabel.Name      = "iconChangeLabel";
            this.iconChangeLabel.Size      = new System.Drawing.Size(100, 15);
            this.iconChangeLabel.TabIndex  = 27;
            this.iconChangeLabel.Text      = "Systray Icon";
            this.iconChangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // iconChangeChoicesComboBox
            // 
            this.iconChangeChoicesComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.iconChangeChoicesComboBox.FormattingEnabled    =  true;
            this.iconChangeChoicesComboBox.Location             =  new System.Drawing.Point(108, 51);
            this.iconChangeChoicesComboBox.Name                 =  "iconChangeChoicesComboBox";
            this.iconChangeChoicesComboBox.Size                 =  new System.Drawing.Size(247, 21);
            this.iconChangeChoicesComboBox.TabIndex             =  26;
            this.iconChangeChoicesComboBox.SelectedIndexChanged += new System.EventHandler(this.iconChangeChoicesComboBox_SelectedIndexChanged);
            // 
            // selectSoundFileDialog
            // 
            this.selectSoundFileDialog.FileName = "customSound";
            // 
            // hotkeysCheckBox
            // 
            this.hotkeysCheckBox.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotkeysCheckBox.AutoSize                =  true;
            this.hotkeysCheckBox.Location                =  new System.Drawing.Point(163, 372);
            this.hotkeysCheckBox.Name                    =  "hotkeysCheckBox";
            this.hotkeysCheckBox.Size                    =  new System.Drawing.Size(94, 17);
            this.hotkeysCheckBox.TabIndex                =  20;
            this.hotkeysCheckBox.Text                    =  "Enable hotkey";
            this.hotkeysCheckBox.UseVisualStyleBackColor =  true;
            this.hotkeysCheckBox.CheckedChanged          += new System.EventHandler(this.hotkeysCheckbox_CheckedChanged);
            // 
            // hotKeyControl
            // 
            this.hotKeyControl.Anchor        =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.hotKeyControl.Location      =  new System.Drawing.Point(19, 369);
            this.hotKeyControl.Name          =  "hotKeyControl";
            this.hotKeyControl.Size          =  new System.Drawing.Size(138, 20);
            this.hotKeyControl.TabIndex      =  21;
            this.hotKeyControl.TextAlign     =  System.Windows.Forms.HorizontalAlignment.Center;
            this.hotKeyControl.HotKeyChanged += new System.EventHandler<SoundSwitch.UI.Component.HotKeyTextBox.Event>(this.hotKeyControl_HotKeyChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton        = this.closeButton;
            this.ClientSize          = new System.Drawing.Size(771, 404);
            this.Controls.Add(this.hotKeyControl);
            this.Controls.Add(this.hotkeysCheckBox);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.closeButton);
            this.MinimumSize = new System.Drawing.Size(787, 443);
            this.Name        = "SettingsForm";
            this.Text        = "Settings";
            this.tabControl.ResumeLayout(false);
            this.playbackTabPage.ResumeLayout(false);
            this.recordingTabPage.ResumeLayout(false);
            this.tabProfile.ResumeLayout(false);
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
        private System.Windows.Forms.Label notificationLabel;
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
        private System.Windows.Forms.TabPage tabProfile;
        private System.Windows.Forms.ComboBox tooltipInfoComboBox;
        private System.Windows.Forms.Label tooltipOnHoverLabel;
        private System.Windows.Forms.RadioButton updateNeverRadioButton;
        private System.Windows.Forms.RadioButton updateNotifyRadioButton;
        private System.Windows.Forms.GroupBox updateSettingsGroupBox;
        private System.Windows.Forms.RadioButton updateSilentRadioButton;

        #endregion
    }
}