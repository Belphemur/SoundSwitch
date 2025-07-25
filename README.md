<a href="https://soundswitch.aaflalo.me" title="SoundSwitch Website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="SoundSwitch Logo" height="180px"></a>

[![.NET](https://github.com/Belphemur/SoundSwitch/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Belphemur/SoundSwitch/actions/workflows/dotnet.yml)[![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://soundswitch.aaflalo.me) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://soundswitch.aaflalo.me/) [![Translate](https://hosted.weblate.org/widgets/soundswitch/-/svg-badge.svg)](https://hosted.weblate.org/projects/soundswitch/) [![Donate](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://soundswitch.aaflalo.me) [![Help](https://img.shields.io/badge/Discord-Community%20&%20Help-green?style=flat-square&logo=discord)](https://discord.gg/gUCw3Ue)

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->

[![All Contributors](https://img.shields.io/badge/all_contributors-13-orange.svg?style=flat-square)](#contributors-)

<!-- ALL-CONTRIBUTORS-BADGE:END -->

**SoundSwitch** offers you the opportunity to switch your playback and recording devices using simple **hotkeys**.

**No more** navigating througth several menus and screens. Just configure SoundSwitch **once** and you are able to switch between your devices faster than ever before!

## ✨ Preview

![Preview](https://soundswitch.aaflalo.me/img/preview.gif?v=20191124)

## Requirements

- ⚠ Operating System: Windows 7 or newer

## Donations

If you'd like to support development, we would love to see you [here ❤](https://soundswitch.aaflalo.me/#donate).

## Configuration

_Right click_ the SoundSwitch icon in your system tray and choose _Settings_. Now select the devices between which you want to toggle. Optionally, you can also change the keyboard combination. If you want the application to start automatically when your PC boots up, check the box _Start automatically with Windows_.

## Usage

First set up your devices you want to cycle through, using _right click_ on the _System Tray Icon_ of SoundSwitch.

**After you configured** SoundSwitch you can use the following hotkeys:

- 🔊 To cycle through the **playback devices** press:

  - `Ctrl` + `Alt` + `F11` (default) **or**
  - Double click on the `System Tray Icon` of SoundSwitch.

- 🎙 To cycle through the **recording devices** press:

  - `Ctrl` + `Alt` + `F7` (default)

- 🔇 To mute the **default microphone** press:

  - `Ctrl` + `Alt` + `M` (default)

  When a microphone is muted, a persistent banner will appear on your screen to remind you that your microphone is muted. The banner will remain visible until you unmute the microphone or click on the banner to unmute it directly.

## 🖱️ System Tray Icon Actions

The system tray icon supports configurable double-click actions. You can choose what happens when you double-click the SoundSwitch icon:

- **🔊 Switch Device** (default): Cycles through your configured playback devices
- **📋 Switch Profile**: Cycles through your configured audio profiles
- **⚙️ Open Settings**: Opens the SoundSwitch settings window

This behavior can be customized in the SoundSwitch settings to match your preferred workflow.

## Command Line Interface

SoundSwitch includes a powerful CLI that allows you to control the application through command line:

- Switch between playback/recording devices
- Control microphone mute state
- Manage audio profiles
- Access settings

See the [CLI documentation](SoundSwitch.CLI/README.md) for more details about available commands and usage.

## _Switched_ Notification

SoundSwitch provides four types of notification when a device is changed:

- #### 🎟 Banner Notification

  Uses a custom always-on-top frame, useful for in-game usage. This is the recommended default display style.

- #### 🗨 Windows Notification

  Uses the balloon tip of Windows. In the case of Windows 7, it's the little balloon that opens next to the systray icon. For Windows 10, it's the notification system that slides from the right corner of the screen.

- #### 🎵 Sound Notification
  This notification is a sound played on the switched device. This way when you are switching devices, the new device will 'chime' to tell you it's selected.

## Profiles

Using profiles, you can automatically switch to specific audio devices when certain conditions are met. Profiles support multiple trigger types and advanced device management:

### 🎯 Profile Triggers

- **⌨️ Hotkey Triggers**: Switch devices using custom key combinations. Multiple profiles can share the same hotkey and cycle through them automatically.

- **💫 Application Triggers**: Automatically switch devices when specific applications gain focus. For example, route Spotify to speakers while games use your headset.

- **🪟 Window Triggers**: Switch devices based on window titles. Useful for applications that change their window names dynamically.

- **🎮 Steam Big Picture**: Special profile that activates automatically when Steam Big Picture mode is launched.

- **📱 UWP App Triggers**: Support for Universal Windows Platform applications with automatic device switching.

- **🚀 Startup Triggers**: Profiles that activate automatically when SoundSwitch starts.

- **🔄 Device Changed Triggers**: Force profiles that maintain specific device configurations even when Windows tries to change them.

- **📋 Tray Menu Triggers**: Profiles accessible directly from the system tray context menu.

### 🎚️ Advanced Profile Features

- **Multi-Device Support**: Configure separate devices for playback, communication, recording, and recording communication
- **Smart Device Restoration**: Automatically restore previous audio settings when a profile deactivates
- **Foreground App Switching**: Option to switch only the focused application's audio instead of system-wide
- **Default Device Control**: Choose whether to change Windows default devices or only application-specific routing
- **Notification Control**: Enable/disable notifications when profiles activate
- **Device Validation**: Automatic checking for device availability with fallback handling

### 🔄 Hotkey Cycling

When multiple profiles share the same hotkey, SoundSwitch automatically cycles through them. If Quick Menu is enabled, a visual selector appears allowing you to choose the specific profile to activate.

## Advanced

### 🎙 Communications

SoundSwitch can also change the **Default Communication Device** when asked in the Settings. Windows differentiates between Multimedia and Communication; it means if an application asks to have access to communications audio device, it will receive the Default Communication Device. By default SoundSwitch only changes the multimedia device and not the communication device. Now if the checkbox is checked in the settings, it will also change the Communication Device.

### 📥 Auto-Updater

Every 24 hours SoundSwitch checks the GitHub repository (thanks to the GitHub API) for a new release. If a new one is available you will get a notification and the 'No update available' in the context menu will change to 'Update Available'. The new version gets automatically downloaded and installed, depeding on your _Update Mode_. We also provide a changelog with the latest improvements of SoundSwitch.

#### 🚥 Update Modes

There are three different options available on how updates are installed:

- **Silent**, means the program updates itself in the background without any prompts.
- **Notify**, you will be notified when there's an update available.
- **Never**, well this is self-explained.

### 🌎 Multi-Language Support

SoundSwitch is available in more than 20 languages including **English**, **French**, **German**, **Spanish**, **Italian**, **Portuguese (Brazilian)**, **Russian**, **Chinese**, and many more.

Want to improve an existing language or add another one? Translations are online editable [right here](https://hosted.weblate.org/projects/soundswitch/#languages)!

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://fire-emerald.com"><img src="https://avatars.githubusercontent.com/u/1107939?v=4?s=100" width="100px;" alt="FireEmerald"/><br /><sub><b>FireEmerald</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=FireEmerald" title="Code">💻</a> <a href="#translation-FireEmerald" title="Translation">🌍</a> <a href="https://github.com/Belphemur/SoundSwitch/commits?author=FireEmerald" title="Documentation">📖</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ramon18"><img src="https://avatars.githubusercontent.com/u/5236262?v=4?s=100" width="100px;" alt="ramon18"/><br /><sub><b>ramon18</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=ramon18" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Ephemerality"><img src="https://avatars.githubusercontent.com/u/7145692?v=4?s=100" width="100px;" alt="Ephemerality"/><br /><sub><b>Ephemerality</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=Ephemerality" title="Code">💻</a> <a href="https://github.com/Belphemur/SoundSwitch/commits?author=Ephemerality" title="Tests">⚠️</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/adamblackburn"><img src="https://avatars.githubusercontent.com/u/3804080?v=4?s=100" width="100px;" alt="Adam Blackburn"/><br /><sub><b>Adam Blackburn</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=adamblackburn" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/linadesteem"><img src="https://avatars.githubusercontent.com/u/35618068?v=4?s=100" width="100px;" alt="linadesteem"/><br /><sub><b>linadesteem</b></sub></a><br /><a href="#design-linadesteem" title="Design">🎨</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/westonhowe98"><img src="https://avatars.githubusercontent.com/u/16272087?v=4?s=100" width="100px;" alt="westonhowe98"/><br /><sub><b>westonhowe98</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=westonhowe98" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://jarlob.github.io"><img src="https://avatars.githubusercontent.com/u/26652396?v=4?s=100" width="100px;" alt="Jaroslav Lobačevski"/><br /><sub><b>Jaroslav Lobačevski</b></sub></a><br /><a href="#security-JarLob" title="Security">🛡️</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/lpv11"><img src="https://avatars.githubusercontent.com/u/7538200?v=4?s=100" width="100px;" alt="lpv"/><br /><sub><b>lpv</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=lpv11" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/patgrosse"><img src="https://avatars.githubusercontent.com/u/23578938?v=4?s=100" width="100px;" alt="Patrick Große"/><br /><sub><b>Patrick Große</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=patgrosse" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/codemann8"><img src="https://avatars.githubusercontent.com/u/1323444?v=4?s=100" width="100px;" alt="codemann8"/><br /><sub><b>codemann8</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=codemann8" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/XangelMusic"><img src="https://avatars.githubusercontent.com/u/22012301?v=4?s=100" width="100px;" alt="XangelMusic"/><br /><sub><b>XangelMusic</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=XangelMusic" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://gradyn.com"><img src="https://avatars.githubusercontent.com/u/20762604?v=4?s=100" width="100px;" alt="Gradyn Wursten"/><br /><sub><b>Gradyn Wursten</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=GNUGradyn" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Scordo"><img src="https://avatars.githubusercontent.com/u/2535846?v=4?s=100" width="100px;" alt="Scordo"/><br /><sub><b>Scordo</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=Scordo" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

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

Copyright (C) 2015-2025 Antoine Aflalo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

The complete GPLv2 license file is located [here](https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE.txt).
