# SoundSwitch [![Build status](https://ci.appveyor.com/api/projects/status/bt0yr309rq74tbvc?svg=true)](https://ci.appveyor.com/project/Belphemur/soundswitch)
SoundSwitch makes it easier to switch playback devices (sound cards).

Normally, to switch a Playback device you need to right click the sound icon in the bottom right corner of your screen (system tray),
choose "Playback devices" and then change the default playback device.
Every time you want to switch.

With SoundSwitch you just configure once between which Playback devices you want to toggle and then you can press Ctrl+Alt+F11 to toggle automatically!

## Dependencies
You need .NET 4.5.2 installed to work: [Microsoft .NET Framework 4.5.2 (Offline Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=42642) 

You also need [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48145)

## Configuration
Right click the SoundSwitch icon in your system tray and choose Settings.
Now select the devices between which you want to toggle.
Optionally you can also change the keyboard combination, by default this is {Ctrl + Alt + F11}
If you want the application to start automatically when your pc boots up, check the box "Run at startup".

## Usage
Press Ctrl + Alt + F11 to cycle through the playback devices.
(Or a different key combination if you changed it)

## Features

### Auto-Updater
This is one of the interesting feature added in Isabelline Gold, the auto-updater. Every 12h SoundSwitch will check the github repository (thanks to the GitHub API) for a new release, if a new one is available, the user will get a notification and the  “No Update” in the context menu will change to “Update Available (X.X.X)” where X.X.X represent the new version number. When the user click on it, a new Window opens with a progress bar (see screenshots). The new version get automatically downloaded in the temp folder of the user. When the download is finished the user can install the Update by just clicking the install button. A changelog is also provided by getting the information set in the release on GitHub.

### Communications
SoundSwitch can also change the communications devices when asked in the Settings. Windows make a differentiation between Multimedia and Communication, it means if an application ask to have access to Communications Audio Device (like Skype), it will receive the default Communications device. By default SoundSwitch only change the Multimedia device, and not the communications. Now if the checkbox is checked in the settings, it will also change the Communications device.

## License

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
