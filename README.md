# SoundSwitch [![Build status](https://ci.appveyor.com/api/projects/status/bt0yr309rq74tbvc?svg=true)](https://ci.appveyor.com/project/Belphemur/soundswitch) [![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://github.com/Belphemur/SoundSwitch/releases) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://www.aaflalo.me/downloads/) [![AppVeyor branch](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://www.aaflalo.me/donate/)
SoundSwitch makes it easier to switch playback devices (sound cards).

Normally, to switch a Playback device you need to right click the sound icon in the bottom right corner of your screen (system tray),
choose "Playback devices" and then change the default playback device.
Every time you want to switch.

With SoundSwitch you just configure once between which Playback devices you want to toggle and then you can press Ctrl+Alt+F11 to toggle automatically!

## Dependencies
You need .NET 4.6.2 installed to work: [Microsoft .NET Framework 4.5.2 (Web Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=53345) 

You also need [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48145)

## Donations
If you want to donate for the development don't hesite to do it here: https://www.aaflalo.me/donate/

## Configuration
Right click the SoundSwitch icon in your system tray and choose Settings.
Now select the devices between which you want to toggle.
Optionally you can also change the keyboard combination, by default this is {Ctrl + Alt + F11}
If you want the application to start automatically when your pc boots up, check the box "Run at startup".

## Usage
Press Ctrl + Alt + F11 to cycle through the playback devices.
(Or a different key combination if you changed it)

For Recording devices the default is : Ctrl + Alt + F7

### Double clicks
You can also double click on the systray icon to switch audio devices.

## Features

### Recording Devices
SoundSwitch can also switch your recording devices. You can set HotKeys as for the Playback devices.

### Auto-Updater
This is one of the interesting feature added in Isabelline Gold, the auto-updater. Every 12h SoundSwitch will check the github repository (thanks to the GitHub API) for a new release, if a new one is available, the user will get a notification and the  “No Update” in the context menu will change to “Update Available (X.X.X)” where X.X.X represent the new version number. When the user click on it, a new Window opens with a progress bar (see screenshots). The new version get automatically downloaded in the temp folder of the user. When the download is finished the user can install the Update by just clicking the install button. A changelog is also provided by getting the information set in the release on GitHub.

### Communications
SoundSwitch can also change the communications devices when asked in the Settings. Windows make a differentiation between Multimedia and Communication, it means if an application ask to have access to Communications Audio Device, it will receive the default Communications device. By default SoundSwitch only change the Multimedia device, and not the communications. Now if the checkbox is checked in the settings, it will also change the Communications device.

### Notifications
SoundSwitch provides 3 types of notification.

#### Windows Notification
This is the default notification, it use the ballontip of Windows. In the case of Windows 7, it's the little ballon that open next to the systray icon. For Windows 10, it's the notification system that slide from the right corner of the screen.

#### Sound Notification
This notification is a sound played on the switched device. This way when you are switching device, the new device will "chime" to tell you it's selected.

#### Toast Notification (>=Win 8)
If you are on Windows 8 or more you can use this type of Toast notification. More personalization are possible, by default the notification is silent but you can set a custom sound for it (mp3 or wav for now).

If you want to return to a silent Toast notification, open the file selector, and just do Cancel. Doing that will deconfigure the sound.

#### No Notification
In case you're not interested of getting any sort of notification.

## Awards

### Giga 5 Stars
[![Giga 5 stars](http://i.imgur.com/19GaPLQ.png)](http://www.giga.de/downloads/soundswitch/)

## License: GPLv2

Copyright (C) 2015 Jeroen Pelgrims

Copyright (C) 2015 Antoine Aflalo

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
