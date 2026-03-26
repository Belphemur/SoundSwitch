/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2017 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.Logger.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.TrayIcon.IconChanger;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;
using SoundSwitch.UI.Component.ListView;
using SoundSwitch.Util;
using SoundSwitch.Util.Url;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;

namespace SoundSwitch.UI.Forms;

public sealed partial class SettingsForm : Form
{
    private static readonly Icon ResourceSettingsIcon = Resources.SettingsIcon;
    private static readonly IconHandle FallbackAppIconHandle = IconExtractor.CreatePermanent(Resources.program);

    private readonly bool _loaded;
    private readonly IAudioDeviceLister _audioDeviceLister;
    private readonly BannerManager _bannerManager = new();

    private const int RECT_PEN_WIDTH = 4;
    private const int OFFSET_W = 15;
    private const int OFFSET_H = 12;

    private static Pen PenLine(int width = 1) => new(Color.Gainsboro, width);

    private static Rectangle RectOutline(int offsetW, int offsetH, Control topLeft, Control bottomRight) =>
        new(topLeft.Location.X - offsetW, topLeft.Location.Y - offsetH,
            bottomRight.Location.X + bottomRight.Width - topLeft.Location.X + offsetW * 2,
            bottomRight.Location.Y + bottomRight.Height - topLeft.Location.Y + offsetH * 2);
    
    private static Point CenterPoint(Control control, Control container = null) =>
        new((container != null ? container.Location.X : 0) + control.Location.X + control.Width / 2,
            (container != null ? container.Location.Y : 0) + control.Location.Y + control.Height / 2);

