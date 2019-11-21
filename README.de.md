<a href="https://soundswitch.aaflalo.me" title="SoundSwitch website"><img src="https://soundswitch.aaflalo.me/img/Main-Logo-Blue.svg" alt="logo SoundSwitch" height="180px" style="margin-left:auto;margin-right:auto;display:block;"></a>

[![Last Release](https://img.shields.io/github/release/Belphemur/SoundSwitch.svg)](https://soundswitch.aaflalo.me) [![Downloads for last Release](https://img.shields.io/github/downloads/Belphemur/SoundSwitch/total.svg)](https://soundswitch.aaflalo.me/) [![Donate](https://img.shields.io/badge/Donate-paypal%2Fcc-blue.svg)](https://soundswitch.aaflalo.me) [![Translate](https://hosted.weblate.org/widgets/soundswitch/-/svg-badge.svg)](https://hosted.weblate.org/projects/soundswitch/)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch?ref=badge_shield)

**SoundSwitch** bietet Ihnen die Möglichkeit mit einfachen Hotkeys Ihre Wiedergabe- und/oder Aufnahmegeräte zu wechseln.

**Nie wieder** durch mehrere Bildschirme und Menüs navigieren. SoundSwitch einfach **einmalig** einstellen - und schon können Sie zwischen Geräten schneller wechseln, als jemals zuvor!

## Vorschau
![preview](https://soundswitch.aaflalo.me/img/preview.gif)

## Abhängigkeiten
Die folgenden Komponenten werden benötigt, damit SoundSwitch auf Ihrem Windows Gerät funktioniert:
- [Microsoft .NET Framework 4.7 (Web Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=55170)

## Spenden
Wenn Sie eine Spende geben möchten für die Entwicklung, zögern Sie nicht, [dies hier zu tun](https://soundswitch.aaflalo.me/#donate).

## Einrichten
``Rechtsklicken`` Sie das SoundSwitch Symbol in Ihrem Systemtray und wählen Sie  `Einstellungen`. Jetzt wählen Sie aus, zwischen welchen Geräten Sie wechseln möchten. Optional können Sie hier auch die Tastatur-Kombination ändern, welche die Geräte wechselt. Dies ist für Wiedergabe- und Aufnahmegeräte über die entsprechenden Reiter oben im Fenster seperat möglich. Wenn Sie möchten, dass das Programm automatisch startet, wenn Sie Ihren PC einschalten, wählen Sie die Schaltfläche `Automatisch mit Windows starten` unter dem Reiter Einstellungen aus.

## Bedienung

Wählen Sie zuerst wie im vorigen Abschnitt beschrieben Ihre gewünschten Geräte aus.

**Nachdem Sie SoundSwitch eingerichtet haben** können Sie die folgenden Hotkeys verwenden.

- Um durch Ihre **Wiedergabegeräte** zu schalten:
  - `Ctrl` + `Alt` + `F11` (Standard) **ODER**
  - Doppelklicken auf das `SoundSwitch Symbol im Systemtray`.

- Um durch Ihre **Aufnahmegeräte** zu wechseln verwenden Sie:
  - `Ctrl` + `Alt` + `F7` (Standard)

---

## Funktionen

### Aufnahmegeräte
SoundSwitch kann sich auch um Ihre Aufnahmegeräte kümmern. Sie können einen spezifischen Hotkey hierfür einstellen, genau wie bei den Wiedergabegeräten.

### Benachrichtigungen
SoundSwitch bietet eine Auswahl aus fünf verschiedenen Benachrichtigungen, wenn ein Gerät gewechselt wird.

- #### Banner
Verwendet einen maßgefertigten Rahmen, welcher über allem angezeigt wird (always-on-top), nützlich für Verwendung in Spielen. Dies ist der vorgeschlagene Standard Anzeige Stil.

- #### Windows Benachrichtigung
Verwendet den Ballon Top von Windows. Im Falle von Windows 7, ist dies der kleine Ballon, welcher neben dem Systemtray Symbol auftaucht. Für Windows 10, ist dies das Benachrichtigungssystem, welches von der rechten Kante des Bildschirms ins Bild gleitet.

- #### Akustische Benachrichtigung
Diese Benachrichtigung ist ein Geräusch, das auf dem Gerät, zu dem gewechselt wurde, ausgegeben wird. Auf diese Weise wird das soeben gewechselte Gerät einen 'Ton' von sich geben, damit Sie wissen, dass es ausgewählt wurde.

- #### Angepasste Akustische Benachrichtigung
Genau wie bei Akustischer Benachrichtigung, aber Sie können selbst bestimmen, welches Geräusch abgespielt wird.

Wenn Sie die lautlose Benachrichtigung zurückhaben möchten, öffnen Sie einfach den Dateiauswahl Dialog und drücken Abbrechen. Dies wird das eingestellte Geräusch entfernen.

---

## Erweiterte Funktionen

### Kommunikationen
SoundSwitch kann ebenfalls ihr **Standardkommunikationsgerät** wechseln, wenn Sie dies in den Einstellungen auswählen. Windows unterscheidet zwischen Multimedia- und Kommunikationsgeräten; dies bedeutet, wenn ein Programm Zugriff auf das Kommunikationsgerät haben möchte, liefert Windows das Standardkommunikationsgerät. Standardmäßig ändert SoundSwitch nur das Multimediagerät, nicht jedoch das Kommunikationsgerät. Wenn Sie jedoch die entsprechende Option in den Einstellungen wählen, wird zusätzlich das Kommunikationsgerät geändert.

### Auto-Updater
Dies ist eines der interessanteren Features, der Auto-Updater. Alle 24 Stunden, überorüft SoundSwitch das GitHub repository (dank der Github API) auf eine neue Version. Wenn eine verfügbar ist, bekommt der Benutzer eine Benachrichtigung und der Menüpunkt 'Kein Update verfügbar' ändert sich zu 'Update verfügbar (X.X.X)' wobei X.X.X für die neue Versionsnummer steht. Wenn der Anwender diesen Punkt anklickt, öffnet sich ein neues Fenster mit einem Fortschrittsbalken (siehe Screenshots). Die neue Version wird automatisch in den Temporären Ordner des Benutzers heruntergeladen. Wenn der Download beendet ist, kann der Benutzer das Update installieren, indem er einfach den Installieren Knopf betätigt. Ein changelog wird ebenfalls bereit gestellt, indem die neuesten Infos von GitHub abgefragt werden.

#### Update Einstellungen
Es stehen drei verschiedene Optionen zur Verfügung: **Updates automatisch installieren**, heißt, dass das Programm sich automatisch im Hintergrund aktualisiert, ohne Nutzereingaben. **Benachrichtigen falls Update verfügbar**, benachrichtigt Sie, im Falle eines Updates. **Niemals**, ist selbsterklärend.

### Unterstütze Sprachen
Es stehen sechs Sprachen zur Auswahl: **Englisch**, **Französisch**, **Deutsch**, **Spanisch**, **Italienisch ** and **Portugiesisch(Brasilien)**.

Möchten Sie und mit einer Übersetzung helfen? Großartig! Besuchen Sie [Add or modify another language](https://github.com/Belphemur/SoundSwitch/wiki/Add-or-modify-another-language) für weitere Informationen.

## Auszeichungen

<a href="http://www.giga.de/downloads/soundswitch/" target="_blank"><img src="https://i.imgur.com/19GaPLQ.png" alt="Giga 5 stars" height="100" hspace="10"/></a><a href="http://www.softpedia.com/get/Multimedia/Audio/Other-AUDIO-Tools/SoundSwitch.shtml#status" target="_blank"><img src="http://s1.softpedia-static.com/_img/sp100free.png" alt="Softpedia" height="100" hspace="10"/></a><a href="http://www.chip.de/downloads/SoundSwitch_94258571.html" target="_blank"><img src="https://i.imgur.com/Nedw1su.png" alt="Chip Online de" height="100" hspace="10"/></a><a href="https://www.netzwelt.de/download/24278-soundswitch.html" target="_blank"><img src="https://i.imgur.com/VaMTnxV.png" alt="netzwelt GmbH" height="100" hspace="10"/></a>

## Beitragende

- Ursprünglicher Entwickler: [Jeroen Pelgrims](http://jeroenpelgrims.be)
- Benachrichtigungen ausschalten [#33](https://github.com/Belphemur/SoundSwitch/pull/33) [@adamblackburn](https://github.com/adamblackburn)
- Lokalisation und deutsche Sprache [#157](https://github.com/Belphemur/SoundSwitch/pull/157) [@FireEmerald](https://github.com/FireEmerald) 
- Banner Benachrichtigung [#186](https://github.com/Belphemur/SoundSwitch/pull/186) [@ramon18](https://github.com/ramon18)
- Keyboard hook, [Christian Liensberger](http://www.liensberger.it/web/blog/?p=207).
- Standardsoundgerät wechseln, [EreTIk](http://eretik.omegahg.com/).
- Akustische Benachrichtigung, [Music box notification sound by Robinhood76](https://www.freesound.org/people/Robinhood76/sounds/216676/).
- Spanische Sprache [#244](https://github.com/Belphemur/SoundSwitch/pull/244) [@plextoriano](https://github.com/plextoriano)
- Portugiesisch (Brasilien) [#258](https://github.com/Belphemur/SoundSwitch/pull/258) [@aleczk](https://github.com/aleczk)
- Beeindruckendes Logo [#278](https://github.com/Belphemur/SoundSwitch/pull/278) [@linadesteem](https://github.com/linadesteem)

## Danksagungen

### JetBrains

Für ihre Open-Source Lizenz und die hervorragenden IDEs und addons wie ReSharper für Visual Studio.

<a href="https://www.jetbrains.com" target="_blank"><img src="https://i.imgur.com/opT9XBj.png" alt="JetBrain Tooling" height="100" hspace="10"/></a>

## Credits

**Symbole**, das [Pastel SVG icon set](https://codefisher.org/pastel-svg/). Erstellt von Michael Buckley. ([CC BY-NC-SA 4.0](http://creativecommons.org/licenses/by-nc-sa/4.0/ ))

## Lizenz: GPLv2

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


[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FBelphemur%2FSoundSwitch?ref=badge_large)
