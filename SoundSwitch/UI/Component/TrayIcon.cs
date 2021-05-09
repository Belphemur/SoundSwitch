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

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Profile.UI;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Forms;
using SoundSwitch.Util;
using SoundSwitch.Util.Url;
using TimerForm = System.Windows.Forms.Timer;

namespace SoundSwitch.UI.Component
{
    public sealed class TrayIcon : IDisposable
    {
        private static readonly Bitmap RessourceUpdateBitmap = Resources.Update;
        private static readonly Bitmap RessourceSettingsSmallBitmap = Resources.SettingsSmall;
        private static readonly Bitmap RessourcePlaybackDevicesBitmap = Resources.PlaybackDevices;
        private static readonly Bitmap RessourceMixerBitmap = Resources.Mixer;
        private static readonly Bitmap RessourceInfoHelpBitmap = Resources.InfoHelp;
        private static readonly Bitmap ResourceDonateBitmap = Resources.donate;
        private static readonly Bitmap RessourceHelpSmallBitmap = Resources.HelpSmall;
        private static readonly Bitmap RessourceExitBitmap = Resources.exit;
        private static readonly Icon RessourceUpdateIconBitmap = Resources.UpdateIcon;
        private static readonly Icon SoundSwitchLogoIcon = Resources.Switch_SoundWave;
        private static readonly Icon ResourceDiscord = Resources.DiscordIcon;

        private readonly ContextMenuStrip _selectionMenu = new();
        private readonly ContextMenuStrip _settingsMenu = new();

        private readonly SynchronizationContext _context =
            SynchronizationContext.Current ?? new SynchronizationContext();

        public NotifyIcon NotifyIcon { get; } = new()
        {
            Visible = true,
            Text = Application.ProductName
        };

        private readonly TooltipInfoManager _tooltipInfoManager;
        private readonly ProfileTrayIconBuilder _profileTrayIconBuilder;

        private readonly ToolStripMenuItem _updateMenuItem;
        private TimerForm _animationTimer;
        private readonly UpdateDownloadForm _updateDownloadForm;
        private readonly MethodInfo? _showContextMenu;

        public TrayIcon()
        {
            UpdateIcon();
            _showContextMenu = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            _tooltipInfoManager = new TooltipInfoManager(NotifyIcon);
            _profileTrayIconBuilder = new ProfileTrayIconBuilder();

            _updateMenuItem = new ToolStripMenuItem(AppConfigs.Configuration.UpdateMode == UpdateMode.Never ? TrayIconStrings.updateDisabled : TrayIconStrings.noUpdate, RessourceUpdateBitmap, OnUpdateClick)
            {
                Enabled = false
            };
            NotifyIcon.ContextMenuStrip = _settingsMenu;

            PopulateSettingsMenu();

            _selectionMenu.Items.Add(TrayIconStrings.noDevicesSelected, RessourceSettingsSmallBitmap, (sender, e) => ShowSettings());

            //NotifyIcon.MouseDoubleClick += (sender, args) => { AppModel.Instance.CycleActiveDevice(DataFlow.Render); };

            NotifyIcon.MouseDown += (sender, e) =>
            {
                Log.Debug("Click on systray icon: {times} {button}", e.Clicks, e.Button);
                if (e.Clicks == 2)
                {
                    NotifyIcon.ContextMenuStrip.Close();
                    AppModel.Instance.CycleActiveDevice(DataFlow.Render);
                    return;
                }

                if (e.Button != MouseButtons.Left) return;

                if (_updateMenuItem.Tag != null)
                {
                    OnUpdateClick(sender, e);
                    return;
                }

                UpdateDeviceSelectionList();
                NotifyIcon.ContextMenuStrip = _selectionMenu;
                _showContextMenu.Invoke(NotifyIcon, null);

                NotifyIcon.ContextMenuStrip = _settingsMenu;
            };

            NotifyIcon.MouseMove += (sender, args) =>
            {
                try
                {
                    if (!NotifyIcon.Visible)
                        return;
                    _tooltipInfoManager.ShowTooltipInfo();
                }
                catch (Exception)
                {
                    //ignored
                }
            };
            _selectionMenu.Closed += (sender, args) => _tooltipInfoManager.IsBallontipVisible = false;
            _settingsMenu.Closed += (sender, args) => _tooltipInfoManager.IsBallontipVisible = false;
            SetEventHandlers();
            _updateDownloadForm = new UpdateDownloadForm();
        }

