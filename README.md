<a href="https://soundswitch.aaflalo.me" title="SoundSwitch Website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="SoundSwitch Logo" height="180px"></a>
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-2-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

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

## Contributors

<!-- readme: collaborators,contributors -start -->
<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/Ephemerality"><img src="https://avatars.githubusercontent.com/u/7145692?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Ephemerality</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=Ephemerality" title="Code">💻</a></td>
    <td align="center"><a href="https://fire-emerald.com"><img src="https://avatars.githubusercontent.com/u/1107939?v=4?s=100" width="100px;" alt=""/><br /><sub><b>FireEmerald</b></sub></a><br /><a href="https://github.com/Belphemur/SoundSwitch/commits?author=FireEmerald" title="Code">💻</a> <a href="#translation-FireEmerald" title="Translation">🌍</a> <a href="https://github.com/Belphemur/SoundSwitch/commits?author=FireEmerald" title="Documentation">📖</a></td>
  </tr>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
<table>
<tr>
    <td align="center">
        <a href="https://github.com/Belphemur">
            <img src="https://avatars.githubusercontent.com/u/197810?v=4" width="100;" alt="Belphemur"/>
            <br />
            <sub><b>Antoine Aflalo</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/FireEmerald">
            <img src="https://avatars.githubusercontent.com/u/1107939?v=4" width="100;" alt="FireEmerald"/>
            <br />
            <sub><b>FireEmerald</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/weblate">
            <img src="https://avatars.githubusercontent.com/u/1607653?v=4" width="100;" alt="weblate"/>
            <br />
            <sub><b>Weblate (bot)</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/comradekingu">
            <img src="https://avatars.githubusercontent.com/u/13802408?v=4" width="100;" alt="comradekingu"/>
            <br />
            <sub><b>Allan Nordhøy</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/semantic-release-bot">
            <img src="https://avatars.githubusercontent.com/u/32174276?v=4" width="100;" alt="semantic-release-bot"/>
            <br />
            <sub><b>Semantic Release Bot</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/milotype">
            <img src="https://avatars.githubusercontent.com/u/43657314?v=4" width="100;" alt="milotype"/>
            <br />
            <sub><b>Milotype</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Hwaro-k">
            <img src="https://avatars.githubusercontent.com/u/63298848?v=4" width="100;" alt="Hwaro-k"/>
            <br />
            <sub><b>Hwaro-k</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bovirus">
            <img src="https://avatars.githubusercontent.com/u/1262554?v=4" width="100;" alt="bovirus"/>
            <br />
            <sub><b>Bovirus</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/kbasaran">
            <img src="https://avatars.githubusercontent.com/u/8277075?v=4" width="100;" alt="kbasaran"/>
            <br />
            <sub><b>Kbasaran</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/igorruckert">
            <img src="https://avatars.githubusercontent.com/u/3202395?v=4" width="100;" alt="igorruckert"/>
            <br />
            <sub><b>Igor Rückert</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/VenusGirl">
            <img src="https://avatars.githubusercontent.com/u/53147200?v=4" width="100;" alt="VenusGirl"/>
            <br />
            <sub><b>VenusGirl</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/laralem">
            <img src="https://avatars.githubusercontent.com/u/80917261?v=4" width="100;" alt="laralem"/>
            <br />
            <sub><b>Laralem</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/kreis117">
            <img src="https://avatars.githubusercontent.com/u/74777259?v=4" width="100;" alt="kreis117"/>
            <br />
            <sub><b>Kreis117</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/SantosSi">
            <img src="https://avatars.githubusercontent.com/u/31543615?v=4" width="100;" alt="SantosSi"/>
            <br />
            <sub><b>Silvério Santos</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/wvxwxvw">
            <img src="https://avatars.githubusercontent.com/u/13194155?v=4" width="100;" alt="wvxwxvw"/>
            <br />
            <sub><b>Wvxwxvw</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/KovalevArtem">
            <img src="https://avatars.githubusercontent.com/u/36500228?v=4" width="100;" alt="KovalevArtem"/>
            <br />
            <sub><b>KovalevArtem</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/maiieam">
            <img src="https://avatars.githubusercontent.com/u/12435196?v=4" width="100;" alt="maiieam"/>
            <br />
            <sub><b>IaMMaI</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/PolarniMeda">
            <img src="https://avatars.githubusercontent.com/u/32743409?v=4" width="100;" alt="PolarniMeda"/>
            <br />
            <sub><b>PolarniMeda</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/fitojb">
            <img src="https://avatars.githubusercontent.com/u/554953?v=4" width="100;" alt="fitojb"/>
            <br />
            <sub><b>Adolfo Jayme-Barrientos</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/AlexisKerib">
            <img src="https://avatars.githubusercontent.com/u/9393411?v=4" width="100;" alt="AlexisKerib"/>
            <br />
            <sub><b>AlexisKerib</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/JadranR">
            <img src="https://avatars.githubusercontent.com/u/53031490?v=4" width="100;" alt="JadranR"/>
            <br />
            <sub><b>Jadran</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Montibus">
            <img src="https://avatars.githubusercontent.com/u/38812680?v=4" width="100;" alt="Montibus"/>
            <br />
            <sub><b>Montibus</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Megaranator">
            <img src="https://avatars.githubusercontent.com/u/23362846?v=4" width="100;" alt="Megaranator"/>
            <br />
            <sub><b>Martin Mojžíšek</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/rdwolff">
            <img src="https://avatars.githubusercontent.com/u/1839530?v=4" width="100;" alt="rdwolff"/>
            <br />
            <sub><b>Robert De Wolff</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/XMoon">
            <img src="https://avatars.githubusercontent.com/u/1174109?v=4" width="100;" alt="XMoon"/>
            <br />
            <sub><b>月白</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/fx02">
            <img src="https://avatars.githubusercontent.com/u/2067180?v=4" width="100;" alt="fx02"/>
            <br />
            <sub><b>Zvonimir Bužanić</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/aleczk">
            <img src="https://avatars.githubusercontent.com/u/16522249?v=4" width="100;" alt="aleczk"/>
            <br />
            <sub><b>Alecsander Camilo</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/yarons">
            <img src="https://avatars.githubusercontent.com/u/406826?v=4" width="100;" alt="yarons"/>
            <br />
            <sub><b>Yaron Shahrabani</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ArquesMartin">
            <img src="https://avatars.githubusercontent.com/u/58190872?v=4" width="100;" alt="ArquesMartin"/>
            <br />
            <sub><b>ArquesMartin</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/chendong135">
            <img src="https://avatars.githubusercontent.com/u/45531525?v=4" width="100;" alt="chendong135"/>
            <br />
            <sub><b>Chendong135</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/NathanBnm">
            <img src="https://avatars.githubusercontent.com/u/45366162?v=4" width="100;" alt="NathanBnm"/>
            <br />
            <sub><b>Nathan Bonnemains</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ztoldy">
            <img src="https://avatars.githubusercontent.com/u/6718491?v=4" width="100;" alt="ztoldy"/>
            <br />
            <sub><b>Ztoldy</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bh19954">
            <img src="https://avatars.githubusercontent.com/u/31904160?v=4" width="100;" alt="bh19954"/>
            <br />
            <sub><b>Bh19954</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/zprood">
            <img src="https://avatars.githubusercontent.com/u/692206?v=4" width="100;" alt="zprood"/>
            <br />
            <sub><b>Liamz</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/linadesteem">
            <img src="https://avatars.githubusercontent.com/u/35618068?v=4" width="100;" alt="linadesteem"/>
            <br />
            <sub><b>Linadesteem</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ramon18">
            <img src="https://avatars.githubusercontent.com/u/5236262?v=4" width="100;" alt="ramon18"/>
            <br />
            <sub><b>Ramon18</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/plextoriano">
            <img src="https://avatars.githubusercontent.com/u/2480960?v=4" width="100;" alt="plextoriano"/>
            <br />
            <sub><b>Mario Campo</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/schwaben-github">
            <img src="https://avatars.githubusercontent.com/u/1545878?v=4" width="100;" alt="schwaben-github"/>
            <br />
            <sub><b>Tibor W.</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ys27">
            <img src="https://avatars.githubusercontent.com/u/5151718?v=4" width="100;" alt="ys27"/>
            <br />
            <sub><b>Andrew An</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/j-quintas">
            <img src="https://avatars.githubusercontent.com/u/25798963?v=4" width="100;" alt="j-quintas"/>
            <br />
            <sub><b>Jorge Quintas</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/O11Software">
            <img src="https://avatars.githubusercontent.com/u/58270063?v=4" width="100;" alt="O11Software"/>
            <br />
            <sub><b>O11Software</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/patgrosse">
            <img src="https://avatars.githubusercontent.com/u/23578938?v=4" width="100;" alt="patgrosse"/>
            <br />
            <sub><b>Patrick Große</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Vipcioo">
            <img src="https://avatars.githubusercontent.com/u/19477134?v=4" width="100;" alt="Vipcioo"/>
            <br />
            <sub><b>Vipcioo</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/adamblackburn">
            <img src="https://avatars.githubusercontent.com/u/3804080?v=4" width="100;" alt="adamblackburn"/>
            <br />
            <sub><b>Adam Blackburn</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Blueberryy">
            <img src="https://avatars.githubusercontent.com/u/36592509?v=4" width="100;" alt="Blueberryy"/>
            <br />
            <sub><b>Blueberryy</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/d3fz">
            <img src="https://avatars.githubusercontent.com/u/22578839?v=4" width="100;" alt="d3fz"/>
            <br />
            <sub><b>Filipe A.</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mansil">
            <img src="https://avatars.githubusercontent.com/u/3278371?v=4" width="100;" alt="mansil"/>
            <br />
            <sub><b>Manuela Silva</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/MauricioGloria">
            <img src="https://avatars.githubusercontent.com/u/55420083?v=4" width="100;" alt="MauricioGloria"/>
            <br />
            <sub><b>MauricioGloria</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/kuzmeech">
            <img src="https://avatars.githubusercontent.com/u/2181438?v=4" width="100;" alt="kuzmeech"/>
            <br />
            <sub><b>Kuz8</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Niko-O">
            <img src="https://avatars.githubusercontent.com/u/4943909?v=4" width="100;" alt="Niko-O"/>
            <br />
            <sub><b>Niko-O</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/fossabot">
            <img src="https://avatars.githubusercontent.com/u/29791463?v=4" width="100;" alt="fossabot"/>
            <br />
            <sub><b>Fossabot</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/lpv-repo">
            <img src="https://avatars.githubusercontent.com/u/7538200?v=4" width="100;" alt="lpv-repo"/>
            <br />
            <sub><b>Lpv</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/opus-2">
            <img src="https://avatars.githubusercontent.com/u/24445823?v=4" width="100;" alt="opus-2"/>
            <br />
            <sub><b>Opus2</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/westonhowe98">
            <img src="https://avatars.githubusercontent.com/u/16272087?v=4" width="100;" alt="westonhowe98"/>
            <br />
            <sub><b>Westonhowe98</b></sub>
        </a>
    </td></tr>
</table>
<!-- readme: collaborators,contributors -end -->

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
