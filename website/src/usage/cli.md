---
title: Command-Line Interface
description: Automate SoundSwitch from scripts and shortcuts with the bundled SoundSwitch.CLI — switch playback, recording, profiles and microphone mute from any terminal.
---

# CLI

SoundSwitch includes a command-line interface (`SoundSwitch.CLI`) for advanced users who want to control audio devices, manage profiles, and automate device switching through scripts.

All commands accept `--json` for machine-readable output; failures in JSON mode print a JSON object with an `error` field and exit with code 1.

## Available Commands

The CLI provides the following capabilities:

- **Switch devices** — Cycle through available playback or recording devices.
- **Mute microphone** — Toggle or set the mute state of the default communication microphone.
- **Manage profiles** — Switch between saved audio profiles or list available profiles.
- **Access settings** — Open the SoundSwitch settings window.
- **Check status** — Display the active profile and current default audio endpoints (playback, recording, and communication).
- **List devices** — Show devices that are both active and selected for switching in SoundSwitch settings.

## Using the CLI

Run `SoundSwitch.CLI.exe --help` (or `--version`) from the command line to see all available commands and their syntax. The CLI communicates with the main SoundSwitch process via IPC, so the main application should be running.

## Examples

| Command | Description |
|---------|-------------|
| `SoundSwitch.CLI.exe switch --type Playback` | Cycle to the next playback device. |
| `SoundSwitch.CLI.exe switch --type Recording` | Cycle to the next recording device. |
| `SoundSwitch.CLI.exe switch --type Playback --json` | Cycle to the next playback device and print JSON status. |
| `SoundSwitch.CLI.exe mute --toggle` | Toggle the microphone mute state. |
| `SoundSwitch.CLI.exe mute --state false` | Unmute the microphone. |
| `SoundSwitch.CLI.exe mute --json` | Print the current microphone mute state as JSON. |
| `SoundSwitch.CLI.exe profile --list` | List all available audio profiles. |
| `SoundSwitch.CLI.exe profile --list --json` | List all available audio profiles as JSON. |
| `SoundSwitch.CLI.exe profile --name "Gaming"` | Activate the "Gaming" audio profile. |
| `SoundSwitch.CLI.exe status` | Show active profile and current default audio devices. |
| `SoundSwitch.CLI.exe status --json` | Show active profile and current devices in machine-readable JSON. |
| `SoundSwitch.CLI.exe devices` | List devices selected for switching (active and selected in settings). |
| `SoundSwitch.CLI.exe devices --json` | List switchable devices in machine-readable JSON. |
| `SoundSwitch.CLI.exe settings` | Open the SoundSwitch settings window. |
| `SoundSwitch.CLI.exe settings --json` | Open the SoundSwitch settings window and print the result as JSON. |

For the full list of commands and options, run `SoundSwitch.CLI.exe --help` or see the [SoundSwitch.CLI README](https://github.com/Belphemur/SoundSwitch/blob/master/SoundSwitch.CLI/README.md) on GitHub.

## JSON Output

When used with `--json`, each command produces machine-readable JSON output. The most commonly scripted commands are shown below.

### status --json

```json
{
  "activeProfile": "Gaming",
  "playbackDevice": "Speakers (Realtek(R) Audio)",
  "recordingDevice": "Microphone (USB Audio Device)",
  "playbackCommunicationDevice": "Headset Earphone (HyperX Cloud II)",
  "recordingCommunicationDevice": "Headset Microphone (HyperX Cloud II)"
}
```

`activeProfile` is `null` when no profile has been triggered since startup.

Device fields are empty strings (`""`) when no matching device is present.

### devices --json

```json
{
  "playbackDevices": [
    "Speakers (Realtek(R) Audio)",
    "Headset Earphone (HyperX Cloud II)"
  ],
  "recordingDevices": [
    "Microphone (USB Audio Device)"
  ]
}
```

### profile --list --json

```json
[
  {
    "name": "Gaming",
    "playbackDevice": "Speakers (Realtek(R) Audio)",
    "playbackCommunicationDevice": "Headset Earphone (HyperX Cloud II)",
    "recordingDevice": "Microphone (USB Audio Device)",
    "recordingCommunicationDevice": "Headset Microphone (HyperX Cloud II)"
  }
]
```

Action commands (`switch`, `mute`, `settings`, `profile --name`) return simpler responses: `{ "success": true, ... }`, `{ "deviceName": "...", "isMuted": false }`, or `{ "success": true }`. On any failure in `--json` mode, the CLI prints `{ "error": "..." }` to stdout and exits with code 1.
