<a href="https://soundswitch.aaflalo.me" title="SoundSwitch Website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="SoundSwitch Logo" height="180px"></a>

[![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://soundswitch.aaflalo.me) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://soundswitch.aaflalo.me/) [![Translate](https://hosted.weblate.org/widgets/soundswitch/-/svg-badge.svg)](https://hosted.weblate.org/projects/soundswitch/) [![Donate](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://soundswitch.aaflalo.me) [![Help](https://img.shields.io/badge/Discord-Community%20&%20Help-green?style=flat-square&logo=discord)](https://discord.gg/gUCw3Ue)

**SoundSwitch** bietet Ihnen die Möglichkeit mit einfachen **Hotkeys** Ihre Wiedergabe- und Aufnahmegeräte zu wechseln.

**Nie wieder** durch mehrere Bildschirme und Menüs navigieren. SoundSwitch **einmalig** einstellen - und schon können Sie zwischen Geräten schneller wechseln, als jemals zuvor!

## ✨ Vorschau

![Vorschau](https://soundswitch.aaflalo.me/img/preview.gif)

## Anforderungen

- ⚠ Betriebssystem: Windows 7 oder neuer

## Spenden

Wenn Sie die Entwicklung unterstützen möchten, würden wir uns freuen, Sie [hier ❤](https://soundswitch.aaflalo.me/#donate) zu sehen.

## Einrichtung

_Rechtsklicken_ Sie das SoundSwitch Symbol in ihrer Taskleiste und wählen Sie _Einstellungen_. Jetzt wählen Sie aus, zwischen welchen Geräten Sie wechseln möchten. Optional können Sie hier auch die Tastatur-Kombinationen ändern, welche die Geräte wechselt. Wenn Sie möchten, dass das Programm automatisch startet wenn Sie Ihren PC einschalten, wählen Sie die Schaltfläche _Automatisch mit Windows starten_ aus.

## Bedienung

Wählen Sie zuerst, wie im vorigen Abschnitt beschrieben, Ihre gewünschten Geräte aus zwischen welchen Sie wecheln möchten.

**Nachdem Sie SoundSwitch eingerichtet haben** können Sie die folgenden Hotkeys verwenden:

- 🔊 Um durch Ihre **Wiedergabegeräte** zu schalten:

  - `Strg` + `Alt` + `F11` (Standard) **oder**
  - Doppelklicken auf das `SoundSwitch Symbol in der Taskleiste`.

- 🎙 Um durch Ihre **Aufnahmegeräte** zu wechseln, verwenden Sie:

  - `Strg` + `Alt` + `F7` (Standard)

- 🔇 Um das **Standardmikrofon** stummzuschalten:

  - `Strg` + `Alt` + `M` (Standard)

  Wenn ein Mikrofon stummgeschaltet wird, erscheint ein dauerhaftes Banner auf Ihrem Bildschirm, um Sie daran zu erinnern, dass Ihr Mikrofon stummgeschaltet ist. Das Banner bleibt sichtbar, bis Sie die Stummschaltung des Mikrofons aufheben oder auf das Banner klicken, um es direkt zu aktivieren.

## 🖱️ Taskleisten-Symbol Aktionen

Das Taskleisten-Symbol unterstützt konfigurierbare Doppelklick-Aktionen. Sie können wählen, was beim Doppelklick auf das SoundSwitch-Symbol passiert:

- **🔊 Gerät wechseln** (Standard): Wechselt durch Ihre konfigurierten Wiedergabegeräte
- **📋 Profil wechseln**: Wechselt durch Ihre konfigurierten Audio-Profile
- **⚙️ Einstellungen öffnen**: Öffnet das SoundSwitch-Einstellungsfenster

Dieses Verhalten kann in den SoundSwitch-Einstellungen angepasst werden, um Ihrem bevorzugten Arbeitsablauf zu entsprechen.

## 💻 Kommandozeile (CLI)

SoundSwitch verfügt über eine leistungsfähige Kommandozeilen-Schnittstelle, mit der Sie die Anwendung über die Befehlszeile steuern können:

- Zwischen Wiedergabe- und Aufnahmegeräten wechseln
- Mikrofon-Stummschaltung steuern
- Profile verwalten
- Einstellungen aufrufen

Weitere Details zu den verfügbaren Befehlen und deren Verwendung finden Sie in der [CLI-Dokumentation](SoundSwitch.CLI/README.md).

## _Umschalten_ Benachrichtigung

SoundSwitch bietet vier verschiedenen Arten der Benachrichtigung, wenn ein Gerät gewechselt wird:

- #### 🎟 Banner Benachrichtigung

  Verwendet ein Anzeigeelement, welches über allem liegt (always-on-top), nützlich für die Verwendung in Spielen. Dies ist die Standardart der Benachrichtigung.

- #### 🗨 Windows Benachrichtigung

  Verwendet die Standard Windows Benachrichtigung. Im Falle von Windows 7, ist dies das kleine Popup, welches überhalb der Taskleiste auftaucht. Für Windows 10, ist dies das Benachrichtigungssystem, welches von der rechten Kante des Bildschirms ins Bild gleitet.

- #### 🎵 Akustische Benachrichtigung
  Diese Benachrichtigung ist ein Ton, der über das aktuell aktive Augabegerät abgespielt wird. Auf diese Weise teilt das neue Gerät ihnen mit, dass es ausgewählt ist.

## Profile

Mithilfe von Profilen können Sie automatisch zu bestimmten Audiogeräten wechseln, wenn bestimmte Bedingungen erfüllt sind. Profile unterstützen mehrere Auslösertypen und erweiterte Geräteverwaltung:

### 🎯 Profil-Auslöser

- **⌨️ Hotkey-Auslöser**: Geräte mit benutzerdefinierten Tastenkombinationen wechseln. Mehrere Profile können dieselbe Tastenkombination verwenden und automatisch durchwechseln.

- **💫 Anwendungs-Auslöser**: Automatischer Gerätewechsel, wenn bestimmte Anwendungen den Fokus erhalten. Zum Beispiel Spotify auf Lautsprecher leiten, während Spiele das Headset verwenden.

- **🪟 Fenster-Auslöser**: Gerätewechsel basierend auf Fenstertiteln. Nützlich für Anwendungen, die ihre Fensternamen dynamisch ändern.

- **🎮 Steam Big Picture**: Spezielles Profil, das automatisch aktiviert wird, wenn der Steam Big Picture-Modus gestartet wird.

- **📱 UWP-App-Auslöser**: Unterstützung für Universal Windows Platform-Anwendungen mit automatischem Gerätewechsel.

- **🚀 Startup-Auslöser**: Profile, die automatisch aktiviert werden, wenn SoundSwitch startet.

- **🔄 Geräte-Geändert-Auslöser**: Erzwungene Profile, die bestimmte Gerätekonfigurationen beibehalten, auch wenn Windows versucht, sie zu ändern.

- **📋 Tray-Menü-Auslöser**: Profile, die direkt über das Kontextmenü der Taskleiste zugänglich sind.

### 🎚️ Erweiterte Profil-Funktionen

- **Multi-Geräte-Unterstützung**: Separate Geräte für Wiedergabe, Kommunikation, Aufnahme und Aufnahme-Kommunikation konfigurieren
- **Intelligente Gerätewiederherstllung**: Automatische Wiederherstellung vorheriger Audioeinstellungen, wenn ein Profil deaktiviert wird
- **Vordergrund-App-Wechsel**: Option, nur die Audio-Einstellungen der fokussierten Anwendung zu wechseln anstatt systemweit
- **Standard-Geräte-Kontrolle**: Wählen Sie, ob Windows-Standardgeräte geändert oder nur anwendungsspezifisches Routing verwendet werden soll
- **Benachrichtigungskontrolle**: Benachrichtigungen bei Profilaktivierung aktivieren/deaktivieren
- **Gerätevalidierung**: Automatische Überprüfung der Geräteverfügbarkeit mit Fallback-Behandlung

### 🔄 Hotkey-Durchschaltung

Wenn mehrere Profile dieselbe Tastenkombination verwenden, wechselt SoundSwitch automatisch zwischen ihnen durch. Wenn das Schnellmenü aktiviert ist, erscheint ein visueller Selektor, mit dem Sie das spezifische zu aktivierende Profil auswählen können.

## Erweiterte Funktionen

### 🎙 Kommunikation

SoundSwitch kann ebenfalls ihr **Standardkommunikationsgerät** wechseln, wenn Sie dies in den Einstellungen auswählen. Windows unterscheidet zwischen Multimedia- und Kommunikationsgeräten; dies bedeutet, wenn ein Programm Zugriff auf das Kommunikationsgerät haben möchte, liefert Windows das Standardkommunikationsgerät. Standardmäßig ändert SoundSwitch nur das Multimediagerät, nicht jedoch das Kommunikationsgerät. Wenn Sie jedoch die entsprechende Option in den Einstellungen wählen, wird zusätzlich das Kommunikationsgerät geändert.

### 📥 Auto-Updater

Alle 24 Stunden überprüft SoundSwitch das GitHub-Repository (dank der GitHub-API) auf ein neues Release. Falls ein neues Update verfügbar ist, erhalten Sie eine Benachrichtigung und der Menüpunkt "Kein Update verfügbar", im Kontextmenü des Taskleisten-Symbols, ändert sich zu "Update verfügbar". Die neue Version wird automatisch heruntergeladen und installiert, je nach aktueller _Update Einstellung_. Wir bieten dabei auch einen Changelog mit den neuesten Verbesserungen von SoundSwitch.

#### 🚥 Update Einstellungen

Es gibt drei verschiedene Arten, wie Updates installiert werden:

- **Updates automatisch installieren**, das Programm aktualisiert sich im Hintergrund selbständig ohne Nutzerinteraktion.
- **Benachrichtigen falls Update verfügbar**, benachrichtigt Sie im Falle das ein Update verfügbar ist.
- **Niemals**, ist selbsterklärend.

### 🌎 Unterstütze Sprachen

SoundSwitch ist in mehr als 20 Sprachen verfügbar, darunter **Englisch**, **Französisch**, **Deutsch**, **Spanisch**, **Italienisch**, **Portugiesisch (Brasilien)**, **Russisch**, **Chinesisch** und viele mehr.

Sie möchten etwas verbessern oder eine neue Sprache hinzufügen? Übersetzungen sind online editierbar [gleich hier](https://hosted.weblate.org/projects/soundswitch/#languages)!

## Auszeichungen

<a href="http://www.giga.de/downloads/soundswitch/"><img src="https://i.imgur.com/19GaPLQ.png" alt="Giga 5 stars" height="100" hspace="10"/></a><a href="http://www.softpedia.com/get/Multimedia/Audio/Other-AUDIO-Tools/SoundSwitch.shtml#status"><img src="http://s1.softpedia-static.com/_img/sp100free.png" alt="Softpedia" height="100" hspace="10"/></a><a href="http://www.chip.de/downloads/SoundSwitch_94258571.html"><img src="https://i.imgur.com/Nedw1su.png" alt="Chip Online de" height="100" hspace="10"/></a><a href="https://www.netzwelt.de/download/24278-soundswitch.html"><img src="https://i.imgur.com/VaMTnxV.png" alt="netzwelt GmbH" height="100" hspace="10"/></a>

## Danksagungen

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
- Icons [Font Awesome](https://fontawesome.com/), Creative Commons Attribution 4.0 International license: [License](https://fontawesome.com/license/free)

### 🤝 JetBrains ![JetBrain Tooling](https://i.imgur.com/SN2qAuL.png "JetBrain Tooling")

Vielen Dank für die Open-Source-Lizenz für ihre ausgezeichneten IDEs und Addons wie z.B. [ReSharper](https://www.jetbrains.com/resharper) für Visual Studio.

## Lizenz: GPLv2

<a href="https://app.fossa.io/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch?ref=badge_large"><img alt="FOSSA Status" align="right" src="https://app.fossa.io/api/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch.svg?type=large"></a>

Copyright (C) 2015 Jeroen Pelgrims

Copyright (C) 2015-2025 Antoine Aflalo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

Die vollständige GPLv2-Lizenzdatei befindet sich [hier](https://github.com/Belphemur/SoundSwitch/blob/master/LICENSE.txt).
