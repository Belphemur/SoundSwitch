---
title: Command-Line Interface
description: Automate SoundSwitch from scripts and shortcuts with the bundled SoundSwitch.CLI — switch playback, recording, profiles and microphone mute from any terminal.
---

# CLI

SoundSwitch includes a command-line interface (`SoundSwitch.CLI`) for advanced users who want to control audio devices, manage profiles, and automate device switching through scripts.

## Available Commands

The CLI provides the following capabilities:

- **Switch devices** — Change the active playback or recording device by name or index.
- **Mute microphone** — Toggle or set the mute state of the default communication microphone.
- **Manage profiles** — Switch between saved audio profiles or list available profiles.
- **Access settings** — Open the SoundSwitch settings window.
- **Check status** — Display the active profile and current default playback/recording devices.

## Using the CLI

Run `SoundSwitch.CLI.exe --help` (or `--version`) from the command line to see all available commands and their syntax. The CLI communicates with the main SoundSwitch process via IPC, so the main application should be running.

## Examples

| Command | Description |
|---------|-------------|
| `SoundSwitch.CLI.exe switch --type Playback` | Cycle to the next playback device. |
| `SoundSwitch.CLI.exe switch --type Recording` | Cycle to the next recording device. |
| `SoundSwitch.CLI.exe mute --toggle` | Toggle the microphone mute state. |
| `SoundSwitch.CLI.exe mute --state false` | Unmute the microphone. |
| `SoundSwitch.CLI.exe profile --list` | List all available audio profiles. |
| `SoundSwitch.CLI.exe profile --name "Gaming"` | Activate the "Gaming" audio profile. |
| `SoundSwitch.CLI.exe status` | Show active profile and current default audio devices. |
| `SoundSwitch.CLI.exe status --json` | Show active profile and current devices in machine-readable JSON. |
| `SoundSwitch.CLI.exe settings` | Open the SoundSwitch settings window. |

For the full list of commands and options, run `SoundSwitch.CLI.exe --help` or see the [SoundSwitch.CLI README](https://github.com/Belphemur/SoundSwitch/blob/master/SoundSwitch.CLI/README.md) on GitHub.
