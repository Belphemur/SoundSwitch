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
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.IconChanger.Changer
{
    public class AlwaysIconChanger : AbstractIconChanger
    {
        public override IconChangerEnum TypeEnum => IconChangerEnum.Always;
        public override string Label => TrayIconStrings.iconChanger_both;

        protected override bool NeedsToChangeIcon(DeviceInfo deviceInfo) => true;

        protected override DataFlow Flow => DataFlow.Render;
    }
}