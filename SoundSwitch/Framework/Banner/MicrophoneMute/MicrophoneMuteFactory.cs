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

using SoundSwitch.Banner;
using SoundSwitch.Common.Framework.Factory;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Banner.MicrophoneMute;

public interface IMicrophoneMute : IEnumImpl<SoundSwitch.Banner.MicrophoneMute> { }

public class MicrophoneMuteNone : IMicrophoneMute
{
    public SoundSwitch.Banner.MicrophoneMute TypeEnum => SoundSwitch.Banner.MicrophoneMute.None;
    public string Label => SettingsStrings.none;
}

public class MicrophoneMuteFading : IMicrophoneMute
{
    public SoundSwitch.Banner.MicrophoneMute TypeEnum => SoundSwitch.Banner.MicrophoneMute.Fading;
    public string Label => SettingsStrings.banner_mute_option_fading;
}

public class MicrophoneMutePersistent : IMicrophoneMute
{
    public SoundSwitch.Banner.MicrophoneMute TypeEnum => SoundSwitch.Banner.MicrophoneMute.Persistent;
    public string Label => SettingsStrings.banner_mute_option_persistent;
}

public class MicrophoneMuteFactory() : AbstractFactory<SoundSwitch.Banner.MicrophoneMute, IMicrophoneMute>(new EnumImplList<SoundSwitch.Banner.MicrophoneMute, IMicrophoneMute>
{
    new MicrophoneMuteNone(),
    new MicrophoneMuteFading(),
    new MicrophoneMutePersistent()
});