    public SettingsForm(IAudioDeviceLister audioDeviceLister)
    {
        _audioDeviceLister = audioDeviceLister;
        // Form itself
        InitializeComponent();
        Icon = ResourceSettingsIcon;
        Text = AssemblyUtils.GetReleaseState() == AssemblyUtils.ReleaseState.Beta
            ? $"{SettingsStrings.settings} {AssemblyUtils.GetReleaseState()}"
            : SettingsStrings.settings;
        LocalizeForm();

        new ToolTip().SetToolTip(closeButton, SettingsStrings.buttonClose_tooltip);

        hotKeyControl.HotKey = AppConfigs.Configuration.PlaybackHotKey;
        hotKeyControl.Tag =
            new Tuple<HotKeyAction, HotKey>(HotKeyAction.Playback, AppConfigs.Configuration.PlaybackHotKey);
        hotKeyControl.Enabled = hotKeyCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKey.Enabled;

        muteHotKey.HotKey = AppConfigs.Configuration.MuteRecordingHotKey;
        muteHotKey.Tag = new Tuple<HotKeyAction, HotKey>(HotKeyAction.Mute, AppConfigs.Configuration.MuteRecordingHotKey);
        muteHotKey.Enabled = muteHotKeyCheckBox.Checked = AppConfigs.Configuration.MuteRecordingHotKey.Enabled;

        new ToolTip().SetToolTip(hotKeyCheckBox, SettingsStrings.hotkey_tooltip);

        // Settings - Basic
        startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;

        new IconChangerFactory().ConfigureListControl(iconChangeChoicesComboBox);
        iconChangeChoicesComboBox.SelectedValue = AppConfigs.Configuration.SwitchIcon;
        new ToolTip().SetToolTip(iconChangeChoicesComboBox, SettingsStrings.iconChange_tooltip);

        new IconDoubleClickFactory().ConfigureListControl(iconDoubleClickComboBox);
        iconDoubleClickComboBox.SelectedValue = AppConfigs.Configuration.IconDoubleClick;
        new ToolTip().SetToolTip(iconDoubleClickComboBox, SettingsStrings.iconDoubleClick_tooltip);

        // Settings - Audio
        switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;
        new ToolTip().SetToolTip(switchCommunicationDeviceCheckBox, SettingsStrings.communicationsDevice_tooltip);

        foregroundAppCheckbox.Checked = AppModel.Instance.SwitchForegroundProgram;
        new ToolTip().SetToolTip(foregroundAppCheckbox, SettingsStrings.foregroundApp_tooltip);

        quickMenuCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance,
            nameof(AppModel.QuickMenuEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
        new ToolTip().SetToolTip(quickMenuCheckbox, SettingsStrings.quickMenu_tooltip);

        keepVolumeCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance,
            nameof(AppModel.KeepVolumeEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
        new ToolTip().SetToolTip(keepVolumeCheckbox, SettingsStrings.keepVolume_tooltip);

        new TooltipInfoFactory().ConfigureListControl(tooltipInfoComboBox);
        tooltipInfoComboBox.SelectedValue = TooltipInfoManager.CurrentTooltipInfo;

        new DeviceCyclerFactory().ConfigureListControl(cycleThroughComboBox);
        cycleThroughComboBox.SelectedValue = DeviceCyclerManager.CurrentCycler;
        new ToolTip().SetToolTip(cycleThroughComboBox, SettingsStrings.cycleThrough_tooltip);

        // Settings - Notification
        var isAdvancedMode = AppModel.Instance.NotificationAdvancedMode;
        advancedModeCheckBox.Checked = isAdvancedMode;
        NotificationAdvancedMode(isAdvancedMode);

        var notificationFactory = new NotificationFactory();
        notificationFactory.ConfigureListControl(switchDeviceNotificationComboBox);
        notificationFactory.ConfigureListControl(switchProfileNotificationComboBox);
        notificationFactory.ConfigureListControl(microphoneMuteNotificationComboBox);
        switchDeviceNotificationComboBox.SelectedValue = AppModel.Instance.SwitchDeviceNotification;
        switchProfileNotificationComboBox.SelectedValue = AppModel.Instance.SwitchProfileNotification;
        microphoneMuteNotificationComboBox.SelectedValue = AppModel.Instance.MicrophoneMuteNotification;
        new ToolTip().SetToolTip(switchDeviceNotificationComboBox, SettingsStrings.notification_tooltip);
        new ToolTip().SetToolTip(switchProfileNotificationComboBox, SettingsStrings.notification_tooltip);
        new ToolTip().SetToolTip(microphoneMuteNotificationComboBox, SettingsStrings.notification_tooltip);

        onScreenUpDown.DataBindings.Add(nameof(NumericUpDown.Value), AppModel.Instance,
            nameof(AppModel.BannerOnScreenTimeSecs), false, DataSourceUpdateMode.OnPropertyChanged);
        var onScreenTimeTooltip = new ToolTip();
        onScreenTimeTooltip.SetToolTip(onScreenUpDown, SettingsStrings.banner_onscreen_time_tooltip);
        onScreenTimeTooltip.SetToolTip(onScreenTimeLabel, SettingsStrings.banner_onscreen_time_tooltip);

        opacityUpDown.DataBindings.Add(nameof(NumericUpDown.Value), AppModel.Instance,
            nameof(AppModel.BannerOpacityPercentage), false, DataSourceUpdateMode.OnPropertyChanged);
        new ToolTip().SetToolTip(opacityUpDown, SettingsStrings.banner_opacity_tooltip);

        singleNotificationCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance,
            nameof(AppModel.IsSingleNotification), false, DataSourceUpdateMode.OnPropertyChanged);
        new ToolTip().SetToolTip(singleNotificationCheckbox, SettingsStrings.notification_single_tooltip);

        usePrimaryScreenCheckbox.Checked = AppModel.Instance.NotifyUsingPrimaryScreen;
        new ToolTip().SetToolTip(usePrimaryScreenCheckbox, SettingsStrings.usePrimaryScreen_tooltip);

        new MicrophoneMuteFactory().ConfigureListControl(microphoneMuteBannerComboBox);
        microphoneMuteBannerComboBox.SelectedValue = AppModel.Instance.MicrophoneMuteBanner;
        new ToolTip().SetToolTip(microphoneMuteBannerComboBox, SettingsStrings.banner_mute_tooltip);

        new MicrophoneMuteFactory().ConfigureListControl(microphoneUnmuteBannerComboBox);
        microphoneUnmuteBannerComboBox.SelectedValue = AppModel.Instance.MicrophoneUnmuteBanner;
        new ToolTip().SetToolTip(microphoneUnmuteBannerComboBox, SettingsStrings.banner_mute_tooltip);

        new BannerDisplayInfoFactory().ConfigureListControl(bannerDisplayComboBox);
        bannerDisplayComboBox.SelectedValue = AppModel.Instance.BannerDisplayInfo;
        new ToolTip().SetToolTip(bannerDisplayComboBox, SettingsStrings.banner_displayInfo_tooltip);

        CustomSoundNotificationCheck();

        selectSoundFileDialog.Filter = SettingsStrings.audioFiles + @" (*.wav;*.mp3)|*.wav;*.mp3;*.aiff";
        selectSoundFileDialog.FileOk += SelectSoundFileDialog_FileOk;
        selectSoundFileDialog.CheckFileExists = true;
        selectSoundFileDialog.CheckPathExists = true;

        deleteSoundButton.Visible = AppModel.Instance.CustomNotificationSound != null;
        new ToolTip().SetToolTip(deleteSoundButton, SettingsStrings.disableCustomSound_tooltip);        
        new ToolTip().SetToolTip(selectSoundButton, SettingsStrings.selectSoundButton_tooltip);

        switch (AppModel.Instance.BannerPosition)
        {
            case BannerPosition.TopLeft:
                positionTopLeftRadioButton.Checked = true;
                break;
            case BannerPosition.TopCenter:
                positionTopCenterRadioButton.Checked = true;
                break;
            case BannerPosition.TopRight:
                positionTopRightRadioButton.Checked = true;
                break;
            case BannerPosition.CenterLeft:
                positionCenterLeftRadioButton.Checked = true;
                break;
            case BannerPosition.Center:
                positionCenterRadioButton.Checked = true;
                break;
            case BannerPosition.CenterRight:
                positionCenterRightRadioButton.Checked = true;
                break;
            case BannerPosition.BottomLeft:
                positionBottomLeftRadioButton.Checked = true;
                break;
            case BannerPosition.BottomCenter:
                positionBottomCenterRadioButton.Checked = true;
                break;
            case BannerPosition.BottomRight:
                positionBottomRightRadioButton.Checked = true;
                break;
            case BannerPosition.Custom:
                positionCustomRadioButton.Checked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Settings - Update
        includeBetaVersionsCheckBox.Checked = AppModel.Instance.IncludeBetaVersions;

        switch (AppModel.Instance.UpdateMode)
        {
            case UpdateMode.Silent:
                updateSilentRadioButton.Checked = true;
                break;
            case UpdateMode.Notify:
                updateNotifyRadioButton.Checked = true;
                break;
            case UpdateMode.Never:
                updateNeverRadioButton.Checked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        new ToolTip().SetToolTip(updateSilentRadioButton, SettingsStrings.updateInstallAutomatically_tooltip);
        new ToolTip().SetToolTip(updateNotifyRadioButton, SettingsStrings.updateNotify_tooltip);
        new ToolTip().SetToolTip(updateNeverRadioButton, SettingsStrings.updateNever_tooltip);
        new ToolTip().SetToolTip(includeBetaVersionsCheckBox, SettingsStrings.updateIncludeBetaVersions_tooltip);

        // Settings - Language
        new LanguageFactory().ConfigureListControl(languageComboBox);
        languageComboBox.SelectedValue = AppModel.Instance.Language;

        muteHotKey.Visible = false;
        muteHotKeyCheckBox.Visible = false;
        toggleMuteLabel.Visible = false;

        telemetryCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance, nameof(AppModel.Telemetry), false, DataSourceUpdateMode.OnPropertyChanged);
        new ToolTip().SetToolTip(telemetryCheckbox, SettingsStrings.telemetry_tooltip);

        PopulateSettings();

        _loaded = true;
    }

    private void PopulateSettings()
    {
        PopulateAudioDevices();
        playbackListView.SetGroupsState(ListViewGroupState.Collapsible);
        recordingListView.SetGroupsState(ListViewGroupState.Collapsible);
        // Profiles
        PopulateProfiles();
    }

    private void PopulateProfiles()
    {
        profilesListView.Columns.Add(SettingsStrings.profile_name, 50, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.profile_program, 100, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.hotkey, 150, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.playback, 150, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.recording, 150, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.communication, 150, HorizontalAlignment.Left);
        profilesListView.Columns.Add(SettingsStrings.communication, 150, HorizontalAlignment.Left);

        RefreshProfiles();
    }

    public void RefreshProfiles()
    {
        ListViewItem ProfileToListViewItem(Profile profile)
        {
            var listViewItem = new ListViewItem(profile.Name) { Tag = profile };
            IconHandle appIconHandle = null;
            DeviceFullInfo recording = null;
            DeviceFullInfo playback = null;
            DeviceFullInfo communication = null;
            DeviceFullInfo recordingCommunication = null;

            var applicationTrigger = profile.Triggers.FirstOrDefault(trig => trig.Type == TriggerFactory.Enum.Process);
            var hotkeyTrigger = profile.Triggers.FirstOrDefault(trig => trig.Type == TriggerFactory.Enum.HotKey);

            if (applicationTrigger != null)
            {
                try
                {
                    appIconHandle = IconExtractor.Extract(applicationTrigger.ApplicationPath, 0, false);
                }
                catch
                {
                    appIconHandle = FallbackAppIconHandle.Acquire();
                }
            }

            if (profile.Playback != null)
            {
                playback = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).FirstOrDefault(info =>
                    info.Equals(profile.Playback));
            }

            if (profile.Recording != null)
            {
                recording = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active).FirstOrDefault(info =>
                    info.Equals(profile.Recording));
            }

            if (profile.Communication != null)
            {
                communication = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).FirstOrDefault(info =>
                    info.Equals(profile.Communication));
            }

            if (profile.RecordingCommunication != null)
            {
                recordingCommunication = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active).FirstOrDefault(info =>
                    info.Equals(profile.RecordingCommunication));
            }