        public void Dispose()
        {
            _selectionMenu.Dispose();
            _settingsMenu.Dispose();
            if (NotifyIcon.Icon != null)
            {
                NotifyIcon.Icon.Dispose();
            }

            NotifyIcon.Dispose();
            _updateMenuItem.Dispose();
            _updateDownloadForm.Dispose();
        }

        public void ReplaceIcon(Icon newIcon)
        {
            if (newIcon.Equals(NotifyIcon.Icon))
                return;

            var oldIcon = NotifyIcon.Icon;
            NotifyIcon.Icon = (Icon) newIcon.Clone();
            try
            {
                oldIcon?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void UpdateIcon()
        {
            new IconChangerFactory().Get(AppConfigs.Configuration.SwitchIcon).ChangeIcon(this);
        }

        private void PopulateSettingsMenu()
        {
            var applicationDirectory = Path.GetDirectoryName(ApplicationPath.Executable);
            Debug.Assert(applicationDirectory != null, "applicationDirectory != null");
            var readmeHtml = Path.Combine(applicationDirectory, "Readme.html");
            _settingsMenu.Items.Add(Application.ProductName + ' ' + AssemblyUtils.GetReleaseState() + " (" + Application.ProductVersion + ")", SoundSwitchLogoIcon.ToBitmap());
            _settingsMenu.Items.Add(new ToolStripSeparator());
            _settingsMenu.Items.Add(TrayIconStrings.playbackDevices, RessourcePlaybackDevicesBitmap,
                (sender, e) => { Process.Start(new ProcessStartInfo("control", "mmsys.cpl sounds")); });
            _settingsMenu.Items.Add(TrayIconStrings.mixer, RessourceMixerBitmap,
                (sender, e) => { Process.Start(new ProcessStartInfo("sndvol.exe")); });
            _settingsMenu.Items.Add(new ToolStripSeparator());
            _settingsMenu.Items.Add(_updateMenuItem);
            _settingsMenu.Items.Add(TrayIconStrings.settings, RessourceSettingsSmallBitmap, (sender, e) => ShowSettings());
            _settingsMenu.Items.Add(new ToolStripSeparator());
            _settingsMenu.Items.Add(TrayIconStrings.help, RessourceInfoHelpBitmap, (sender, e) =>
            {
                if (!File.Exists(readmeHtml))
                {
                    Log.Error("File {readme} doesn\'t exists", readmeHtml);
                    return;
                }

                BrowserUtil.OpenUrl(readmeHtml);
            });
            _settingsMenu.Items.Add(TrayIconStrings.communityMenu, ResourceDiscord.ToBitmap(), (sender, e) => { BrowserUtil.OpenUrl("https://discord.gg/gUCw3Ue"); });
            _settingsMenu.Items.Add(TrayIconStrings.donate, ResourceDonateBitmap,
                (sender, e) => BrowserUtil.OpenUrl($"https://soundswitch.aaflalo.me/?utm_campaign=application&utm_source={Application.ProductVersion}#donate"));
            _settingsMenu.Items.Add(TrayIconStrings.about, RessourceHelpSmallBitmap, (sender, e) => new About().Show());
            _settingsMenu.Items.Add(new ToolStripSeparator());
            _settingsMenu.Items.Add(TrayIconStrings.exit, RessourceExitBitmap, (sender, e) => Application.Exit());
        }

        private void OnUpdateClick(object sender, EventArgs eventArgs)
        {
            if (_updateMenuItem.Tag == null)
                return;
            if (_updateDownloadForm.Visible)
            {
                _updateDownloadForm.Focus();
                return;
            }

            StopAnimationIconUpdate();
            NotifyIcon.BalloonTipClicked -= OnUpdateClick;
            _updateDownloadForm.DownloadRelease((Release) _updateMenuItem.Tag);
        }

        private void SetEventHandlers()
        {
            AppModel.Instance.ErrorTriggered += (sender, @event) =>
            {
                if (@event.Exception is AppModel.NoDevicesException)
                {
                    ShowNoDevices();
                }
                else
                {
                    Log.Warning(@event.Exception, "Exception managed");
                    ShowError(@event.Exception.Message, @event.Exception.GetType().Name);
                }
            };
            AppModel.Instance.DefaultDeviceChanged += (sender, audioChangeEvent) =>
            {
                var iconChanger = new IconChangerFactory().Get(AppConfigs.Configuration.SwitchIcon);
                iconChanger.ChangeIcon(this, new DeviceFullInfo(audioChangeEvent.Device));
            };
            AppModel.Instance.NewVersionReleased += (sender, @event) =>
            {
                if (@event.UpdateMode == UpdateMode.Notify)
                    _context.Send(s => { NewReleaseAvailable(sender, @event); }, null);
            };
        }

        private void NewReleaseAvailable(object sender, UpdateChecker.NewReleaseEvent newReleaseEvent)
        {
            StartAnimationIconUpdate();
            _updateMenuItem.Tag = newReleaseEvent.Release;
            _updateMenuItem.Text =
                string.Format(TrayIconStrings.updateAvailable, newReleaseEvent.Release.ReleaseVersion);
            _updateMenuItem.Enabled = true;
            NotifyIcon.BalloonTipClicked += OnUpdateClick;
            NotifyIcon.ShowBalloonTip(3000,
                string.Format(TrayIconStrings.versionAvailable, newReleaseEvent.Release.ReleaseVersion),
                newReleaseEvent.Release.Name + '\n' + TrayIconStrings.clickToUpdate, ToolTipIcon.Info);
        }

        /// <summary>
        /// Make the icon flicker between default Icon and Update icon
        /// Used to notify the user of an update
        /// </summary>
        private void StartAnimationIconUpdate()
        {
            if (_animationTimer == null)
            {
                _animationTimer = new TimerForm() {Interval = 1000};
                var tick = 0;
                _animationTimer.Tick += (sender, args) =>
                {
                    ReplaceIcon(tick == 0
                        ? SoundSwitchLogoIcon
                        : RessourceUpdateIconBitmap);
                    tick = ++tick % 2;
                };
            }

            _animationTimer.Start();
        }

        /// <summary>
        /// Stop the animation of the Icon and reset the icon
        /// </summary>
        private void StopAnimationIconUpdate()
        {
            if (_animationTimer == null)
                return;

            _animationTimer.Stop();
            UpdateIcon();
        }

        public void ShowSettings()
        {
            _context.Send(state => { new SettingsForm(AppModel.Instance.ActiveUnpluggedAudioLister).Show(); }, null);
        }

        /// <summary>
        ///     Sets the names of devices that show up in the menu
        /// </summary>
        public void UpdateDeviceSelectionList()
        {
            Log.Information("Set tray icon menu devices");
            _selectionMenu.Items.Clear();
            var playbackDevices = AppModel.Instance.AvailablePlaybackDevices;
            var recordingDevices = AppModel.Instance.AvailableRecordingDevices;
            var profiles = _profileTrayIconBuilder.GetMenuItems().ToArray();

            if (profiles.Length > 0)
            {
                _selectionMenu.Items.AddRange(profiles);
                _selectionMenu.Items.Add(new ToolStripSeparator());
            }

            if (playbackDevices.Count < 0 &&
                recordingDevices.Count < 0)
            {
                Log.Information("Device list empty");
                return;
            }

            var defaultPlayback = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
            _selectionMenu.Items.AddRange(playbackDevices.Select(info => new ToolStripDeviceItem(DeviceClicked, info, info.Equals(defaultPlayback))).ToArray());

            if (recordingDevices.Count > 0)
            {
                var defaultRecording = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eConsole);

                _selectionMenu.Items.Add(new ToolStripSeparator());
                _selectionMenu.Items.AddRange(recordingDevices.Select(info => new ToolStripDeviceItem(DeviceClicked, info, info.Equals(defaultRecording))).ToArray());
            }
        }

        private void DeviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem) sender;
                AppModel.Instance.SetActiveDevice(item.AudioDevice);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            Log.Error("No devices available");
            NotifyIcon.ShowBalloonTip(3000,
                TrayIconStrings.configurationNeeded,
                TrayIconStrings.configurationNeededExplanation, ToolTipIcon.Warning);
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorTitle"></param>
        public void ShowError(string errorMessage, string errorTitle)
        {
            NotifyIcon.ShowBalloonTip(3000,
                $"{Application.ProductName}: {errorTitle}",
                errorMessage, ToolTipIcon.Error);
        }
    }
}