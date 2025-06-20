
﻿using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

internal class IconDoubleClickSwitchProfile: IIconDoubleClick
{
    public IconDoubleClickEnum TypeEnum => IconDoubleClickEnum.SwitchProfile;
    public string Label => SettingsStrings.switchProfile;
}