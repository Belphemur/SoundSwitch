# Changelog

## [v5.5.1](https://github.com/Belphemur/SoundSwitch/tree/v5.5.1) (2020-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.5.0...v5.5.1)

**Implemented enhancements:**

- When Steam Big Picture is closed, returned the audio default devices as they were before switching [\#490](https://github.com/Belphemur/SoundSwitch/issues/490)
- Request: Run profile on startup [\#489](https://github.com/Belphemur/SoundSwitch/issues/489)
- Change "No update available" text when update-check is disabled [\#487](https://github.com/Belphemur/SoundSwitch/issues/487)

**Fixed bugs:**

- High DPI Mode not working anymore \(4K screen with zooming 150%\) [\#494](https://github.com/Belphemur/SoundSwitch/issues/494)
- Wrong error message when adding a profile without triggers [\#488](https://github.com/Belphemur/SoundSwitch/issues/488)

## [v5.5.0](https://github.com/Belphemur/SoundSwitch/tree/v5.5.0) (2020-07-28)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.4.0...v5.5.0)

**Implemented enhancements:**

- ui window position when starting from task bar.. [\#484](https://github.com/Belphemur/SoundSwitch/issues/484)
- Notifications on profile activation [\#483](https://github.com/Belphemur/SoundSwitch/issues/483)
- Reconnected devices and profiles [\#482](https://github.com/Belphemur/SoundSwitch/issues/482)
- Make the installer update silent when using the changelog view [\#480](https://github.com/Belphemur/SoundSwitch/issues/480)
- Integrate an option to automatic switch the sound device when Steam Big Picture ist running [\#478](https://github.com/Belphemur/SoundSwitch/issues/478)
- Better support for MultiScreen: Banner displayed on active screen [\#475](https://github.com/Belphemur/SoundSwitch/issues/475)
- \[Enhancements\] Profile editing [\#431](https://github.com/Belphemur/SoundSwitch/issues/431)
- Default Communication Device support [\#348](https://github.com/Belphemur/SoundSwitch/issues/348)
- Add feature: Switch only communications device [\#252](https://github.com/Belphemur/SoundSwitch/issues/252)

**Fixed bugs:**

- Installer cannot detect correctly version of .NET Core runtime when installed with the SDK [\#486](https://github.com/Belphemur/SoundSwitch/issues/486)
- Crash when trying to display the tooltip of the playback device [\#481](https://github.com/Belphemur/SoundSwitch/issues/481)
- Installation crashes at .NET Core 3.1.4 since Update from Microsoft [\#479](https://github.com/Belphemur/SoundSwitch/issues/479)
- Installer Offers To Install Older Desktop Runtime [\#474](https://github.com/Belphemur/SoundSwitch/issues/474)
- HDMI sound device is not switched to using the hotkey [\#466](https://github.com/Belphemur/SoundSwitch/issues/466)

**Closed issues:**

- SoundSwitch crash when switching profile with a device that isn't active at time [\#485](https://github.com/Belphemur/SoundSwitch/issues/485)
- SounSwitch crashes after launch in Windows 7 x64  [\#477](https://github.com/Belphemur/SoundSwitch/issues/477)
- Crash at startup when soundswitch is installed on multiple users on Windows 10 [\#476](https://github.com/Belphemur/SoundSwitch/issues/476)

## [v5.4.0](https://github.com/Belphemur/SoundSwitch/tree/v5.4.0) (2020-06-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.3.1...v5.4.0)

**Implemented enhancements:**

- Make an item in the menu for the Community / Discord [\#472](https://github.com/Belphemur/SoundSwitch/issues/472)
- SoundSwitch - Forum, Discord Chat & FAQs. [\#470](https://github.com/Belphemur/SoundSwitch/issues/470)
- Add Croatian to the app [\#465](https://github.com/Belphemur/SoundSwitch/issues/465)

**Fixed bugs:**

- Banner + acoustic signal when switching output device [\#471](https://github.com/Belphemur/SoundSwitch/issues/471)
- Settings Tab Not Displaying Correctly [\#467](https://github.com/Belphemur/SoundSwitch/issues/467)
- SoundSwitch crash at boot: Instance not initialized [\#464](https://github.com/Belphemur/SoundSwitch/issues/464)
- SoundSwitch crash at start: Named Pipe access denied [\#463](https://github.com/Belphemur/SoundSwitch/issues/463)
- Crash when switching to a profile where the device isn't connected [\#462](https://github.com/Belphemur/SoundSwitch/issues/462)
- SwoundSwitch preventing Windows 10 Sleep mode [\#439](https://github.com/Belphemur/SoundSwitch/issues/439)

**Closed issues:**

- Soundswitch crashes at startup [\#468](https://github.com/Belphemur/SoundSwitch/issues/468)
- Unable to donate via PayPal [\#312](https://github.com/Belphemur/SoundSwitch/issues/312)

**Merged pull requests:**

- feature/ui-improvements: Various small improvements for the UI [\#461](https://github.com/Belphemur/SoundSwitch/pull/461) ([FireEmerald](https://github.com/FireEmerald))

## [v5.3.1](https://github.com/Belphemur/SoundSwitch/tree/v5.3.1) (2020-05-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.3.0...v5.3.1)

**Fixed bugs:**

- Trying to get Help/Donate using the menu make SoundSwitch crash [\#460](https://github.com/Belphemur/SoundSwitch/issues/460)

**Merged pull requests:**

- Update SettingsStrings.ru-RU.resx [\#459](https://github.com/Belphemur/SoundSwitch/pull/459) ([wvxwxvw](https://github.com/wvxwxvw))

## [v5.3.0](https://github.com/Belphemur/SoundSwitch/tree/v5.3.0) (2020-05-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.2.0...v5.3.0)

**Implemented enhancements:**

- When setting profile using an application, let the user choose if the default audio device is changed too [\#458](https://github.com/Belphemur/SoundSwitch/issues/458)
- Better error message when launching second instance of SoundSwitch [\#456](https://github.com/Belphemur/SoundSwitch/issues/456)

**Fixed bugs:**

- SoundSwitch crashes on startup:  Can't get information about running process for Profile Feature [\#455](https://github.com/Belphemur/SoundSwitch/issues/455)

**Closed issues:**

- Crash after booting up PC: When using Profile feature and the audio device isn't connected [\#457](https://github.com/Belphemur/SoundSwitch/issues/457)

## [v5.2.0](https://github.com/Belphemur/SoundSwitch/tree/v5.2.0) (2020-05-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.1.1...v5.2.0)

**Implemented enhancements:**

- Move to use .NET Core [\#453](https://github.com/Belphemur/SoundSwitch/issues/453)

**Fixed bugs:**

- SoundSwitch doesn't remember when HotKey is disabled [\#451](https://github.com/Belphemur/SoundSwitch/issues/451)
- Recognize installation directory [\#449](https://github.com/Belphemur/SoundSwitch/issues/449)

**Merged pull requests:**

- Net core 3.1 [\#454](https://github.com/Belphemur/SoundSwitch/pull/454) ([Belphemur](https://github.com/Belphemur))

## [v5.1.1](https://github.com/Belphemur/SoundSwitch/tree/v5.1.1) (2020-05-17)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.1.0...v5.1.1)

**Fixed bugs:**

- Upgrade issues: Installer fails to start SW. Hotkey not functional after starting manually [\#447](https://github.com/Belphemur/SoundSwitch/issues/447)
- app crash new beta 5.1.0 [\#446](https://github.com/Belphemur/SoundSwitch/issues/446)

## [v5.1.0](https://github.com/Belphemur/SoundSwitch/tree/v5.1.0) (2020-05-16)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.4...v5.1.0)

**Implemented enhancements:**

- Make the Disconnected device list collapsible [\#445](https://github.com/Belphemur/SoundSwitch/issues/445)
- Add Dutch to the application [\#440](https://github.com/Belphemur/SoundSwitch/issues/440)

**Fixed bugs:**

- Recognize re-connected devices as the same as before [\#444](https://github.com/Belphemur/SoundSwitch/issues/444)
- Keybind not working, crashing when trying to edit keybind. [\#443](https://github.com/Belphemur/SoundSwitch/issues/443)

**Closed issues:**

- cra\<sh [\#438](https://github.com/Belphemur/SoundSwitch/issues/438)

## [v5.0.4](https://github.com/Belphemur/SoundSwitch/tree/v5.0.4) (2020-05-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.3...v5.0.4)

**Implemented enhancements:**

- When deleting a profile reset all changes made by profile to the Windows audio system [\#430](https://github.com/Belphemur/SoundSwitch/issues/430)

**Fixed bugs:**

- Need to close the log file before packing when app crashed [\#434](https://github.com/Belphemur/SoundSwitch/issues/434)
- NullReferenceException when the last playback device is disabled [\#432](https://github.com/Belphemur/SoundSwitch/issues/432)
- Hotkey doesn't register & hotkey bar display issue [\#429](https://github.com/Belphemur/SoundSwitch/issues/429)
- Crashes at startup when all playback devices are disabled [\#427](https://github.com/Belphemur/SoundSwitch/issues/427)
- Duplicate device created/previous removed when reconnected. [\#425](https://github.com/Belphemur/SoundSwitch/issues/425)
- Default Communication Device changing when it shouldn't. [\#418](https://github.com/Belphemur/SoundSwitch/issues/418)

**Merged pull requests:**

- Close the log file before packing to zip [\#435](https://github.com/Belphemur/SoundSwitch/pull/435) ([moomons](https://github.com/moomons))
- Fix NullReferenceException when the last playback device is disabled [\#433](https://github.com/Belphemur/SoundSwitch/pull/433) ([moomons](https://github.com/moomons))
- Fix crash at startup when all playback devices are disabled [\#428](https://github.com/Belphemur/SoundSwitch/pull/428) ([moomons](https://github.com/moomons))
- Reconnected devices are not added back to the selected list [\#426](https://github.com/Belphemur/SoundSwitch/pull/426) ([ys27](https://github.com/ys27))

## [v5.0.3](https://github.com/Belphemur/SoundSwitch/tree/v5.0.3) (2020-04-18)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.2...v5.0.3)

**Implemented enhancements:**

- Make the updater detect if installed as User or Admin [\#416](https://github.com/Belphemur/SoundSwitch/issues/416)

**Fixed bugs:**

- Can't use Windows Key as part of the HotKey [\#421](https://github.com/Belphemur/SoundSwitch/issues/421)

## [v5.0.2](https://github.com/Belphemur/SoundSwitch/tree/v5.0.2) (2020-04-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.1...v5.0.2)

**Fixed bugs:**

- Security Vulnerability in the Updater [\#415](https://github.com/Belphemur/SoundSwitch/issues/415)
- 5.0.1 crashing on first run [\#413](https://github.com/Belphemur/SoundSwitch/issues/413)
- My hot-key stopped working after the last patch [\#408](https://github.com/Belphemur/SoundSwitch/issues/408)
- Install / Upgrade issue [\#404](https://github.com/Belphemur/SoundSwitch/issues/404)
- SoundSwitchAggregateException / error message [\#397](https://github.com/Belphemur/SoundSwitch/issues/397)
- Auto Updater is Crashing on v4.15.\* [\#376](https://github.com/Belphemur/SoundSwitch/issues/376)

**Closed issues:**

- Bug - Cannot set hot keys [\#412](https://github.com/Belphemur/SoundSwitch/issues/412)
- Crash when no audio device available [\#411](https://github.com/Belphemur/SoundSwitch/issues/411)
- url [\#407](https://github.com/Belphemur/SoundSwitch/issues/407)
- Hotkey gets various characters in addition to the key I actually want [\#405](https://github.com/Belphemur/SoundSwitch/issues/405)
- Randomly switch on it own and back when viewing websites, desktop or doing anything  [\#403](https://github.com/Belphemur/SoundSwitch/issues/403)
- SoundSwitch\_v5.0.1.14646\_Release [\#402](https://github.com/Belphemur/SoundSwitch/issues/402)
- Forced mode [\#400](https://github.com/Belphemur/SoundSwitch/issues/400)

## [v5.0.1](https://github.com/Belphemur/SoundSwitch/tree/v5.0.1) (2020-01-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v5.0.0...v5.0.1)

**Fixed bugs:**

- Crash when switching with v5.0.0 [\#401](https://github.com/Belphemur/SoundSwitch/issues/401)

## [v5.0.0](https://github.com/Belphemur/SoundSwitch/tree/v5.0.0) (2020-01-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.17.1...v5.0.0)

**Implemented enhancements:**

- Support install through Chocolatey package manager [\#372](https://github.com/Belphemur/SoundSwitch/issues/372)

**Closed issues:**

- What folder should I backup? [\#399](https://github.com/Belphemur/SoundSwitch/issues/399)
- Error when installing just for me [\#398](https://github.com/Belphemur/SoundSwitch/issues/398)

## [v4.17.1](https://github.com/Belphemur/SoundSwitch/tree/v4.17.1) (2019-12-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.17.0...v4.17.1)

**Implemented enhancements:**

- Combined hotkey for In+Output device [\#110](https://github.com/Belphemur/SoundSwitch/issues/110)

**Fixed bugs:**

- Unable to create a profile without linked application [\#395](https://github.com/Belphemur/SoundSwitch/issues/395)

**Merged pull requests:**

- Improved English readme and synced with German [\#396](https://github.com/Belphemur/SoundSwitch/pull/396) ([FireEmerald](https://github.com/FireEmerald))

## [v4.17.0](https://github.com/Belphemur/SoundSwitch/tree/v4.17.0) (2019-12-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.3...v4.17.0)

**Implemented enhancements:**

- Add Korean language [\#391](https://github.com/Belphemur/SoundSwitch/issues/391)
- A hotkey to switch audio devices only for specific applications [\#257](https://github.com/Belphemur/SoundSwitch/issues/257)
- Add device profiles tab, and add new hotkey combo to switch between them [\#207](https://github.com/Belphemur/SoundSwitch/issues/207)

**Fixed bugs:**

- Unable to set custom sound notification [\#386](https://github.com/Belphemur/SoundSwitch/issues/386)

**Closed issues:**

- Latest version won't install [\#388](https://github.com/Belphemur/SoundSwitch/issues/388)

**Merged pull requests:**

- Feature profile [\#393](https://github.com/Belphemur/SoundSwitch/pull/393) ([Belphemur](https://github.com/Belphemur))

## [v4.16.3](https://github.com/Belphemur/SoundSwitch/tree/v4.16.3) (2019-12-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.2...v4.16.3)

**Closed issues:**

- Shortcut error [\#387](https://github.com/Belphemur/SoundSwitch/issues/387)
- Russian language [\#384](https://github.com/Belphemur/SoundSwitch/issues/384)
- Crash report [\#383](https://github.com/Belphemur/SoundSwitch/issues/383)
- Installer for 14.6.2.3656 fails [\#382](https://github.com/Belphemur/SoundSwitch/issues/382)

**Merged pull requests:**

- Update SettingsStrings.ru-RU.resx [\#385](https://github.com/Belphemur/SoundSwitch/pull/385) ([wvxwxvw](https://github.com/wvxwxvw))

## [v4.16.2](https://github.com/Belphemur/SoundSwitch/tree/v4.16.2) (2019-11-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.1...v4.16.2)

**Fixed bugs:**

- The option "Change Icon" is misleading and not self-explanatory [\#374](https://github.com/Belphemur/SoundSwitch/issues/374)

**Closed issues:**

- Access violation error when updating [\#375](https://github.com/Belphemur/SoundSwitch/issues/375)

**Merged pull requests:**

- Polish translation added [\#381](https://github.com/Belphemur/SoundSwitch/pull/381) ([ArquesMartin](https://github.com/ArquesMartin))
- Update AboutStrings.ru-RU.resx [\#380](https://github.com/Belphemur/SoundSwitch/pull/380) ([wvxwxvw](https://github.com/wvxwxvw))
- Update SettingsStrings.ru-RU.resx [\#379](https://github.com/Belphemur/SoundSwitch/pull/379) ([wvxwxvw](https://github.com/wvxwxvw))
- Update TrayIconStrings.ru-RU.resx [\#378](https://github.com/Belphemur/SoundSwitch/pull/378) ([wvxwxvw](https://github.com/wvxwxvw))
- Update UpdateDownloadStrings.ru-RU.resx [\#377](https://github.com/Belphemur/SoundSwitch/pull/377) ([wvxwxvw](https://github.com/wvxwxvw))

## [v4.16.1](https://github.com/Belphemur/SoundSwitch/tree/v4.16.1) (2019-11-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.16.0...v4.16.1)

**Implemented enhancements:**

- Possibility to link application to Audio Device [\#13](https://github.com/Belphemur/SoundSwitch/issues/13)

## [v4.16.0](https://github.com/Belphemur/SoundSwitch/tree/v4.16.0) (2019-11-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.15.1...v4.16.0)

**Implemented enhancements:**

- Icon can change when any device is switched [\#373](https://github.com/Belphemur/SoundSwitch/issues/373)
- User can choose how the SoundSwitch Icon changes [\#368](https://github.com/Belphemur/SoundSwitch/issues/368)
- Would you like a translated readme in German? [\#366](https://github.com/Belphemur/SoundSwitch/issues/366)

**Fixed bugs:**

- SoundSwitch crashes when switching to "Customized Sound Notification" [\#365](https://github.com/Belphemur/SoundSwitch/issues/365)
- Possible Windows/Nvidia update causing persistent 4.10 and 4.15 issues. [\#363](https://github.com/Belphemur/SoundSwitch/issues/363)
- Speakers registering as disconnected in SoundSwitch since update 4.15.0.40592 [\#361](https://github.com/Belphemur/SoundSwitch/issues/361)
- Working device showing as "Disconnected" after latest update [\#360](https://github.com/Belphemur/SoundSwitch/issues/360)
- Shortcut Not Working [\#359](https://github.com/Belphemur/SoundSwitch/issues/359)
- Missing Audio Device [\#358](https://github.com/Belphemur/SoundSwitch/issues/358)
- Missing second playback device with same name [\#338](https://github.com/Belphemur/SoundSwitch/issues/338)

**Closed issues:**

- Certificates are missing in the repository [\#367](https://github.com/Belphemur/SoundSwitch/issues/367)
- Cannot Switch after the latest update [\#364](https://github.com/Belphemur/SoundSwitch/issues/364)
- Bluetooth Speakers Not Shown [\#362](https://github.com/Belphemur/SoundSwitch/issues/362)
- Switching Audio Devices Frequently Leads To Audio Output From Multiple Audio Devices [\#356](https://github.com/Belphemur/SoundSwitch/issues/356)
- Request: custom volume per device [\#333](https://github.com/Belphemur/SoundSwitch/issues/333)
- Feature Request: Delay Launch for SoundSwitch [\#332](https://github.com/Belphemur/SoundSwitch/issues/332)

**Merged pull requests:**

- Fresh new look for README [\#371](https://github.com/Belphemur/SoundSwitch/pull/371) ([FireEmerald](https://github.com/FireEmerald))
- Added german README, based on translations from Overload86 [\#369](https://github.com/Belphemur/SoundSwitch/pull/369) ([FireEmerald](https://github.com/FireEmerald))

## [v4.15.1](https://github.com/Belphemur/SoundSwitch/tree/v4.15.1) (2019-11-13)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.15.0...v4.15.1)

**Fixed bugs:**

- Cycle throu selected  not working [\#357](https://github.com/Belphemur/SoundSwitch/issues/357)

## [v4.15.0](https://github.com/Belphemur/SoundSwitch/tree/v4.15.0) (2019-11-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.14.0...v4.15.0)

**Implemented enhancements:**

- The installer dumps a setup log file next to itself after normal installation [\#337](https://github.com/Belphemur/SoundSwitch/issues/337)
- Don't launch the application after a silent install [\#336](https://github.com/Belphemur/SoundSwitch/issues/336)

**Fixed bugs:**

- Incorrect tray icon displayed [\#340](https://github.com/Belphemur/SoundSwitch/issues/340)

**Closed issues:**

- Sound Switch is not opening after the newest windows update \(1903\) [\#355](https://github.com/Belphemur/SoundSwitch/issues/355)
- Soundswitch shortcut doesn't work anymore [\#354](https://github.com/Belphemur/SoundSwitch/issues/354)
- Switch audio outputs when SoundSwitch is run while already running. [\#353](https://github.com/Belphemur/SoundSwitch/issues/353)
- \[Feature request\] option to hide tray icon [\#352](https://github.com/Belphemur/SoundSwitch/issues/352)
- This program messed up my sound settings after uninstalling [\#350](https://github.com/Belphemur/SoundSwitch/issues/350)
- Unable to download installer - SSL Issues [\#349](https://github.com/Belphemur/SoundSwitch/issues/349)
- Hotkey not working [\#347](https://github.com/Belphemur/SoundSwitch/issues/347)
- \[Feature Request\] Switch output by a shortcut without SoundSwitch running in tray [\#346](https://github.com/Belphemur/SoundSwitch/issues/346)
- Icon in start menu is small when medium icon size is selected [\#345](https://github.com/Belphemur/SoundSwitch/issues/345)
- Not working in Chrome [\#344](https://github.com/Belphemur/SoundSwitch/issues/344)
- Default audio is switching but my opened programs do not. [\#343](https://github.com/Belphemur/SoundSwitch/issues/343)
- Audio out of only one source [\#342](https://github.com/Belphemur/SoundSwitch/issues/342)
- Microphone level is reset to 0 after switching [\#339](https://github.com/Belphemur/SoundSwitch/issues/339)
- Not switching audio devices without restarting program with bootup on start enabled [\#335](https://github.com/Belphemur/SoundSwitch/issues/335)

## [v4.14.0](https://github.com/Belphemur/SoundSwitch/tree/v4.14.0) (2019-04-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.13.0...v4.14.0)

**Implemented enhancements:**

- Switch the foreground app also \[WIN10\] [\#334](https://github.com/Belphemur/SoundSwitch/issues/334)

**Closed issues:**

- \[Bug\] No Logitech G533 after updating soundswitch to 4.13 [\#331](https://github.com/Belphemur/SoundSwitch/issues/331)

## [v4.13.0](https://github.com/Belphemur/SoundSwitch/tree/v4.13.0) (2019-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.12...v4.13.0)

**Fixed bugs:**

- High CPU came back [\#330](https://github.com/Belphemur/SoundSwitch/issues/330)
- Soundswitch crash at start when no internet [\#326](https://github.com/Belphemur/SoundSwitch/issues/326)
- Hotkeys do not work even after 4.12 [\#324](https://github.com/Belphemur/SoundSwitch/issues/324)

**Closed issues:**

- Link Playback and Recording devices together [\#328](https://github.com/Belphemur/SoundSwitch/issues/328)
- Installer crashing [\#327](https://github.com/Belphemur/SoundSwitch/issues/327)
- \[Feature\] Switch device on single click on tray icon [\#325](https://github.com/Belphemur/SoundSwitch/issues/325)
- Characters in french are not displayed correctly in the installer [\#313](https://github.com/Belphemur/SoundSwitch/issues/313)
- SoundSwitch not launching on Windows 8.1 [\#136](https://github.com/Belphemur/SoundSwitch/issues/136)
- Problem with VC Redist and Win 10 anniversary update [\#114](https://github.com/Belphemur/SoundSwitch/issues/114)

## [v4.12](https://github.com/Belphemur/SoundSwitch/tree/v4.12) (2019-02-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.11...v4.12)

**Fixed bugs:**

- SoundSwitch does not refresh itself when connecting new device to computer [\#323](https://github.com/Belphemur/SoundSwitch/issues/323)
- Toast Notification not working [\#321](https://github.com/Belphemur/SoundSwitch/issues/321)

**Closed issues:**

- Not switching to HDMI Audio Device [\#322](https://github.com/Belphemur/SoundSwitch/issues/322)
- Failure to install Visual C++ Redistributable during first installation. [\#320](https://github.com/Belphemur/SoundSwitch/issues/320)
- Breaks Spotify [\#318](https://github.com/Belphemur/SoundSwitch/issues/318)
- Soundswitch does not open itself at boot. [\#317](https://github.com/Belphemur/SoundSwitch/issues/317)
- Visual C++ Redistributable for Visual Studio 2017 issues with  other program [\#315](https://github.com/Belphemur/SoundSwitch/issues/315)
- 4.11.69 Bug with hotkeys [\#314](https://github.com/Belphemur/SoundSwitch/issues/314)

## [v4.11](https://github.com/Belphemur/SoundSwitch/tree/v4.11) (2018-12-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.10...v4.11)

**Implemented enhancements:**

- Add Chinese language [\#311](https://github.com/Belphemur/SoundSwitch/issues/311)

**Fixed bugs:**

- High CPU usage with 4.10.6899.14921 when coming back from hibernation/sleep [\#309](https://github.com/Belphemur/SoundSwitch/issues/309)

## [v4.10](https://github.com/Belphemur/SoundSwitch/tree/v4.10) (2018-11-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.9...v4.10)

**Fixed bugs:**

- Lag opening system tray context menu [\#298](https://github.com/Belphemur/SoundSwitch/issues/298)
- 4.9 Update will not finish [\#294](https://github.com/Belphemur/SoundSwitch/issues/294)
- Crashing, unstableness and messed up dialogues. [\#293](https://github.com/Belphemur/SoundSwitch/issues/293)
- SoundSwitch crashing ... again [\#292](https://github.com/Belphemur/SoundSwitch/issues/292)

**Closed issues:**

- Default Communications Device [\#308](https://github.com/Belphemur/SoundSwitch/issues/308)
- I can not install or update SoundSwitch. [\#307](https://github.com/Belphemur/SoundSwitch/issues/307)
- no way to set a shotcut [\#305](https://github.com/Belphemur/SoundSwitch/issues/305)
- Crashing at launch [\#303](https://github.com/Belphemur/SoundSwitch/issues/303)
- Odd issue with soundswitch [\#302](https://github.com/Belphemur/SoundSwitch/issues/302)
- Enhancement - Leftclick to switch devices [\#301](https://github.com/Belphemur/SoundSwitch/issues/301)
- Soundswitch App Wont Open  [\#296](https://github.com/Belphemur/SoundSwitch/issues/296)
- Feat.Req.: create and switch-to "profiles" \(e.g. Docked-Softphone, Docked-VideoConf\) [\#295](https://github.com/Belphemur/SoundSwitch/issues/295)
- Soundswitch won't appear on the screen even thought it is running in the task manager [\#243](https://github.com/Belphemur/SoundSwitch/issues/243)
- SoundSwitch is crashing frequently [\#232](https://github.com/Belphemur/SoundSwitch/issues/232)

**Merged pull requests:**

- Add license scan report and status [\#300](https://github.com/Belphemur/SoundSwitch/pull/300) ([fossabot](https://github.com/fossabot))

## [v4.9](https://github.com/Belphemur/SoundSwitch/tree/v4.9) (2018-06-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.8...v4.9)

**Implemented enhancements:**

- Modern Logo for Soundswitch [\#277](https://github.com/Belphemur/SoundSwitch/issues/277)

**Fixed bugs:**

- New logo mostly invisible on white background [\#287](https://github.com/Belphemur/SoundSwitch/issues/287)
- Switching between Audio sources delayed - v4.8 [\#286](https://github.com/Belphemur/SoundSwitch/issues/286)
- SoundSwitch not starting after installation finished [\#276](https://github.com/Belphemur/SoundSwitch/issues/276)

**Closed issues:**

- Much slower switching ever since 4.8 [\#291](https://github.com/Belphemur/SoundSwitch/issues/291)
- Interface slow to respond to click to open [\#288](https://github.com/Belphemur/SoundSwitch/issues/288)

**Merged pull requests:**

- Fix performance [\#290](https://github.com/Belphemur/SoundSwitch/pull/290) ([Belphemur](https://github.com/Belphemur))
- Fix Icon [\#289](https://github.com/Belphemur/SoundSwitch/pull/289) ([Belphemur](https://github.com/Belphemur))
- Bump version [\#285](https://github.com/Belphemur/SoundSwitch/pull/285) ([Belphemur](https://github.com/Belphemur))

## [v4.8](https://github.com/Belphemur/SoundSwitch/tree/v4.8) (2018-06-06)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.7...v4.8)

**Implemented enhancements:**

- Fallback on Name when Id not matching [\#283](https://github.com/Belphemur/SoundSwitch/issues/283)

**Closed issues:**

- Remove duplicate trayicon file [\#281](https://github.com/Belphemur/SoundSwitch/issues/281)
- How to test my forked Soundswitch-dev installer? [\#273](https://github.com/Belphemur/SoundSwitch/issues/273)
- Crash on boot after motherboard change [\#251](https://github.com/Belphemur/SoundSwitch/issues/251)

**Merged pull requests:**

- Name fallback [\#284](https://github.com/Belphemur/SoundSwitch/pull/284) ([Belphemur](https://github.com/Belphemur))
- Cleaned and updated Makefiles [\#280](https://github.com/Belphemur/SoundSwitch/pull/280) ([FireEmerald](https://github.com/FireEmerald))
- Added logo and ico [\#278](https://github.com/Belphemur/SoundSwitch/pull/278) ([linadesteem](https://github.com/linadesteem))

## [v4.7](https://github.com/Belphemur/SoundSwitch/tree/v4.7) (2018-05-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.6...v4.7)

**Fixed bugs:**

- Crashing when mouse over tray icon [\#272](https://github.com/Belphemur/SoundSwitch/issues/272)
- Readme missing screenshot [\#271](https://github.com/Belphemur/SoundSwitch/issues/271)

**Closed issues:**

- Sound Switch keeps closing [\#270](https://github.com/Belphemur/SoundSwitch/issues/270)
- SoundSwitch isn't working with chrome sometimes [\#269](https://github.com/Belphemur/SoundSwitch/issues/269)

## [v4.6](https://github.com/Belphemur/SoundSwitch/tree/v4.6) (2018-05-13)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.5...v4.6)

**Fixed bugs:**

- App randomly crashes, every 2 hours or so, with latest 4.5 update.  [\#268](https://github.com/Belphemur/SoundSwitch/issues/268)
- Double Click to switch output device crashes app [\#267](https://github.com/Belphemur/SoundSwitch/issues/267)

**Closed issues:**

- With default communications checked, it would not switch properly [\#256](https://github.com/Belphemur/SoundSwitch/issues/256)

**Merged pull requests:**

- Update SettingsStrings.pt-BR.resx [\#266](https://github.com/Belphemur/SoundSwitch/pull/266) ([opus-2](https://github.com/opus-2))
- Update italian.iss [\#265](https://github.com/Belphemur/SoundSwitch/pull/265) ([bovirus](https://github.com/bovirus))
- Several updates [\#264](https://github.com/Belphemur/SoundSwitch/pull/264) ([bovirus](https://github.com/bovirus))
- Update Italian strings [\#263](https://github.com/Belphemur/SoundSwitch/pull/263) ([bovirus](https://github.com/bovirus))

## [v4.5](https://github.com/Belphemur/SoundSwitch/tree/v4.5) (2018-05-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.4...v4.5)

**Implemented enhancements:**

- Add option to disable selected sound on device switch [\#261](https://github.com/Belphemur/SoundSwitch/issues/261)

**Merged pull requests:**

- Add Portuguese\(Brazilian\) to the installer. [\#275](https://github.com/Belphemur/SoundSwitch/pull/275) ([aleczk](https://github.com/aleczk))
- Maybe fix memory leak caused by not disposing Icons and Bitmaps. [\#262](https://github.com/Belphemur/SoundSwitch/pull/262) ([Niko-O](https://github.com/Niko-O))
- Update ISS file \(add Italian language\) [\#260](https://github.com/Belphemur/SoundSwitch/pull/260) ([bovirus](https://github.com/bovirus))

## [v4.4](https://github.com/Belphemur/SoundSwitch/tree/v4.4) (2018-05-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.3...v4.4)

**Implemented enhancements:**

- New Language: Portuguese \(Brazilian\) - Pull Request [\#258](https://github.com/Belphemur/SoundSwitch/issues/258)
- Improve logging [\#254](https://github.com/Belphemur/SoundSwitch/issues/254)

**Fixed bugs:**

- Crash observed after restart driven by Windows Update [\#248](https://github.com/Belphemur/SoundSwitch/issues/248)
- In x86, the installer always force to install VC++ Redist [\#245](https://github.com/Belphemur/SoundSwitch/issues/245)

**Closed issues:**

- Identify Re-Connected Displays /w Audio [\#250](https://github.com/Belphemur/SoundSwitch/issues/250)

**Merged pull requests:**

- Improve logging [\#259](https://github.com/Belphemur/SoundSwitch/pull/259) ([Belphemur](https://github.com/Belphemur))
- Norwegian language [\#255](https://github.com/Belphemur/SoundSwitch/pull/255) ([Belphemur](https://github.com/Belphemur))
- Added Spanish language in installer [\#246](https://github.com/Belphemur/SoundSwitch/pull/246) ([plextoriano](https://github.com/plextoriano))

## [v4.3](https://github.com/Belphemur/SoundSwitch/tree/v4.3) (2018-03-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.2...v4.3)

**Closed issues:**

- Unable to set shortcut [\#241](https://github.com/Belphemur/SoundSwitch/issues/241)
- Different Hotkey per device? [\#238](https://github.com/Belphemur/SoundSwitch/issues/238)
- Switch Device by system time [\#235](https://github.com/Belphemur/SoundSwitch/issues/235)

**Merged pull requests:**

- Spanish language added [\#244](https://github.com/Belphemur/SoundSwitch/pull/244) ([plextoriano](https://github.com/plextoriano))

## [v4.2](https://github.com/Belphemur/SoundSwitch/tree/v4.2) (2017-12-03)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.1...v4.2)

**Implemented enhancements:**

- Rewrite of the core of SoundSwitch [\#224](https://github.com/Belphemur/SoundSwitch/issues/224)

**Fixed bugs:**

- Race condition of the WindowsAPIThread leading to crash on start. [\#229](https://github.com/Belphemur/SoundSwitch/issues/229)

## [v4.1](https://github.com/Belphemur/SoundSwitch/tree/v4.1) (2017-11-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v4.0...v4.1)

**Implemented enhancements:**

- Add choice in uninstaller to remove application configuration [\#216](https://github.com/Belphemur/SoundSwitch/issues/216)
- Possible to make a portable install? [\#208](https://github.com/Belphemur/SoundSwitch/issues/208)

**Fixed bugs:**

- Settings crash when device don't have a friendly name [\#225](https://github.com/Belphemur/SoundSwitch/issues/225)
- Locking up on switch since Fall Creator's Update [\#219](https://github.com/Belphemur/SoundSwitch/issues/219)
- Application crashes when activating a bluetooth device - Win7 [\#217](https://github.com/Belphemur/SoundSwitch/issues/217)
- Soundswitch crash on startup [\#214](https://github.com/Belphemur/SoundSwitch/issues/214)
- SoundSwitch Freezing randomly. [\#200](https://github.com/Belphemur/SoundSwitch/issues/200)
- New-Old Issue - Switching to Bluetooth Audio [\#166](https://github.com/Belphemur/SoundSwitch/issues/166)

**Merged pull requests:**

- Rewrite [\#226](https://github.com/Belphemur/SoundSwitch/pull/226) ([Belphemur](https://github.com/Belphemur))

## [v4.0](https://github.com/Belphemur/SoundSwitch/tree/v4.0) (2017-11-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.2...v4.0)

## [v3.15.2](https://github.com/Belphemur/SoundSwitch/tree/v3.15.2) (2017-11-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.1...v3.15.2)

**Implemented enhancements:**

- Add ability to link output/recording devices [\#206](https://github.com/Belphemur/SoundSwitch/issues/206)

**Fixed bugs:**

- App Crash with MP3 file used for Banner Notification \(works fine in Custom Sound Notification\) [\#218](https://github.com/Belphemur/SoundSwitch/issues/218)
- Invalid sound file make SoundSwitch crash [\#210](https://github.com/Belphemur/SoundSwitch/issues/210)

**Closed issues:**

- SoundSwitch won't launch [\#205](https://github.com/Belphemur/SoundSwitch/issues/205)
- Crashes when remote connecting to machine using RDP [\#197](https://github.com/Belphemur/SoundSwitch/issues/197)

## [v3.15.1](https://github.com/Belphemur/SoundSwitch/tree/v3.15.1) (2017-07-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.15.0...v3.15.1)

**Fixed bugs:**

- Can't install vcredist with the built in installer provided by soundswitch [\#203](https://github.com/Belphemur/SoundSwitch/issues/203)
- SoundSwitch in Homeoffice/Office [\#199](https://github.com/Belphemur/SoundSwitch/issues/199)
- Updater Progress bar crash with Classic Visual Theme [\#194](https://github.com/Belphemur/SoundSwitch/issues/194)

**Closed issues:**

- Crash when not selecting a file for the custom sound notification option [\#202](https://github.com/Belphemur/SoundSwitch/issues/202)
- High CPU usage because of "Windows Audio Device Graph Isolation" process [\#198](https://github.com/Belphemur/SoundSwitch/issues/198)
- Error popup when trying to install latest update [\#196](https://github.com/Belphemur/SoundSwitch/issues/196)
- Branding version for GTribe [\#148](https://github.com/Belphemur/SoundSwitch/issues/148)

**Merged pull requests:**

- Add VC Redist 2017 as dependency [\#204](https://github.com/Belphemur/SoundSwitch/pull/204) ([Belphemur](https://github.com/Belphemur))
- Fixed a crash which happened if the user disabled visual styles e.g. with the 'Windows Classic' theme of Windows 7. [\#195](https://github.com/Belphemur/SoundSwitch/pull/195) ([FireEmerald](https://github.com/FireEmerald))

## [v3.15.0](https://github.com/Belphemur/SoundSwitch/tree/v3.15.0) (2017-05-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.2...v3.15.0)

**Implemented enhancements:**

- Make the banner notification use the image of the Device [\#192](https://github.com/Belphemur/SoundSwitch/issues/192)
- Unable to bind ScrollLock as a hotkey [\#151](https://github.com/Belphemur/SoundSwitch/issues/151)

**Fixed bugs:**

- Installer don't detect .net 4.7 [\#193](https://github.com/Belphemur/SoundSwitch/issues/193)
- Soundswitch crashes on RDP connect [\#187](https://github.com/Belphemur/SoundSwitch/issues/187)

**Closed issues:**

- Recording device hotkey doesn't cycle [\#191](https://github.com/Belphemur/SoundSwitch/issues/191)

**Merged pull requests:**

- Translations [\#190](https://github.com/Belphemur/SoundSwitch/pull/190) ([Belphemur](https://github.com/Belphemur))

## [v3.14.2](https://github.com/Belphemur/SoundSwitch/tree/v3.14.2) (2017-05-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.1...v3.14.2)

**Fixed bugs:**

- Unresponsive after RDC [\#112](https://github.com/Belphemur/SoundSwitch/issues/112)

**Closed issues:**

- Error after Creator's Update [\#185](https://github.com/Belphemur/SoundSwitch/issues/185)
- Kaspersky Internet Security blocks download and execution of latest versoin [\#181](https://github.com/Belphemur/SoundSwitch/issues/181)
- Kaspersky Internet Security detects Virus and crashes SoundSwitch while updating \(false-positive\) [\#180](https://github.com/Belphemur/SoundSwitch/issues/180)

**Merged pull requests:**

- Add thanks category [\#188](https://github.com/Belphemur/SoundSwitch/pull/188) ([Belphemur](https://github.com/Belphemur))
- Some improvements and a feature [\#186](https://github.com/Belphemur/SoundSwitch/pull/186) ([ramon18](https://github.com/ramon18))

## [v3.14.1](https://github.com/Belphemur/SoundSwitch/tree/v3.14.1) (2017-04-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.14.0...v3.14.1)

**Implemented enhancements:**

- Update German [\#189](https://github.com/Belphemur/SoundSwitch/issues/189)
- Update AudioEndPointLibrary [\#178](https://github.com/Belphemur/SoundSwitch/issues/178)

**Fixed bugs:**

- SoundSwitch Crash when Custom Notification is set and no Sound is chosen: System.FormatException Fatal Error [\#175](https://github.com/Belphemur/SoundSwitch/issues/175)

**Closed issues:**

- Program keeps crashing if notification setting is set to "Customized Sound Notification" and no sound file is chosen.  [\#177](https://github.com/Belphemur/SoundSwitch/issues/177)
- SoundSwitch doesn't start - Windows 8.1 [\#176](https://github.com/Belphemur/SoundSwitch/issues/176)

## [v3.14.0](https://github.com/Belphemur/SoundSwitch/tree/v3.14.0) (2017-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.2...v3.14.0)

**Implemented enhancements:**

- Improve the notifcation of new update [\#168](https://github.com/Belphemur/SoundSwitch/issues/168)
- Corrected anchors, added high-dpi support. [\#169](https://github.com/Belphemur/SoundSwitch/pull/169) ([FireEmerald](https://github.com/FireEmerald))

**Fixed bugs:**

- Notification BallonTip not showing [\#173](https://github.com/Belphemur/SoundSwitch/issues/173)
- Settings dialog resize issues [\#164](https://github.com/Belphemur/SoundSwitch/issues/164)
- Crash on Launch, Win7 x64 [\#161](https://github.com/Belphemur/SoundSwitch/issues/161)

## [v3.13.2](https://github.com/Belphemur/SoundSwitch/tree/v3.13.2) (2017-03-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.1...v3.13.2)

**Implemented enhancements:**

- Rename the Installer EXE when autoupdate to something more meaningful [\#167](https://github.com/Belphemur/SoundSwitch/issues/167)

**Fixed bugs:**

- Problem with hotkey since new version [\#171](https://github.com/Belphemur/SoundSwitch/issues/171)
- Hotkey issue [\#170](https://github.com/Belphemur/SoundSwitch/issues/170)

**Closed issues:**

- Crashing every time [\#165](https://github.com/Belphemur/SoundSwitch/issues/165)

## [v3.13.1](https://github.com/Belphemur/SoundSwitch/tree/v3.13.1) (2017-03-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.13.0...v3.13.1)

**Implemented enhancements:**

- Translate missing strings in German [\#159](https://github.com/Belphemur/SoundSwitch/issues/159)

**Merged pull requests:**

- Assigned values to DeviceCyclerTypeEnum and NotificationTypeEnum entries. [\#162](https://github.com/Belphemur/SoundSwitch/pull/162) ([FireEmerald](https://github.com/FireEmerald))
- Translated some strings into German introduced in 4dedc42. [\#160](https://github.com/Belphemur/SoundSwitch/pull/160) ([FireEmerald](https://github.com/FireEmerald))

## [v3.13.0](https://github.com/Belphemur/SoundSwitch/tree/v3.13.0) (2017-03-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.8...v3.13.0)

**Implemented enhancements:**

- Improve french translation [\#158](https://github.com/Belphemur/SoundSwitch/issues/158)
- Localization improvements [\#156](https://github.com/Belphemur/SoundSwitch/issues/156)
- Ability to disable auto update checker [\#102](https://github.com/Belphemur/SoundSwitch/issues/102)

**Fixed bugs:**

- Unable to launch on Windows 7 32bit: Universal C Runtime missing [\#155](https://github.com/Belphemur/SoundSwitch/issues/155)
- Tray icon doesn't update until switched  [\#154](https://github.com/Belphemur/SoundSwitch/issues/154)

**Closed issues:**

- Resolve AppVeyor problems [\#153](https://github.com/Belphemur/SoundSwitch/issues/153)

**Merged pull requests:**

- Improved the multi-language support, this includes: [\#157](https://github.com/Belphemur/SoundSwitch/pull/157) ([FireEmerald](https://github.com/FireEmerald))
- Improved the update system to support three different modes [\#152](https://github.com/Belphemur/SoundSwitch/pull/152) ([FireEmerald](https://github.com/FireEmerald))

## [v3.12.8](https://github.com/Belphemur/SoundSwitch/tree/v3.12.8) (2017-02-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.7...v3.12.8)

**Implemented enhancements:**

- Use radio button for the Auto-Update [\#150](https://github.com/Belphemur/SoundSwitch/issues/150)

## [v3.12.7](https://github.com/Belphemur/SoundSwitch/tree/v3.12.7) (2017-02-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.6...v3.12.7)

**Implemented enhancements:**

- Check the update is signed with a trusted key [\#147](https://github.com/Belphemur/SoundSwitch/issues/147)
- Make the installer install the certificate used by SoundSwitch [\#145](https://github.com/Belphemur/SoundSwitch/issues/145)
- When stealth update is activated, don't auto-update if there is an app in fullscreen [\#144](https://github.com/Belphemur/SoundSwitch/issues/144)

**Fixed bugs:**

- Updater crash with Full updater and progress bar [\#83](https://github.com/Belphemur/SoundSwitch/issues/83)

## [v3.12.6](https://github.com/Belphemur/SoundSwitch/tree/v3.12.6) (2017-02-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.5...v3.12.6)

**Fixed bugs:**

- Crashed when Bluetooth headphones conneted [\#140](https://github.com/Belphemur/SoundSwitch/issues/140)

**Closed issues:**

- Add donation url in the installer [\#142](https://github.com/Belphemur/SoundSwitch/issues/142)
- Similar to \#140 - Crashes when CONNECTING Bluetooth Device/Headphones [\#141](https://github.com/Belphemur/SoundSwitch/issues/141)

## [v3.12.5](https://github.com/Belphemur/SoundSwitch/tree/v3.12.5) (2017-01-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.4...v3.12.5)

**Fixed bugs:**

- Reinit the systray icon on Keep Systray setting change [\#135](https://github.com/Belphemur/SoundSwitch/issues/135)

**Merged pull requests:**

- TrayIcon is correctly selected after startup [\#134](https://github.com/Belphemur/SoundSwitch/pull/134) ([patgrosse](https://github.com/patgrosse))

## [v3.12.4](https://github.com/Belphemur/SoundSwitch/tree/v3.12.4) (2016-12-09)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.3...v3.12.4)

**Implemented enhancements:**

- When on Beta channel, also install releases [\#132](https://github.com/Belphemur/SoundSwitch/issues/132)

**Fixed bugs:**

- Audio device's selection menu on the systray stopped updating/working [\#131](https://github.com/Belphemur/SoundSwitch/issues/131)

## [v3.12.3](https://github.com/Belphemur/SoundSwitch/tree/v3.12.3) (2016-12-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.2...v3.12.3)

**Implemented enhancements:**

- Setting to keep the normal SoundSwitch TrayIcon [\#128](https://github.com/Belphemur/SoundSwitch/issues/128)

**Closed issues:**

- Hotkey to switch playback no longer works [\#130](https://github.com/Belphemur/SoundSwitch/issues/130)

## [v3.12.2](https://github.com/Belphemur/SoundSwitch/tree/v3.12.2) (2016-12-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.1...v3.12.2)

**Implemented enhancements:**

- Add a donate button [\#127](https://github.com/Belphemur/SoundSwitch/issues/127)

**Fixed bugs:**

- Installer doesn't check for the right version of VC Redist 2015 [\#126](https://github.com/Belphemur/SoundSwitch/issues/126)
- Device Icon used in Settings is the small one [\#125](https://github.com/Belphemur/SoundSwitch/issues/125)
- App crashes on launch [\#124](https://github.com/Belphemur/SoundSwitch/issues/124)

## [v3.12.1](https://github.com/Belphemur/SoundSwitch/tree/v3.12.1) (2016-11-25)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.12.0...v3.12.1)

**Implemented enhancements:**

- Device-dependent system tray icon [\#122](https://github.com/Belphemur/SoundSwitch/issues/122)

**Fixed bugs:**

- SoundSwitch crash when stopping [\#123](https://github.com/Belphemur/SoundSwitch/issues/123)

## [v3.12.0](https://github.com/Belphemur/SoundSwitch/tree/v3.12.0) (2016-09-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.11.0...v3.12.0)

**Implemented enhancements:**

- Update to .NET 4.6.2 [\#118](https://github.com/Belphemur/SoundSwitch/issues/118)
- Accessing 'help' at any time [\#105](https://github.com/Belphemur/SoundSwitch/issues/105)
- Request: Notification pop up with custom sound? [\#73](https://github.com/Belphemur/SoundSwitch/issues/73)

**Fixed bugs:**

- Don't switch to already default device [\#117](https://github.com/Belphemur/SoundSwitch/issues/117)

**Closed issues:**

- beta 3.11.0.32914 [\#115](https://github.com/Belphemur/SoundSwitch/issues/115)

## [v3.11.0](https://github.com/Belphemur/SoundSwitch/tree/v3.11.0) (2016-08-21)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.2...v3.11.0)

**Implemented enhancements:**

- Request: WinKey as modifier [\#109](https://github.com/Belphemur/SoundSwitch/issues/109)

**Fixed bugs:**

- Not switching "default communications device" [\#106](https://github.com/Belphemur/SoundSwitch/issues/106)

**Closed issues:**

- Windows 10: Shorter notification duration when 'browsing' with hotkey [\#108](https://github.com/Belphemur/SoundSwitch/issues/108)
- Nevermind [\#104](https://github.com/Belphemur/SoundSwitch/issues/104)

## [v3.10.2](https://github.com/Belphemur/SoundSwitch/tree/v3.10.2) (2016-06-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.1...v3.10.2)

**Fixed bugs:**

- C++ 2015 x64 redist installed but installer tries and fails to download and install it again [\#101](https://github.com/Belphemur/SoundSwitch/issues/101)

## [v3.10.1](https://github.com/Belphemur/SoundSwitch/tree/v3.10.1) (2016-05-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.10.0...v3.10.1)

**Implemented enhancements:**

- Support HTTPS download in the installer [\#99](https://github.com/Belphemur/SoundSwitch/issues/99)

**Fixed bugs:**

- SoundSwitch install not working, VCRedist detect and download problem v3.10 Stable [\#98](https://github.com/Belphemur/SoundSwitch/issues/98)

## [v3.10.0](https://github.com/Belphemur/SoundSwitch/tree/v3.10.0) (2016-05-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.9...v3.10.0)

**Implemented enhancements:**

- High Resolution Icons [\#94](https://github.com/Belphemur/SoundSwitch/issues/94)

## [v3.9.9](https://github.com/Belphemur/SoundSwitch/tree/v3.9.9) (2016-05-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.8...v3.9.9)

**Implemented enhancements:**

- Use the Tooltip of the Systray instead of a Baloontip to display active device [\#90](https://github.com/Belphemur/SoundSwitch/issues/90)

**Fixed bugs:**

- When launching SoundSwitch, it always set as default the already default device [\#93](https://github.com/Belphemur/SoundSwitch/issues/93)
- Missing Beta mode in Settings [\#92](https://github.com/Belphemur/SoundSwitch/issues/92)
- Infinite notification popup [\#91](https://github.com/Belphemur/SoundSwitch/issues/91)

## [v3.9.8](https://github.com/Belphemur/SoundSwitch/tree/v3.9.8) (2016-04-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.7...v3.9.8)

**Implemented enhancements:**

- Show active device on hover Systray Icon [\#88](https://github.com/Belphemur/SoundSwitch/issues/88)
- Playback devices constantly go in the Disconnected section [\#87](https://github.com/Belphemur/SoundSwitch/issues/87)

## [v3.9.7](https://github.com/Belphemur/SoundSwitch/tree/v3.9.7) (2016-04-15)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.6...v3.9.7)

**Implemented enhancements:**

- Request: Cycle through connected devices by double clicking on the system tray icon [\#80](https://github.com/Belphemur/SoundSwitch/issues/80)
- Generate HTML for Changelog file and add it to the installer [\#79](https://github.com/Belphemur/SoundSwitch/issues/79)
- Providing 'help' for first time users at the time of installation [\#78](https://github.com/Belphemur/SoundSwitch/issues/78)

**Fixed bugs:**

- SoundSwitch 3.9.6 Cannot install visual c++ redist [\#84](https://github.com/Belphemur/SoundSwitch/issues/84)
- Pipe system broken: New instance of SoundSwitch doesn't close the previous one [\#81](https://github.com/Belphemur/SoundSwitch/issues/81)
- Switching already running applications [\#70](https://github.com/Belphemur/SoundSwitch/issues/70)

**Closed issues:**

- SoundSwitch crashes when logging in to a second user in Win10 [\#82](https://github.com/Belphemur/SoundSwitch/issues/82)
- Per-application switch [\#77](https://github.com/Belphemur/SoundSwitch/issues/77)
- Unable to installl dependencies, but Sound Switch would stil install. Crash at start [\#74](https://github.com/Belphemur/SoundSwitch/issues/74)

## [v3.9.6](https://github.com/Belphemur/SoundSwitch/tree/v3.9.6) (2016-03-10)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.5...v3.9.6)

**Implemented enhancements:**

- Add a "stealth" component to the Auto-Update [\#64](https://github.com/Belphemur/SoundSwitch/issues/64)

**Fixed bugs:**

- AutoUpdate doesn't restart correctly SoundSwitch [\#76](https://github.com/Belphemur/SoundSwitch/issues/76)
- Installer: Don't install if not Windows 7 SP1 or newer [\#75](https://github.com/Belphemur/SoundSwitch/issues/75)
- At windows startup, hotkeys don't work [\#72](https://github.com/Belphemur/SoundSwitch/issues/72)

## [v3.9.5](https://github.com/Belphemur/SoundSwitch/tree/v3.9.5) (2016-02-28)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.4...v3.9.5)

## [v3.9.4](https://github.com/Belphemur/SoundSwitch/tree/v3.9.4) (2016-02-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.2...v3.9.4)

**Fixed bugs:**

- Empty Icon defined for an AudioDevice [\#71](https://github.com/Belphemur/SoundSwitch/issues/71)

**Closed issues:**

- Suggestion: Invoke command line to directly choose a source [\#69](https://github.com/Belphemur/SoundSwitch/issues/69)
- series of popups every restart [\#68](https://github.com/Belphemur/SoundSwitch/issues/68)

## [v3.9.2](https://github.com/Belphemur/SoundSwitch/tree/v3.9.2) (2016-01-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.1...v3.9.2)

**Implemented enhancements:**

- Make new instance of SoundSwitch close the previous one [\#66](https://github.com/Belphemur/SoundSwitch/issues/66)

**Closed issues:**

- Command line [\#61](https://github.com/Belphemur/SoundSwitch/issues/61)

## [v3.9.1](https://github.com/Belphemur/SoundSwitch/tree/v3.9.1) (2016-01-08)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.9.0...v3.9.1)

**Implemented enhancements:**

- Disabling completely the hotkeys [\#62](https://github.com/Belphemur/SoundSwitch/issues/62)

**Fixed bugs:**

- Sporadically hangs [\#43](https://github.com/Belphemur/SoundSwitch/issues/43)

## [v3.9.0](https://github.com/Belphemur/SoundSwitch/tree/v3.9.0) (2016-01-05)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.4...v3.9.0)

**Implemented enhancements:**

- Make update notification clearer for Windows 10 [\#59](https://github.com/Belphemur/SoundSwitch/issues/59)
- Personalized Notification Sound [\#52](https://github.com/Belphemur/SoundSwitch/issues/52)

**Closed issues:**

- Ability to switch default communications device as well. [\#60](https://github.com/Belphemur/SoundSwitch/issues/60)

## [v3.8.4](https://github.com/Belphemur/SoundSwitch/tree/v3.8.4) (2016-01-01)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.3...v3.8.4)

**Implemented enhancements:**

- Beta Channel [\#58](https://github.com/Belphemur/SoundSwitch/issues/58)

## [v3.8.3](https://github.com/Belphemur/SoundSwitch/tree/v3.8.3) (2015-12-31)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.2...v3.8.3)

**Fixed bugs:**

- SoundSwitch crash when disabling all Playback devices in the Control Panel [\#57](https://github.com/Belphemur/SoundSwitch/issues/57)
- Notification settings reset when Soundswitch closes when choosing "No Notification" \(3.8.2.25464\) [\#56](https://github.com/Belphemur/SoundSwitch/issues/56)

## [v3.8.2](https://github.com/Belphemur/SoundSwitch/tree/v3.8.2) (2015-12-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.1...v3.8.2)

**Fixed bugs:**

- Crash when using Windows Notification after SoundSwitch restart and switch device [\#55](https://github.com/Belphemur/SoundSwitch/issues/55)

## [v3.8.1](https://github.com/Belphemur/SoundSwitch/tree/v3.8.1) (2015-12-30)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.8.0...v3.8.1)

**Fixed bugs:**

- Soundswitch 3.7.0.23142 crashed when I tried to update it through the update button. [\#54](https://github.com/Belphemur/SoundSwitch/issues/54)
- App Freeze when using Sound Notification and Unplug device while sound playing. [\#53](https://github.com/Belphemur/SoundSwitch/issues/53)
- Crashes everytime I try to use it  [\#51](https://github.com/Belphemur/SoundSwitch/issues/51)

## [v3.8.0](https://github.com/Belphemur/SoundSwitch/tree/v3.8.0) (2015-12-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.7.0...v3.8.0)

**Implemented enhancements:**

- Personalize the Sound when switching device and play it on the new device. [\#49](https://github.com/Belphemur/SoundSwitch/issues/49)
- Create minidump in case of crash [\#48](https://github.com/Belphemur/SoundSwitch/issues/48)
- When an update is available, clicking on the notification launch the download [\#47](https://github.com/Belphemur/SoundSwitch/issues/47)

**Fixed bugs:**

- French language not loaded by SoundSwitch [\#50](https://github.com/Belphemur/SoundSwitch/issues/50)

## [v3.7.0](https://github.com/Belphemur/SoundSwitch/tree/v3.7.0) (2015-12-26)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.5...v3.7.0)

**Implemented enhancements:**

- Remove the use of Device Name in configuration for device ID [\#46](https://github.com/Belphemur/SoundSwitch/issues/46)

**Fixed bugs:**

- Version 3.6.5.13463 does not recognize playback devices, version 3.6.2.37344 does. [\#45](https://github.com/Belphemur/SoundSwitch/issues/45)
- By default SoundSwitch doesn't switch the Multimedia Device [\#44](https://github.com/Belphemur/SoundSwitch/issues/44)
- Crash and Restart when using Windows Remote Desktop. Problem when new device is connected. [\#41](https://github.com/Belphemur/SoundSwitch/issues/41)

## [v3.6.5](https://github.com/Belphemur/SoundSwitch/tree/v3.6.5) (2015-12-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.2...v3.6.5)

**Closed issues:**

- Default Audio device switching - programs are not [\#42](https://github.com/Belphemur/SoundSwitch/issues/42)

## [v3.6.2](https://github.com/Belphemur/SoundSwitch/tree/v3.6.2) (2015-12-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.1...v3.6.2)

**Implemented enhancements:**

- Add/Correct possibility to add only one key as hotkeys [\#37](https://github.com/Belphemur/SoundSwitch/issues/37)
- Toggle Option in the trayicon's menu [\#36](https://github.com/Belphemur/SoundSwitch/issues/36)

**Fixed bugs:**

- SoundSwitch Crashing when connecting a device [\#40](https://github.com/Belphemur/SoundSwitch/issues/40)
- Installer not detecting correctly VS 2015 Redist [\#39](https://github.com/Belphemur/SoundSwitch/issues/39)

**Closed issues:**

- Switching output bug in Chrome [\#38](https://github.com/Belphemur/SoundSwitch/issues/38)

## [v3.6.1](https://github.com/Belphemur/SoundSwitch/tree/v3.6.1) (2015-12-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.6.0...v3.6.1)

**Fixed bugs:**

- Installer doesn't detect DotNet 4.6.1 [\#35](https://github.com/Belphemur/SoundSwitch/issues/35)

**Merged pull requests:**

- Allow enable/disable display of notifications [\#33](https://github.com/Belphemur/SoundSwitch/pull/33) ([adamblackburn](https://github.com/adamblackburn))

## [v3.6.0](https://github.com/Belphemur/SoundSwitch/tree/v3.6.0) (2015-11-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.3...v3.6.0)

**Fixed bugs:**

- Display notification only once when default device is changed [\#32](https://github.com/Belphemur/SoundSwitch/issues/32)
- Soundswitch won't start. No crash report. [\#31](https://github.com/Belphemur/SoundSwitch/issues/31)

## [v3.5.3](https://github.com/Belphemur/SoundSwitch/tree/v3.5.3) (2015-11-12)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.2...v3.5.3)

## [v3.5.2](https://github.com/Belphemur/SoundSwitch/tree/v3.5.2) (2015-11-11)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.1...v3.5.2)

## [v3.5.1](https://github.com/Belphemur/SoundSwitch/tree/v3.5.1) (2015-09-18)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.5.0...v3.5.1)

**Implemented enhancements:**

- Update AudioEndPointLibrary [\#30](https://github.com/Belphemur/SoundSwitch/issues/30)

## [v3.5.0](https://github.com/Belphemur/SoundSwitch/tree/v3.5.0) (2015-09-14)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.2...v3.5.0)

**Implemented enhancements:**

- Implements improvement given by underlying library [\#29](https://github.com/Belphemur/SoundSwitch/issues/29)
- In case of crash. Zip the Log folder for easy reporting [\#24](https://github.com/Belphemur/SoundSwitch/issues/24)

## [v3.4.2](https://github.com/Belphemur/SoundSwitch/tree/v3.4.2) (2015-09-04)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.1...v3.4.2)

**Implemented enhancements:**

- Add localization [\#27](https://github.com/Belphemur/SoundSwitch/issues/27)
- Bigger Icons in Settings. Support non standard icons. [\#26](https://github.com/Belphemur/SoundSwitch/issues/26)

**Fixed bugs:**

- crash on Start: Recording Devices: Call IMMDeviceEnumerator::GetDefaultAudioEndpoint\(...\)  [\#25](https://github.com/Belphemur/SoundSwitch/issues/25)

## [v3.4.1](https://github.com/Belphemur/SoundSwitch/tree/v3.4.1) (2015-09-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.4.0...v3.4.1)

**Fixed bugs:**

- crashes on start: IndexOutOfRangeException [\#23](https://github.com/Belphemur/SoundSwitch/issues/23)

## [v3.4.0](https://github.com/Belphemur/SoundSwitch/tree/v3.4.0) (2015-09-02)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.3.1...v3.4.0)

**Implemented enhancements:**

- Group devices by state in Settings [\#22](https://github.com/Belphemur/SoundSwitch/issues/22)
- Switch Recording devices [\#19](https://github.com/Belphemur/SoundSwitch/issues/19)

**Closed issues:**

- Error: Index was outside the bounds of the array [\#20](https://github.com/Belphemur/SoundSwitch/issues/20)

## [v3.3.1](https://github.com/Belphemur/SoundSwitch/tree/v3.3.1) (2015-08-29)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.3.0...v3.3.1)

**Fixed bugs:**

- Settings not saving [\#21](https://github.com/Belphemur/SoundSwitch/issues/21)

## [v3.3.0](https://github.com/Belphemur/SoundSwitch/tree/v3.3.0) (2015-08-27)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.2...v3.3.0)

**Implemented enhancements:**

- Access the Windows Sound Mixer [\#18](https://github.com/Belphemur/SoundSwitch/issues/18)
- Update Icons to higher quality [\#17](https://github.com/Belphemur/SoundSwitch/issues/17)
- Add devices icons in the Settings Form [\#16](https://github.com/Belphemur/SoundSwitch/issues/16)
- Use AudioEndPointLibrary to manage audio devices [\#15](https://github.com/Belphemur/SoundSwitch/issues/15)
- Idea: Add function to switch "Default communication device" [\#14](https://github.com/Belphemur/SoundSwitch/issues/14)
- Auto-Updater [\#12](https://github.com/Belphemur/SoundSwitch/issues/12)

## [v3.2.2](https://github.com/Belphemur/SoundSwitch/tree/v3.2.2) (2015-08-24)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.1...v3.2.2)

**Implemented enhancements:**

- Add a logger to easily debug release version [\#9](https://github.com/Belphemur/SoundSwitch/issues/9)

**Fixed bugs:**

- Installer doesn't detect .NET 4.6 Preview [\#11](https://github.com/Belphemur/SoundSwitch/issues/11)
- SoundSwitch Settings menu cause appcrash [\#10](https://github.com/Belphemur/SoundSwitch/issues/10)
- SoundSwitch appears to crash immediately after launch [\#5](https://github.com/Belphemur/SoundSwitch/issues/5)

**Closed issues:**

- Application crashing at launch [\#8](https://github.com/Belphemur/SoundSwitch/issues/8)

## [v3.2.1](https://github.com/Belphemur/SoundSwitch/tree/v3.2.1) (2015-08-23)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.2.0...v3.2.1)

**Implemented enhancements:**

- Sign the application and installer [\#7](https://github.com/Belphemur/SoundSwitch/issues/7)

## [v3.2.0](https://github.com/Belphemur/SoundSwitch/tree/v3.2.0) (2015-08-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.2...v3.2.0)

**Implemented enhancements:**

- Make the uinstaller remove the startup registry key [\#4](https://github.com/Belphemur/SoundSwitch/issues/4)
- Add Changelog in installer [\#3](https://github.com/Belphemur/SoundSwitch/issues/3)
- Implement a better AutoStart feature [\#2](https://github.com/Belphemur/SoundSwitch/issues/2)

**Fixed bugs:**

- Program prevents system logout/shutdown [\#1](https://github.com/Belphemur/SoundSwitch/issues/1)

## [v3.1.2](https://github.com/Belphemur/SoundSwitch/tree/v3.1.2) (2015-08-22)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.1...v3.1.2)

## [v3.1.1](https://github.com/Belphemur/SoundSwitch/tree/v3.1.1) (2015-08-20)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.1.0...v3.1.1)

## [v3.1.0](https://github.com/Belphemur/SoundSwitch/tree/v3.1.0) (2015-08-20)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.0.1...v3.1.0)

## [v3.0.1](https://github.com/Belphemur/SoundSwitch/tree/v3.0.1) (2015-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v3.0.0...v3.0.1)

## [v3.0.0](https://github.com/Belphemur/SoundSwitch/tree/v3.0.0) (2015-08-19)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/v2.5.0...v3.0.0)

## [v2.5.0](https://github.com/Belphemur/SoundSwitch/tree/v2.5.0) (2015-08-17)

[Full Changelog](https://github.com/Belphemur/SoundSwitch/compare/1c44822dcabe2ace0323550db929762f62a6a710...v2.5.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