            listViewItem.SubItems.AddRange(new[]
            {
                new ListViewItem.ListViewSubItem(listViewItem, applicationTrigger?.ApplicationPath.Split('\\').Last() ?? "")
                    { Tag = appIconHandle },
                new ListViewItem.ListViewSubItem(listViewItem, hotkeyTrigger?.HotKey.ToString() ?? ""),
                new ListViewItem.ListViewSubItem(listViewItem, playback?.NameClean ?? profile.Playback?.ToString() ?? "")
                    { Tag = playback?.SmallIcon },
                new ListViewItem.ListViewSubItem(listViewItem,
                    recording?.NameClean ?? profile.Recording?.ToString() ?? "") { Tag = recording?.SmallIcon },
                new ListViewItem.ListViewSubItem(listViewItem,
                    communication?.NameClean ?? profile.Communication?.ToString() ?? "") { Tag = communication?.SmallIcon },
                new ListViewItem.ListViewSubItem(listViewItem,
                    recordingCommunication?.NameClean ?? profile.RecordingCommunication?.ToString() ?? "") { Tag = recordingCommunication?.SmallIcon },
            });

            return listViewItem;
        }

        DisposeProfileListViewIconHandles();
        profilesListView.Items.Clear();

        foreach (var profile in AppModel.Instance.ProfileManager.Profiles)
        {
            var listViewItem = ProfileToListViewItem(profile);
            profilesListView.Items.Add(listViewItem);
        }

        if (AppModel.Instance.ProfileManager.Profiles.Count <= 0) return;
        foreach (ColumnHeader column in profilesListView.Columns)
            column.Width = -2;
    }

    /// <summary>
    /// Disposes all <see cref="IconHandle"/> objects stored in the profile list-view sub-item Tags.
    /// Must be called before <c>profilesListView.Items.Clear()</c> so that GDI references are
    /// released promptly.
    /// </summary>
    private void DisposeProfileListViewIconHandles()
    {
        foreach (ListViewItem item in profilesListView.Items)
        {
            foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
            {
                if (subItem.Tag is IconHandle iconHandle)
                    iconHandle.Dispose();
            }
        }
    }

    private void PopulateAudioDevices()
    {
        var selectedDevices = AppModel.Instance.SelectedDevices;
        PopulateAudioList(playbackListView, selectedDevices, _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged));
        PopulateAudioList(recordingListView, selectedDevices, _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged));
    }

    private void LocalizeForm()
    {
        RightToLeft = new LanguageFactory().Get(AppModel.Instance.Language).IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
        // TabPages
        playbackTabPage.Text = SettingsStrings.playback;
        playbackListView.Groups[0].Header = SettingsStrings.selected;

        recordingTabPage.Text = SettingsStrings.recording;
        recordingListView.Groups[0].Header = SettingsStrings.selected;

        profileTabPage.Text = SettingsStrings.profile_tab;
        appSettingTabPage.Text = SettingsStrings.general;
        notificationsTabPage.Text = SettingsStrings.notifications;
        appSoundLockTabPage.Text = SettingsStrings.appSoundLock_tab;
        troubleshootingTabPage.Text = SettingsStrings.troubleshooting;

        // Settings - Basic
        basicSettingsGroupBox.Text = SettingsStrings.basicSettings;
        startWithWindowsCheckBox.Text = SettingsStrings.startWithWindows;
        iconChangeLabel.Text = SettingsStrings.iconChange;
        iconDoubleClickLabel.Text = SettingsStrings.iconDoubleClick;

        // Settings - Audio
        audioSettingsGroupBox.Text = SettingsStrings.audioSettings;
        switchCommunicationDeviceCheckBox.Text = SettingsStrings.communicationsDevice;
        tooltipOnHoverLabel.Text = SettingsStrings.tooltipOnHover;
        cycleThroughLabel.Text = SettingsStrings.cycleThrough;
        foregroundAppCheckbox.Text = SettingsStrings.foregroundApp;
        quickMenuCheckbox.Text = SettingsStrings.quickMenu;
        keepVolumeCheckbox.Text = SettingsStrings.keepVolume;

        // Settings - Profile
        profileExplanationLabel.Text = SettingsStrings.profile_explanation;
        addProfileButton.Text = SettingsStrings.buttonAdd;
        deleteProfileButton.Text = SettingsStrings.buttonDelete;
        editProfileButton.Text = SettingsStrings.buttonEdit;

        // Settings - Update
        updateSettingsGroupBox.Text = SettingsStrings.updateSettings;
        updateSilentRadioButton.Text = SettingsStrings.updateInstallAutomatically;
        updateNotifyRadioButton.Text = SettingsStrings.updateNotify;
        updateNeverRadioButton.Text = SettingsStrings.updateNever;
        includeBetaVersionsCheckBox.Text = SettingsStrings.updateIncludeBetaVersions;
        telemetryCheckbox.Text = SettingsStrings.telemetry;

        // Settings - Language
        languageGroupBox.Text = SettingsStrings.language;

        // Settings - Notification
        notificationsGroupBox.Text = SettingsStrings.notification_type;
        customSoundFileGroupBox.Text = SettingsStrings.customSoundFile;
        bannerOptionsGroupBox.Text = SettingsStrings.notification_bannerOptions;
        positionGroupBox.Text = SettingsStrings.position;
        onScreenTimeLabel.Text = SettingsStrings.banner_onscreen_time;
        onScreenUpDown.TextUnit = "s";
        usePrimaryScreenCheckbox.Text = SettingsStrings.usePrimaryScreen;
        singleNotificationCheckbox.Text = SettingsStrings.notification_single;
        opacityLabel.Text = SettingsStrings.banner_opacity;
        opacityUpDown.TextUnit = "%";
        displayInfoLabel.Text = SettingsStrings.banner_displayInfo;

        // Settings - Troubleshooting
        resetAudioDevicesGroupBox.Text = SettingsStrings.resetAudioDevices;
        resetAudioDevicesLabel.Text = SettingsStrings.resetAudioDevices_desc;
        resetAudioDevicesButton.Text = SettingsStrings.buttonReset;

        exportLogFilesGroupBox.Text = SettingsStrings.exportLogFiles;
        exportLogFilesLabel.Text = SettingsStrings.exportLogFiles_desc;
        exportLogFilesButton.Text = SettingsStrings.buttonExport;

        configFileGroupBox.Text = SettingsStrings.importExportConfigFile;
        configFileLabel.Text = SettingsStrings.importExportConfigFile_desc;
        exportConfigFileButton.Text = SettingsStrings.buttonExport;
        importConfigFileButton.Text = SettingsStrings.buttonImport;

        appNameLabel.Text = Application.ProductName;
        troubleshootingLabel.Text = SettingsStrings.troubleshooting_desc;
        gitHubHelpLinkLabel.Text = SettingsStrings.link_help;
        discordCommunityLinkLabel.Text = SettingsStrings.link_community;
        donateLinkLabel.Text = SettingsStrings.link_donate;

        // Misc
        hotKeyCheckBox.Text = SettingsStrings.hotkeyEnabled;
        closeButton.Text = SettingsStrings.buttonClose;
        switchDeviceLabel.Text = SettingsStrings.switchDevice;
        toggleMuteLabel.Text = SettingsStrings.mute_toggle_label;
        muteHotKeyCheckBox.Text = SettingsStrings.hotkeyEnabled;

        addProfileButton.Image = Resources.profile_menu_add;
        editProfileButton.Image = Resources.profile_menu_edit;
        deleteProfileButton.Image = Resources.profile_menu_delete;

        addAppSoundRuleButton.Text = SettingsStrings.buttonAdd;
        editAppSoundRuleButton.Text = SettingsStrings.buttonEdit;
        deleteAppSoundRuleButton.Text = SettingsStrings.buttonDelete;

        addAppSoundRuleButton.Image = Resources.profile_menu_add;
        editAppSoundRuleButton.Image = Resources.profile_menu_edit;
        deleteAppSoundRuleButton.Image = Resources.profile_menu_delete;
    }

    private void CloseButton_Click(object sender, EventArgs e) => Close();

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Dispose all IconHandle objects stored in the profile list-view so GDI handles are
        // released promptly rather than waiting for finalizer collection.
        DisposeProfileListViewIconHandles();
        base.OnFormClosed(e);
    }

    private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        var tabControlSender = (TabControl)sender;
        if (tabControlSender.SelectedTab == playbackTabPage)
        {
            SetHotKeyFieldsVisibility(true, false);
            SetHotKeyValues(AppConfigs.Configuration.PlaybackHotKey, HotKeyAction.Playback);
        }
        else if (tabControlSender.SelectedTab == recordingTabPage)
        {
            SetHotKeyFieldsVisibility(true, true);
            SetHotKeyValues(AppConfigs.Configuration.RecordingHotKey, HotKeyAction.Recording);
        }
        else if (tabControlSender.SelectedTab == appSoundLockTabPage)
        {
            SetHotKeyFieldsVisibility(false, false);
            PopulateAppSoundLockRules();
        }
        else
        {
            SetHotKeyFieldsVisibility(false, false);
        }
    }

    private void SetHotKeyValues(HotKey hotkey, HotKeyAction action)
    {
        hotKeyControl.HotKey = hotkey;
        hotKeyControl.Tag = new Tuple<HotKeyAction, HotKey>(action, hotkey);
        hotKeyCheckBox.Checked = hotkey.Enabled;
    }

    private void SetHotKeyFieldsVisibility(bool switchHotKeyVisibility, bool muteHotKeyVisibility)
    {
        hotKeyControl.Visible = switchHotKeyVisibility;
        hotKeyCheckBox.Visible = switchHotKeyVisibility;
        switchDeviceLabel.Visible = switchHotKeyVisibility;

        muteHotKey.Visible = muteHotKeyVisibility;
        muteHotKeyCheckBox.Visible = muteHotKeyVisibility;
        toggleMuteLabel.Visible = muteHotKeyVisibility;
    }

    private void HotKeyCheckBox_CheckedChanged(object sender, EventArgs e) =>
        ForceSetHotkeys(sender, hotKeyControl);

    private void MuteHotKeyCheckBox_CheckedChanged(object sender, EventArgs e) =>
        ForceSetHotkeys(sender, muteHotKey);

    private static void ForceSetHotkeys(object sender, HotKeyTextBox hotKeyTextBox)
    {
        var control = (CheckBox)sender;
        if (hotKeyTextBox.Tag == null) return;

        var (action, hotKey) = (Tuple<HotKeyAction, HotKey>)hotKeyTextBox.Tag;
        var currentState = hotKey.Enabled;
        hotKeyTextBox.Enabled = hotKey.Enabled = control.Checked;
        if (currentState != hotKey.Enabled)
            AppModel.Instance.SetHotkeyCombination(hotKey, action, true);
    }

    private void HotKeyControl_HotKeyChanged(object sender, HotKeyTextBox.Event e)
    {
        var control = (HotKeyTextBox)sender;
        var tuple = (Tuple<HotKeyAction, HotKey>)control.Tag;
        if (tuple == null) return;

        var newTuple = new Tuple<HotKeyAction, HotKey>(tuple.Item1, control.HotKey);
        hotKeyControl.Tag = newTuple;

        AppModel.Instance.SetHotkeyCombination(newTuple.Item2, newTuple.Item1);
    }

    private void SetComboBoxValue<T>(object sender, Action<DisplayEnumObject<T>> saveSetting) where T : Enum, IConvertible
    {
        if (!_loaded) return;
        var selectedItem = (DisplayEnumObject<T>)((ComboBox)sender).SelectedItem;
        if (selectedItem == null) return;
        saveSetting(selectedItem);
    }

    #region Device List Playback

    private void PopulateAudioList(ListView listView, IEnumerable<DeviceInfo> selectedDevices, IEnumerable<DeviceFullInfo> audioDevices)
    {
        try
        {
            PopulateDeviceTypeGroups(listView);

            listView.SmallImageList = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit
            };

            listView.Columns.Add("Device", -3, HorizontalAlignment.Center);
            var items =
                audioDevices.Select(device =>
                    {
                        AddDeviceIconSmallImage(device, listView);

                        return GenerateListViewItem(device, selectedDevices, listView);
                    })
                    .OrderBy(item => item.Text);
            listView.Items.AddRange(items.ToArray());
        }
        finally
        {
            listView.ItemCheck += ListViewItemChecked;
        }
    }

    /// <summary>
    /// Using the information of the AudioDeviceWrapper, generate a ListViewItem
    /// </summary>
    /// <param name="device"></param>
    /// <param name="selected"></param>
    /// <param name="listView"></param>
    /// <returns></returns>
    private ListViewItem GenerateListViewItem(DeviceFullInfo device, IEnumerable<DeviceInfo> selected, ListView listView)
    {
        var listViewItem = new ListViewItem
        {
            Text = device.NameClean,
            ImageKey = device.IconPath,
            Tag = device
        };
        var isSelected = selected.Contains(device);
        if (device.State == DeviceState.Active && isSelected)
            listViewItem.Group = listView.Groups["selectedGroup"];
        else
            listViewItem.Group = GetGroup(device.State, listView);

        listViewItem.Checked = isSelected;
        return listViewItem;
    }

    /// <summary>
    /// Using the DeviceClassIconPath, get the Icon
    /// </summary>
    /// <param name="device"></param>
    /// <param name="listView"></param>
    private void AddDeviceIconSmallImage(DeviceFullInfo device, ListView listView)
    {
        if (!listView.SmallImageList.Images.ContainsKey(device.IconPath))
        {
            using var iconHandle = device.LargeIcon;
            listView.SmallImageList.Images.Add(device.IconPath, iconHandle.Icon);
        }
    }

    private void ListViewItemChecked(object sender, ItemCheckEventArgs e)
    {
        try
        {
            switch (e.NewValue)
            {
                case CheckState.Checked:
                    AppModel.Instance.SelectDevice((DeviceFullInfo)((ListView)sender).Items[e.Index].Tag);
                    break;
                case CheckState.Unchecked:
                    AppModel.Instance.UnselectDevice((DeviceFullInfo)((ListView)sender).Items[e.Index].Tag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception)
        {
            e.NewValue = e.CurrentValue;
        }
    }

    #endregion

    #region Groups

    /// <summary>
    /// Get the ListViewItem group in which the device belongs.
    /// </summary>
    /// <param name="deviceState"></param>
    /// <param name="listView"></param>
    /// <returns></returns>
    private ListViewGroup GetGroup(DeviceState deviceState, ListView listView)
    {
        switch (deviceState)
        {
            case DeviceState.Active:
                return listView.Groups[nameof(DeviceState.Active)];
            default:
                return listView.Groups[nameof(DeviceState.NotPresent)];
        }
    }

    private void PopulateDeviceTypeGroups(ListView listView)
    {
        listView.Groups.Add(new ListViewGroup(nameof(DeviceState.Active), SettingsStrings.connected));
        listView.Groups.Add(new ListViewGroup(nameof(DeviceState.NotPresent), SettingsStrings.disconnected));
    }

    #endregion

    #region Profiles

    private void AddProfileButton_Click(object sender, EventArgs e)
    {
        var form = new UpsertProfileExtended(new Profile(), _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged), _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged), this);
        form.Show(this);
    }

    private void ProfilesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        editProfileButton.Enabled = profilesListView.SelectedIndices.Count == 1;
        deleteProfileButton.Enabled = profilesListView.SelectedIndices.Count > 0;
    }

    private void DeleteProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListView.SelectedItems.Count <= 0) return;

        var profiles = profilesListView.SelectedItems.Cast<ListViewItem>()
            .Select(item => (Profile)item.Tag);
        AppModel.Instance.ProfileManager.DeleteProfiles(profiles);
        deleteProfileButton.Enabled = false;
        editProfileButton.Enabled = false;
        RefreshProfiles();
    }

    private void EditProfileButton_Click(object sender, EventArgs e)
    {
        if (profilesListView.SelectedItems.Count <= 0) return;

        var profile = (Profile)profilesListView.SelectedItems[0].Tag;
        var form = new UpsertProfileExtended(profile, _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged), _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged), this, true);
        form.Show(this);
    }

    private void ProfilesListView_DoubleClick(object sender, EventArgs e)
    {
        EditProfileButton_Click(sender, e);
    }

    #endregion

    #region Basic Settings

    private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            AppModel.Instance.RunAtStartup = startWithWindowsCheckBox.Checked;
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
            startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;
        }
    }

    private void IconChangeChoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<IconChanger>(sender, selectedItem =>
        {
            AppConfigs.Configuration.SwitchIcon = selectedItem.Enum;
            AppConfigs.Configuration.Save();
            new IconChangerFactory().Get(selectedItem.Enum).ChangeIcon(AppModel.Instance.TrayIcon);
        });
    }

    private void IconDoubleClickComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<IconDoubleClick>(sender, selectedItem =>
            AppModel.Instance.IconDoubleClick = selectedItem.Enum);
    }

    #endregion

    #region Audio Settings

    private void CommunicationCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        var comm = switchCommunicationDeviceCheckBox.Checked;
        try
        {
            AppModel.Instance.SetCommunications = comm;
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
            switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;
        }
    }

    private void ForegroundAppCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        AppModel.Instance.SwitchForegroundProgram = foregroundAppCheckbox.Checked;
    }

    private void KeepVolumeCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        AppModel.Instance.KeepVolumeEnabled = keepVolumeCheckbox.Checked;
    }

    private void TooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<TooltipInfoType>(sender, selectedItem =>
            TooltipInfoManager.CurrentTooltipInfo = selectedItem.Enum);
    }

    private void CyclerComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<DeviceCyclerType>(sender, selectedItem =>
            DeviceCyclerManager.CurrentCycler = selectedItem.Enum);
    }

    #endregion

    #region Update Settings

    private static void SetUpdateValue(RadioButton radioButton, UpdateMode updateMode)
    {
        if (radioButton.Checked)
            AppModel.Instance.UpdateMode = updateMode;
    }

    private void UpdateSilentRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetUpdateValue(updateSilentRadioButton, UpdateMode.Silent);

    private void UpdateNotifyRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetUpdateValue(updateNotifyRadioButton, UpdateMode.Notify);

    private void UpdateNeverRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetUpdateValue(updateNeverRadioButton, UpdateMode.Never);

    private void BetaVersionCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        AppModel.Instance.IncludeBetaVersions = includeBetaVersionsCheckBox.Checked;
    }

    #endregion

    #region Language

    private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<Language>(sender, selectedItem =>
        {
            if (AppModel.Instance.Language == selectedItem.Enum) return;
            AppModel.Instance.Language = selectedItem.Enum;

            if (MessageBox.Show(SettingsStrings.languageRestartRequired,
                    SettingsStrings.restartRequired_caption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            Program.RestartApp();
        });
    }

    #endregion

    #region Notifications

    private void NotificationAdvancedMode(bool isAdvancedMode)
    {
        switchProfileNotificationComboBox.Visible = isAdvancedMode;
        switchProfileNotificationLabel.Visible = isAdvancedMode;
        microphoneMuteNotificationComboBox.Visible = isAdvancedMode;
        microphoneMuteNotificationLabel.Visible = isAdvancedMode;
        notificationsGroupBox.Height = isAdvancedMode ? 166 : 93;
    }

    private void AdvancedModeCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        var isAdvancedMode = advancedModeCheckBox.Checked;
        NotificationAdvancedMode(isAdvancedMode);
        AppModel.Instance.NotificationAdvancedMode = isAdvancedMode;
    }

    private void SwitchDeviceNotificationComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<NotificationType>(sender, selectedItem =>
        {
            var notificationType = selectedItem.Enum;
            if (notificationType == AppModel.Instance.SwitchDeviceNotification) return;
            AppModel.Instance.SwitchDeviceNotification = notificationType;
        });
    }

    private void SwitchProfileNotificationComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<NotificationType>(sender, selectedItem =>
            AppModel.Instance.SwitchProfileNotification = selectedItem.Enum);
    }

    private void MicrophoneMuteNotificationComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<NotificationType>(sender, selectedItem =>
            AppModel.Instance.MicrophoneMuteNotification = selectedItem.Enum);
    }

    private void CustomSoundNotificationCheck()
    {
        var customSound = AppModel.Instance.CustomNotificationSound;
        void SetProperties(string text, Color color, FontStyle style = FontStyle.Regular)
        {
            var original = customSoundFileGroupBox.Width - 12;
            selectSoundButton.Text = text;
            selectSoundButton.ForeColor = color;
            selectSoundButton.Width = customSound != null ? original - deleteSoundButton.Width - 6 : original;
            selectSoundButton.Font = new Font("Segoe UI", 9F, style);
        }

        if (customSound == null)
            SetProperties(SettingsStrings.buttonSelect + "…", SystemColors.ControlText);
        else if (File.Exists(customSound.FilePath))
            SetProperties(Path.GetFileName(customSound.FilePath), SystemColors.ControlText, FontStyle.Italic);
        else
            SetProperties(SettingsStrings.selectSoundButton_error, Color.Red, FontStyle.Bold);
    }

    private void SelectSoundButton_Click(object sender, EventArgs e)
    {
        selectSoundFileDialog.ShowDialog(this);
    }

    private void SelectSoundFileDialog_FileOk(object sender, CancelEventArgs cancelEventArgs)
    {
        try
        {
            AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
        }
        catch (Exception)
        {
            MessageBox.Show(@"Please select another file", @"Invalid Sound file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        deleteSoundButton.Visible = true;
        CustomSoundNotificationCheck();
    }

    private void DeleteSoundButton_Click(object sender, EventArgs e)
    {
        AppModel.Instance.CustomNotificationSound = null;
        deleteSoundButton.Visible = false;
        CustomSoundNotificationCheck();
    }

    private void UsePrimaryScreenCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        AppModel.Instance.NotifyUsingPrimaryScreen = usePrimaryScreenCheckbox.Checked;
    }

    private void MicrophoneMuteBannerComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<MicrophoneMute>(sender, selectedItem =>
            AppModel.Instance.MicrophoneMuteBanner = selectedItem.Enum);
    }

    private void MicrophoneUnmuteBannerComboBox_SelectedValueChanged(object sender, EventArgs e)
    {
        SetComboBoxValue<MicrophoneMute>(sender, selectedItem =>
            AppModel.Instance.MicrophoneUnmuteBanner = selectedItem.Enum);
    }

    private void PositionGroupBox_Paint(object sender, PaintEventArgs e)
    {
        Size round =  new(RECT_PEN_WIDTH * 4, RECT_PEN_WIDTH * 4);
        Rectangle rect = RectOutline(OFFSET_W, OFFSET_H, positionTopLeftRadioButton, positionBottomRightRadioButton);

        e.Graphics.FillRoundedRectangle(new SolidBrush(Color.AliceBlue), rect, round);
        e.Graphics.DrawRoundedRectangle(PenLine(RECT_PEN_WIDTH), rect, round);

        e.Graphics.DrawLine(PenLine(),
            CenterPoint(positionCustomRadioButton),
            CenterPoint(selectCustomPositionButton));
    }

    private static void SetBannerPositionValue(RadioButton radioButton, BannerPosition position)
    {
        if (!radioButton.Checked) return;
        AppModel.Instance.BannerPosition = position;
    }
    private void PositionTopLeftRadioButton_CheckedChanged(object sender, EventArgs e) =>

        SetBannerPositionValue(positionTopLeftRadioButton, BannerPosition.TopLeft);
    private void PositionTopCenterRadioButton_CheckedChanged(object sender, EventArgs e) =>

        SetBannerPositionValue(positionTopCenterRadioButton, BannerPosition.TopCenter);

    private void PositionTopRightRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionTopRightRadioButton, BannerPosition.TopRight);

    private void PositionCenterLeftRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionCenterLeftRadioButton, BannerPosition.CenterLeft);

    private void PositionCenterRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionCenterRadioButton, BannerPosition.Center);

    private void PositionCenterRightRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionCenterRightRadioButton, BannerPosition.CenterRight);

    private void PositionBottomLeftRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionBottomLeftRadioButton, BannerPosition.BottomLeft);

    private void PositionBottomCenterRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionBottomCenterRadioButton, BannerPosition.BottomCenter);

    private void PositionBottomRightRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionBottomRightRadioButton, BannerPosition.BottomRight);

    private void PositionCustomRadioButton_CheckedChanged(object sender, EventArgs e) =>
        SetBannerPositionValue(positionCustomRadioButton, BannerPosition.Custom);

    private void SelectCustomPositionButton_Click(object sender, EventArgs e)
    {
        positionCustomRadioButton.Checked = true;

        var audioDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
        using var iconHandle = audioDevice.LargeIcon;
        var bannerData = new BannerData
        {
            Image = iconHandle.ToBitmap(),
            Title = SettingsStrings.customPositionBanner_title,
            Text = SettingsStrings.customPositionBanner_text,
            Position = AppModel.Instance.BannerPositionImpl,
            Ttl = TimeSpan.FromSeconds(6),
            CustomPositionMode = true
        };

        _bannerManager.ShowNotification(bannerData);
    }

    #endregion

    #region Troubleshooting

    private void ResetAudioDevicesButton_Click(object sender, EventArgs e)
    {
        AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
    }

    private void PrepareZipArchive(string title, string fileName, Action<ZipArchive> exportArchive)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = title,
            FileName = fileName,
            DefaultExt = "zip",
            Filter = "Zip Archive (*.zip)|*.zip",
            RestoreDirectory = true,
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        if (File.Exists(saveFileDialog.FileName))
            File.Delete(saveFileDialog.FileName);

        using var archive = ZipFile.Open(saveFileDialog.FileName, ZipArchiveMode.Create);
        exportArchive(archive);
    }

    private void ExportLogFilesButton_Click(object sender, EventArgs e)
    {
        PrepareZipArchive(SettingsStrings.exportLogFiles, "soundswitch_logs", archive =>
        {
            Log.CloseAndFlush();

            var files = Directory.EnumerateFiles(ApplicationPath.Logs, "*.log");
            foreach (var file in files)
                // Add the entry for each file
                archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);

            LoggerConfigurator.ConfigureLogger();
        });
    }

    private void ExportConfigFileButton_Click(object sender, EventArgs e)
    {
        PrepareZipArchive(SettingsStrings.exportConfigFile, "soundswitch_config", archive =>
        {
            const string configFile = "SoundSwitchConfiguration.json";
            archive.CreateEntryFromFile(Path.Combine(ApplicationPath.Default, configFile), configFile, CompressionLevel.Optimal);
        });
    }

    private void ImportConfigFileButton_Click(object sender, EventArgs e)
    {
        const string configFile = "SoundSwitchConfiguration.json";
        var filePath = Path.Combine(ApplicationPath.Default, configFile);

        var openFileDialog = new OpenFileDialog
        {
            Title = SettingsStrings.importConfigFile,
            Filter = "Zip Archive (*.zip)|*.zip",
            RestoreDirectory = true,
        };

        while (true)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            using var archive = ZipFile.Open(openFileDialog.FileName, ZipArchiveMode.Read);
            var entry = archive.GetEntry(configFile);
            if (entry == null)
            {
                MessageBox.Show(SettingsStrings.importConfigErrorMessage,
                    SettingsStrings.importConfigErrorMessage_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                continue;
            }

            // Read content of the entry
            string jsonContent;
            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                jsonContent = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(SettingsStrings.importConfigErrorReadingFile, configFile, ex.Message);
                Log.Error(ex, "Error reading configuration file '{ConfigFile}' from archive during import.", configFile);
                MessageBox.Show(errorMessage,
                    SettingsStrings.importConfigErrorMessage_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                continue;
            }

            try
            {
                // Validate JSON structure
                JObject.Parse(jsonContent);

                // Attempt to deserialize
                var config = JsonConvert.DeserializeObject<SoundSwitchConfiguration>(jsonContent);
                if (config == null)
                {
                    var errorMessage = string.Format(SettingsStrings.importConfigErrorDeserializationNull, configFile);
                    Log.Warning("Configuration file '{ConfigFile}' deserialized to null during import.", configFile);
                    MessageBox.Show(errorMessage,
                        SettingsStrings.importConfigErrorMessage_caption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    continue;
                }
            }
            catch (JsonReaderException jsonEx)
            {
                var errorMessage = string.Format(SettingsStrings.importConfigErrorInvalidJson, configFile, jsonEx.Message);
                Log.Warning(jsonEx, "Configuration file '{ConfigFile}' is not valid JSON during import.", configFile);
                MessageBox.Show(errorMessage,
                    SettingsStrings.importConfigErrorMessage_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                continue;
            }
            catch (Exception ex) // Catches other errors during deserialization
            {
                var errorMessage = string.Format(SettingsStrings.importConfigErrorDeserializationFailed, configFile, ex.Message);
                Log.Warning(ex, "Failed to deserialize configuration file '{ConfigFile}' during import.", configFile);
                MessageBox.Show(errorMessage,
                    SettingsStrings.importConfigErrorMessage_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                continue;
            }

            if (MessageBox.Show(SettingsStrings.importConfigRestartRequired,
                    SettingsStrings.restartRequired_caption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;

            File.Copy(filePath, Path.ChangeExtension(filePath, ".old"), true);
            entry.ExtractToFile(filePath, true);
            break;
        }

        Program.RestartApp();
    }

    public static void GitHubHelpLink_LinkClicked(object sender, EventArgs e)
    {
        BrowserUtil.OpenUrl("https://github.com/Belphemur/SoundSwitch/discussions");
    }

    public static void DiscordCommunityLink_LinkClicked(object sender, EventArgs e)
    {
        BrowserUtil.OpenUrl("https://discord.gg/gUCw3Ue");
    }

    public static void DonateLink_LinkClicked(object sender, EventArgs e)
    {
        BrowserUtil.OpenUrl($"https://soundswitch.aaflalo.me/?utm_campaign=application&utm_source={Application.ProductVersion}#donate");
    }

    private void PopulateAppSoundLockRules()
    {
        appSoundLockListView.Columns.Clear();
        appSoundLockListView.Columns.Add(SettingsStrings.profile_program, 150);
        appSoundLockListView.Columns.Add(SettingsStrings.appSoundLock_rule_windowTitle, 150);
        appSoundLockListView.Columns.Add(SettingsStrings.playback, 150);
        appSoundLockListView.Columns.Add(SettingsStrings.recording, 150);

        appSoundLockListView.Items.Clear();

        var playbacks = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled).ToList();
        var recordings = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled).ToList();

        appSoundLockListView.ItemCheck -= AppSoundLockListView_ItemCheck;
        foreach (var rule in AppConfigs.Configuration.AppSoundRules)
        {
            var playback = playbacks.FirstOrDefault(d => d.Id == rule.PlaybackDeviceId);
            var recording = recordings.FirstOrDefault(d => d.Id == rule.RecordingDeviceId);

            var processName = GetCleanProcessName(rule.ProcessPath);
            IconHandle processIcon = null;
            if (!string.IsNullOrEmpty(rule.ProcessPath) && File.Exists(rule.ProcessPath))
            {
                try { processIcon = IconExtractor.Extract(rule.ProcessPath, 0, true); }
                catch
                {
                    // ignored
                }
            }

            var item = new ListViewItem(processName)
            {
                Tag = rule,
                Checked = rule.Enabled
            };
            item.SubItems[0].Tag = processIcon;

            // Window Title
            item.SubItems.Add(rule.WindowName);

            // Playback
            var playbackSubItem = item.SubItems.Add(playback?.NameClean ?? rule.PlaybackDeviceId ?? string.Empty);
            playbackSubItem.Tag = playback?.SmallIcon;

            // Recording
            var recordingSubItem = item.SubItems.Add(recording?.NameClean ?? rule.RecordingDeviceId ?? string.Empty);
            recordingSubItem.Tag = recording?.SmallIcon;

            appSoundLockListView.Items.Add(item);
        }
        appSoundLockListView.ItemCheck += AppSoundLockListView_ItemCheck;

        if (AppConfigs.Configuration.AppSoundRules.Count <= 0) return;
        foreach (ColumnHeader column in appSoundLockListView.Columns)
            column.Width = -2;
    }

    private string GetCleanProcessName(string processPath)
    {
        if (string.IsNullOrEmpty(processPath)) return string.Empty;

        // Extract filename from .*Regex\.Escape(filename).*
        var match = System.Text.RegularExpressions.Regex.Match(processPath, @"\.\*(?<name>.*?)\.\*");
        if (match.Success)
        {
            try { return System.Text.RegularExpressions.Regex.Unescape(match.Groups["name"].Value); } 
            catch (Exception ex) { Log.Debug(ex, "Failed to unescape regex pattern for display name: {Pattern}", processPath); }
        }

        // Fallback to filename if it looks like a path
        try { return Path.GetFileName(processPath); } catch { return processPath; }
    }

    private void AppSoundLockListView_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        var rule = (AppSoundRule)appSoundLockListView.Items[e.Index].Tag;
        rule.Enabled = e.NewValue == CheckState.Checked;
        AppConfigs.Configuration.Save();
    }

    private void AddAppSoundRuleButton_Click(object sender, EventArgs e)
    {
        var playbacks = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active);
        var recordings = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active);
        
        using var form = new UpsertAppSoundLockRule(new AppSoundRule(), playbacks, recordings);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            AppConfigs.Configuration.AppSoundRules.Add(form.Rule);
            AppConfigs.Configuration.Save();
            PopulateAppSoundLockRules();
        }
    }

    private void EditAppSoundRuleButton_Click(object sender, EventArgs e)
    {
        if (appSoundLockListView.SelectedItems.Count == 0) return;
        var rule = (AppSoundRule)appSoundLockListView.SelectedItems[0].Tag;

        var playbacks = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active);
        var recordings = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active);

        using var form = new UpsertAppSoundLockRule(rule, playbacks, recordings, true);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            AppConfigs.Configuration.AppSoundRules.Remove(rule);
            AppConfigs.Configuration.AppSoundRules.Add(form.Rule);
            AppConfigs.Configuration.Save();
            PopulateAppSoundLockRules();
        }
    }

    private void DeleteAppSoundRuleButton_Click(object sender, EventArgs e)
    {
        if (appSoundLockListView.SelectedItems.Count == 0) return;
        var rule = (AppSoundRule)appSoundLockListView.SelectedItems[0].Tag;

        if (MessageBox.Show($"Are you sure you want to delete this rule?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            AppConfigs.Configuration.AppSoundRules.Remove(rule);
            AppConfigs.Configuration.Save();
            PopulateAppSoundLockRules();
        }
    }

    private void AppSoundLockListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        var hasSelection = appSoundLockListView.SelectedItems.Count > 0;
        editAppSoundRuleButton.Enabled = hasSelection;
        deleteAppSoundRuleButton.Enabled = hasSelection;
    }

    private void AppSoundLockListView_DoubleClick(object sender, EventArgs e)
    {
        EditAppSoundRuleButton_Click(sender, e);
    }

    #endregion
}
