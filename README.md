# SoundSwitch [![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://github.com/Belphemur/SoundSwitch/releases) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://www.aaflalo.me/downloads/) [![AppVeyor branch](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://www.aaflalo.me/donate/)
SoundSwitch makes it easier to switch playback devices (sound cards).

Normally, to switch a playback device you need to right click the sound icon in the bottom right corner of your screen (system tray), choose 'Playback Devices' and then change the Default Playback Device. Every time you want to switch.

With SoundSwitch you just configure once between which playback devices you want to toggle and then you can press Ctrl+Alt+F11 to toggle automatically!

## Preview image
![preview](img/preview.gif)

## Dependencies
You need .NET 4.6.2 installed to work: [Microsoft .NET Framework 4.6.2 (Web Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=53345) 

You also need [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53587)

## Donations
If you want to donate for the development don't hesite to do it here: https://www.aaflalo.me/donate/

## Configuration
Right click the SoundSwitch icon in your system tray and choose Settings. Now select the devices between which you want to toggle. Optionally you can also change the keyboard combination, by default this is {Ctrl + Alt + F11} If you want the application to start automatically when your PC boots up, check the box 'Start automatically with Windows'.

## Usage
Press Ctrl + Alt + F11 to cycle through the playback devices. (Or a different key combination if you changed it)

For recording devices the default is: Ctrl + Alt + F7

### Double clicks
You can also double click on the systray icon to switch audio devices.

### Left Click
If you use the left click on the systray icon, you'll see a menu appear where you can select the wanted device.

## Features

### Recording Devices
SoundSwitch can also take care of your recording devices. You can set specifics HotKeys as for the playback devices.

### Auto-Updater
This is one of the interesting feature added in Isabelline Gold, the auto-updater. Every 24h SoundSwitch will check the GitHub repository (thanks to the GitHub API) for a new release. If a new one is available the user will get a notification and the 'No update available' in the context menu will change to 'Update Available (X.X.X)' where X.X.X represent the new version number. When the user clicks on it, a new window opens with a progress bar (see screenshots). The new version gets automatically downloaded in the temp folder of the user. When the download is finished, the user can install the Update by just clicking the install button. A changelog is also provided by getting the information set in the release on GitHub.

#### Update Modes
There are three different options available: Silent, means the program updates itself in the background without any prompts. Notify, you will be notified when there's an update available. Never, well this is self-explained.

### Communications
SoundSwitch can also change the 'Default Communication Device' when asked in the Settings. Windows makes a differentiation between Multimedia and Communication; it means if an application asks to have access to communications audio device, it will receive the Default Communication Device. By default SoundSwitch only changes the multimedia device and not the communication. Now if the checkbox is checked in the settings, it will also change the Communication Device.

### Notifications
SoundSwitch provides four types of notification.

#### Windows Notification
This is the default notification; it uses the balloon tip of Windows. In the case of Windows 7, it's the little balloon that opens next to the systray icon. For Windows 10, it's the notification system that slides from the right corner of the screen.

#### Sound Notification
This notification is a sound played on the switched device. This way when you are switching devices, the new device will 'chime' to tell you it's selected.

### Customized Sound Notification
The same as a Sound Notification but you can specify the sound which is played.

#### Toast Notification (≥Win 8)
If you are on Windows 8 or later, you can use this type of Toast Notification. More personalization is possible, by default the notification is silent, but you can set a custom sound for it (mp3 or wav for now).

If you want to return to a silent Toast notification, open the file selector, and just do Cancel. Doing that will remove the set sound.

#### No Notification
In case you're not interested in getting any notification.

### Multi-Language Support
There are three languages available: English, French and German.

Would you like to help us with the localization? Great! See [Add or modify another language](../../wiki/Add-or-modify-another-language) for further informations.

## Awards

### Giga 5 Stars
[![Giga 5 stars](http://i.imgur.com/19GaPLQ.png)](http://www.giga.de/downloads/soundswitch/)

### Softpedia
[![Softpedia](http://s1.softpedia-static.com/_img/sp100free.png)](http://www.softpedia.com/get/Multimedia/Audio/Other-AUDIO-Tools/SoundSwitch.shtml#status)

### Chip Online
[![Chip Online de](http://i.imgur.com/Nedw1su.png)](http://www.chip.de/downloads/SoundSwitch_94258571.html)


## License: GPLv2

Copyright (C) 2015 Jeroen Pelgrims

Copyright (C) 2015-2017 Antoine Aflalo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

## Credits

### Original Developer
[Jeroen Pelgrims](http://jeroenpelgrims.be)

### Icons
The [Pastel SVG icon set](https://codefisher.org/pastel-svg/) Created by Michael Buckley is licensed under the:

Creative Commons Attribution NonCommercial Share Alike 4.0
http://creativecommons.org/licenses/by-nc-sa/4.0/ 

### Keyboard hook
[Christian Liensberger](http://www.liensberger.it/web/blog/?p=207)

### Changing default sound device
[EreTIk](http://eretik.omegahg.com/)

### Disabling notification
[@adamblackburn](https://github.com/adamblackburn) with merge request [#33](https://github.com/Belphemur/SoundSwitch/pull/33)

### Notification Sound
[Music box notification sound by Robinhood76](https://www.freesound.org/people/Robinhood76/sounds/216676/)
