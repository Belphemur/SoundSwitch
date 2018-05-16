<a href="https://soundswitch.aaflalo.me" title="SoundSwitch website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="logo SoundSwitch" height="180px" style="margin-left:auto;margin-right:auto;display:block;"></a>

[![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://soundswitch.aaflalo.me) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://soundswitch.aaflalo.me/) [![Donate](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://soundswitch.aaflalo.me) [![Translate](https://hosted.weblate.org/widgets/soundswitch/-/svg-badge.svg)](https://hosted.weblate.org/projects/soundswitch/)

**SoundSwitch** offers you the opportunity to switch your playback devices and/or recording devices using simple hotkeys.

**No more** navigating througth several menus and screens. Just configure SoundSwitch **once** - and you are able to switch between your devices as fast as never before!

## Preview image
![preview](https://soundswitch.aaflalo.me/img/preview.gif)

## Dependencies
The following components are required to run SoundSwitch on your Windows device:
- [Microsoft .NET Framework 4.6.2 (Web Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=53345) 
- [Visual C++ Redistributable for Visual Studio 2017](https://go.microsoft.com/fwlink/?LinkId=746572)

## Donations
If you want to donate for the development don't hesite [to do it here](https://www.aaflalo.me/donate/).

## Configuration
``Right click`` the SoundSwitch icon in your system tray and choose `Settings`. Now select the devices between which you want to toggle. Optionally you can also change the keyboard combination. If you want the application to start automatically when your PC boots up, check the box `Start automatically with Windows`.

## Usage

First set up your devices you want to cycle through, using `right click` onto the `System Tray Icon` of SoundSwitch.

**After you configured** SoundSwitch you can use the following Hotkeys.

- To cycle through the **playback devices** press:
  - `Ctrl` + `Alt` + `F11` (default) **OR**
  - Double click onto the `System Tray Icon` of SoundSwitch.

- To cycle through the **recording devices** press:
  - `Ctrl` + `Alt` + `F7` (default)

---

## Features

### Recording Devices
SoundSwitch can also take care of your recording devices. You can set specifics hotkeys as for the playback devices.

### Notifications
SoundSwitch provides five types of notification when a device was changed.

- #### Banner
Uses a custom always-on-top frame, useful for in-game usage. This is the recommended default display style.

- #### Windows Notification
Uses the balloon tip of Windows. In the case of Windows 7, it's the little balloon that opens next to the systray icon. For Windows 10, it's the notification system that slides from the right corner of the screen.

- #### Sound Notification
This notification is a sound played on the switched device. This way when you are switching devices, the new device will 'chime' to tell you it's selected.

- #### Customized Sound Notification
The same as a Sound Notification but you can specify the sound which is played.

- #### Toast Notification (≥Win 8)
If you are on Windows 8 or later, you can use this type of Toast Notification. More personalization is possible, by default the notification is silent, but you can set a custom sound for it (mp3 or wav for now).

If you want to return to a silent Toast Notification, open the file selector, and just do Cancel. Doing that will remove the set sound.

---

## Advanced

### Communications
SoundSwitch can also change the **Default Communication Device** when asked in the Settings. Windows makes a differentiation between Multimedia and Communication; it means if an application asks to have access to communications audio device, it will receive the Default Communication Device. By default SoundSwitch only changes the multimedia device and not the communication. Now if the checkbox is checked in the settings, it will also change the Communication Device.

### Auto-Updater
This is one of the interesting feature, the auto-updater. Every 24h SoundSwitch will check the GitHub repository (thanks to the GitHub API) for a new release. If a new one is available the user will get a notification and the 'No update available' in the context menu will change to 'Update Available (X.X.X)' where X.X.X represent the new version number. When the user clicks on it, a new window opens with a progress bar (see screenshots). The new version gets automatically downloaded in the temp folder of the user. When the download is finished, the user can install the Update by just clicking the install button. A changelog is also provided by getting the information set in the release on GitHub.

#### Update Modes
There are three different options available: **Silent**, means the program updates itself in the background without any prompts. **Notify**, you will be notified when there's an update available. **Never**, well this is self-explained.

### Multi-Language Support
There are five languages available: **English**, **French**, **German**, **Spanish**, **Italian** and **Portuguese(Brazil)**.

Would you like to help us with the localization? Great! See [Add or modify another language](https://github.com/Belphemur/SoundSwitch/wiki/Add-or-modify-another-language) for further informations.

## Awards

<a href="http://www.giga.de/downloads/soundswitch/" target="_blank"><img src="https://i.imgur.com/19GaPLQ.png" alt="Giga 5 stars" height="100" hspace="10"/></a><a href="http://www.softpedia.com/get/Multimedia/Audio/Other-AUDIO-Tools/SoundSwitch.shtml#status" target="_blank"><img src="http://s1.softpedia-static.com/_img/sp100free.png" alt="Softpedia" height="100" hspace="10"/></a><a href="http://www.chip.de/downloads/SoundSwitch_94258571.html" target="_blank"><img src="https://i.imgur.com/Nedw1su.png" alt="Chip Online de" height="100" hspace="10"/></a><a href="https://www.netzwelt.de/download/24278-soundswitch.html" target="_blank"><img src="https://i.imgur.com/VaMTnxV.png" alt="netzwelt GmbH" height="100" hspace="10"/></a>

## Contributors

- Original Developer: [Jeroen Pelgrims](http://jeroenpelgrims.be)
- Disabling Notification [#33](https://github.com/Belphemur/SoundSwitch/pull/33) [@adamblackburn](https://github.com/adamblackburn)
- Localization and German language [#157](https://github.com/Belphemur/SoundSwitch/pull/157) [@FireEmerald](https://github.com/FireEmerald) 
- Banner Notification [#186](https://github.com/Belphemur/SoundSwitch/pull/186) [@ramon18](https://github.com/ramon18)
- Keyboard hook, [Christian Liensberger](http://www.liensberger.it/web/blog/?p=207).
- Changing default sound device, [EreTIk](http://eretik.omegahg.com/).
- Notification Sound, [Music box notification sound by Robinhood76](https://www.freesound.org/people/Robinhood76/sounds/216676/).
- Spanish Language [#244](https://github.com/Belphemur/SoundSwitch/pull/244) [@plextoriano](https://github.com/plextoriano)
- Portuguese (Brazilian) [#258](https://github.com/Belphemur/SoundSwitch/pull/258) [@aleczk](https://github.com/aleczk)
- Awesome Logo  [#278](https://github.com/Belphemur/SoundSwitch/pull/278) [@linadesteem](https://github.com/linadesteem)

## Thanks

### JetBrains

Thanks for their Open-Source licence to their amazing IDEs and addons like ReSharper for Visual Studio.

<a href="https://www.jetbrains.com" target="_blank"><img src="https://i.imgur.com/opT9XBj.png" alt="JetBrain Tooling" height="100" hspace="10"/></a>

## Credits

**Icons**, the [Pastel SVG icon set](https://codefisher.org/pastel-svg/). Created by Michael Buckley. ([CC BY-NC-SA 4.0](http://creativecommons.org/licenses/by-nc-sa/4.0/ ))

## License: GPLv2

Copyright (C) 2015 Jeroen Pelgrims

Copyright (C) 2015-2018 Antoine Aflalo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
