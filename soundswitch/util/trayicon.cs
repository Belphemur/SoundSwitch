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
using AudioDefaultSwitcherWrapper;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.TooltipInfoManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SoundSwitch.Util
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
        private static readonly Icon RessourceSoundSwitchIconWhite = Resources.SoundSwitchLogoWhite;
        private static readonly Icon RessourceSoundSwitchIconBlue = Resources.SoundSwitchLogoBlue;

        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();
        private readonly SynchronizationContext _context = SynchronizationContext.Current ?? new SynchronizationContext();
        public NotifyIcon NotifyIcon { get; } = new NotifyIcon
        {
            Visible = true,
            Text = Application.ProductName
        };

        private readonly TooltipInfoManager _tooltipInfoManager;

        private readonly ToolStripMenuItem _updateMenuItem;
        private Timer _animationTimer;

        public TrayIcon()
        {
            UpdateIcon();
            _tooltipInfoManager = new TooltipInfoManager(NotifyIcon);
            _updateMenuItem = new ToolStripMenuItem(TrayIconStrings.noUpdate, RessourceUpdateBitmap, OnUpdateClick)
            {
                Enabled = false
            };
            NotifyIcon.ContextMenuStrip = _settingsMenu;

            PopulateSettingsMenu();

            _selectionMenu.Items.Add(TrayIconStrings.noDevicesSelected, RessourceSettingsSmallBitmap, (sender, e) => ShowSettings());

            NotifyIcon.MouseDoubleClick += (sender, args) =>
            {
                AppModel.Instance.CycleActiveDevice(DeviceType.Playback);
            };

            NotifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left) return;

                if (_updateMenuItem.Tag != null)
                {
                    OnUpdateClick(sender, e);
                    return;
                }

                UpdateDeviceSelectionList();
                NotifyIcon.ContextMenuStrip = _selectionMenu;
                var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(NotifyIcon, null);

                NotifyIcon.ContextMenuStrip = _settingsMenu;
            };

            NotifyIcon.MouseMove += (sender, args) =>
            {
                if (!NotifyIcon.Visible)
                    return;
                _tooltipInfoManager.ShowTooltipInfo();
            };
            _selectionMenu.Closed += (sender, args) => _tooltipInfoManager.IsBallontipVisible = false;
            _settingsMenu.Closed += (sender, args) => _tooltipInfoManager.IsBallontipVisible = false;
            SetEventHandlers();
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
        }

        private void ReplaceIcon(Icon newIcon)
        {
            var oldIcon = NotifyIcon.Icon;
            NotifyIcon.Icon = (Icon)newIcon.Clone();
            try
            {
                oldIcon?.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }

        }

        public void UpdateIcon()
        {
            if (AppConfigs.Configuration.KeepSystrayIcon)
            {
                ReplaceIcon(RessourceSoundSwitchIconWhite);
                return;
            }

            try
            {
                using (var devices = AppModel.Instance.ActiveAudioDeviceLister.GetPlaybackDevices())
                {
                    var defaultDevice = devices
                        .First(device =>
                        AudioController.IsDefault(device.ID, (DeviceType) device.DataFlow, DeviceRole.Console));
                    ReplaceIcon(AudioDeviceIconExtractor.ExtractIconFromAudioDevice(defaultDevice, false));
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void PopulateSettingsMenu()
        {
            var applicationDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            Debug.Assert(applicationDirectory != null, "applicationDirectory != null");
            var readmeHtml = Path.Combine(applicationDirectory, "Readme.html");
            _settingsMenu.Items.Add(
                Application.ProductName + ' ' + AssemblyUtils.GetReleaseState() + " (" + Application.ProductVersion +
                ")", RessourceSoundSwitchIconBlue.ToBitmap());
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(TrayIconStrings.playbackDevices, RessourcePlaybackDevicesBitmap,
                (sender, e) => { Process.Start(new ProcessStartInfo("control", "mmsys.cpl sounds")); });
            _settingsMenu.Items.Add(TrayIconStrings.mixer, RessourceMixerBitmap,
                (sender, e) => { Process.Start(new ProcessStartInfo("sndvol.exe")); });
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(_updateMenuItem);
            _settingsMenu.Items.Add(TrayIconStrings.settings, RessourceSettingsSmallBitmap, (sender, e) => ShowSettings());
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(TrayIconStrings.help, RessourceInfoHelpBitmap, (sender, e) =>
            {
                if (!File.Exists(readmeHtml))
                {
                    Log.Error("File {readme} doesn\'t exists", readmeHtml);
                    return;
                }
                Process.Start(readmeHtml);
            });
            _settingsMenu.Items.Add(TrayIconStrings.donate, ResourceDonateBitmap, (sender, e) => Process.Start("https://soundswitch.aaflalo.me/?utm_source=application"));
            _settingsMenu.Items.Add(TrayIconStrings.about, RessourceHelpSmallBitmap, (sender, e) => new About().Show());
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(TrayIconStrings.exit, RessourceExitBitmap, (sender, e) => Application.Exit());
        }

        private void OnUpdateClick(object sender, EventArgs eventArgs)
        {
            if (_updateMenuItem.Tag == null)
                return;

            StopAnimationIconUpdate();
            new UpdateDownloadForm((Release)_updateMenuItem.Tag).ShowDialog();
            NotifyIcon.BalloonTipClicked -= OnUpdateClick;
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
                    Log.Error(@event.Exception, "Exception managed");
                    ShowError(@event.Exception.Message, @event.Exception.GetType().Name);
                }

            };
            AppModel.Instance.DefaultDeviceChanged += (sender, audioChangeEvent) =>
            {
                if (AppConfigs.Configuration.KeepSystrayIcon)
                {
                    return;
                }
                ReplaceIcon(AudioDeviceIconExtractor.ExtractIconFromAudioDevice(audioChangeEvent.Device, false));
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
            _updateMenuItem.Text = string.Format(TrayIconStrings.updateAvailable, newReleaseEvent.Release.ReleaseVersion);
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
                _animationTimer = new Timer() { Interval = 1000 };
                var tick = 0;
                _animationTimer.Tick += (sender, args) =>
                {
                    ReplaceIcon(tick == 0
                        ? RessourceSoundSwitchIconWhite
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
            new SettingsForm().Show();
        }

        /// <summary>
        ///     Sets the names of devices that show up in the menu
        /// </summary>
        public void UpdateDeviceSelectionList()
        {

            if (AppModel.Instance.AvailablePlaybackDevices.Count < 0 &&
                AppModel.Instance.AvailableRecordingDevices.Count < 0)
            {
                Log.Information("Device list empty");
                return;
            }

            _selectionMenu.Items.Clear();
            Log.Information("Set tray icon menu devices");
            foreach (var item in AppModel.Instance.AvailablePlaybackDevices)
            {
                _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
            }

            if (AppModel.Instance.AvailableRecordingDevices.Count > 0)
            {
                _selectionMenu.Items.Add("-");
                foreach (var item in AppModel.Instance.AvailableRecordingDevices)
                {
                    _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                }
            }

        }

        private void DeviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem)sender;
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