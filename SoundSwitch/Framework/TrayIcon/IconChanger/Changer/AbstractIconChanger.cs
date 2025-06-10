/********************************************************************
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
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.IconChanger.Changer
{
    public abstract class AbstractIconChanger : IIconChanger
    {
        private readonly ILogger _log;

        protected AbstractIconChanger() => _log = Log.ForContext("IconChanger", TypeEnum);

        public abstract IconChangerEnum TypeEnum { get; }
        public abstract string Label { get; }

        protected abstract DataFlow Flow { get; }

        protected virtual bool NeedsToChangeIcon(DeviceInfo deviceInfo) => deviceInfo.Type == Flow;

        public void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            using var audio = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)Flow, ERole.eConsole);
            ChangeIcon(trayIcon, audio, ERole.eConsole);
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role)
        {
            var log = _log.ForContext("Device", deviceInfo);
            log.Information("Changing icon");
            if (deviceInfo == null)
            {
                return;
            }

            //Don't change icon for communication device
            if (role == ERole.eCommunications)
            {
                return;
            }

            if (!NeedsToChangeIcon(deviceInfo))
            {
                log.Information("No need to change icon");
                return;
            }


            trayIcon.ReplaceIcon(deviceInfo.SmallIcon);
            log.Information("Icon replaced");
        }
    }
}