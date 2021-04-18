<a href="https://soundswitch.aaflalo.me" title="SoundSwitch Website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="SoundSwitch Logo" height="180px"></a>

[![.NET](https://github.com/Belphemur/SoundSwitch/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Belphemur/SoundSwitch/actions/workflows/dotnet.yml)[![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://soundswitch.aaflalo.me) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://soundswitch.aaflalo.me/) [![Translate](https://hosted.weblate.org/widgets/soundswitch/-/svg-badge.svg)](https://hosted.weblate.org/projects/soundswitch/) [![Donate](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://soundswitch.aaflalo.me) [![Help](https://img.shields.io/badge/Discord-Community%20&%20Help-green?style=flat-square&logo=discord)](https://discord.gg/gUCw3Ue)

**SoundSwitch** offers you the opportunity to switch your playback and recording devices using simple **hotkeys**.

**No more** navigating througth several menus and screens. Just configure SoundSwitch **once** and you are able to switch between your devices as fast as never before!

## ✨ Preview
![Preview](https://soundswitch.aaflalo.me/img/preview.gif?v=20191124)

## Requirements
- ⚠ Operating System: Windows 7 or newer

## Donations
If you'd like to support the development, we would love to see you [here ❤](https://soundswitch.aaflalo.me/#donate).

## Configuration
_Right click_ the SoundSwitch icon in your system tray and choose _Settings_. Now select the devices between which you want to toggle. Optionally you can also change the keyboard combination. If you want the application to start automatically when your PC boots up, check the box _Start automatically with Windows_.

## Usage

First set up your devices you want to cycle through, using _right click_ onto the _System Tray Icon_ of SoundSwitch.

**After you configured** SoundSwitch you can use the following hotkeys:

- 🔊 To cycle through the **playback devices** press:
  - `Ctrl` + `Alt` + `F11` (default) **or**
  - Double click onto the `System Tray Icon` of SoundSwitch.

- 🎙 To cycle through the **recording devices** press:
  - `Ctrl` + `Alt` + `F7` (default)

- 🔇 To mute the **default microphone** press:
  - `Ctrl` + `Alt` + `M` (default)

## _Switched_ Notification

SoundSwitch provides five types of notification when a device was changed:

- #### 🎟 Banner
Uses a custom always-on-top frame, useful for in-game usage. This is the recommended default display style.

- #### 🗨 Windows Notification
Uses the balloon tip of Windows. In the case of Windows 7, it's the little balloon that opens next to the systray icon. For Windows 10, it's the notification system that slides from the right corner of the screen.

- #### 🎵 Sound Notification
This notification is a sound played on the switched device. This way when you are switching devices, the new device will 'chime' to tell you it's selected.

- #### 🎶 Customized Sound Notification
The same as a Sound Notification but you can specify the sound which is played.

If you want to return to a silent Toast Notification, open the file selector, and just do Cancel. Doing that will remove the set sound.

## Profiles

Using profiles, it is possible to switch to a specific device when a condition occurs. Profiles can be defined for the following purposes:

- ### 💫 Application profile
When a application is focused, the sound settings are switched based on the profile. For example create a profile for Spotify to only play music on your speaker, while your favorite game is using your headset.

- ### ⌨️ Hot key profile
When a hot key combination is pressed, the sound settings are switched based on the profile. For example you define a special combination to switch your playback device to your television.

## Advanced

### 🎙 Communications
SoundSwitch can also change the **Default Communication Device** when asked in the Settings. Windows makes a differentiation between Multimedia and Communication; it means if an application asks to have access to communications audio device, it will receive the Default Communication Device. By default SoundSwitch only changes the multimedia device and not the communication. Now if the checkbox is checked in the settings, it will also change the Communication Device.

### 📥 Auto-Updater
Every 24 hours SoundSwitch checks the GitHub repository (thanks to the GitHub API) for a new release. If a new one is available you will get a notification and the 'No update available' in the context menu will change to 'Update Available'. The new version gets automatically downloaded and installed, depeding on your _Update Mode_. We also provide a changelog with the latest improvements of SoundSwitch.

#### 🚥 Update Modes
There are three different options available on how updates are installed:
- **Silent**, means the program updates itself in the background without any prompts.
- **Notify**, you will be notified when there's an update available.
- **Never**, well this is self-explained.

### 🌎 Multi-Language Support
There are five languages available: **English**, **French**, **German**, **Spanish**, **Italian** and **Portuguese (Brazilian)**.

Improve an existing or add another language? Translations are online editable [right here](https://hosted.weblate.org/projects/soundswitch/#languages)!

## Awards

<a href="http://www.giga.de/downloads/soundswitch/"><img src="https://i.imgur.com/19GaPLQ.png" alt="Giga 5 stars" height="100" hspace="10"/></a><a href="http://www.softpedia.com/get/Multimedia/Audio/Other-AUDIO-Tools/SoundSwitch.shtml#status"><img src="http://s1.softpedia-static.com/_img/sp100free.png" alt="Softpedia" height="100" hspace="10"/></a><a href="http://www.chip.de/downloads/SoundSwitch_94258571.html"><img src="https://i.imgur.com/Nedw1su.png" alt="Chip Online de" height="100" hspace="10"/></a><a href="https://www.netzwelt.de/download/24278-soundswitch.html"><img src="https://i.imgur.com/VaMTnxV.png" alt="netzwelt GmbH" height="100" hspace="10"/></a>

## Thanks

### 🐱‍💻 Credits

- Original Developer: [Jeroen Pelgrims](http://jeroenpelgrims.be)
- Disabling Notification [#33](https://github.com/Belphemur/SoundSwitch/pull/33) [@adamblackburn](https://github.com/adamblackburn)
- Localization and german translation [#157](https://github.com/Belphemur/SoundSwitch/pull/157) [@FireEmerald](https://github.com/FireEmerald) 
- Banner Notification [#186](https://github.com/Belphemur/SoundSwitch/pull/186) [@ramon18](https://github.com/ramon18)
- Keyboard hook, [Christian Liensberger](http://www.liensberger.it/web/blog/?p=207)
- Changing default sound device, [EreTIk](http://eretik.omegahg.com/)
- Notification Sound, [Music box notification sound by Robinhood76](https://www.freesound.org/people/Robinhood76/sounds/216676/)
- Spanish translation [#244](https://github.com/Belphemur/SoundSwitch/pull/244) [@plextoriano](https://github.com/plextoriano)
- Portuguese (Brazilian) translation [#258](https://github.com/Belphemur/SoundSwitch/pull/258) [@aleczk](https://github.com/aleczk)
- Awesome Logo [#278](https://github.com/Belphemur/SoundSwitch/pull/278) [@linadesteem](https://github.com/linadesteem)
- Icons [Pastel SVG icon set](https://codefisher.org/pastel-svg/), by Michael Buckley ([CC BY-NC-SA 4.0](http://creativecommons.org/licenses/by-nc-sa/4.0/))
- Discovered and reported a security vulnerability with the updater and its code signature checker [#415](https://github.com/Belphemur/SoundSwitch/issues/415) [@JarLob](https://github.com/JarLob)
- Free Icons from [Font Awesome](https://fontawesome.com/), Creative Commons Attribution 4.0 International license: [License](https://fontawesome.com/license/free)
### 🤝 JetBrains ![JetBrain Tooling](https://i.imgur.com/SN2qAuL.png "JetBrain Tooling")

Thanks for their Open-Source licence to their amazing IDEs and addons like [ReSharper](https://www.jetbrains.com/resharper) for Visual Studio.

## License: GPLv2

<a href="https://app.fossa.io/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch?ref=badge_large"><img alt="FOSSA Status" align="right" src="https://app.fossa.io/api/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch.svg?type=large"></a>

Copyright (C) 2015 Jeroen Pelgrims

Copyright (C) 2015-2021 Antoine Aflalo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

The complete GPLv2 license file is located [here](https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE.txt).
